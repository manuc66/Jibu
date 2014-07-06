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
using System.Threading;

namespace Jibu
{
#if DEBUG
    public
#else
    internal
#endif
    class Deque<T> where T : class
    {
        volatile DequeFixed<T> deque;
        public Deque(int initialSize)
        {
            deque = new DequeFixed<T>(initialSize);
        }

        //This is only thread safe when called from the thread that owns the Deque.
        public bool Empty
        {
            get
            {
                return deque.Empty;
            }
        }

        //This is only thread safe when called from the thread that owns the Deque.
        //This is intended for use in the unit tests
        public int Capacity
        {
            get { return deque.Capacity; }
        }

        //This is only thread safe when called from the thread that owns the Deque.
        public void Push(T w)
        {
            if (!deque.Push(w))
            {
                DequeFixed<T> old = deque;
                DequeFixed<T> other = new DequeFixed<T>(CalculateNewCapacity(deque));
                deque = other;
                MoveElements(old, other);
                other.Push(w);
            }
        }

        //This is only thread safe when called from the thread that owns the Deque.
        public T Pop()
        {
            return deque.Pop();
        }

        //This is only thread safe when called from the thread that owns the Deque.
        //Returns true if t is swiped from the Deque
        public bool Swipe(T t)
        {
            return deque.Swipe(t);
        }

        //May be called from any thread
        public T Steal()
        {
            return deque.Steal();
        }


        static private int CalculateNewCapacity(DequeFixed<T> d)
        {
            int newCapacity = d.Capacity;
            if (d.Count > newCapacity / 2)   //More than half full
                newCapacity *= 2;
            return newCapacity;
        }

        static private void MoveElements(DequeFixed<T> old, DequeFixed<T> cur)
        {
            while (true)
            {
                T move = old.Steal();
                if (move == null)
                {
                    break;
                }
                else
                {
                    cur.Push(move);
                }
            }
        }

    }
}
