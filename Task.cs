// Jibu - Software library for parallel programming
// Copyright(C) 2008 Axon7

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jibu
{
    internal enum TaskStatus { NOTSTARTED, RUNNING, DONE }

    /// <summary>
    /// The base Task class.
    /// </summary>
    /// <remarks> 
    /// ## Intro
    /// Every task that is run through the Jibu library is executed inside a Task. This is done by subclassing Async or Future
    /// and calling Jibu.Async.Start/Jibu.Future.Start
    /// or one of the static methods of the Parallel class, e.g. Jibu.Parallel.Run.
    /// DelegateAsync and DelegateFuture are implementations of Async and Future, and allows you to create
    /// tasks using only a delegate.
    /// The Task class supplies the context for the running task, and enables checking for cancellation and gives access to the Send and Receive methods of
    /// the current task.<p/>    
    /// ## Scheduling
    /// The Jibu library contains a sophisticated user level task-scheduler. The aim of the Jibu scheduler is to dynamically adjust the number of concurrent
    /// running threads, based on the workload and the number of processors. That is the Jibu scheduler tries to keep the number of running threads equal to the number cores.
    /// The Jibu scheduler manages a pool of threads that each have their own task-queues. The threads automatically distribute work between them by 
    /// performing work stealing, when the cores aren't fully utilized. Work stealing is done by idle threads managed by the Jibu library.<p/>
    /// Task scheduling means that tasks are not guaranteed to be executed in parallel, but this doesn't mean that you can't use blocking communication. 
    /// The Jibu scheduler detects if a supplied task is doing a blocking operation, and if the cores aren't fully utilized, then a new thread is spawned 
    /// and it immediately starts stealing work.
    /// This enables full utilization and lets tasks communicate with potentially blocking calls. Excess threads are removed from the scheduler
    /// when the cores are fully utilized when they are no longer needed.<p/>
    /// ## MailBox
    /// Every Task is born with a mailbox. The mailbox enables you to send messages of any type between Tasks, through the protected
    /// Send, Receive and ReceiveFrom methods. If you create a task using one of DelegateAsync or DelegateFuture, then mailbox communication will not be available.  
    /// ## Notes
    /// Note:
    /// Since the Jibu scheduler tries to keep the number of running threads equal to the number cores, you should watch out when creating 
    /// non-blocking tasks that spin or loop forever. Any such Async/Future should be started with the newThread parameter of the Start-method set to true, to
    /// bypass the Jibu scheduler.
    /// Jibu can't detect that a task is blocked in an I/O operation, so any I/O intensive task should also be run with the
    /// newThread parameter set to true.
    /// </remarks>
    // <example>
    // - Async Example -
    // <code><include AsyncExample/AsyncExample.cs></code>
    // </example>
    // <example>
    // - Future Example -
    // <code><include FutureExample/FutureExample.cs></code>
    // </example>
    // <example>
    // - Cancel Example -
    // <code><include CancelTaskExample/CancelTaskExample.cs></code>
    // </example>
    // <example>
    // - Mailbox Example -
    // <code><include MailBoxExample/MailBoxExample.cs></code>
    // </example>
    public abstract class Task
    {
        internal Exception caughtException = null;
        internal ThreadDelegate runMethod;
        internal volatile TaskStatus status = TaskStatus.NOTSTARTED;
        internal volatile bool waiting;
        private volatile bool cancelled = false;
        private Task parent;
        private Dictionary<Task, object> children;
        internal Address taskAddress;
        [ThreadStatic]
        private static Task currentTask;

        private int started = 0;

        internal Task()
        {
            children = new Dictionary<Task, object>();
            InitMailBox();
        }        
        
        #region private and internal members

        private bool TryReserve()
        {
            return (Interlocked.CompareExchange(ref started, 1, 0) == 0);               
        }

        internal void StartTask(bool newThread)
        {
            if (cancelled)
                ThrowCancelException();

            if (TryReserve())
            {

                parent = Task.CurrentTask;

                if (parent != null)
                {
                    // register at our parent-task
                    parent.RegisterChild(this);

                }

                if (newThread)
                {
                    JibuThreadPool.runTask(RunTask);
                }
                else
                {
                    TaskScheduler curTaskScheduler = TaskScheduler.CurrentTaskScheduler;
                    if (curTaskScheduler == null)
                        ThreadScheduler.AddTask(this);
                    else
                        curTaskScheduler.QueuedTasks.Push(this);
                }
            }
        }

        internal void RunTask()
        {         
            if (!cancelled)
            {
                this.status = TaskStatus.RUNNING;
                Task oldCurrentTask = Task.CurrentTask;
                Task.CurrentTask = this;

                try
                {
                    runMethod();
                }
                catch (Exception e)
                {
                    caughtException = e;
                    this.Cancel();
                }

                if (parent != null)
                    parent.UnRegisterChild(this);

                Task.CurrentTask = oldCurrentTask;
            }

            Monitor.Enter(this);
            this.status = TaskStatus.DONE;
            
            if (waiting)
                Monitor.PulseAll(this);            
            Monitor.Exit(this);
        }

        private void RegisterChild(Task w)
        {
            if (IsCancelled)
                throw new CancelException("Task created inside cancelled Task.");

            lock (children)
            {
                children.Add(w, null);
            }
        }

        private void UnRegisterChild(Task w)
        {
            lock (children)
            {
                children.Remove(w);
            }
        }
                
        private void ThrowCancelException()
        {
            CancelException exception;

            if (caughtException != null)
            {
                exception = new CancelException("Task was cancelled because an exception was caught. Check the InnerException property fore more information",
                    caughtException);
            }
            else
            {
                exception = new CancelException("Task was cancelled.");
            }

            throw exception;
        }

        internal void Wait()
        {
            switch (status)
            {
                case TaskStatus.DONE:
                    break;
                case TaskStatus.NOTSTARTED:
                    if (TryReserve())
                    {
                        // No one has called Start yet
                        // but now we stole the task
                        RunTask();
                    }
                    else
                    {
                        // Find our local scheduler
                        TaskScheduler curTaskScheduler = TaskScheduler.CurrentTaskScheduler;
                        if (curTaskScheduler != null)
                        {
                            if (curTaskScheduler.QueuedTasks.Swipe(this))
                            {
                                RunTask();
                            }
                            else
                            {
                                Monitor.Enter(this);
                                if (status != TaskStatus.DONE)
                                {
                                    waiting = true;
                                    Monitor.Wait(this);
                                }
                                Monitor.Exit(this);
                            }
                        }
                        else
                        {
                            // We're not in a JibuThread
                            Monitor.Enter(this);
                            if (status != TaskStatus.DONE)
                            {
                                waiting = true;
                                Monitor.Wait(this);
                            }
                            Monitor.Exit(this);
                        }
                    }
                    break;
                case TaskStatus.RUNNING:
                    Monitor.Enter(this);
                    if (status != TaskStatus.DONE)
                    {
                        waiting = true;
                        Monitor.Wait(this);
                    }
                    Monitor.Exit(this);
                    break;
            }

            if (this.IsCancelled)
            {
                ThrowCancelException();
            }
        }
        
        #endregion

        #region public members

        /// <summary>
        /// Gets the current executing Task.
        /// </summary>
        /// <remarks>
        /// Gets the current executing Task.
        /// For performance reasons it is a good idea only to access this property
        /// once, and assign the Task to a local variable.
        /// </remarks>
        public static Task CurrentTask
        {
            get { return Task.currentTask; }
            internal set { Task.currentTask = value; }
        }

        /// <summary>
        /// Cancels this Task, and all the running Tasks it has spawned.
        /// </summary>
        /// <remarks>
        /// Cancels this Task, and all the running Tasks it has spawned.
        /// If a Task is cancelled, all calls to WaitFor, IsDone, and the mailbox will throw 
        /// a CancelException.
        /// </remarks>
        // <example>
        // - Cancel Example -
        // <code><include CancelTaskExample/CancelTaskExample.cs></code>
        // </example>
        public void Cancel()
        {
            if (!cancelled)
            {
                cancelled = true;

                // cancel MailBox and pulse possible hanging receiver
                taskAddress.MailBox.Cancel();

                lock (children)
                {
                    foreach (KeyValuePair<Task, object> w in children)
                    {
                        w.Key.Cancel();
                    }
                }
            }
        }

        /// <summary>
        /// IsCancelled determines whether the Task has been cancelled.
        /// </summary>
        /// <remarks>
        /// IsCancelled determines whether the Task has been cancelled.
        /// </remarks>
        // <example>
        // - Cancel Example -
        // <code><include CancelTaskExample/CancelTaskExample.cs></code>
        // </example>
        public bool IsCancelled
        {
            get
            {
                return cancelled;
            }
        }

        /// <summary>
        /// IsDone determines whether the Task has finished execution.
        /// </summary>
        /// <remarks>
        /// IsDone determines whether the Task has finished execution. If the Task
        /// has been cancelled a CancelException will be thrown.
        /// </remarks>
        /// <exception cref="Jibu.CancelException">Thrown if the Task has been cancelled.</exception>
        public bool IsDone
        {
            get
            {
                if (this.IsCancelled)
                    ThrowCancelException();      

                return status == TaskStatus.DONE;
            }
        }
        
        #endregion      

        # region MailBox stuff

        /// <summary>
        /// Gets the Address of this Task instance.
        /// </summary>
        /// <remarks>
        /// Gets the Address of this Task instance.
        /// </remarks>
        /// <exception cref="Jibu.CancelException">Thrown if the Task has been cancelled.</exception> 
        // <example>
        // - Mailbox Example -
        // <code><include MailBoxExample/MailBoxExample.cs></code>
        // </example>
        public Address Address
        {
            get
            {
                if (this.IsCancelled)
                    ThrowCancelException();                             

                return taskAddress;
            }
        }

        /// <summary>
        /// Receives a mailbox message of type T from any sender. Sender Address is discarded.
        /// </summary>
        /// <remarks>
        /// Receives a mailbox message of type T from any sender. Sender Address is discarded. 
        /// Receive blocks until a message has been received.
        /// </remarks>
        /// <typeparam name="T">The type of the message</typeparam>
        /// <returns>The received message</returns>
        /// <exception cref="Jibu.CancelException">Thrown if the Task has been cancelled.</exception>
        // <example>
        // - Mailbox Example -
        // <code><include MailBoxExample/MailBoxExample.cs></code>
        // </example>
        protected T Receive<T>()
        {
            if (this.IsCancelled)
                ThrowCancelException();

            return this.taskAddress.MailBox.Get<T>();
        }

        /// <summary>
        /// Receives a mailbox message of type T from any sender. Sender Address is put in out parameter. 
        /// </summary>
        /// <remarks>
        /// Receives a mailbox message of type T from any sender. Sender Address is put in out parameter. 
        /// Receive blocks until a message has been received.
        /// </remarks>
        /// <typeparam name="T">The type of the message</typeparam>
        /// <returns>The received message</returns>
        /// <exception cref="Jibu.CancelException">Thrown if the Task has been cancelled.</exception>
        // <example>
        // - Mailbox Example -
        // <code><include MailBoxExample/MailBoxExample.cs></code>
        // </example>
        protected T Receive<T>(out Address address)
        {
            if (this.IsCancelled)
                ThrowCancelException();

            return this.taskAddress.MailBox.Get<T>(out address);
        }

        /// <summary>
        /// Receives a mailbox message of type T sent from the specified Address.
        /// </summary>
        /// <remarks>
        /// Receives a mailbox message of type T sent from the specified Address.
        /// ReceiveFrom blocks until a message has been received.
        /// </remarks>
        /// <param name="address">The address of the sender</param>
        /// <typeparam name="T">The type of the message</typeparam>
        /// <returns>The received message</returns>
        /// <exception cref="Jibu.CancelException">Thrown if the Task has been cancelled.</exception>
        // <example>
        // - Mailbox Example -
        // <code><include MailBoxExample/MailBoxExample.cs></code>
        // </example>
        protected T ReceiveFrom<T>(Address address)
        {
            if (this.IsCancelled)
                ThrowCancelException();

            return this.taskAddress.MailBox.GetFrom<T>(address);
        }

        /// <summary>
        /// Sends a mailbox message to the specified Address.
        /// </summary>
        /// <remarks>
        /// Sends a mailbox message to the specified Address.
        /// </remarks>
        /// <param name="data">The message</param>
        /// <param name="address">The Address of the destination</param>
        /// <exception cref="Jibu.CancelException">Thrown if the Task has been cancelled.</exception>
        // <example>
        // - Mailbox Example -
        // <code><include MailBoxExample/MailBoxExample.cs></code>
        // </example>        
        protected void Send(object data, Address address)
        {
            if (this.IsCancelled)
                ThrowCancelException();

            address.MailBox.Put(data, this.Address);
        }


        private void InitMailBox()
        {
            taskAddress = new Address();
        }

        #endregion
    }

    /// <summary>
    /// An asynchronous Task that does not return a result.
    /// </summary>
    /// <remarks>
    /// This class extends the abstract Task class, but also exposes the WaitFor and Start methods.
    /// When creating an Async task, you must implement the abstract Run method.
    /// Start immediately schedules the Async for execution.
    /// WaitFor waits for the Task to finish execution. Any uncaught exception thrown by the
    /// task will be rethrown by WaitFor, encapsulated in a CancelException. If the Async is cancelled, then WaitFor throws a CancelException.<p/>
    /// For information about the Jibu scheduler and examples demonstrating the mailbox system, please refer to the Task class. 
    /// </remarks>
    // <example>
    // - Async Example -
    // <code><include AsyncExample/AsyncExample.cs></code>
    // </example>
    public abstract class Async : Task
    {

        /// <summary>
        /// Creates a new Async instance.
        /// </summary>
        protected Async()
        {
            runMethod = delegate()
            {
                this.Run();
            };
        }

        /// <summary>
        /// This method must contain the work to be done in this Async.
        /// </summary>
        public abstract void Run();


        /// <summary>
        /// Immediately schedules this Async instance for execution.
        /// </summary>
        /// <remarks>
        /// Immediately schedules this Async instance for execution in Jibu's built-in task scheduler.<p/>
        /// An Async can only be started once. Every subsequent attempt to start the same Async will be ignored.
        /// </remarks>
        /// <returns>This Async instance</returns>
        /// <exception cref="Jibu.CancelException">Thrown if this Async is cancelled or it is started inside a cancelled Task.</exception>
        // <example>
        // - Async Example -
        // <code><include AsyncExample/AsyncExample.cs></code>
        // </example>
        public Async Start()
        {
            this.StartTask(false);
            return this;
        }

        /// <summary>
        /// Starts this Async instance.
        /// </summary>
        /// <remarks>
        /// Starts this Async instance.
        /// Depending on the value of newThread the task is either executed
        /// in its own separate thread, or it is scheduled for execution in Jibu's built-in task scheduler.<p/>
        /// An Async can only be started once. Every subsequent attempt to start the same Async will be ignored.
        /// </remarks>
        /// <param name="newThread">If true the task is run in a separate thread, else the task is run in Jibu's built-in task scheduler.</param>
        /// <returns>This Async instance</returns>
        /// <exception cref="Jibu.CancelException">Thrown if this Async is cancelled or it is started inside a cancelled Task.</exception>
        // <example>
        // - Async Example -
        // <code><include AsyncExample/AsyncExample.cs></code>
        // </example>
        public Async Start(bool newThread)
        {
            this.StartTask(newThread);
            return this;
        }

        /// <summary>
        /// Waits until the Async has finished execution.
        /// </summary>
        /// <remarks>
        /// Waits until the Async has finished execution.<p/>
        /// If the Start method hasn't been invoked, then the Async will be executed immediately in this thread, bypassing
        /// the Jibu scheduler.<p/>
        /// If the Async has been scheduled and is running, then WaitFor waits until it has finished execution 
        /// else if the Async is already done, WaitFor returns immediately.<p/>
        /// Any uncaught exception thrown by the executed task will be rethrown by WaitFor, encapsulated in a CancelException. If the Async is cancelled
        /// a CancelException is thrown.
        /// </remarks>
        /// <exception cref="Jibu.CancelException">Thrown if the Async has been cancelled, or if user code has generated an exception. In the latter case the 
        /// InnerException property will be set.</exception>
        // <example>
        // - Async Example -
        // <code><include AsyncExample/AsyncExample.cs></code>
        // </example>
        public void WaitFor()
        {
            base.Wait(); 
        }
    }

    /// <summary>
    /// An asynchronous Task that returns a result.
    /// </summary>
    /// <remarks>
    /// This class extends the abstract Task class, but also exposes the Result and Start methods.
    /// When creating a Future task, you must implement the abstract Run method.
    /// Start immediately schedules the Future for execution.
    /// Result waits for the Task to finish execution and returns the result. Any uncaught exception thrown by the
    /// task will be rethrown by Result, encapsulated in a CancelException. If the Future is cancelled, then Result throws a CancelException.<p/>
    /// For information about the Jibu scheduler and examples demonstrating the mailbox system, please refer to the Task class. 
    /// </remarks>
    /// <typeparam name="T">The type of the result</typeparam>
    // <example>
    // - Future Example -
    // <code><include FutureExample/FutureExample.cs></code>
    // </example>
    public abstract class Future<T> : Task
    {
        private T result;        

        /// <summary>
        /// Creates a new Future instance
        /// </summary>
        protected Future()
        {
            runMethod = delegate()
            {
                result = this.Run();
            };
        }        
        
        #region public constructors
        /*
        /// <summary>
        /// Creates a new Future instance that immediately schedules the supplied IFutureTask for execution.
        /// </summary>
        /// <remarks>
        /// Creates a new Future instance that immediately schedules the supplied IFutureTask for execution in Jibu's built-in task scheduler.
        /// </remarks>
        /// <param name="task">The task to be run.</param>
        /// <exception cref="Jibu.CancelException">Thrown if CurrentTask has been cancelled.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public Future(IFutureTask<T> task)
            : this(task, false)
        {

        }

        /// <summary>
        /// Creates a new Future instance that immediately schedules the supplied FutureTask delegate for execution.
        /// </summary>
        /// <remarks>
        /// Creates a new Future instance that immediately schedules the supplied FutureTask delegate for execution in Jibu's built-in task scheduler.
        /// </remarks>
        /// <param name="task">The task to be run.</param>
        /// <exception cref="Jibu.CancelException">Thrown if CurrentTask has been cancelled.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public Future(FutureTask<T> task)
            : this(task, false)
        {

        }

        /// <summary>
        /// Creates a new Future instance, the task execution method is dependent on the value of the newThread parameter.
        /// </summary>
        /// <remarks>
        /// Creates a new Future instance. Depending on the value of newThread the task is either executed
        /// in its own separate thread, or it is scheduled for execution in Jibu's built-in task scheduler.
        /// </remarks>
        /// <param name="task">The task to be run.</param>
        /// <param name="newThread">If true the task is run in a separate thread, else the task is run in Jibu's built-in task scheduler.</param>
        /// <exception cref="Jibu.CancelException">Thrown if CurrentTask has been cancelled.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public Future(IFutureTask<T> task, bool newThread)
            : base()
        {
            runMethod = delegate()
            {
                result = task.Run();
            };

            Start(newThread);
        }

        /// <summary>
        /// Creates a new Future instance, the task execution method is dependent on the value of the newThread parameter.
        /// </summary>
        /// <remarks>
        /// Creates a new Future instance. Depending on the value of newThread the task is either executed
        /// in its own separate thread, or it is scheduled for execution in Jibu's built-in task scheduler.
        /// </remarks>
        /// <param name="task">The task to be run.</param>
        /// <param name="newThread">If true the task is run in a separate thread, else the task is run in Jibu's built-in task scheduler.</param>
        /// <exception cref="Jibu.CancelException">Thrown if CurrentTask has been cancelled.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public Future(FutureTask<T> task, bool newThread)
            : base()
        {
            runMethod = delegate()
            {
                result = task();
            };

            Start(newThread);
        }*/

        #endregion

        /// <summary>
        /// This method must contain the work to be done in this Future.
        /// </summary>
        /// <returns>The result of the calculations</returns>
        public abstract T Run();

        /// <summary>
        /// Immediately schedules this Future instance for execution.
        /// </summary>
        /// <remarks>
        /// Immediately schedules this Future instance for execution in Jibu's built-in task scheduler.<p/>
        /// An Future can only be started once. Every subsequent attempt to start the same Future will be ignored.
        /// </remarks>
        /// <returns>This Future instance</returns>
        /// <exception cref="Jibu.CancelException">Thrown if this Future is cancelled or it is started inside a cancelled Task.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public Future<T> Start()
        {
            this.StartTask(false);
            return this;
        }

        /// <summary>
        /// Starts this Future instance.
        /// </summary>
        /// <remarks>
        /// Starts this Future instance.
        /// Depending on the value of newThread the task is either executed
        /// in its own separate thread, or it is scheduled for execution in Jibu's built-in task scheduler.<p/>
        /// A Future can only be started once. Every subsequent attempt to start the same Future will be ignored.
        /// </remarks>
        /// <param name="newThread">If true the task is run in a separate thread, else the task is run in Jibu's built-in task scheduler.</param>
        /// <returns>This Future instance</returns>
        /// <exception cref="Jibu.CancelException">Thrown if this Future is cancelled or it is started inside a cancelled Task.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public Future<T> Start(bool newThread)
        {
            this.StartTask(newThread);
            return this;
        }

        /// <summary>
        /// Waits until the Future has finished execution, and returns the result.
        /// </summary>
        /// <remarks>
        /// Waits until the Future has finished execution, and returns the result.<p/>
        /// If the Start method hasn't been invoked, then the Future will be executed immediately in this thread, bypassing
        /// the Jibu scheduler.<p/>
        /// If the Future has been scheduled and is running, then Result waits until it has finished execution 
        /// else if the Future is already done, Result returns immediately.<p/>
        /// Any uncaught exception thrown by the executed task will be rethrown by Result, encapsulated in a CancelException. If the Future is cancelled
        /// a CancelException is thrown.
        /// </remarks>
        /// <exception cref="Jibu.CancelException">Thrown if the Future has been cancelled, or if user code has generated an exception. In the latter case the 
        /// InnerException property will be set.</exception>
        // <example>
        // - Future Example -
        // <code><include FutureExample/FutureExample.cs></code>
        // </example>
        public T Result()
        {
            Wait();
            return result;
        }

    }

    /// <summary>
    /// The AsyncWork delegate provides the signature for running 
    /// an asynchronous task with the DelegateAsync class.
    /// </summary>
    /// <remarks>
    /// The AsyncWork delegate provides the signature for running 
    /// an asynchronous task with the DelegateAsync class.
    /// <param name="async">The Async instance that is executing this delegate</param>
    /// </remarks>
    // <example>
    // - AsyncWork Example -
    // <code><include DelegateExample/DelegateExample.cs></code>
    // </example>
    public delegate void AsyncWork(Async async);

    /// <summary>
    /// The FutureWork delegate provides the signature for running
    /// an asynchronous task that returns a result with the DelegateFuture class.
    /// </summary>
    /// <typeparam name="T">The return type of the asynchronous task.</typeparam>
    /// <remarks>
    /// The FutureWork delegate provides the signature for running
    /// an asynchronous task that returns a result with the DelegateFuture class.
    /// </remarks>
    /// <param name="future">The Future instance that is executing this delegate</param>
    /// <returns>A result</returns>
    // <example>
    // - FutureWork Example -
    // <code><include DelegateExample/DelegateExample.cs></code>
    // </example>
    public delegate T FutureWork<T>(Future<T> future);
    
    

    /// <summary>
    /// An implementation of Async, that can run a supplied delegate method.
    /// </summary>
    // <example>
    // - DelegateAsync Example -
    // <code><include DelegateExample/DelegateExample.cs></code>
    // </example>
    public class DelegateAsync : Async
    {
        private AsyncWork taskDelegate;

        /// <summary>
        /// Creates a new DelegateAsync that can execute the supplied delegate.
        /// </summary>
        /// <param name="work">The work to be done</param>
        // <example>
        // - DelegateAsync Example -
        // <code><include DelegateExample/DelegateExample.cs></code>
        // </example>
        public DelegateAsync(AsyncWork work)
        {
            taskDelegate = work;
        }

        /// <summary>
        /// Runs the supplied delegate.
        /// </summary>
        // <example>
        // - DelegateAsync Example -
        // <code><include DelegateExample/DelegateExample.cs></code>
        // </example>
        public override void Run()
        {
            taskDelegate(this);
        }
    }

    /// <summary>
    /// An implementation of Future, that can run a supplied delegate method
    /// and return the result.
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    // <example>
    // - DelegateFuture Example -
    // <code><include DelegateExample/DelegateExample.cs></code>
    // </example>
    public class DelegateFuture<T> : Future<T>
    {
        private FutureWork<T> taskDelegate;

        /// <summary>
        /// Creates a new DelegateFuture that can execute the supplied delegate
        /// and return the result.
        /// </summary>
        /// <param name="work">The work to be done</param>
        // <example>
        // - DelegateFuture Example -
        // <code><include DelegateExample/DelegateExample.cs></code>
        // </example>
        public DelegateFuture(FutureWork<T> work)
        {
            taskDelegate = work;
        }

        /// <summary>
        /// Runs the supplied delegate.
        /// </summary>
        /// <returns></returns>
        // <example>
        // - DelegateFuture Example -
        // <code><include DelegateExample/DelegateExample.cs></code>
        // </example>
        public override T Run()
        {
            return taskDelegate(this);
        }
    }

}
