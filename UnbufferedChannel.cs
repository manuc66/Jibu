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
using System.Threading;
using System.ComponentModel;


namespace Jibu
{    
    internal class UnbufferedChannel<T> : BaseChannel<T>
    {
        internal T tempdata;
        internal bool dataPending, readerPending;
        internal bool poisoned;
        internal object modifying;
        internal object writerLock;
        internal object readerLock;
        internal ChoiceType choiceType;
        private Alternative alternativeForPassingToChoice;
        internal Queue<Choice> choiceQueue;
        internal bool reserved;
        internal int reservedId;
        private ChannelReader<T> channelReader;
        private ChannelWriter<T> channelWriter;

        /// Creates a Channel. 
        internal UnbufferedChannel()
        {
            poisoned = false;
            modifying = new object();
            writerLock = new object();
            readerLock = new object();
            dataPending = readerPending = false;
            choiceType = ChoiceType.None;
            choiceQueue = new Queue<Choice>();
            reserved = false;
            channelReader = new ChannelReader<T>(this);
            channelWriter = new ChannelWriter<T>(this);
        }

        public override void Write(T data)
        {
            Monitor.Enter(writerLock);
            Monitor.Enter(modifying);

            while (reserved && !(reservedId == Thread.CurrentThread.ManagedThreadId) && !poisoned)
            {               
                Monitor.Exit(modifying);
                Monitor.Wait(writerLock);
                Monitor.Enter(modifying);
            }
         
            reserved = false;
            if (!poisoned)
            {
                tempdata = data;
                dataPending = true;
                if (readerPending)
                {
                    Monitor.Pulse(modifying);
                }
                else
                {
                    if (choiceType == ChoiceType.Input)
                    {
                        int threadId;
                        while (choiceQueue.Count > 0)
                        {                       
                            threadId = choiceQueue.Dequeue().SignalChoice(alternativeForPassingToChoice);
                            if (threadId != -1)
                            {
                                reserved = true;
                                reservedId = threadId;
                                break;
                            }
                        }
                    }
                }
                Monitor.Wait(modifying);
                if (!poisoned)
                {
                    Monitor.Exit(modifying);
                    Monitor.Pulse(writerLock);
                    Monitor.Exit(writerLock);
                    return;
                }
            }
            Monitor.Exit(modifying);
            Monitor.Pulse(writerLock);
            Monitor.Exit(writerLock);
            throw new PoisonException("Channel was poisoned.");
        }

        public override T Read()
        {
            T tempData;

            Monitor.Enter(readerLock);
            Monitor.Enter(modifying);

            while (reserved && !(reservedId == Thread.CurrentThread.ManagedThreadId) && !poisoned)
            {
                Monitor.Exit(modifying);
                Monitor.Wait(readerLock);
                Monitor.Enter(modifying);
            }

            reserved = false;

            if (!poisoned)
            {
                if (dataPending)
                {
                    tempData = tempdata;
                    dataPending = false;
                    Monitor.Pulse(modifying);
                    Monitor.Pulse(readerLock);
                    Monitor.Exit(modifying);
                    Monitor.Exit(readerLock);
                    return tempData;
                }
                else
                {
                    if (choiceType == ChoiceType.Output)
                    {
                        int threadId;
                        while (choiceQueue.Count > 0)
                        {                          
                            threadId = choiceQueue.Dequeue().SignalChoice(alternativeForPassingToChoice);
                            if (threadId != -1)
                            {
                                reserved = true;
                                reservedId = threadId;                                
                                break;
                            }
                        }
                    }
                }

                readerPending = true;
                Monitor.Wait(modifying);
                tempData = tempdata;
                dataPending = false;
                readerPending = false;
                Monitor.Pulse(modifying);
                if (!poisoned)
                {
                    Monitor.Exit(modifying);
                    Monitor.Pulse(readerLock);
                    Monitor.Exit(readerLock);
                    return tempData;
                }
            }

            Monitor.Exit(modifying);
            Monitor.Pulse(readerLock);
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
            Monitor.Exit(modifying);

            // If a Choice task reserves a channel and then call poison
            // instead of write/read, it's necessary to explicite release
            // the non-reserved tasks hanging in writerlock/readerlock.

            if (choiceType == ChoiceType.Input)
            {          
                Monitor.Enter(readerLock);
                Monitor.Pulse(readerLock);
                Monitor.Exit(readerLock);
            }
            else
            {           
                Monitor.Enter(writerLock);
                Monitor.Pulse(writerLock);
                Monitor.Exit(writerLock);
            }
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
            if (!poisoned)
            {
                if (choiceQueue.Count == 0 && !reserved)
                {
                    if ((choiceType == ChoiceType.Input  &&  dataPending && !readerPending) ||
                        (choiceType == ChoiceType.Output && !dataPending &&  readerPending))
                        return AlternativeType.Channel;
                }

                if (!choiceQueue.Contains(choice))
                    choiceQueue.Enqueue(choice);
               
                
                Monitor.Exit(modifying);
                return AlternativeType.False;
            }
            Monitor.Exit(modifying);
            throw new PoisonException("Channel was poisoned.");
        }

        //Mark the channel as an input alternative.
        
        ///The In method marks the channel as an input alternative, e.i. a Choice object using the channel
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
        // <returns>The channel is returned, but now marked as an output alternative.</returns>
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
                reserved = true;
                reservedId = threadId;
            }
            Monitor.Exit(modifying);
        }
    }    
}
