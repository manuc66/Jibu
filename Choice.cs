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

namespace Jibu
{    
    /// <summary>Listen for events from different alternatives</summary>    
    /// The Choice class allows you to listen for some predefined events
    /// on alternatives. Jibu provides the following alternatives:
    /// 
    /// 1) ChannelWriter <p/>
    /// 2) ChannelReader <p/>
    /// 3) Timer <p/>     
    /// 
    /// Below is a description of how the alternatives can be used in Choice
    /// 
    /// 1) You can connect a ChannelReader, 
    ///    to a Choice object and be notified when data
    ///    is available in the Channel.
    /// 
    /// 2) You can connect a ChannelWriter, 
    ///    to a Choice object and be notified when another task is ready to read 
    ///    data from the Channel, that is when another task is 
    ///    pending in Read, waiting for data.<p/>
    ///    Note that if the ChannelWriter is a writing end to a buffered Channel you
    ///    are notified when it is possible to write data to the Channel without
    ///    getting blocked, that is when the buffer is not full.
    /// 
    /// 3) You can connect a Timer to a Choice object and be notified when the
    ///    timer times out.<p/>
    /// 
    /// ChannelWriters and ChannelReaders can be connected to several Choice
    /// objects at the same time. That means that, for instance, a ChannelReader may be
    /// connected to three different Choice objects, all listening for available data in the Channel.
    /// The programmer need not worry about having multiple Choice objects using the
    /// same alternative, because Jibu guaranties that only one Choice object
    /// will notify the programmer if, for instance, data arrives in a Channel. Jibu also guaranties
    /// that no task, Choice object, etc. can "steal" the data in the Channel once
    /// the Choice object has notified the programmer.<p/>
    /// That means that you can use a Choice object to listen for data to arrive at
    /// a Channel and when you are notified that data has arrived, you are guaranteed
    /// to receive the data when you call Read on the Channel. You don't have to worry
    /// about whether other tasks, Choice objects and so on are using the same Channel.<p/>
    /// The same rules apply for the ChannelWriter alternative - if the programmer is notified, that
    /// another task has called Read and is ready to read data when Write is called, Jibu
    /// guaranties that no other writing task "steals" the reading task.
    /// 
    /// A Timer can only be connected to one Choice object.
    /// 
    /// It isn't possible to connect both the ChannelReader and ChannelWriter of the same Channel
    /// to a Choice object, even if they are connected to different 
    /// Choice objects. The reason it isn't allowed is that deadlock could occur if, for instance, one
    /// Choice object was waiting for data to arrive in a Channel, while another Choice object was 
    /// waiting for a reading task to call Read.<p/>
    /// Jibu will throw a JibuException if this rule is violated.
    /// 
    /// The last and probably the most important thing to know about the Choice class
    /// is how to listen for events on alternatives. The Choice class provides
    /// the following methods:
    /// 
    /// PriSelect:
    /// Returns an index of a ready alternative. If multiple alternatives are ready, the one with the highest priority is selected. 
    /// PriSelect is blocked until one of the alternatives are ready.<p/>
    /// 
    /// FairSelect: 
    /// Like PriSelect, but guaranties a fair selection of ready alternatives. FairSelect guaranties that a ready alternative
    /// will be selected within n calls to FairSelect, where n is the number of alternatives in the Choice object. FairSelect is blocked until one
    /// of the alternatives are ready.<p/>
    /// 
    /// TryPriSelect:
    /// If you just want to check if some alternatives are ready, but don't want to block if that is not the case, you can use 
    /// TryPriSelect. Like PriSelect, TryPriSelect returns the ready alternative with the highest priority, if there exists a ready
    /// alternative, otherwise -1 is returned.<p/>
    /// 
    /// TryFairSelect:
    /// Like FairSelect, but without blocking if no alternatives are ready. In that case -1 is returned.<p/>
    /// 
    /// Note: A Choice object cannot be shared between different tasks. One Choice object works in one task only. If more are needed, create more instances.
    public class Choice
    {     
        int fairIndex, selected;
        Alternative[] alts;     
        Dictionary<Alternative, int> indices;
        int choiceThreadId;        
        object lockObject;
        private choiceState choiceStatus;
        private int traverseIndex;
        private bool poisoned;
              
        /// <summary>Creates a Choice object with the submitted alternatives.</summary>
        public Choice(params Alternative[] alternatives)
        {
            alts = alternatives;
            indices = new Dictionary<Alternative, int>();
            for (int i = 0; i < alts.Length; i++)
            {
                alts[i].Mark();    
                indices.Add(alts[i], i);
            }
            lockObject = new object();        
            choiceStatus = choiceState.INACTIVE;
        }

