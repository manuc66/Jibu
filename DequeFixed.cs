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
    class DequeFixed<T> where T : class
    {
        private volatile T[] deque;
        private Int64 age;
        private UInt32 bottom;
        private Int32 swipedElements;

        public DequeFixed(int dequesize)
        {
            deque = new T[dequesize];
            age = 0;
            bottom = 0;
            swipedElements = 0;
        }

        //This is only thread safe when called from the thread that owns the DequeFixed.
        public bool Empty
        {
            get
            {
                return Count==0;
            }
        }

        public int Count
        {
            get
            {
                UInt32 bot = Thread.VolatileRead(ref bottom);
                UInt32 top = TopFromAge(Thread.VolatileRead(ref age));
                UInt32 swiped = (UInt32)Thread.VolatileRead(ref swipedElements);
                return (int) (bot - top - swiped);
            }
        }

        public int Capacity
        {
            get { return deque.Length; }
        }

        //This is only thread safe when called from the thread that owns the DequeFixed.
        //Returns false if the element could not be pushed (overflow)
        public bool Push(T w)
        {
            UInt32 bot = Thread.VolatileRead(ref bottom);
            if (bot == deque.Length)
                return false;
            deque[bot] = w;
            ++bot;
            Thread.VolatileWrite(ref bottom, bot);
            return true;
        }

        //This is only thread safe when called from the thread that owns the DequeFixed.
        public T Pop()
        {
            while (true)
            {
                UInt32 bot = Thread.VolatileRead(ref bottom);
                if (bot == 0)
                    return null;

                bot--;
                Thread.VolatileWrite(ref bottom, bot);
                T w = deque[bot];

                Int64 oldage = Thread.VolatileRead(ref age);
                UInt32 top = TopFromAge(oldage);
                if (bot > top)
                {
                    if (w != null)
                        return w;
                    Interlocked.Decrement(ref swipedElements);
                    continue;
                }

                Thread.VolatileWrite(ref bottom, 0);
                Int64 newage = AgeFromTagAndTop(TagFromAge(oldage) + 1, 0);
                if (bot == top)
                {
                    if (Interlocked.CompareExchange(ref age, newage, oldage) == oldage)
                    {
                        if (w!=null)
                            return w;
                        Interlocked.Decrement(ref swipedElements);
                        continue;
                    }
                }

                Thread.VolatileWrite(ref age, newage);
                return null;
            }
        }

        //This is only thread safe when called from the thread that owns the Deque.
        //Returns true if t is swiped from the Deque
        public bool Swipe(T t)
        {
            //First we test if the element to swipe is at the bottom
            //and if so, we can simply pop it
            UInt32 bot = Thread.VolatileRead(ref bottom);
            if (bot>0 && deque[bot-1]==t)
            {
                T popped=Pop();
                //Assert popped==null || popped==t
                return popped!=null;
            } else
            {
                UInt32 top = TopFromAge(Thread.VolatileRead(ref age));
                for (uint i=top; i<bot; ++i)
                {
                    if (t==deque[i])
                    {
                        while (true)
                        {
                            Int64 oldage=Thread.VolatileRead(ref age);
                            UInt32 oldtop=TopFromAge(oldage);
                            UInt32 oldtag=TagFromAge(oldage);
                            if (oldtop>i)
                                return false;   //Element has been stolen

                            //Move the top down to bottom (ie make the queue seem empty)
                            Int64 newage=AgeFromTagAndTop(oldtag, bot);                            
                            if (Interlocked.CompareExchange(ref age, newage, oldage)!=oldage)
                                continue;
                            //Queue is now "empty", null out the element, and restore the
                            //queue
                            deque[i]=null;
                            newage=AgeFromTagAndTop(oldtag+1, oldtop);
                            //Blind write - no one will have stolen from the empty queue.
                            Thread.VolatileWrite(ref age, newage);
                            Interlocked.Increment(ref swipedElements);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //May be called from any thread
        public T Steal()
        {
            while (true)
            {
                Int64 oldage = Thread.VolatileRead(ref age);
                UInt32 bot = Thread.VolatileRead(ref bottom);
                UInt32 top = TopFromAge(oldage);
                UInt32 tag = TagFromAge(oldage);

                if (bot <= top)
                    return null;

                T w = deque[top];
                Int64 newage = AgeFromTagAndTop(tag, top + 1);
                if (Interlocked.CompareExchange(ref age, newage, oldage) == oldage)
                {
                    if (w != null)
                        return w;
                    Interlocked.Decrement(ref swipedElements);
                }
            }
        }

        //Age is a signed int 64 (since we can't CAS an unsigned int64).
        //The unsigned representation of age is equal to (tag << 32 + top).
        //We convert to uint64 when extracting tag/value to avoid sign extension
        static UInt32 TopFromAge(Int64 age)
        {
            return (UInt32) (((UInt64) age) & 0xFFFFFFFF);
        }

        static UInt32 TagFromAge(Int64 age)
        {
            return (UInt32) (((UInt64) age) >> 32);
        }

        static Int64 AgeFromTagAndTop(UInt32 tag, UInt32 top)
        {
            UInt64 t=tag;
            t=(t << 32) + top;
            return (Int64)t;
        }
    }
}
