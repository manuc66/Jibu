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
using System.Collections;

namespace Jibu
{
    /// <summary>
    /// The LoopDelegate type used in the For method.
    /// </summary>
    /// <param name="i">For loop index, which is incremented by one after each iteration</param>
    public delegate void LoopDelegate(int i);
    /// <summary>
    /// The ForeachDelegate type used in the Foreach method.
    /// </summary>
    /// <typeparam name="T">Type of collection</typeparam>
    /// <param name="element">An element the IEnumerable collection</param>
    public delegate void ForeachDelegate<T>(T element);

    /// <summary>
    /// The ReduceDelegate type used in the Reduce method.
    /// </summary>
    /// <typeparam name="T">At type that must be associative</typeparam>
    /// <param name="i">The index, which is incremented by one after each iteration</param>
    /// <returns></returns>
    public delegate T ReduceDelegate<T>(int i);

    /// <summary>
    /// The CombineDelegate used in the Reduce method.
    /// </summary>
    /// <typeparam name="T">At type that must be associative</typeparam>
    /// <param name="operand1">First operand</param>
    /// <param name="operand2">Second operand</param>
    /// <returns></returns>
    public delegate T CombineDelegate<T>(T operand1, T operand2);


    /// <summary>
    /// The Parallel class provides some basic parallel methods.
    /// </summary>
    /// <remarks>
    /// Parallel is a static class providing some basic parallel methods
    /// such as For, Reduce, Run and Gather.<p/>
    /// All methods, except some versions of Run, are implemented with Jibu's built-in scheduler that allows multiple tasks to run
    /// efficiently on the available processors/cores. It's important to understand that the built-in scheduler
    /// is not preemptive, meaning that it's not meant for executing tasks that run forever. If a task temporarily
    /// blocks, the scheduler detects it and ensures that the processor is kept busy executing another task.
    /// For more information about the scheduler, see the Task class.
    /// </remarks>
    public static class Parallel
    {
        /// <summary>
        /// A parallel version of a standard for loop.
        /// </summary>
        /// <remarks>
        /// Parallel.For is similar to a standard for loop starting at index i = start and running as long as i is less than end.
        /// loopbody is called once in each iteration and index i is incremented by one after each iteration.<p/>
        /// Grain size is implicitly set to two, implying that the unit of work is one call to loopbody. Hence one task is created for
        /// each index i. It's often possible to increase performance by adjusting the grain size. To do so use 
        /// Parallel.For(int start, int end, int grainSize, LoopDelegate loopbody).<p/>
        /// Parallel.For catches any uncaught exception raised inside the tasks. Once the exception is caught Parallel.For cancels all
        /// running tasks that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// tasks but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more 
        /// information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Tasks is cancelled, Parallel.For cancels the remaining Tasks
        /// and throws a Jibu.CancelException.</exception>
        /// <param name="start">First index, which is incremented by one until it equals end.</param>
        /// <param name="end">Last index, which is not included in the loop. The For loop runs as long as start is less than end.</param>
        /// <param name="loopbody">A delegate containing the work. Loopbody is executed once for each iteration.</param>
        // <example>
        // - Parallel For Example - 
        // <code><include ForExample/ForExample.cs></code>
        // </example>
        public static void For(int start, int end, LoopDelegate loopbody)
        {
            For(start, end, 1, loopbody);
        }


