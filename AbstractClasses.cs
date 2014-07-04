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
    /* ---------------------- Abstract Alternative class ------------------------*/
   
    
    /// <summary>
    /// Abstract class, used by the Choice class
    /// </summary>
    /// <remarks>
    /// The Choice class allows the programmer to choose between
    /// different events occurring in different alternatives and all
    /// alternatives are subclasses of the Alternative class. 
    /// </remarks>
    public abstract class Alternative
    {        
        internal abstract AlternativeType Enable(Choice choice);
        internal abstract void Reserve(int threadId);
        internal abstract void Mark();
    }
    /*-------------------- Abstract Buffer classes ----------------------------*/

    // Enumeration that describes the state of a buffer.
    // FULL: means that the buffer is full.
    // EMPTY: means that the buffer is empty.
    // NONFULLEMPTY: means that the buffer is neither full nor empty
    internal enum BufferState { Full, Empty, NonFullEmpty };

    internal abstract class Buffer<T>
    {
        internal abstract void Put(T data);
        internal abstract T Get();
        internal abstract BufferState GetState();
        internal abstract int GetDataCount();
    }

    internal abstract class BaseChannel<T>
    {
        public abstract ChannelReader<T> ChannelReader { get; }
        public abstract ChannelWriter<T> ChannelWriter { get; }
        public abstract bool IsPoisoned { get; }
        public abstract void Poison();
        public abstract T Read();
        public abstract void Write(T data);
        internal abstract AlternativeType Enable(Choice choice);
        internal abstract void Reserve(int threadId);
        internal abstract void In();
        internal abstract void Out();
    }
}
