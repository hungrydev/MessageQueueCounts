using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Messaging;

namespace MessageQueueCounts
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new MSMQCountSpeedTest();
            x.Go();
            // wait
            Console.WriteLine("**** Press any key ****");
            Console.ReadKey();




        }
    }
}
