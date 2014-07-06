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
using System.ComponentModel;

namespace Jibu
{
    internal enum ChoiceType { Input, Output, None }; 

    internal enum choiceState { INACTIVE, INPROGRESS, WAITING };

    internal enum AlternativeType {Channel, Timer, False };   

    /// <summary>
    /// Jibu provides three different buffers, a Fifo, Lifo and Infinite buffer, that
    /// may be used in the Channel.</summary>
    /// <remarks>
    /// The Fifo buffer is a first in first out buffer of a specified size.
    /// The Lifo buffer is a last in first out buffer of a specified size.
    /// The Infinite buffer is a Fifo buffer of infinite size, that is the buffer
    /// can grow until the system runs out of memory.
    /// </remarks>
    public enum JibuBuffer { 
        
        /// <summary>
        /// Specifies a first in first out buffer
        /// </summary>    
        Fifo, 

        /// <summary>
        /// Specifies a last in first out buffer
        /// </summary>
        Lifo,

        /// <summary>
        /// Specifies a first in first out buffer that will
        /// grow until the system runs out of memory.
        /// </summary>
        Infinite 
    };  
}
