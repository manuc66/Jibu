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

    /// <summary>Reading end of a Channel.</summary>
    /// <remarks>The Channel class has a ChannelReader property that
    /// returns the reading end of the Channel.
    /// The ChannelReader allows the programmer to read from the Channel and poison it.
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
    public class ChannelReader<T> : Alternative, IPoisonable
    {
        private BaseChannel<T> channel;

        internal ChannelReader(BaseChannel<T> channel)
        {
            this.channel = channel;
        }

        /// <summary>Reads data from the underlying Channel.</summary>
        /// <remarks>The Read method reads and returns the data available in the Channel. If no data is available Read will
        /// block until data is available.</remarks>   
        /// <exception cref="PoisonException">The channel has been poisoned</exception>
        /// <returns>The data read from the Channel.</returns>
        // <example>
        // - Unbuffered Channel Example -
        // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
        // </example>
        // <example>
        // - Buffered Channel Example -
        // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
        // </example>
        public T Read()
        {
            return channel.Read();
        }

        /// <summary>Poisons the underlying Channel.</summary>
        /// <remarks>Poisons the underlying Channel. Trying to read to or write from a poisoned channel
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
            channel.In();
        }
    }

}
