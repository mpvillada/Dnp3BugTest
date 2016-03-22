using Automatak.DNP3.Interface;
using System;
using System.Collections.Generic;

namespace Dnp3BugTest.master
{
    public class Dnp3DataAccesor<T>
    {
        public Func<T, float> Quality;
        public Func<T, DateTime> timestamp;
        public Func<T, double> Value;
    }

    public class Dnp3DataHandler : ISOEHandler
    {
        private MasterConnector master;
        public Dnp3DataHandler(MasterConnector master)
        {
            this.master = master;
        }

        public void Start()
        {
        }

        public void End()
        {
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Analog>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<Analog>
                {
                    Quality = (x) => CalculateQualityAnalog(x.Quality),
                    timestamp = (x) => x.Timestamp,
                    Value = (x) => ConvertOrMapValue(x.Value),
                });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<FrozenCounter>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<FrozenCounter>
                {
                    Quality = (x) => CalculateQualityCounter(x.Quality),
                    timestamp = (x) => x.Timestamp,
                    Value = (x) => ConvertOrMapValue(x.Value),
                });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<AnalogOutputStatus>> values)
        {
            //ProcessGeneric(info, values,
            //    new Dnp3Data<AnalogOutputStatus>
            //    {
            //        Quality = (x,s) => CalculateQualityAnalog(x.Quality),
            //        timestamp = (x,s) => x.Timestamp,
            //        Value = (x, s) => ConvertOrMapValue(x.Value, s),
            //    });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<TimeAndInterval>> values)
        {
            // this callback may be used for synchronization between outstation->master
            //ProcessGeneric(info, values,
            //    new Dnp3Data<TimeAndInterval>
            //    {
            //        Quality = (x,s) => 0f,
            //        timestamp = (x,s) => DateTime.UtcNow,
            //        Value = (x, s) => ConvertOrMapValue(x.time, s),
            //    });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<AnalogCommandEvent>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<AnalogCommandEvent>
                {
                    Quality = (x) => 0f,
                    timestamp = (x) => x.Timestamp,
                    Value = (x) => ConvertOrMapValue(x.Value),
                });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<SecurityStat>> values)
        {
            //ProcessGeneric(info, values,
            //    new Dnp3Data<SecurityStat>
            //    {
            //        Quality = (x,s) => CalculateQualityAnalog(x.Quality),
            //        timestamp = (x,s) => x.Timestamp,
            //        Value = (x, s) => ConvertOrMapValue(x.Value, s),
            //    });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<BinaryCommandEvent>> values)
        {
            // this callback may be used for synchronization between outstation->master
            //ProcessGeneric(info, values,
            //    new Dnp3Data<BinaryCommandEvent>
            //    {
            //        Quality = (x,s) => 0f,
            //        timestamp = (x,s) => x.Timestamp,
            //        Value = (x, s) => ConvertOrMapValue(x.Value, s),
            //    });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<OctetString>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<OctetString>
                {
                    Quality = (x) => 0f,
                    timestamp = (x) => DateTime.UtcNow,
                    Value = (x) => ConvertOrMapValue(x.Bytes),
                });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<BinaryOutputStatus>> values)
        {
            //ProcessGeneric(info, values,
            //    new Dnp3Data<BinaryOutputStatus>
            //    {
            //        Quality = (x,s) => CalculateQualityBase(x.Quality),
            //        timestamp = (x,s) => x.Timestamp,
            //        Value = (x, s) => ConvertOrMapValue(x.Value, s),
            //    });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Counter>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<Counter>
                {
                    Quality = (x) => CalculateQualityCounter(x.Quality),
                    timestamp = (x) => x.Timestamp,
                    Value = (x) => ConvertOrMapValue(x.Value),
                });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<DoubleBitBinary>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<DoubleBitBinary>
                {
                    Quality = (x) => CalculateQualityBinary(x.Quality),
                    timestamp = (x) => x.Timestamp,
                    Value = (x) => ConvertOrMapValue(x.Value),
                });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Binary>> values)
        {
            ProcessGeneric(info, values,
                new Dnp3DataAccesor<Binary>
                {
                    Quality = (x) => CalculateQualityBinary(x.Quality),
                    timestamp = (x) => x.Timestamp,
                    Value = (x) => ConvertOrMapValue(x.Value),
                });
        }

        public int CalculateQualityBase(byte flag)
        {
            int result = 0;
            return result;
        }

        public float CalculateQualityAnalog(byte flag)
        {
            int result = CalculateQualityBase(flag);
            return (float)result;
        }

        public float CalculateQualityBinary(byte flag)
        {
            int result = CalculateQualityBase(flag);
            return (float)result;
        }
        
        public float CalculateQualityCounter(byte flag)
        {
            int result = CalculateQualityBase(flag);
            return (float)result;
        }

        public void ProcessGeneric<TDNP3Type>(
            HeaderInfo info, 
            IEnumerable<IndexedValue<TDNP3Type>> values, 
            Dnp3DataAccesor<TDNP3Type> accessor)
        {
            foreach (var v in values)
            {
                try
                {
                    var tag = typeof(TDNP3Type).Name + "_" + v.Index;
                    DateTime ts = accessor.timestamp(v.Value);
                    if (info.tsmode == TimestampMode.INVALID)
                    {
                        ts = DateTime.UtcNow;
                    }

                    var value = accessor.Value(v.Value);
                    var quality = accessor.Quality(v.Value);

                    master.Dispatch(tag, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR processing message from <{0},{1}>: {2}", typeof(TDNP3Type).Name, v.Index, e.Message);
                    System.Environment.Exit(-1);
                }
            }
        }

        public double ConvertOrMapValue<T>(T value)
        {
            double result = Convert.ToDouble(value);
            return result;
        }

    }

}
