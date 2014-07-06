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
using System.Runtime.Serialization;

namespace Jibu
{
    /// <summary>
    /// The base class of all Jibu Exceptions
    /// </summary>
    /// <remarks>
    /// The base class of all Jibu Exceptions
    /// JibuException is thrown by classes in the Jibu library.
    /// Examine the Message property for information about the exception.   
    /// </remarks>
    [Serializable]
    public class JibuException : Exception
    {
        /// <summary>
        /// Creates a new JibuException
        /// </summary>
        /// <param name="message">An informational message.</param>
        internal JibuException(string message) 
            : base(message)
        {            
        }

        /// <summary>
        /// Creates a new JibuException
        /// </summary>
        internal JibuException() 
            : base()
        { 
        }

        /// <summary>
        /// Creates a new JibuException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        internal JibuException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Creates a new JibuException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        internal JibuException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// PoisonException is thrown from operations on a poisoned Channel.
    /// </summary>
    /// <remarks>
    /// The PoisonException is thrown if a task tries to read from or write to a poisoned Channel.
    /// Furthermore the exception can be thrown from Jibu.Choice.PriSelect, Jibu.Choice.FairSelect, Jibu.Choice.TryPriSelect and Jibu.Choice.TryFairSelect
    /// methods if a poisoned Channel is part of a Choice.
    /// </remarks>
    // <example>
    // - Unbuffered Channel Example -
    // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
    // </example>
    // <example>
    // - Buffered Channel Example -
    // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
    // </example>
    [Serializable]
    public class PoisonException : JibuException
    {
        // Creates a new Jibu Exception
        internal PoisonException()
            : base()
        {
        }

        // Creates a new PoisonException
        // \param message The message to display when the exception is thrown
        internal PoisonException(string message)
            : base(message)
        {
        }

        // Creates a new PoisonException with a message and an exception
        internal PoisonException(string message, Exception ex)
            : base(message, ex)
        {
        }

        // Creates a new PoisonException
        internal PoisonException(SerializationInfo info, StreamingContext s)
            : base(info, s)
        {
        }
    }

    /// <summary>
    /// CancelException is thrown from operations on a cancelled Task.
    /// </summary>
    /// <remarks>
    /// CancelException is thrown from operations on a cancelled Task.
    /// Creating a new Task inside a cancelled Task will also throw
    /// a CancelException.<p/>
    /// Any uncaught exception that occurs in a task is caught by the Task
    /// and calling Jibu.Async.WaitFor/Jibu.Future.Result results in 
    /// a CancelException being thrown. The InnerException property of the 
    /// CancelException will contain the uncaught exception.
    /// </remarks>
    // <example>
    // - Cancel Example -
    // <code><include CancelTaskExample/CancelTaskExample.cs></code>
    // </example>
    [Serializable]
    public class CancelException : JibuException
    { 
            // Creates a new Jibu Exception
        internal CancelException()
            : base()
        {
        }

        // Creates a new PoisonException
        // \param message The message to display when the exception is thrown
        internal CancelException(string message)
            : base(message)
        {
        }

        // Creates a new PoisonException with a message and an exception
        internal CancelException(string message, Exception ex)
            : base(message, ex)
        {
        }

        // Creates a new PoisonException
        internal CancelException(SerializationInfo info, StreamingContext s)
            : base(info, s)
        {
        }
    }

}
