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

    /// <summary>
    /// A Channel for communicating between tasks.
    /// </summary>
    /// <remarks>
    /// The Channel class provides communication between tasks. Data can be written to the
    /// Channel by calling the Write method and data can be read from the Channel by 
    /// calling Read. Channel is a generic class, thus any kind of data can be sent through a
    /// Channel. <p/>
    /// ## Explain buffered / unbuffered
    /// 
    /// The Channel class can operate in two different modes: buffered and unbuffered. These two modes cause different behaviors
    /// for the Read and Write methods.
    /// * The unbuffered channel is a rendezvous channel, meaning that the Write method writes data to the channel
    ///   and then waits until another task has read the data from the channel before it returns. Naturally the 
    ///   Read call will block until data is written to the channel. That means that the channel
    ///   synchronizes the two tasks writing to and reading from the channel.
    /// * When you create a buffered instance of the Channel class, you specify which type of buffer to use and what size the buffer
    ///   should be. Jibu provides three different buffers for the Channel class, a FIFO and a LIFO buffer of fixed size and an infinite FIFO buffer
    ///   that will hold as many elements as your memory allows.
    ///   A buffered channel isn't a rendezvous channel, meaning that Write will only block
    ///   if the buffer is full, otherwise it will write data to the buffer and return. Read will block if the buffer is empty,
    ///   otherwise it will read data from the buffer and return it.
    /// <p/>
    /// ## multiple readers / writers
    /// A channel may be used by multiple writers and multiple readers, but at most two tasks, one reader and one writer, 
    /// have actual access to the channel at any given at time. That means that
    /// several writers might call Write at the same time, but only one of them will be allowed to write data to the channel.
    /// The remaining writers will be blocked until they are allowed access to
    /// the channel. At the same time several readers might call Read, but only one of them will be allowed to enter the channel and 
    /// get the data.<p/>
    /// 
    /// ## Poison
    /// It's sometimes useful to shut down a Channel when it isn't needed anymore. The Channel class 
    /// provides the Poison method for that purpose. Both readers and writers can poison a Channel by calling Poison. If a 
    /// Channel is poisoned it will throw a PoisonException when Write is called. If Read is called it will throw a
    /// PoisonException, unless the channel is buffered and the buffer is not empty. In this case the remaining elements
    /// are allowed to be read. That means that you can read data that was written to the buffered channel before it was poisoned.
    /// Readers or writers waiting on the channel when it is poisoned, will of course also get the exception. The channel 
    /// will also throw a PoisonException if the Choice class "asks" the channel if it is ready for input or output. Hence you can 
    /// catch the exception and shut down your tasks if you want to - that way you can shut down entire networks of tasks. Once a channel
    /// has been poisoned it can not be un-poisoned.<p/>
    ///
    /// ## ChannelReader / ChannelWriter
    /// The ChannelWriter property returns a writing end for the Channel, in the shape of a ChannelWriter
    /// object and the ChannelReader property returns a reading end, i.e. a ChannelReader object.
    /// When creating a Choice in which a Channel takes part, you have to specify which end of the Channel the 
    /// Choice is listening on. This is done by supplying either the ChannelReader or ChannelWriter for the Channel.
    /// ChannelReader and ChannelWriter can also be poisoned
    /// using the Poison method. This has the same effect as poisoning the underlying Channel<p/>
    /// 
    /// ## Notes
    /// Note: Channel adheres to the standard pass by reference and pass by value
    /// rules, meaning that primitive data is copied, while references are used for objects. That means you should
    /// avoid modifying data in a task after you have sent the data through a Channel to another task. If you 
    /// chose to do so anyway, use standard locking mechanisms when modifying and accessing the data.
    /// 
    /// Note: If you write to a Channel with multiple readers, you do not know which reader will actually get
    /// the data or if anyone will ever read it at all, but you know that one and only one of the readers will get it, if anyone gets it.
    /// If you read from a channel with multiple writers, you do not know which of the writers actually wrote the data to the channel.
    /// </remarks>
    /// <typeparam name="T">The type to be sent through the channel.</typeparam>
    // <example>
    // - Unbuffered Channel Example -
    // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
    // </example>
    // <example>
    // - Buffered Channel Example -
    // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
    // </example>
    public class Channel<T> : IPoisonable
    {

        private BaseChannel<T> internalChannel;

        /// <summary>
        /// Creates an unbuffered rendezvous Channel.
        /// </summary>
        /// <returns>An unbuffered Channel instance</returns>
        public Channel()
        {
            internalChannel = new UnbufferedChannel<T>();
        }

        /// <summary>
        /// Creates a buffered Channel with the specified buffer. 
        /// </summary>
        /// <remarks>
        /// If the specified JibuBuffer is different from Infinite, then a default size of 10 is used.
        /// </remarks>
        /// <param name="bufferType">The type of buffer.</param>
        /// <returns>A buffered Channel instance.</returns>
        public Channel(JibuBuffer bufferType)
        {
            internalChannel = new BufferedChannel<T>(bufferType, 10);
        }

        /// <summary>
        /// Creates a buffered Channel with the specified buffer and size.
        /// </summary>        
        /// <remarks>
        /// If the specified JibuBuffer is Infinite, then the size argument will be ignored.
        /// </remarks>
        /// <param name="bufferType">The type of buffer.</param>
        /// <param name="size">The size of the buffer. The argument is ignored if the buffer type is InfiniteBuffer</param>
        /// <returns>A buffered Channel instance.</returns>
        public Channel(JibuBuffer bufferType, int size)
        {
            internalChannel = new BufferedChannel<T>(bufferType, size);
        }

        /// <summary>
        /// Gets the Jibu.ChannelReader for this Channel
        /// </summary>
        public ChannelReader<T> ChannelReader
        {
            get { return internalChannel.ChannelReader; }
        }

        /// <summary>
        /// Gets the Jibu.ChannelWriter for this Channel
        /// </summary>
        public ChannelWriter<T> ChannelWriter
        {
            get { return internalChannel.ChannelWriter; }
        }

        /// <summary>
        /// Poisons this Channel
        /// </summary>
        /// <remarks>
        /// Poisons this Channel instance. Trying to read or write from a poisoned Channel
        /// will result in the Channel throwing a PoisonException. Also if a Choice
        /// instance uses the Channel in one of it's four Select methods, a PoisonException
        /// is thrown from the Channel. 
        /// </remarks>
        // <example>
        // - Unbuffered Channel Example -
        // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
        // </example>
        // <example>
        // - Buffered Channel Example -
        // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
        // </example>
        public void Poison() { internalChannel.Poison(); }

        /// <summary>
        /// Determines if this Channel has been poisoned.
        /// </summary>
        public bool IsPoisoned
        {
            get { return internalChannel.IsPoisoned; }
        }

        /// <summary>Writes data to the Channel</summary>
        /// <remarks>
        /// If this Channel has an underlying buffer, and this buffer is full, then Write blocks
        /// until a reader has read from the Channel. Otherwise Write puts the data into the buffer and returns.<p/>
        /// If this is an unbuffered Channel, then Write waits until data is read by a reader.
        /// </remarks>        
        /// <param name="data">Data to be written to the Channel</param>    
        /// <exception cref="Jibu.PoisonException">Thrown if the channel has been poisoned.</exception>
        // <example>
        // - Unbuffered Channel Example -
        // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
        // </example>
        // <example>
        // - Buffered Channel Example -
        // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
        // </example>
        public void Write(T data) { internalChannel.Write(data); }
        
        /// <summary>
        /// Reads data from the Channel.</summary>
        /// <remarks>
        /// If this Channel has an underlying buffer, and this buffer is empty, then Read blocks
        /// until a writer has written to the Channel. Otherwise Read removes an element from the buffer and returns.<p/>
        /// If this is an unbuffered Channel, then Read waits until data is written by a writer.
        /// </remarks>
        /// <returns>The data read from the Channel.</returns>
        /// <exception cref="Jibu.PoisonException">Thrown if the channel has been poisoned.</exception>
        // <example>
        // - Unbuffered Channel Example -
        // <code><include UnbufferedChannelExample/UnbufferedChannelExample.cs></code>
        // </example>
        // <example>
        // - Buffered Channel Example -
        // <code><include BufferedChannelExample/BufferedChannelExample.cs></code>
        // </example>
        public T Read() { return internalChannel.Read(); }

    }

}