        internal void SignalPoison()
        {
            Monitor.Enter(lockObject);
            poisoned = true;

            if (choiceStatus == choiceState.WAITING)
                Monitor.Pulse(lockObject);
            choiceStatus = choiceState.INACTIVE;
            Monitor.Exit(lockObject);
        }

        internal int SignalChoice(Alternative alt)
        {
            
            lock (lockObject)
            {                
                if (choiceStatus == choiceState.INACTIVE)
                    return -1;
                
                int index = indices[alt];
                
                if (choiceStatus == choiceState.WAITING)
                {
                    
                    choiceStatus = choiceState.INACTIVE;                    
                    Monitor.Pulse(lockObject);                    
                    selected = index;                    
                    return choiceThreadId;                 
                }
                
                if (traverseIndex > fairIndex)
                {                    
                    if (index > traverseIndex || index < fairIndex)
                        return -1;
                }
                else
                {                    
                    if (index > traverseIndex && index < fairIndex)
                        return -1;
                }                
                
                choiceStatus = choiceState.INACTIVE;
                selected = index;
                
                return choiceThreadId;
            }
            
        }

        /// <summary>Non-blocked fair selection of alternatives</summary>        
        /// <remarks>Fair selection of alternatives. If no alternatives are ready, -1 is returned.
        /// TryFairSelect guarantee that a ready alternative will be chosen within n calls to TryFairSelect,
        /// where n is the number of alternatives. </remarks>
        /// <returns>An index of a ready alternative or -1 if no alternatives are ready. </returns>
        /// <exception cref="Jibu.PoisonException">Thrown if a Channel that takes part in the Choice has been poisoned.</exception>
        // <example>
        // - TryFairSelectExample - 
        // <code><include TryFairSelectExample/TryFairSelectExample.cs></code>
        // </example>        
        public int TryFairSelect()
        {
            int tempSelect;
            Monitor.Enter(lockObject);

            choiceThreadId = Thread.CurrentThread.ManagedThreadId;
            selected = -1;
            choiceStatus = choiceState.INPROGRESS;
            traverseIndex = fairIndex;

            Monitor.Exit(lockObject);

            FairTraverse(alts.Length);

            Monitor.Enter(lockObject);

            tempSelect = selected;
            choiceStatus = choiceState.INACTIVE;
            if (selected != -1)
            {
                fairIndex = selected + 1;
                if (fairIndex == alts.Length)
                    fairIndex = 0;
            }
            else
            {
                if (poisoned)
                {
                    Monitor.Exit(lockObject);
                    throw new PoisonException("A Channel in this Choice was poisoned.");
                }
            }
            
            Monitor.Exit(lockObject);
            return tempSelect;
        }

        /// <summary>Fair selection of alternatives</summary>
        /// <remarks>FairSelect guaranties that a ready alternative
        /// will be selected within n calls to FairSelect, where n is the number of submitted alternatives. FairSelect is blocked until one
        /// of the alternatives are ready.</remarks>
        /// <returns>An index of a ready alternative.</returns>
        /// <exception cref="Jibu.PoisonException">Thrown if a Channel that takes part in the Choice has been poisoned.</exception>
        // - FairSelectExample - 
        // <code><include FairSelectExample/FairSelectExample.cs></code>
        // </example>
        public int FairSelect()
        {
            int tempSelect;            
            Monitor.Enter(lockObject);
            
            choiceThreadId = Thread.CurrentThread.ManagedThreadId;            
            selected = -1;            
            choiceStatus = choiceState.INPROGRESS;            
            traverseIndex = fairIndex;
            
            Monitor.Exit(lockObject);
            
            FairTraverse(alts.Length);
            
            Monitor.Enter(lockObject);
            
            if (selected == -1)
            {                
                choiceStatus = choiceState.WAITING;
                if (!poisoned)                
                    Monitor.Wait(lockObject);                
                
                if (poisoned)
                {
                    choiceStatus = choiceState.INACTIVE;
                    Monitor.Exit(lockObject);
                    throw new PoisonException("A Channel in this Choice was poisoned.");
                }
            }            
            tempSelect = selected;            
            fairIndex = selected+1;            
            if (fairIndex == alts.Length)
                fairIndex = 0;
            
            Monitor.Exit(lockObject);            
            return tempSelect;
        }

