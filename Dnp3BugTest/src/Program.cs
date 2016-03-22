using Dnp3BugTest.master;
using Dnp3BugTest.outstation;
using System;
using System.Threading;

namespace Dnp3BugTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting master...");
            Dnp3EventRecorder eventRecorder = new Dnp3EventRecorder();
            var master = new MasterConnector(1, (tag, value) =>
            {
                Console.WriteLine("Getting data:" + tag + " value:" + value);
                if (value != 0 && !eventRecorder.CheckEvent(tag, value))
                {
                    Console.WriteLine("ERROR with tag:" + tag + " value:" + value);
                    System.Environment.Exit(-1);
                }
            });

            master.InitChannel();
            master.Run();

            Thread.Sleep(10000);

            Console.WriteLine("Starting Outstation... ");
            var testConfig = new Dnp3TestOutstationConfig
            {
                seed = 35,
                host = "127.0.0.1",
                port = 20000,
                actionsPerCycle = 100,
                testTime = 1000,
                eventRecorder = eventRecorder,
            };

            Thread thread = new Thread(() => new Dnp3TestOutstation(testConfig).Run() );
            thread.Start();
        }
    }
}
