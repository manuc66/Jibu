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

namespace Jibu
{   

    /// <summary>Writing end for Channels.</summary>
    /// <remarks>Channel has a ChannelWriter property that
    /// returns a writing end of the Channel.
    /// The ChannelWriter allows the programmer to write data to a 
    /// Channel and to poison a Channel.<p/>
    /// When creating a Choice in which a Channel takes part, you have to specify which end of the Channel the 
    /// Choice is listening on. This is done by supplying either the ChannelReader or ChannelWriter for the Channel.
    /// </remarks>
    // <example>
    // - Unbuffered Channel Example -
    // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
    // </example>
    // <example>
    // - Buffered Channel Example -
    // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
    // </example>
    public class ChannelWriter<T> : Alternative, IPoisonable
    {
        private BaseChannel<T> channel;

        internal ChannelWriter(BaseChannel<T> channel)
        {
            this.channel = channel;
        }

        /// <summary>Writes data to the Channel.</summary>        
        /// <remarks>The Write method writes data to the Channel. If the ChannelWriter
        /// is the writing end of a buffered Channel, Write will return once it has written
        /// data to the buffer. If the ChannelWriter is a writing end of an unbuffered Channel, Write returns
        /// when, and only when, the data is read by another task.</remarks>
        /// <exception cref="PoisonException">If the Channel is poisoned</exception>
        /// <param name="data">The data to write to the Channel.</param>
        // <example>
        // - Unbuffered Channel Example -
        // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
        // </example>
        // <example>
        // - Buffered Channel Example -
        // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
        // </example>
        public void Write(T data)
        {
            channel.Write(data);
        }

        /// <summary>Poisons the underlying Channel.</summary>
        /// <remarks>Poisons the underlying Channel. Trying to read or write from a poisoned channel
        /// will result in the channel throwing a PoisonException. Also if a Choice
        /// object uses the channel in one of it's four Select methods, a PoisonException
        /// is thrown from the channel. </remarks>
        // <example>
        // - Unbuffered Channel Example -
        // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
        // </example>
        // <example>
        // - Buffered Channel Example -
        // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
        // </example>
        public void Poison()
        {
            channel.Poison();
        }

        /// <summary>
        /// Determines if the underlying Channel has been poisoned.
        /// </summary>        
        /// <remarks>
        /// Determines if the underlying Channel has been poisoned.
        /// </remarks>
        public bool IsPoisoned
        {
            get { return channel.IsPoisoned; }
        }

        internal override AlternativeType Enable(Choice choice)
        {
            return channel.Enable(choice);
        }

        internal override void Reserve(int threadId)
        {
            channel.Reserve(threadId);
        }

        internal override void Mark()
        {
            channel.Out();
        }
    }

}