        /// <summary>
        /// A parallel version of a standard for loop.
        /// </summary>
        /// <remarks>
        /// Parallel.For is similar to a standard for loop starting at index i = start and running as long as i is less than end.
        /// loopbody is called once in each iteration and index i is incremented by one after each iteration. <p/>
        /// GrainSize specifies a reasonable number of iterations in each task. If the number of iterations is larger than grainSize, data 
        /// is split in two and handled separately. 
        /// Adjusting the grain size can lead to better performance, but the optimal grain
        /// size depends on the problem at hand. Increasing the grain size will decrease the amount of tasks, 
        /// and thus decrease the overhead of setting up tasks. But a small amount of tasks might introduce some load balancing problems,
        /// if no tasks are available for free processors. Decreasing the grain size will increase the amount of tasks and thereby ensure
        /// better load balancing, but on the other hand it will also increase the overhead of setting up tasks. Use 
        /// Parallel.For(int start, int end, LoopDelegate loopbody) at first and once your code is working, experiment with
        /// the grain size.<p/>
        /// Parallel.For catches any uncaught exception raised inside the tasks. Once the exception is caught Parallel.For cancels all
        /// running tasks that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// tasks but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Tasks is cancelled, Parallel.For cancels the remaining Tasks
        /// and throws a Jibu.CancelException.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Is thrown if the grain size is less than 1.</exception>
        /// <param name="start">First index, which is incremented by one until it equals end.</param>
        /// <param name="end">Last index, which is not included in the loop. The For loop runs as long as start is less than end.</param>
        /// <param name="grainSize">A reasonable number of iterations in each task</param>
        /// <param name="loopbody">A delegate containing the work. Loopbody is executed once for each iteration.</param> 
        // <example>
        // - Parallel For Example -
        // <code><include ForExample/ForExample.cs></code>
        // </example>
        public static void For(int start, int end, int grainSize, LoopDelegate loopbody)
        {

            if (grainSize < 1)
                throw new ArgumentOutOfRangeException("Grain size cannot be less than 1.");

            new DelegateAsync(delegate { ForTask(start, end, grainSize, loopbody); }).WaitFor();

        }

        /// <summary>
        /// A parallel foreach.
        /// </summary>
        /// <remarks>
        /// Parallel.Foreach is similar to a standard foreach. Loopbody, which takes an element of type T,
        /// is called once for each element in the collection. Hence loopbody contains the code you would normally
        /// put inside the foreach body.<p/>
        /// Parallel.Foreach catches any uncaught exception raised inside the tasks. Once the exception is caught Parallel.Foreach cancels all
        /// running tasks that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// tasks but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException.<p/>
        /// In this version of Foreach no grain size is specified and each iteration is processed in its own task.
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Tasks is cancelled, Parallel.Foreach cancels the remaining Tasks
        /// and throws a Jibu.CancelException.</exception>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="collection">An IEnumerable collection of type T</param>
        /// <param name="loopbody">A delegate containing the work. Loopbody is executed once for each iteration.</param>
        // <example>
        // - Parallel ForEach Example -
        // <code><include ForEachExample/ForEachExample.cs></code>
        // </example>
        public static void Foreach<T>(IEnumerable<T> collection, ForeachDelegate<T> loopbody)
        {
            Foreach(collection, 1, loopbody);
        }

        /// <summary>
        /// A parallel foreach.
        /// </summary>
        /// <remarks>
        /// Parallel.Foreach is similar to a standard foreach. Loopbody, which takes an element of type T,
        /// is called once for each element in the collection. Hence loopbody contains the code you would normally
        /// put inside the foreach loop.<p/>
        /// GrainSize specifies a reasonable number of iterations in each task. If the number of iterations is more than grainSize, data 
        /// is split in two and handled separately. 
        /// Adjusting the grain size can lead to better performance, but the optimal grain
        /// size depends on the problem at hand. Increasing the grain size will decrease the amount of tasks, 
        /// and thus decrease the overhead of setting up tasks. But a small amount of tasks might introduce some load balancing problems,
        /// if no tasks are available for free processors. Decreasing the grain size will increase the amount of tasks and thereby ensure
        /// better load balancing, but on the other hand it will also increase the overhead of setting up tasks.<p/>
        /// Parallel.Foreach catches any uncaught exception raised inside the tasks. Once the exception is caught Parallel.Foreach cancels all
        /// running tasks that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// tasks but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Tasks is cancelled, Parallel.Foreach cancels the remaining Tasks
        /// and throws a Jibu.CancelException.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Is thrown if the grain size is less than 1.</exception>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="collection">An IEnumerable collection of type T</param>
        /// <param name="grainSize">A reasonable number of iterations in each task</param>
        /// <param name="loopbody">A delegate containing the work. Loopbody is executed once for each iteration.</param>
        // <example>
        // - Parallel ForEach Example -
        // <code><include ForEachExample/ForEachExample.cs></code>
        // </example>
        public static void Foreach<T>(IEnumerable<T> collection, int grainSize, ForeachDelegate<T> loopbody)
        {

            if (grainSize < 1)
                throw new ArgumentOutOfRangeException("Grain size cannot be less than 1.");

            new DelegateAsync(
                delegate
                {
                    IEnumerator<T> ie = collection.GetEnumerator();
                    Async parent = null;

                    while (ie.MoveNext())
                    {
                        T[] data = new T[grainSize];
                        int i = 0;

                        do
                        {
                            data[i] = ie.Current;
                            i++;
                        }
                        while (i < grainSize && ie.MoveNext());

                        if (i == grainSize)
                        {
                            Async w = parent;
                            parent = new DelegateAsync(delegate { ForeachTask(data, i, loopbody, w); }).Start();
                        }
                        else
                        {
                            ForeachTask(data, i, loopbody, parent);
                            return;
                        }
                    }
                    if (parent != null)
                        parent.WaitFor();

                }).WaitFor();
        }


