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
using ST = System.Threading;
using System.ComponentModel;

namespace Jibu
{

    /// <summary>A timer that can be used as a standalone timer or as part of Choice</summary>    
    /// To use the Timer as a regular timer, use the static SleepFor method, 
    /// which will block for the specified period of time.
    /// In the Choice class you might want to be notified if the Timer times out. In that case you 
    /// should use RelativeTimeOut instead.
    /// This method is non-blocking, but when the Timer times out after the specified period of time it will notify
    /// the Choice instance associated with the Timer.
    /// 
    /// Note: A Timer instance is only meant to be used in a single Choice/task. If more Timers are needed
    /// create more instances.
    /// 
    // <example>
    // - Timer Example -
    // <code><include TimerExample/TimerExample.cs></code>
    // </example>
    public class Timer : Alternative, IDisposable
    {
        private System.Threading.Timer timer;
        private Choice currentChoice;
        private bool pending;
        private object lockObject;
        private bool disposed;


        /// <summary>
        /// Creates a new Timer instance.
        /// </summary>
        /// <remarks>
        /// Creates a new Timer instance.
        /// </remarks>
        public Timer()
            : base()
        {
            timer = new ST.Timer(new ST.TimerCallback(timerCallBack), null, ST.Timeout.Infinite, ST.Timeout.Infinite);
            lockObject = new object();
        }

        private void timerCallBack(object o)
        {
            lock (lockObject)
            {
                pending = true;
                if (currentChoice != null)
                    currentChoice.SignalChoice(this);

            }
        }


        /// <summary>
        /// Puts the current task to sleep for the specified period of time.
        /// </summary>
        /// <remarks>
        /// Puts the current task to sleep for the specified period of time.
        /// </remarks>
        /// <param name="milliSeconds">The number of milliseconds to sleep.</param>
        // <example>
        // - Timer Example -
        // <code><include TimerExample/TimerExample.cs></code>
        // </example>
        public static void SleepFor(int milliSeconds)
        {
            ST.Thread.Sleep(milliSeconds);
        }

        /// <summary>
        /// Times out after the specified period of time.
        /// </summary>
        /// <remarks>
        /// Times out after a given number of milliseconds. Used when the Timer is used as a alternative
        /// in the Choice class. The timer does not block the current task of execution like SleepFor does. 
        /// </remarks>
        /// <param name="milliSeconds">The number of milliseconds until the timeout is reached.</param>
        // <example>
        // - Timer Example -
        // <code><include TimerExample/TimerExample.cs></code>
        // </example>
        public void RelativeTimeOut(int milliSeconds)
        {
            DisableTimer();
            lock (lockObject)
            {
                pending = false;
                timer.Change(milliSeconds, ST.Timeout.Infinite);
            }
        }

        private void DisableTimer()
        {
            timer.Change(ST.Timeout.Infinite, ST.Timeout.Infinite);
        }

        /// <summary>
        /// Disposes this Timer instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Timer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the Timer
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
          
                timer.Dispose();
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.

            }
            disposed = true;
        }

        internal override AlternativeType Enable(Choice choice)
        {
            lock (lockObject)
            {
                if (!pending)
                {
                    currentChoice = choice;
                    return AlternativeType.False;
                }
                return AlternativeType.Timer;
            }
        }

        internal override void Reserve(int threadId)
        {
            throw new JibuException("The method or operation is not implemented.");
        }

        internal override void Mark()
        {
        }
    }
}
