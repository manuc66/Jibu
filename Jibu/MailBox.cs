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
    internal class MailBoxElement
    {
        public object data;
        public Address address;

        public MailBoxElement(object data, Address mailBox)
        {
            this.data = data;
            this.address = mailBox;
        }
    }

    internal class MailBox
    {
        private List<MailBoxElement> mail;
        private object waitObject;
        private bool cancelled;
        private Type waitType;
        private Address waitAddress;

        internal MailBox()
        {
            waitObject = new object();
            mail = new List<MailBoxElement>();
        }

        internal void Cancel()
        {            
            lock (waitObject)
            {
                cancelled = true;
                Monitor.Pulse(waitObject);
            }
        }

        internal void Put(object data, Address address)
        {
            lock (waitObject)
            {
                mail.Add(new MailBoxElement(data, address));

                if (waitType != null && waitType.IsInstanceOfType(data))
                {
                    if (waitAddress == null || address == waitAddress)
                        Monitor.Pulse(waitObject);
                }
            }
        }

        internal T GetFrom<T>(Address address)
        {
            lock (waitObject)
            {
                T message;

                Type t = typeof(T);

                waitAddress = address;
                waitType = t;

                int i;

                for (i = 0; i < mail.Count; i++)
                {
                    MailBoxElement curMail = mail[i];
                    if (address == curMail.address && curMail.data is T)
                    {
                        message = (T)curMail.data;
                        mail.RemoveAt(i);
                        waitAddress = null;
                        waitType = null;
                        return message;
                    }
                }

                Monitor.Wait(waitObject);

                if (cancelled)
                    throw new CancelException("Cancelled while receiving from mail box.");

                for (; i < mail.Count; i++)
                {
                    MailBoxElement curMail = mail[i];
                    if (address == curMail.address && curMail.data is T)
                    {
                        message = (T)curMail.data;
                        mail.RemoveAt(i);
                        waitAddress = null;
                        waitType = null;
                        return message;
                    }
                }

                // we're here if we've been pulsed and no data was available, should not be possible
                throw new JibuException("Mail box data not present.");
            }
        }

        internal T Get<T>()
        {
            Address dontCare;
            return Get<T>(out dontCare);
        }

        internal T Get<T>(out Address address)
        {
            lock (waitObject)
            {
                T message;
                Type t = typeof(T);

                waitType = t;
                int i;

                for (i = 0; i < mail.Count; i++)
                {
                    MailBoxElement curMail = mail[i];
                    if (curMail.data is T)
                    {
                        message = (T)curMail.data;
                        address = curMail.address;
                        mail.RemoveAt(i);
                        waitType = null;
                        return message;
                    }
                }

                Monitor.Wait(waitObject);

                if (cancelled)
                    throw new CancelException("Cancelled while receiving from mail box.");

                for (; i < mail.Count; i++)
                {
                    MailBoxElement curMail = mail[i];
                    if (curMail.data is T)
                    {
                        message = (T)curMail.data;
                        address = curMail.address;
                        mail.RemoveAt(i);
                        waitType = null;
                        return message;
                    }
                }


                // we're here if we've been pulsed and no data was available, should not be possible
                throw new JibuException("Mail box data not present.");
            }
        }
    }

    /// <summary>
    /// Address represents the mail box address of a Task. 
    /// </summary>
    /// <remarks>
    /// Every task has its own mailbox-system which is used to communicate
    /// with other tasks. Data received in the mailbox is retrieved by calling the
    /// Jibu.Task.Receive and Jibu.Task.ReceiveFrom methods. Sending messages to another
    /// mailbox is done with the Jibu.Task.Send method.
    /// </remarks>
    // <example>
    // - Mail Box Example -
    // <code><include MailBoxExample/MailBoxExample.cs></code>
    // </example>
    public class Address
    {
        MailBox privateMailBox;

        // Constructs a public mailbox with the corresponding private MailBox
        internal Address()
        {
            privateMailBox = new MailBox();
        }


        internal MailBox MailBox
        {
            get { return privateMailBox; }
        }
    }
}
