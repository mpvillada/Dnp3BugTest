using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using System;
using System.Threading;

namespace Dnp3BugTest.outstation
{
    public class Dnp3TestOutstationConfig
    {
        public Dnp3EventRecorder eventRecorder;
        public int seed;
        public string host;
        public ushort port;
        public int actionsPerCycle;
        public int testTime;
    }

    public class Dnp3TestOutstation
    {
        private IChannel channel;
        private IOutstation outstation;
        private Random random;

        private Dnp3TestOutstationConfig testConfig;

        public Dnp3TestOutstation(Dnp3TestOutstationConfig config)
        {
            testConfig = config;
        }

        public void Run()
        {
            random = new Random(testConfig.seed);
            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1);
            channel = mgr.AddTCPClient("client", LogLevels.NONE, ChannelRetry.Default, testConfig.host, testConfig.port);
            channel.AddStateListener(state => { });

            ConfigureOutstation();
            Console.WriteLine("[{0}] Starting Value change...", testConfig.seed);

            while (true)
            {
                for (int i = 0; i < testConfig.actionsPerCycle; ++i)
                {
                    ExecuteSignal();
                }
                Thread.Sleep(testConfig.testTime);
                var eventRecorder = testConfig.eventRecorder;
                Console.WriteLine("");
                Console.WriteLine(
                    "  --- Batch executed: PendingS:" + eventRecorder.GetPending() +
                    " ErrorsS:" + eventRecorder.Errors +
                    " OfS:" + eventRecorder.Total);

            }
        }

        private double[] values = new double[256];
        private void ExecuteSignal()
        {
            var index = (random.Next()) % 255;

            var q = (byte)BinaryQuality.ONLINE;
            var value = (int)(random.NextDouble() * 10000.0);
            ChangeSignal(index,
                () => value,
                (x) => x.Update(new Analog(value, q, DateTime.Now), (ushort)index));
        }

        private void ChangeSignal(int index, Func<IConvertible> getValue, Action<ChangeSet> Adder)
        {
            var changeset = new ChangeSet();
            var value = getValue();

            var dvalue = Convert.ToDouble(value);
            var longTag = typeof(Analog).Name + "_" + index;
            if (dvalue == values[index] || testConfig.eventRecorder.HasValue(longTag, dvalue) )
                return;
            values[index] = dvalue;
            Adder(changeset);
            outstation.Load(changeset);
            Console.WriteLine("----------Changing value:" + longTag + " -> " + index + ";Value:" + dvalue);
            testConfig.eventRecorder.AddEvent(longTag, dvalue);
        }

        private void ConfigureOutstation()
        {
            var outstationConfig = new OutstationStackConfig();
            // configure the various measurements in our database
            //outstationConfig.databaseTemplate = new DatabaseTemplate(4, 1, 1, 1, 1, 1, 1, 0);
            outstationConfig.databaseTemplate = new DatabaseTemplate(
                (ushort)0,
                (ushort)0,
                (ushort)255,
                (ushort)0,
                (ushort)0,
                (ushort)0,
                (ushort)0,
                (ushort)0);

            var template = outstationConfig.databaseTemplate;


            outstationConfig.outstation.config.allowUnsolicited = true;

            outstationConfig.link.localAddr = 10;
            outstationConfig.link.remoteAddr = 1;
            outstationConfig.outstation.buffer.maxAnalogEvents = (uint)random.Next();
            outstationConfig.outstation.buffer.maxAnalogOutputStatusEvents = (uint)random.Next();
            outstationConfig.outstation.buffer.maxBinaryEvents = (uint)random.Next();
            outstationConfig.outstation.buffer.maxBinaryOutputStatusEvents = (uint)random.Next();
            outstationConfig.outstation.buffer.maxCounterEvents = (uint)random.Next();
            outstationConfig.outstation.buffer.maxDoubleBinaryEvents = (uint)random.Next();
            outstationConfig.outstation.buffer.maxFrozenCounterEvents = (uint)random.Next();

            outstation = channel.AddOutstation("outstation", RejectingCommandHandler.Instance, DefaultOutstationApplication.Instance, outstationConfig);
            if (outstation != null)
                outstation.Enable(); // enable communications
        }
    }
}