        /// <summary>
        ///  Calculates and combines values to a single value.
        /// </summary>
        /// <remarks>
        /// Parallel.Reduce combines multiple associative values into a single value. The ReduceDelegate loopbody
        /// takes an integer between start and end and returns an element of type T. The CombineDelegate takes two such
        /// elements and combine them. <p/>
        /// Parallel.Reduce avoids using locks by dividing the problem into a binary task tree where each task 
        /// updates it's own local value.<p/>
        /// Grain size is implicit set to two. It's often possible to increase performance by adjusting the grain size. 
        /// To do so use Parallel.Reduce(int start, int end, T initialValue, int grainSize, ReduceDelegate loopbody, CombineDelegate combineMethod).
        /// Parallel.<p/>
        /// Reduce catches any uncaught exception raised inside the tasks. Once the exception is caught Parallel.Reduce cancels all
        /// running tasks that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// tasks but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Tasks is cancelled, Parallel.Reduce cancels the remaining Tasks
        /// and throws a Jibu.CancelException.</exception>
        /// <typeparam name="T">A type that must be associative</typeparam>
        /// <param name="start">First index, which is incremented by one until it equals end.</param>
        /// <param name="end">Last index, which is not included in the loop. The loop runs as long as start is less than end.</param>
        /// <param name="initialValue">The initial value for the result.</param>
        /// <param name="loopbody">A delegate containing the work. Loopbody is executed once for each iteration.</param>
        /// <param name="combineMethod">A delegate used to combine the local results</param>
        /// <returns>A single value of type T</returns>        
        // <example>
        // - Parallel Reduce Example -
        // <code><include ReduceExample/ReduceExample.cs></code>
        // </example>
        public static T Reduce<T>(int start, int end, T initialValue, ReduceDelegate<T> loopbody, CombineDelegate<T> combineMethod)
        {
            return Reduce(start, end, initialValue, 1, loopbody, combineMethod);
        }


