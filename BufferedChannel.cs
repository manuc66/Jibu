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
using System.Collections.Generic;
using System.ComponentModel;

namespace Jibu
{
    internal class BufferedChannel<T> : BaseChannel<T>
    {
        private ChannelReader<T> channelReader;
        private ChannelWriter<T> channelWriter;

        private Buffer<T> buf;
        private object blockedReaderLock, blockedWriterLock;
        private bool writerBlocked;
        private int readersBlocked, writersBlocked;       
        private int size;
        private Dictionary<int, int> reservedIds;

        private bool readerPending;
        private object modifying;
        private object writerLock;
        private object readerLock;
        private bool poisoned;
        private ChoiceType choiceType;
        private Alternative alternativeForPassingToChoice;
        private Queue<Choice> choiceQueue;
               
        
        internal BufferedChannel(JibuBuffer buffertype, int size)
            : base()
        {
            poisoned = false;
            modifying = new object();
            writerLock = new object();
            readerLock = new object();
            readerPending = false;
            choiceType = ChoiceType.None;
            choiceQueue = new Queue<Choice>();
            channelReader = new ChannelReader<T>(this);
            channelWriter = new ChannelWriter<T>(this);

            writerBlocked = false;
            blockedReaderLock = new object();
            blockedWriterLock = new object();
            readersBlocked = 0;
            writersBlocked = 0;
            this.size = size;
            reservedIds = new Dictionary<int, int>();

            switch (buffertype)
            {
                case JibuBuffer.Fifo:
                    buf = new FifoBuffer<T>(size);
                    break;
                case JibuBuffer.Lifo:
                    buf = new LifoBuffer<T>(size);
                    break;
                case JibuBuffer.Infinite:
                    buf = new InfiniteBuffer<T>();
                    this.size = int.MaxValue;
                    break;
            }
        }

