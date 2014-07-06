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
using SD = System.Diagnostics;
using System.Runtime.InteropServices;

namespace Jibu
{

    internal class TaskScheduler
    {
        private Deque<Task> queuedTasks;
        private volatile TaskScheduler next;
        private volatile bool stopped, stopRequested;
        private Thread thread;
        private int win32ThreadId = -1;


        [ThreadStatic]
        internal static TaskScheduler CurrentTaskScheduler;

        internal TaskScheduler()
        {
            queuedTasks = new Deque<Task>(100);
        }

        internal Thread Thread
        {
            get { return thread; }
        }

        internal int Win32ThreadId
        {
            get { return win32ThreadId; }
        }

        internal bool StopRequested
        {
            get { return stopRequested; }
            set { stopRequested = value; }
        }

        internal bool IsStopped
        {
            get { return stopped; }
        }

        internal TaskScheduler Next
        {
            get { return next; }
            set { next = value; }
        }

        internal Deque<Task> QueuedTasks
        {
            get { return queuedTasks; }
            set { queuedTasks = value; }
        }

        internal Task Steal()
        {
            return queuedTasks.Steal();
        }

        internal void Start()
        {
            thread = Thread.CurrentThread;

            CurrentTaskScheduler = this;

            while (!stopRequested)
            {
                Task w = queuedTasks.Pop();

                if (w == null)
                {
                    TaskScheduler other = next;

                    while (other != this)
                    {
                        w = other.Steal();

                        if (w != null)
                        {
                            break;
                        }
                        other = other.next;
                    }

                    if (w == null)
                    {
                        if (!ThreadScheduler.StealDeque(this))
                        {
                            stopped = true;
                            CurrentTaskScheduler = null;
                            return;
                        }
                        else
                        {
                            continue;
                        }

                    }
                }

                // w should not be null at this point
                w.RunTask();

            } // end while(!stopRequested)

            // we have been stopped
            ThreadScheduler.AddTasks(queuedTasks);

            stopped = true;
            CurrentTaskScheduler = null;
        }

    }

    internal static class ThreadScheduler
    {
        // magic sleep time
        private const int sleepTime = 10;
        private static int numCores = Manager.ProcessorCount;
        private static TaskScheduler last;
        private static Deque<Task> queuedTasks = new Deque<Task>(100);
        private static bool waiting;
        private static object dequeLock = new object();

        static ThreadScheduler()
        {
            JibuThreadPool.runTask(Start);
        }

        private static void CreateTaskScheduler()
        {
            TaskScheduler ts = new TaskScheduler();

            if (last == null) // we have no TaskSchedulers
            {
                last = ts;
                // we make it point to itself
                ts.Next = ts;
            }
            else
            {
                ts.Next = last.Next;
                last.Next = ts;
                last = ts;
            }

            if (!queuedTasks.Empty)
                StealDeque(ts);

            JibuThreadPool.runTask(ts.Start);
        }

        internal static void AddTask(Task w)
        {
            lock (dequeLock)
            {
                queuedTasks.Push(w);
                if (waiting)
                    Monitor.Pulse(dequeLock);
            }
        }

        internal static void AddTasks(Deque<Task> toBeAdded)
        {
            lock (dequeLock)
            {
                Task w;
                while ((w = toBeAdded.Steal()) != null)
                    queuedTasks.Push(w);
            }
        }

        internal static bool StealDeque(TaskScheduler tsStealer)
        {
            lock (dequeLock)
            {
                if (queuedTasks.Empty)
                    return false;
                else
                {
                    // swap deques
                    Deque<Task> tmp = tsStealer.QueuedTasks;
                    tsStealer.QueuedTasks = queuedTasks;
                    queuedTasks = tmp;
                    return true;
                }
            }

        }

        internal static void Start()
        {
            while (true)
            {

                int numThreads = 0;
                int numBlockedThreads = 0;
                int numStopRequested = 0;
                bool anyQueuedTasks = false;

                if (last != null)
                {
                    // this is a little messy, but must work for 1 or more threads.
                    TaskScheduler cur = last.Next;
                    TaskScheduler prev = last;

                    bool done = false;

                    while (!done)
                    {
                        if (cur == last) // is this the last iteration?
                            done = true;

                        if (cur.IsStopped)
                        {
                            if (cur == prev) // there's only one thread
                            {
                                last = null;
                                break;
                            }
                            else
                            {
                                // remove cur, prev does not change     
                                prev.Next = cur.Next;

                                if (cur == last) // we're removing the last thread
                                {
                                    last = prev;
                                }
                            }
                        }
                        else
                        {
                            numThreads++;

                            if (cur.Thread != null && (cur.Thread.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin)
                            {
                                numBlockedThreads++;
                            }

                            if (!cur.QueuedTasks.Empty)
                                anyQueuedTasks = true;

                            if (cur.StopRequested)
                            {
                                numStopRequested++;
                            }

                            if (numThreads - numBlockedThreads - numStopRequested > numCores)
                            {
                                numStopRequested++;
                                cur.StopRequested = true;
                            }

                            prev = cur;
                        }
                        cur = cur.Next;
                    }
                }

                int numRunningThreads = numThreads - numBlockedThreads;

                if (numCores > numRunningThreads)
                {

                    if (!queuedTasks.Empty || anyQueuedTasks)
                    {
                        int numNewScheds = numCores - numRunningThreads;

                        for (int i = 0; i < numNewScheds; i++)
                            CreateTaskScheduler();
                    }
                    else if (numThreads == 0)
                    {
                        Monitor.Enter(dequeLock);

                        if (queuedTasks.Empty)
                        {
                            waiting = true;
                            Monitor.Wait(dequeLock);
                            Monitor.Exit(dequeLock);
                        }
                        else
                        {
                            Monitor.Exit(dequeLock);
                            for (int i = 0; i < numCores; i++)
                                CreateTaskScheduler();
                        }
                    }
                }

                Timer.SleepFor(sleepTime);

            }
        }
    }

}