        /// <summary>
        /// Calculates and combines values to a single value.
        /// </summary>
        /// <remarks>
        /// Parallel.Reduce combines multiple associative values into a single value. The ReduceDelegate loopbody
        /// takes an integer between start and end and returns an element of type T. The CombineDelegate takes two such
        /// elements and combine them. <p/>
        /// Parallel.Reduce avoids using locks by dividing the problem into a binary task tree where each task 
        /// updates it's own local value.<p/>
        /// GrainSize specifies a reasonable number of iterations in each task. If the number of iterations is more than grainSize, data 
        /// is split in two and handled separately.  
        /// Adjusting the grain size can lead to better performance, but the optimal grain size depends on the problem at hand.
        /// Increasing the grain size will decrease the
        /// amount of tasks, and thus decrease the overhead of setting up tasks. But a small amount of tasks could introduce some load 
        /// balancing problems, if no tasks are available for free processors. Decreasing the grain size will increase the amount of 
        /// tasks and thereby ensure better load balancing, but on the other hand it will also increase the overhead of setting up tasks.<p/>
        /// Parallel.Reduce catches any uncaught exception raised inside the tasks. Once the exception is caught Parallel.Reduce cancels all
        /// running tasks that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// tasks but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Tasks is cancelled, Parallel.Reduce cancels the remaining Tasks
        /// and throws a Jibu.CancelException.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Is thrown if the grain size is less than 1.</exception>
        /// <typeparam name="T">A type that must be associative</typeparam>
        /// <param name="start">First index, which is incremented by one until it equals end.</param>
        /// <param name="end">Last index, which is not included in the loop. The loop runs as long as start is less than end.</param>
        /// <param name="initialValue">The initial value for the result.</param>
        /// <param name="grainSize">A reasonable number of iterations in each task</param>
        /// <param name="loopbody">A delegate containing the work. Loopbody is executed once for each iteration.</param>
        /// <param name="combineMethod">A delegate used to combine the local results</param>
        /// <returns>A single value of type T</returns>
        // <example>
        // - Parallel Reduce Example -
        // <code><include ReduceExample/ReduceExample.cs></code>
        // </example>
        public static T Reduce<T>(int start, int end, T initialValue, int grainSize, ReduceDelegate<T> loopbody, CombineDelegate<T> combineMethod)
        {
            if (end - start < 1)
                return initialValue;

            if (grainSize < 1)
                throw new ArgumentOutOfRangeException("Grain size cannot be less than 1.");

            return new DelegateFuture<T>(delegate { return combineMethod(initialValue, ReduceTask<T>(start, end, grainSize, loopbody, combineMethod)); }).Result();
        }

        /// <summary>
        /// Executes multiple tasks concurrently.
        /// </summary>
        /// <remarks>
        /// Parallel.Run executes each Async concurrently. All Asyncs are run by Jibu's built-in scheduler, which is
        /// not preemptive, so be careful about using tasks that run forever. If you need to run multiple tasks forever it's often
        /// better to use Parallel.RunAsThreads.
        /// Parallel.Run returns when all Asyncs are done.<p/>
        /// Parallel.Run catches any uncaught exception raised inside the Asyncs. Once the exception is caught Parallel.Run cancels all
        /// running Asyncs that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// Asyncs but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Async.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Asyncs is cancelled, Parallel.Run cancels the remaining Asyncs
        /// and throws a Jibu.CancelException.</exception>
        /// <param name="tasks">The tasks to be run concurrently</param>
        // <example>
        // - Parallel Run Example - 
        // <code><include ParallelExample/ParallelExample.cs></code>
        // </example>
        public static void Run(params Async[] tasks)
        {
            int numOfTasks = tasks.Length;            

            try
            {
                for (int i = 0; i < numOfTasks; i++)
                    tasks[i].Start();

            }
            catch (CancelException)
            {
                for (int i = 0; i < numOfTasks; i++)
                {
                    if (tasks[i] == null)
                        break;
                    else
                        tasks[i].Cancel();
                }

                throw;
            }

            try
            {
                for (int j = numOfTasks - 1; j >= 0; j--)
                    tasks[j].WaitFor();
            }
            catch (CancelException)
            {
                for (int k = 0; k < numOfTasks; k++)
                    tasks[k].Cancel();

                throw;
            }
        }

        /// <summary>
        /// Executes multiple tasks concurrently.
        /// </summary>
        /// <remarks>
        /// Parallel.RunAsThreads executes each Async in a separate OS thread, and is primarily meant for Asyncs 
        /// that run forever. Use Parallel.Run to run tasks with Jibu's built-in
        /// scheduler.
        /// Parallel.RunAsThreads returns when all Asyncs are done.<p/>
        /// Parallel.RunAsThreads catches any uncaught exception raised inside the Asyncs. Once the exception is caught Parallel.RunAsThreads cancels all
        /// running Asyncs that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// Asyncs but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Asyncs is cancelled, Parallel.RunAsThreads cancels the remaining Asyncs
        /// and throws a Jibu.CancelException.</exception>
        /// <param name="tasks">The tasks to be run concurrently</param>
        // <example>
        // - Parallel RunAsThreads Example -
        // <code><include ParallelExample/ParallelExample.cs></code>
        // </example>
        public static void RunAsThreads(params Async[] tasks)
        {
            int numOfTasks = tasks.Length;

            try
            {
                for (int i = 0; i < numOfTasks; i++)
                    tasks[i].Start(true);

            }
            catch (CancelException)
            {
                for (int i = 0; i < numOfTasks; i++)
                {
                    if (tasks[i] == null)
                        break;
                    else
                        tasks[i].Cancel();
                }

                throw;
            }

            try
            {
                for (int j = numOfTasks - 1; j >= 0; j--)
                    tasks[j].WaitFor();
            }
            catch (CancelException)
            {
                for (int k = 0; k < numOfTasks; k++)
                    tasks[k].Cancel();

                throw;
            }
        }



