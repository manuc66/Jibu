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
using System.Diagnostics;



[assembly: CLSCompliant(true)]
namespace Jibu
{
    /// <summary>Static class that manages initialization and global settings of Jibu </summary>
    // <example>
    // - Manager Example -
    // <code><include ManagerExample/ManagerExample.cs></code>
    // </example>
    public static class Manager
    {
        private static int stackSize = 0;


        /// <summary>Returns the number of processors/cores available in the machine.</summary>
        /// <returns>The number of available processors/cores in the machine.</returns>
        // <example>
        // - Manager Example -
        // <code><include ManagerExample/ManagerExample.cs></code>
        // </example>
        public static int ProcessorCount
        {
            get { return Environment.ProcessorCount; }
        }

        /// <summary>Gets the stack size.</summary>
        /// <remarks>
        /// Gets the stack size for all threads created by
        /// the Jibu library.
        /// </remarks>
        // <example>
        // - Manager Example -
        // <code><include ManagerExample/ManagerExample.cs></code>
        // </example>
        public static int StackSize
        {
            get { return stackSize; }
        }


        /// <summary>Initializes the Jibu program.</summary>
        /// <remarks>
        /// Initializes the Jibu program and sets the stack size of all
        /// Jibu tasks to the default stack size, i.e. 1Mb        
        /// </remarks>
        // <example>
        // - Manager Example -
        // <code><include ManagerExample/ManagerExample.cs></code>
        // </example>
        public static void Initialize()
        {
            Initialize(0);
        }

        /// <summary>Initializes the Jibu program.</summary>
        /// <remarks>
        /// Initializes the Jibu program and sets the stack size of all
        /// Jibu tasks to size, i.e. all tasks will
        /// have a stack size of the specified size. The size is in bytes.        
        /// Note: Windows does not allow a stack size below 128KB
        /// </remarks>
        /// <param name="size">Stack size of all Jibu tasks.</param>
        // <example>
        // - Manager Example -
        // <code><include ManagerExample/ManagerExample.cs></code>
        // </example>
        public static void Initialize(int size)
        {
            stackSize = size;
        }

        /// <summary>
        /// Terminates all free threads in Jibu's thread pool.
        /// </summary>
        /// <remarks>
        /// TerminateFreeThreads releases all idle OS threads in Jibu's internal thread pool.
        /// </remarks>
        // <example>
        // - Manager Example -
        // <code><include ManagerExample/ManagerExample.cs></code>
        // </example>
        public static void TerminateFreeThreads()
        {
            JibuThreadPool.TerminateFreeThreads();
        }

        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}
