using System;
using System.Collections.Generic;
using System.Text;
using Jibu;
using System.Threading;

namespace TaxiSample
{
    // The dispatch task received information from the customers
    // task about the location and name of customers wanting a taxi.
    // When a customer is waiting and a taxi is vacant the customer
    // information is read and passed on to the  taxi task.
    // When a taxi has dropped of a customer the dispatch is notified.
    class Dispatch : Async
    {
        // The customer channel. When customers wants a taxi 
        // they send their location and name on this channel
        ChannelReader<Tuple<int, int, string>> customerChan;

        // When a customer has been picked up the taxi sends 
        // and acknowledgement back to the dispatch.
        Channel<string> ackChannel;

        // The taxi channel used to send information about 
        //the customer to a vacant taxi.
        Channel<Tuple<int, int, string>> taxiChan;

        // The number of taxis attached to this taxi dispatch.
        int numTaxis;
        // The number of vacant taxis attached to this dispatch
        int vacantTaxis;


        public Dispatch(ChannelReader<Tuple<int, int, string>> customer, int taxis)
        {
            customerChan = customer;
            ackChannel = new Channel<string>();
            taxiChan = new Channel<Tuple<int, int, string>>();
            numTaxis = vacantTaxis = taxis;            
        }


        public override void Run()
        {            
            // Here the desired number of taxi tasks are created and started.
            // Note that by using AsyncParallel the program can proceed
            // even though the taxi tasks are running. Had we used Parallel
            // the Run method would have blocked until Run returned.

            for (int i = 0; i < numTaxis; i++)
                new Taxi(taxiChan.ChannelReader, ackChannel.ChannelWriter).Start();

            // We create a choice that monitors two alternatived:
            // 1. A reader has read from the taxiChan - in this case meaning 
            // at least one vacant taxi
            // 2. A writer has written to the achChannel - meaning that
            // a taxi has picked up a customer.
            Choice choice = new Choice(taxiChan.ChannelWriter, ackChannel.ChannelReader);

            
            while (true)
            {
                // We make a fair selection over the two alternatives meaning 
                // that we cycle through the alternatives if they are ready. 
                switch(choice.FairSelect())
                {
                    case 0:
                        Tuple<int, int , string> t = customerChan.Read();
                        taxiChan.Write(t);
                        vacantTaxis--;
                        Console.WriteLine("Vacant taxis: " + vacantTaxis);
                        break;
                    case 1:
                        ackChannel.Read();
                        vacantTaxis++;
                        break;
                }
            }
        }
    }
}