        public override void Write(T data)
        {
            Monitor.Enter(writerLock);
            Monitor.Enter(modifying);

            if (choiceType == ChoiceType.Output)
            {
                while (reservedIds.Count > 0 && !(reservedIds.ContainsKey(Thread.CurrentThread.ManagedThreadId)) && (buf.GetDataCount() + reservedIds.Count == size) && !poisoned)
                {
                    writersBlocked++;
                    Monitor.Enter(blockedWriterLock);

                    Monitor.Exit(modifying);
                    Monitor.Exit(writerLock);

                    Monitor.Wait(blockedWriterLock);
                    Monitor.Exit(blockedWriterLock);

                    Monitor.Enter(writerLock);
                    Monitor.Enter(modifying);
                    writersBlocked--;
                }

                if (reservedIds.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    reservedIds.Remove(Thread.CurrentThread.ManagedThreadId);


                if (buf.GetState() == BufferState.Full && !poisoned)
                {
                    writerBlocked = true;
                    Monitor.Wait(modifying);
                    writerBlocked = false;
                }

                if (!poisoned)
                {
                    buf.Put(data);

                    if (readerPending)
                        Monitor.Pulse(modifying);

                    else if(writersBlocked > 0)
                    {
                        Monitor.Enter(blockedWriterLock);
                        Monitor.Pulse(blockedWriterLock);
                        Monitor.Exit(blockedWriterLock);
                    }
                    Monitor.Exit(modifying);
                    Monitor.Exit(writerLock);
                    return;
                }
            }
            else
            {
                if (buf.GetState() == BufferState.Full && !poisoned)
                {
                    writerBlocked = true;
                    Monitor.Wait(modifying);
                    writerBlocked = false;
                }

                if (!poisoned)
                {
                    buf.Put(data);

                    if (readerPending)
                        Monitor.Pulse(modifying);
                    else
                    {
                        if (choiceType == ChoiceType.Input)
                        {
                            if (readersBlocked > 0)
                            {
                                // Pulse one of the reading tasks, blocked in
                                // the reserve while loop, to notify it that
                                // new non reserved data has arrived at the 
                                // channel.
                                Monitor.Enter(blockedReaderLock);
                                Monitor.Pulse(blockedReaderLock);
                                Monitor.Exit(blockedReaderLock);

                                Monitor.Exit(modifying);
                                Monitor.Exit(writerLock);
                                return;
                            }
                            else
                            {
                                int threadId;
                                while (choiceQueue.Count > 0)
                                {
                                    threadId = choiceQueue.Dequeue().SignalChoice(alternativeForPassingToChoice);
                                    if (threadId != -1)
                                    {
                                        reservedIds.Add(threadId, 0);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    Monitor.Exit(modifying);
                    Monitor.Exit(writerLock);
                    return;
                }
            }

            if (writersBlocked > 0)
            {
                Monitor.Enter(blockedWriterLock);
                Monitor.Pulse(blockedWriterLock);
                Monitor.Exit(blockedWriterLock);
            }
            Monitor.Exit(modifying);
            Monitor.Exit(writerLock);

            throw new PoisonException("Channel was poisoned.");
        }

        public override T Read()
        {
            T tempData;

            Monitor.Enter(readerLock);
            Monitor.Enter(modifying);

            if (choiceType == ChoiceType.Input)
            {
                while (reservedIds.Count > 0 && !(reservedIds.ContainsKey(Thread.CurrentThread.ManagedThreadId)) && (buf.GetDataCount() - reservedIds.Count == 0))
                {
                    readersBlocked++;
                    Monitor.Enter(blockedReaderLock);

                    Monitor.Exit(modifying);
                    Monitor.Exit(readerLock);

                    Monitor.Wait(blockedReaderLock);
                    Monitor.Exit(blockedReaderLock);

                    Monitor.Enter(readerLock);
                    Monitor.Enter(modifying);
                    readersBlocked--;

                    if (poisoned && (buf.GetDataCount() - reservedIds.Count == 0))
                    {
                        if (readersBlocked > 0)
                        {
                            Monitor.Enter(blockedReaderLock);
                            Monitor.Pulse(blockedReaderLock);
                            Monitor.Exit(blockedReaderLock);
                        }
                        Monitor.Exit(modifying);
                        Monitor.Exit(readerLock);
                        throw new PoisonException("Channel was poisoned.");
                    }
                }

                if (reservedIds.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    reservedIds.Remove(Thread.CurrentThread.ManagedThreadId);

                if (buf.GetState() != BufferState.Empty)
                {                    
                    tempData = buf.Get();                 

                    if (writerBlocked == true)
                    {
                        Monitor.Pulse(modifying);
                    }
                    if (readersBlocked > 0)
                    {
                        Monitor.Enter(blockedReaderLock);
                        Monitor.Pulse(blockedReaderLock);
                        Monitor.Exit(blockedReaderLock);
                    }
                    Monitor.Exit(modifying);
                    Monitor.Exit(readerLock);
                    return tempData;
                }

                if (!poisoned)
                {
                    readerPending = true;
                    Monitor.Wait(modifying);
                    readerPending = false;

                    if (buf.GetState() != BufferState.Empty) // The if statement is used when the channel is poisened!
                    {
                        tempData = buf.Get();
                        if (readersBlocked > 0)
                        {
                            Monitor.Enter(blockedReaderLock);
                            Monitor.Pulse(blockedReaderLock);
                            Monitor.Exit(blockedReaderLock);
                        }
                        Monitor.Exit(modifying);
                        Monitor.Exit(readerLock);
                        return tempData;
                    }
                }
            }
            else
            {
                if (buf.GetState() != BufferState.Empty)
                {
                    tempData = buf.Get();

                    if (writerBlocked == true)
                    {
                        Monitor.Pulse(modifying);
                    }
                    
                    if (writersBlocked > 0)
                    {
                        Monitor.Enter(blockedWriterLock);
                        Monitor.Pulse(blockedWriterLock);
                        Monitor.Exit(blockedWriterLock);
                    }
                    Monitor.Exit(modifying);
                    Monitor.Exit(readerLock);
                    return tempData;
                }

                if (!poisoned)
                {
                    if (choiceType == ChoiceType.Output)
                    {
                        int threadId;
                        while (choiceQueue.Count > 0 && (buf.GetDataCount() + reservedIds.Count < size))
                        {
                            threadId = choiceQueue.Dequeue().SignalChoice(alternativeForPassingToChoice);
                            if (threadId != -1)
                            {
                                reservedIds.Add(threadId, 0);
                                break;
                            }
                        }
                    }
                    readerPending = true;
                    Monitor.Wait(modifying);
                    readerPending = false;

                    if (buf.GetState() != BufferState.Empty) // The if statement is used when the channel is poisened!
                    {
                        tempData = buf.Get();

                        if (writersBlocked > 0)
                        {
                            Monitor.Enter(blockedWriterLock);
                            Monitor.Pulse(blockedWriterLock);
                            Monitor.Exit(blockedWriterLock);
                        }
                        Monitor.Exit(modifying);
                        Monitor.Exit(readerLock);
                        return tempData;
                    }

                }
            }
            if (readersBlocked > 0)
            {
                Monitor.Enter(blockedReaderLock);
                Monitor.Pulse(blockedReaderLock);
                Monitor.Exit(blockedReaderLock);
            }
            Monitor.Exit(modifying);
            Monitor.Exit(readerLock);
            throw new PoisonException("Channel was poisoned.");
        }

        public override void Poison()
        {
            Monitor.Enter(modifying);
            poisoned = true;

            while (choiceQueue.Count > 0)
                choiceQueue.Dequeue().SignalPoison();

            Monitor.PulseAll(modifying);
            

            // If a Choice task reserves a channel and then call poison
            // instead of write/read, it's necessary to explicite release
            // the non-reserved tasks hanging in writerlock/readerlock.

            if (choiceType == ChoiceType.Input)
            {
                Monitor.Enter(blockedReaderLock);
                Monitor.Pulse(blockedReaderLock);
                Monitor.Exit(blockedReaderLock);
            }
            else
            {
                Monitor.Enter(blockedWriterLock);
                Monitor.Pulse(blockedWriterLock);
                Monitor.Exit(blockedWriterLock);
            }
            Monitor.Exit(modifying);
        }

        public override bool IsPoisoned
        {
            get { return poisoned; }
        }

        public override ChannelReader<T> ChannelReader
        {
            get { return channelReader; }
        }

        public override ChannelWriter<T> ChannelWriter
        {
            get { return channelWriter; }
        }

        internal override AlternativeType Enable(Choice choice)
        {
            Monitor.Enter(modifying);
            if (!poisoned || (choiceType == ChoiceType.Input && !readerPending && (buf.GetDataCount() - reservedIds.Count > 0)))
            {
                if ((choiceType == ChoiceType.Input  && !readerPending && (buf.GetDataCount() - reservedIds.Count > 0)) ||
                    (choiceType == ChoiceType.Output && !writerBlocked && (buf.GetDataCount() + reservedIds.Count < size)))
                    return AlternativeType.Channel;
                if (!choiceQueue.Contains(choice))
                    choiceQueue.Enqueue(choice);

                Monitor.Exit(modifying);
                return AlternativeType.False;
            }
            Monitor.Exit(modifying);
            throw new PoisonException("Channel was poisoned.");
        }

        //Mark the channel as an input alternative.
        //The In method marks the channel as an input alternative, e.i. a Choice object using the channel
        // will know that it should look for data written to the channel.
        // A channel cannot be both an input and an output alternative and it's not possible to change type - 
        // if you call In(), the channel will be an input alternative forever.
        // 
        // Note that the channel is by default neither an input nor an output alternative and you should not
        // mark it as one if it is not part of a Choice. If, however, the channel is used as an input alternative
        // in a Choice object it must be marked as one.
        internal override void In()
        {
            In(channelReader);
        }

        internal void In(Alternative alternative)
        {
            lock (modifying)
            {
                if (choiceType == ChoiceType.Output)
                    throw new JibuException("An ChannelWriter is already using the Channel in another Choice");
                choiceType = ChoiceType.Input;
                alternativeForPassingToChoice = alternative;
            }
        }

        // brief Mark the channel as an output alternative.

        // The Out() method marks the channel as an output alternative, e.i. a Choice object using the channel
        // will know that it should look for readers ready to read data from the channel.
        // A channel cannot be both an input and an output alternative and it's not possible to change type - 
        // if you call Out(), the channel will be an output alternative forever.
        // 
        // Note that the channel is by default neither an input nor an output alternative and you should not
        // mark it as one if it is not part of a Choice. If, however, the channel is used as an output alternative
        // in a Choice object it must be marked as one.
        // \return The channel is returned, but now marked as an output alternative.
        internal override void Out()
        {
            Out(channelWriter);
        }

        internal void Out(Alternative alternative)
        {
            lock (modifying)
            {
                if (choiceType == ChoiceType.Input)
                    throw new JibuException("An ChannelReader is already using the Channel in another Choice");
                choiceType = ChoiceType.Output;
                alternativeForPassingToChoice = alternative;
            }
        }

        internal override void Reserve(int threadId)
        {
            if (threadId != -1)
            {
                reservedIds.Add(threadId, 0);
            }
            Monitor.Exit(modifying);
        }


        internal BufferState BufferStatus
        {
            get { return buf.GetState(); }
        }
       
    }

}
