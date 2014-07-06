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
    //------------------------------------------------------ Infinite FIFO buffer -----------------------------------------------------
    // An infinite FIFO buffer.

    // A FIFO(First In First Out) buffer which is (in principle) unlimited in capacity.
    // It is of course dependent of available memory.
    internal class InfiniteBuffer<T> : Buffer<T>
    {
        private Queue<T> buffer;
        private BufferState state;

        // Constructs a new InfiniteBuffer.
        internal InfiniteBuffer()
        {

            buffer = new Queue<T>(100);
            state = BufferState.Empty;
        }

        // Puts data in the buffer in the back of the queue.
        internal override void Put(T newdata)
        {
            buffer.Enqueue(newdata);
            state = BufferState.NonFullEmpty;
        }

        // Gets data from the buffer. The data is taken from the front of the queue.
        internal override T Get()
        {
            if (buffer.Count - 1 == 0)
                state = BufferState.Empty;

            return buffer.Dequeue();

        }


        // The state of the buffer. Empty, full or neither.
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
