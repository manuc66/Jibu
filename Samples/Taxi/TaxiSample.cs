using System;
using System.Collections.Generic;
using System.Text;
using Jibu;

namespace TaxiSample
{
    // A sample illustrating a taxi dispatch system.
    // We have three separate tasks called Taxi, Dispatch and Customers.
    // In this example we create one Dispatch task, one Customers 
    // task and 5 Taxi tasks. The Customers task generates random customer 
    // data and sends it to the Dispatch task. The Dispatch task sends 
    // waits for avaliable taxis and transmits the customer data to any 
    // vacant taxi. It also received notifications from taxis 
    // having picked up customers. The Dispatch keeps track of the number
    // of vacant taxis.
    // The Taxi tasks receiveds information from the Dispatch and
    // drives to the customer. Having picked up a customer a notification 
    // is sent to the Dispatch. When a taxi has delivered a customer a 
    // message is written to screen.
    // Also the number of vacant taxis are written to screen.
    class TaxiSample
    {
        static void Main(string[] args)
        {
            // We set the default stack size for Jibu tasks to 256,000 bytes.
            // Had this not been set the default size would be 1 MB
            Manager.Initialize(256);
            int numOfTaxis = 5;

            // The channel between the Customers and Dispatch tasks.
            // It is buffered meaning that up to 5 customers can queue up
            // while all taxis are busy.
            Channel<Tuple<int,int,string>> customerChannel =
                new Channel<Tuple<int, int, string>>(JibuBuffer.Fifo, 5);

            // The Dispatch and customer tasks are run in parallel.
            Parallel.Run(new Customers(customerChannel.ChannelWriter), new Dispatch(customerChannel.ChannelReader, numOfTaxis));            
            
        }
    }
}
