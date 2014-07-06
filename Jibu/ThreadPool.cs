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

using System.Collections.Generic;
using System.Threading;

namespace Jibu
{
    internal delegate void ThreadDelegate();
    // This class represents a Jibu Thread and manages the link between Jibu tasks
    // and the actual .NET managed threads. The class is internal and is only used from
    // the ThreadPool class.
    internal class JibuThread
    {
        private ThreadDelegate td;
        private AutoResetEvent runevent;
        private bool terminate;
        private Thread thread;

        public JibuThread()
        {           
            runevent = new AutoResetEvent(false);
            startThread();
        }

        internal ThreadDelegate Task
        {
            set { td = value; }
        }
      
        // Sets a flag indication that the thread should terminate.
        internal bool TerminateThread
        {      
            set { terminate = value; }
        }

        // The event that controls whether the thread should run or wait.
        internal AutoResetEvent RunEvent
        {
            get { return runevent; }
        }        
   
        // Runs the a Jibu thread in an operating system thread.
        private void startThread()
        {
            thread = new Thread(new ThreadStart(this.run), Manager.StackSize);
            thread.IsBackground = true;
            thread.Start();
        }

        public void runThread()
        {
            runevent.Set();
        }

        // The run method given to the operating system thread to start.
        private void run()
        {
            runevent.WaitOne();
            while (!terminate)
            {
                try
                {
                    td();
                    JibuThreadPool.freeThread(this);
                    runevent.WaitOne();
                }
                catch (System.Exception)
                {
                    //This catches both normal user-code caused exceptions and
                    //the ThreadAbortException 
                    terminate = true;
                    JibuThreadPool.freeThreadandRelease(this);
                    throw ;
                }
            }
        }       

        // Sets the priority of the current thread.
        public void setPriority(ThreadPriority pri)
        {
            thread.Priority = pri;
        }

        
    }

    internal static class JibuThreadPool
    {        
        static Stack<JibuThread> freeThreads = new Stack<JibuThread>();
        static Dictionary<JibuThread, JibuThread> busy = new Dictionary<JibuThread, JibuThread>();

        internal static JibuThread getThread()
        {
            lock (typeof(JibuThreadPool))
            {
                JibuThread temp;
                if (freeThreads.Count > 0)
                    temp = freeThreads.Pop();
                else
                {
                    temp = new JibuThread();
                }
                busy.Add(temp,temp);
                return temp;              
            }                
        }

        // Frees a JibuThread making it avaliable to new Jibu tasks.
        // If threadqueue with that stack size exists the free thread is added.
        // Otherwise the threadqueue is created and the free thread is added.
        internal static void freeThread(JibuThread t)
        {
            JibuThread temp;

            lock (typeof(JibuThreadPool))
            {
                temp = busy[t];
                freeThreads.Push(temp);

                temp.Task = null;
                temp.setPriority(ThreadPriority.Normal);
                busy.Remove(t);
            }
        }

        internal static void freeThreadandRelease(JibuThread t)
        {
            lock (typeof(JibuThreadPool))
            {
                if (busy.ContainsKey(t))
                {
                    busy[t].Task = null;
                    // used in PriParallel
                    busy[t].setPriority(ThreadPriority.Normal);
                    busy.Remove(t);
                }             
            }
        }


        public static void TerminateFreeThreads()
        {
            lock (typeof(JibuThreadPool))
            {
                //Queue<JibuThread> threadQueue;
                JibuThread thread;

                while (freeThreads.Count > 0)
                {
                    thread = freeThreads.Pop();
                    thread.TerminateThread = true;
                    thread.RunEvent.Set();
                }
            }
        }

        // Instructs the ThreadPool class to run the given Jibu task with the given priority and 
        // synchronizing on the provided barrier.
        internal static void runTask(ThreadDelegate task, ThreadPriority priority)
        {
            JibuThread thread = JibuThreadPool.getThread();
            thread.setPriority(priority);
            thread.Task = task;
            thread.runThread();            
        }

        internal static void runTask(ThreadDelegate task)
        {
            runTask(task, ThreadPriority.Normal);
        }

      }
}