        /// <summary>Non-blocked prioritized selection of alternatives.</summary>
        /// <remarks>Returns an index of a ready alternative. If multiple alternatives are ready, the one with the highest priority is selected. 
        /// If no alternatives are ready, -1 is returned.</remarks>
        /// <returns>An index of a ready alternative with the highest priority or -1 if no alternative is ready.</returns>        
        /// <exception cref="Jibu.PoisonException">Thrown if a Channel that takes part in the Choice has been poisoned.</exception>
        // - TryPriSelectExample - 
        // <code><include TryPriSelectExample/TryPriSelectExample.cs></code>
        // </example>        
        public int TryPriSelect()
        {
            int tempSelect;
            Monitor.Enter(lockObject);
            choiceThreadId = Thread.CurrentThread.ManagedThreadId;
            selected = -1;
            choiceStatus = choiceState.INPROGRESS;
            traverseIndex = 0;
            fairIndex = 0;
            Monitor.Exit(lockObject);

            PriTraverse();

            Monitor.Enter(lockObject);
            tempSelect = selected;
            choiceStatus = choiceState.INACTIVE; 
            if(tempSelect == -1 && poisoned)
            {
                Monitor.Exit(lockObject);
                throw new PoisonException("A Channel in this Choice was poisoned.");
            }
            Monitor.Exit(lockObject);
            return tempSelect;
        }

        /// <summary>Prioritized selection of alternatives.</summary>        
        /// <remarks>Returns an index of a ready alternative. If multiple alternatives are ready, the one with the highest priority is selected. 
        /// PriSelect is blocked until one of the alternatives are ready.</remarks>
        /// <returns>An index of a ready alternative with the highest priority.</returns> 
        /// <exception cref="Jibu.PoisonException">Thrown if a Channel that takes part in the Choice has been poisoned.</exception>
        // - PriSelectExample - 
        // <code><include PriSelectExample/PriSelectExample.cs></code>
        // </example>        
        public int PriSelect()
        {
            int tempSelect;
            Monitor.Enter(lockObject);
            choiceThreadId = Thread.CurrentThread.ManagedThreadId;
            selected = -1;
            choiceStatus = choiceState.INPROGRESS;
            traverseIndex = 0;
            fairIndex = 0;
            Monitor.Exit(lockObject);

            PriTraverse();

            Monitor.Enter(lockObject);
            if (selected == -1)
            {
                choiceStatus = choiceState.WAITING;
                if (!poisoned)
                    Monitor.Wait(lockObject);
                 
                if (poisoned)
                {
                    choiceStatus = choiceState.INACTIVE;
                    Monitor.Exit(lockObject);
                    throw new PoisonException("A Channel in this Choice was poisoned.");
                }
            }
            tempSelect = selected;
            Monitor.Exit(lockObject);
            return tempSelect;
        }

        private void PriTraverse()
        {
            try
            {
                while (traverseIndex < alts.Length)
                {
                    AlternativeType type = alts[traverseIndex].Enable(this);
                    if (type == AlternativeType.False)
                    {
                        lock (lockObject)
                        {
                            if (choiceStatus == choiceState.INACTIVE)
                                return;

                            traverseIndex++;
                            continue;

                        }
                    }
                    if (type == AlternativeType.Channel)
                    {
                        lock (lockObject)
                        {
                            if (choiceStatus != choiceState.INACTIVE)
                            {
                                selected = traverseIndex;
                                choiceStatus = choiceState.INACTIVE;
                                alts[traverseIndex].Reserve(choiceThreadId);
                                return;
                            }
                            alts[traverseIndex].Reserve(-1);
                            return;
                        }
                    }
                    lock (lockObject)
                    {
                        if (choiceStatus != choiceState.INACTIVE)
                        {
                            selected = traverseIndex;
                            choiceStatus = choiceState.INACTIVE;
                            return;
                        }
                        return;
                    }
                }
            }
            catch (PoisonException)
            {
                SignalPoison();
            }
        }

        private void FairTraverse(int end)
        {
            try
            {
                bool wrapped = false;
                
                while (traverseIndex < end)
                {
                    
                    AlternativeType type = alts[traverseIndex].Enable(this);
                    
                    if (type == AlternativeType.False)
                    {
                        
                        lock (lockObject)
                        {                        
                            if (choiceStatus == choiceState.INACTIVE)
                                return;
                            
                            traverseIndex++;
                            
                            if (traverseIndex == end && !wrapped)
                            {                            
                                traverseIndex = 0;                            
                                end = fairIndex;                            
                                wrapped = true;
                            }                        
                            continue;
                        }
                    }
                    
                    if (type == AlternativeType.Channel)
                    {
                        
                        lock (lockObject)
                        {
                            
                            if (choiceStatus != choiceState.INACTIVE)
                            {                            
                                selected = traverseIndex;                            
                                choiceStatus = choiceState.INACTIVE;                            
                                alts[traverseIndex].Reserve(choiceThreadId);                            
                                return;
                            }
                            
                            alts[traverseIndex].Reserve(-1);                        
                            return;
                        }
                    }
                    lock (lockObject)
                    {
                        
                        if (choiceStatus != choiceState.INACTIVE)
                        {
                            
                            selected = traverseIndex;                        
                            choiceStatus = choiceState.INACTIVE;                        
                            return;
                        }
                        return;

                    }
                }
            }
            catch (PoisonException)
            {
                SignalPoison();
            }
        }      
    }
}
