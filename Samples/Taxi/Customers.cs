using System;
using System.Collections.Generic;
using System.Text;
using Jibu;

namespace TaxiSample
{
    // The customers task simulates a number of customers waiting for a taxi.
    // The task sleeps for a random time and then writes some random
    // customer information to the channel connecting this task to the 
    // Dispatch task.
    class Customers : Async
    {
        ChannelWriter<Tuple<int, int, string>> infoChannel;
        Random rand;
        int customerCounter;
        Timer timer;

        public Customers(ChannelWriter<Tuple<int, int, string>> info)
        {
            infoChannel = info;
            rand = new Random();
            timer = new Timer();
        }

        public override void Run()
        {
            while (true)
            {
                Timer.SleepFor(rand.Next(1000 + rand.Next(2000)));
                infoChannel.Write(new Tuple<int, int, string>(rand.Next(100), rand.Next(100), "customer " 
                    + customerCounter));
                customerCounter++;
            }
        }
    }
}
