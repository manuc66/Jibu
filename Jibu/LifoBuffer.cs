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

namespace Jibu
{
    // Predefined LIFO(Last In First Out) buffer for use in the channels.
    internal class LifoBuffer<T> : Buffer<T>
    {
        private Stack<T> buffer;
        private int bufferSize;
        private BufferState state;

        // Constructor taking the size of the buffer as parameter.
        internal LifoBuffer(int size)
        {
            bufferSize = size;
            buffer = new Stack<T>(size);
            state = BufferState.Empty;
        }

        // Puts data in the buffer. 
        internal override void Put(T data)
        {            
            buffer.Push(data);
            if (buffer.Count == bufferSize)
                state = BufferState.Full;
            else
                state = BufferState.NonFullEmpty;         
        }

        // Gets data from the buffer.
        internal override T Get()
        {            
            if (buffer.Count - 1 == 0)
                state = BufferState.Empty;
            else
                state = BufferState.NonFullEmpty;

            return buffer.Pop();            
        }

        // Return the state of the buffer, in order to determine if it is full, empty or neither.
        internal override BufferState GetState()
        {           
            return state;            
        }

        // Returns the number of data elements currently in the buffer.
        internal override int GetDataCount()
        {            
            return buffer.Count;         
        }
    }
}
