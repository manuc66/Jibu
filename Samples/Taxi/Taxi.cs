using System;
using System.Collections.Generic;
using System.Text;
using Jibu;

namespace TaxiSample
{
    // The taxi task. A taxi reads location and customer 
    // name from the channel connected to the dispatch object.
    // When information is received the taxi drives to the
    // destination and sendsan acknowledgement back to 
    // the dispatch.
    // The stack size of the taxi task is set to 150,000 bytes
    // to save memory compared to standard .NET threads
    class Taxi : Async
    {
        ChannelReader<Tuple<int, int, string>> customerInfoChannel;
        ChannelWriter<string> ackChannel;
        Timer timer;
        Random rand;
       

        // The constructor taking the input info channel and the output acknowledge channel.
        public Taxi(ChannelReader<Tuple<int, int, string>> info, ChannelWriter<string> ack)
        {
            customerInfoChannel = info;
            ackChannel = ack;
            timer = new Timer();           
            rand = new Random();
        }

        public override void Run()
        {
            Tuple<int, int, string> fare;

            while (true)
            {
                fare = customerInfoChannel.Read();
                DriveToDestination();
                Console.WriteLine("Delivered " + fare.Third);
                ackChannel.Write(fare.Third);
            }
        }       

        // Simulates that the taxi drives to another 
        // place in town with a customer.
        // Sleeps for a random period of time.
        private void DriveToDestination()
        {
            Timer.SleepFor(1000 + rand.Next(1000));
        }
      
    }
}
