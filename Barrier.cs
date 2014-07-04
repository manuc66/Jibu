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
using System.Threading;

namespace Jibu
{

    /// <summary>The Barrier class synchronizes multiple tasks. </summary>
    /// <remarks>
    /// The Barrier class synchronizes multiple tasks. The class allows you
    /// to enroll tasks on the barrier and once enrolled a task can call the Synchronize method
    /// to synchronize with all other tasks enrolled on the barrier. 
    /// 
    /// The Synchronize method simply blocks
    /// until each task enrolled on the barrier has called Synchronize, hence the task will block
    /// until it has been synchronized with all enrolled tasks. Enrolled tasks may call Synchronize
    /// multiple times before leaving the barrier - leaving is done by calling Resign.
    /// Leaving the barrier without resigning will cause all the remaining
    /// enrolled tasks to block forever once they call Synchronize to synchronize on the barrier!
    /// 
    /// Note: Every task must call Enroll before it calls Synchronize or Resign. 
    /// The barrier doesn't keep track of each enrolled task and therefore doesn't know whether
    /// a task calling Synchronize or Resign has actually been enrolled on the barrier. Hence the 
    /// programmer is responsible for ensuring proper use of the barrier and failing to do so
    /// will mess up the program. 
    /// 
    /// The proper use of the barrier is to first call Enroll, then 
    /// the task can synchronize multiple times on the barrier by calling Synchronize multiple times.
    /// And finally the task leaves the barrier by calling Resign. 
    /// </remarks>  
    // <example>
    // - Barrier Example -
    // <code><include BarrierExample/BarrierExample.cs></code>
    // </example>    
    public class Barrier : IDisposable
    {
        private int numEnrolled, numSynched, numLeft;
        private ManualResetEvent syncEvent, synchFinish;

        /// <summary>Creates a Barrier with no tasks enrolled</summary>
        /// <remarks>A new Barrier with no attached tasks is constructed. Tasks
        /// are enrolled by calling the enroll method.</remarks>
        // <example>
        // - Barrier Example -
        // <code><include BarrierExample/BarrierExample.cs></code>
        // </example>    
        public Barrier()
        {
            syncEvent = new ManualResetEvent(false);
            synchFinish = new ManualResetEvent(true);      
        }

        
        /// <summary>Creates a Barrier with a specified number of tasks enrolled.</summary>
        /// <remarks>Creates a Barrier with tasksEnrolled tasks. Tasks enrolled on the barrier
        /// in the constructor should not call Enroll, doing so will mess up the program.
        /// Tasks not enrolled by the constructor can, however, enroll later on, but it's the
        /// responsibility of the programmer that tasks don't enroll twice on the barrier or
        /// call Synchronize or Resign without being enrolled on the barrier.</remarks>
        /// <param name="tasksEnrolled">tasksEnrolled The number of enrolled tasks.</param>
        // <example>
        // - Barrier Example -
        // <code><include BarrierExample/BarrierExample.cs></code>
        // </example>    
        public Barrier(int tasksEnrolled)
        {
            this.numEnrolled = tasksEnrolled;            
            syncEvent = new ManualResetEvent(false);
            synchFinish = new ManualResetEvent(true);        
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Barrier()
        {
            Dispose(false);
        }
 
        /// <summary>Enrolls a task on the barrier.</summary>        
        /// <remarks>Enroll enrolls the calling task on the barrier. Once a task is enrolled on
        /// the barrier it can call the Synchronize or Resign methods. The Barrier object doesn't
        /// keep track of the individual tasks, thus the programmer is responsible for not
        /// enrolling a task multiple times!
        /// Calling Enroll increases the number of enrolled tasks on the barrier by one.</remarks>
        // <example>
        // - Barrier Example -
        // <code><include BarrierExample/BarrierExample.cs></code>
        // </example>    
        public void Enroll()
        {
            lock (this)
            {
                numEnrolled++;
            }
        }        
        
        /// <summary>Resigns a task from the barrier.</summary>        
        /// <remarks>Resign must be called when a task wants to leave the barrier. 
        /// It's the  responsibility of the programmer to ensure that every task calling Resign 
        /// is actually enrolled on the barrier.
        /// When Resign is called the number of enrolled tasks on the barrier is decreased by one.</remarks>
        // <example>
        // - Barrier Example -
        // <code><include BarrierExample/BarrierExample.cs></code>
        // </example>    
        public void Resign()
        {
            lock (this)
            {
                numEnrolled--;
                if (numSynched == numEnrolled)
                {
                    if (numEnrolled == 0)
                    {
                        syncEvent.Reset();
                        synchFinish.Set();
                    }
                    else
                    {
                        syncEvent.Set();
                        synchFinish.Reset();
                        numLeft = numSynched;
                        numSynched = 0;
                    }
                }
            }
        }
        
        
        ///  <summary>Synchronizes a task on the barrier.</summary>         
        /// <remarks>The Synchronize method is blocked until every enrolled task has called Synchronize.
        /// When every enrolled task has called Synchronize, the tasks are released and 
        /// Synchronize returns.
        /// Note: New tasks may enroll on the barrier while other
        /// tasks are hanging in the Synchronize method, meaning that these new tasks will have to 
        /// call Synchronize too, before the enrolled tasks are released. If, however, a task enrolls on
        /// the barrier while the barrier is in the process of releasing enrolled task hanging in
        /// Synchronize, the new task isn't enrolled until the original enrolled tasks have been released(synchronized).
        /// 
        /// It's the responsibility of the programmer to ensure that a task calling Synchronize has been
        /// enrolled on the barrier!</remarks>
        // <example>
        // - Barrier Example -
        // <code><include BarrierExample/BarrierExample.cs></code>
        // </example>    
        public void Synchronize()
        {
            synchFinish.WaitOne();
            lock (this)
            {
                numSynched++;
                
                if (numSynched == numEnrolled)
                {
                    syncEvent.Set();
                    synchFinish.Reset();
                    numLeft = numSynched;                   
                    numSynched = 0;
                }
            }

            syncEvent.WaitOne();

            lock (this)
            {
                numLeft--;
                if (numLeft == 0)
                {
                    syncEvent.Reset();
                    synchFinish.Set();                    
                }
            }            
        }
        
        
        
        /// <summary>Gets the number of enrolled tasks the barrier.</summary>
        /// <remarks>The Enrolled property returns the number of enrolled tasks on the barrier.
        /// Note: The number might have changed once the property returns.</remarks> 
        /// <returns>Number of enrolled tasks.</returns>
        public int Enrolled
        {
            get { return numEnrolled; }           
        }

        /// <summary>
        /// Disposes this Barrier
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (syncEvent != null)
                {                    
                    syncEvent.Close();
                    syncEvent = null;
                }
                if (synchFinish != null)
                {
                    synchFinish.Close();
                    synchFinish = null;
                }
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes this Barrier
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
