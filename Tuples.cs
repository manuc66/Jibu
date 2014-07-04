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

namespace Jibu
{
    //DOM-IGNORE-BEGIN
    /// <summary>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message.
    /// </summary>
    /// <remarks>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message. You can create tuples of 2 to 5 elements.
    ///  </remarks>
    /// <typeparam name="T1">Type of the first element</typeparam>
    /// <typeparam name="T2">Type of the second element</typeparam>
    public class Tuple<T1, T2>
    {
        private T1 first;
        private T2 second;

        /// <summary>
        /// Creates a new tuple
        /// </summary>
        /// <param name="first">The value of the first element</param>
        /// <param name="second">The value of the second element</param>
        public Tuple(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }

        /// <summary>
        /// Gets or sets the first element of the tuple
        /// </summary>
        public T1 First
        {
            get { return first; }
            set { first = value; }
        }

        /// <summary>
        /// Gets or sets the second element of the tuple
        /// </summary>
        public T2 Second
        {
            get { return second; }
            set { second = value; }
        }
    }

    /// <summary>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message.
    /// </summary>
    /// <remarks>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message. You can create tuples of 2 to 5 elements.
    ///  </remarks>
    /// <typeparam name="T1">Type of the first element</typeparam>
    /// <typeparam name="T2">Type of the second element</typeparam>
    /// <typeparam name="T3">Type of the third element</typeparam>
    public class Tuple<T1, T2, T3>
    {
        private T1 first;
        private T2 second;
        private T3 third;

        /// <summary>
        /// Creates a new tuple
        /// </summary>
        /// <param name="first">The value of the first element</param>
        /// <param name="second">The value of the second element</param>
        /// <param name="third">The value of the third element</param>
        public Tuple(T1 first, T2 second, T3 third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

        /// <summary>
        /// Gets or sets the first element of the tuple
        /// </summary>
        public T1 First
        {
            get { return first; }
            set { first = value; }
        }

        /// <summary>
        /// Gets or sets the second element of the tuple
        /// </summary>
        public T2 Second
        {
            get { return second; }
            set { second = value; }
        }

        /// <summary>
        /// Gets or sets the third element of the tuple
        /// </summary>
        public T3 Third
        {
            get { return third; }
            set { third = value; }
        }
    }

    /// <summary>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message.
    /// </summary>
    /// <remarks>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message. You can create tuples of 2 to 5 elements.
    ///  </remarks>
    /// <typeparam name="T1">Type of the first element</typeparam>
    /// <typeparam name="T2">Type of the second element</typeparam>
    /// <typeparam name="T3">Type of the third element</typeparam>
    /// <typeparam name="T4">Type of the fourth element</typeparam>
    public class Tuple<T1, T2, T3, T4>
    {
        private T1 first;
        private T2 second;
        private T3 third;
        private T4 fourth;

        /// <summary>
        /// Creates a new tuple
        /// </summary>
        /// <param name="first">The value of the first element</param>
        /// <param name="second">The value of the second element</param>
        /// <param name="third">The value of the third element</param>
        /// <param name="fourth">The value of the fourth element</param>
        public Tuple(T1 first, T2 second, T3 third, T4 fourth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
        }

        /// <summary>
        /// Gets or sets the first element of the tuple
        /// </summary>
        public T1 First
        {
            get { return first; }
            set { first = value; }
        }

        /// <summary>
        /// Gets or sets the second element of the tuple
        /// </summary>
        public T2 Second
        {
            get { return second; }
            set { second = value; }
        }

        /// <summary>
        /// Gets or sets the third element of the tuple
        /// </summary>
        public T3 Third
        {
            get { return third; }
            set { third = value; }
        }

        /// <summary>
        /// Gets or sets the fourth element of the tuple
        /// </summary>
        public T4 Fourth
        {
            get { return fourth; }
            set { fourth = value; }
        }
    }
    //DOM-IGNORE-END

    /// <summary>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message.
    /// </summary>
    /// <remarks>
    /// The Tuple class is used to send tuples of data through 
    /// a Channel or in a mail box message. You can create tuples of 2 to 5 elements.
    ///  </remarks>
    /// <typeparam name="T1">Type of the first element</typeparam>
    /// <typeparam name="T2">Type of the second element</typeparam>
    /// <typeparam name="T3">Type of the third element</typeparam>
    /// <typeparam name="T4">Type of the fourth element</typeparam>
    /// <typeparam name="T5">Type of the fifth element</typeparam>
    public class Tuple<T1, T2, T3, T4, T5>
    {
        private T1 first;
        private T2 second;
        private T3 third;
        private T4 fourth;
        private T5 fifth;

        /// <summary>
        /// Creates a new tuple
        /// </summary>
        /// <param name="first">The value of the first element</param>
        /// <param name="second">The value of the second element</param>
        /// <param name="third">The value of the third element</param>
        /// <param name="fourth">The value of the fourth element</param>
        /// <param name="fifth">The value of the fifth element</param>
        public Tuple(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
            this.fifth = fifth;
        }

        /// <summary>
        /// Gets or sets the first element of the tuple
        /// </summary>
        public T1 First
        {
            get { return first; }
            set { first = value; }
        }

        /// <summary>
        /// Gets or sets the second element of the tuple
        /// </summary>
        public T2 Second
        {
            get { return second; }
            set { second = value; }
        }

        /// <summary>
        /// Gets or sets the third element of the tuple
        /// </summary>
        public T3 Third
        {
            get { return third; }
            set { third = value; }
        }

        /// <summary>
        /// Gets or sets the fourth element of the tuple
        /// </summary>
        public T4 Fourth
        {
            get { return fourth; }
            set { fourth = value; }
        }

        /// <summary>
        /// Gets or sets the fifth element of the tuple
        /// </summary>
        public T5 Fifth
        {
            get { return fifth; }
            set { fifth = value; }
        }
    }   

}