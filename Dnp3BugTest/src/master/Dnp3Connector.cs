using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;

namespace Dnp3BugTest.master
{
    public abstract class DNP3Connector
    {
        protected IChannel channel;
        protected IDNP3Manager manager;

        public DNP3Connector(ushort concurrency)
        {
            manager = DNP3ManagerFactory.CreateManager(concurrency);
        }

        public void InitChannel()
        {
            channel = manager.AddTCPServer("server", LogLevels.NORMAL, ChannelRetry.Default, "0.0.0.0", 20000);
        }

        public abstract void Run();
    }

}