        /// <summary>
        /// Collects individual results from multiple Futures.
        /// </summary>
        /// <remarks>
        /// Parallel.Gather executes multiple Futures concurrently and returns an array containing the result of each
        /// individual Future. Results must be of the same type.<p/>
        /// Parallel.Gather catches any uncaught exception raised inside the Futures. Once the exception is caught Parallel.Gather cancels all
        /// running Futures that it has started and throws a Jibu.CancelException, containing the original exception. Jibu cancels all 
        /// Futures but it doesn't just brutally stop them - the user defined code must detect and handle the cancellation. For more information on cancellation see Jibu.Task.Cancel and Jibu.CancelException
        /// </remarks>
        /// <exception cref="Jibu.CancelException">If one of the Futures is cancelled, Parallel.Gather cancels the remaining Futures
        /// and throws a Jibu.CancelException.</exception>
        /// <typeparam name="T">The type of data returned from each Future</typeparam>
        /// <param name="tasks">The Futures to be run concurrently</param>
        /// <returns>An array of type T containing the Future results</returns>
        // <example>
        // - Parallel Gather Example -
        // <code><include GatherExample/GatherExample.cs></code>
        //</example>
        public static T[] Gather<T>(params Future<T>[] tasks)
        {
            int numOfTasks = tasks.Length;
            
            try
            {
                for (int i = 0; i < numOfTasks; i++)
                    tasks[i].Start();
            }
            catch (CancelException)
            {
                for (int i = 0; i < numOfTasks; i++)
                {
                    if (tasks[i] == null)
                        break;
                    else
                        tasks[i].Cancel();
                }

                throw;
            }

            T[] result = new T[numOfTasks];
            try
            {
                for (int j = 0; j < numOfTasks; j++)
                    result[j] = tasks[j].Result();
            }
            catch (CancelException)
            {
                for (int k = 0; k < numOfTasks; k++)
                    tasks[k].Cancel();

                throw;
            }

            return result;
        }



        // ------------------------------------------------------------ Private auxillary methods -------------------------------------------------


        private static void ForTask(int start, int end, int grainSize, LoopDelegate loopbody)
        {
            int num = end - start;

            if (num > grainSize)
            {
                int middle = (num / 2) + start;
                Async task = new DelegateAsync(delegate { ForTask(start, middle, grainSize, loopbody); }).Start();
                ForTask(middle, end, grainSize, loopbody);

                task.WaitFor();
            }
            else
            {
                for (int i = start; i < end; i++)
                    loopbody(i);
            }

        }


        private static void ForeachTask<T>(T[] data, int length, ForeachDelegate<T> loopbody, Async parent)
        {
            for (int i = 0; i < length; i++)
                loopbody(data[i]);

            if (parent != null)
                parent.WaitFor();
        }

        
        private static T ReduceTask<T>(int start, int end, int grainSize, ReduceDelegate<T> loopbody, CombineDelegate<T> combineMethod)
        {
            int num = end - start;
            T result;

            if (num > grainSize)
            {
                int middle = (num / 2) + start;
                Future<T> task = new DelegateFuture<T>(delegate { return ReduceTask(start, middle, grainSize, loopbody, combineMethod); }).Start();
                result = ReduceTask(middle, end, grainSize, loopbody, combineMethod);

                return combineMethod(result, task.Result());
            }

            // grainSize is never less than 1, thus num cannot be less than one.
            if (num == 1)
                return loopbody(start);

            result = combineMethod(loopbody(start), loopbody(start + 1));
            for (int i = start + 2; i < end; i++)
                result = combineMethod(result, loopbody(i));

            return result;
        }
    }
}
