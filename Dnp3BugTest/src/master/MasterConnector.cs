using Automatak.DNP3.Interface;
using System;

namespace Dnp3BugTest.master
{
    public class MasterConnector : DNP3Connector
    {
        public IMaster Master { get { return master; } }

        protected Dnp3DataHandler dataHandler;
        protected IMaster master;

        private Action<string, double> dispatcher;

        public MasterConnector(ushort concurrency, Action<string,double> dispatcher)
            : base(concurrency)
        {
            this.dispatcher = dispatcher;
            dataHandler = new Dnp3DataHandler(this);
        }

        protected virtual IMaster GetMaster(MasterStackConfig config)
        {
            return channel.AddMaster("master", dataHandler, DefaultMasterApplication.Instance, config);
        }

        public override void Run()
        {
            var masterStack = new MasterStackConfig();
            masterStack.link.localAddr = 1;
            masterStack.link.remoteAddr = 10;

            master = GetMaster(masterStack);

            var integrityPoll = master.AddClassScan(ClassField.AllEventClasses, TimeSpan.FromMilliseconds(500), TaskConfig.Default);

            master.Enable();
        }

        public void Dispatch(string tag, double value)
        {
            dispatcher(tag, value);
        }
    }
}
