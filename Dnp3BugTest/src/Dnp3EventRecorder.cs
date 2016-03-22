using System;
using System.Collections.Generic;
using System.Linq;

namespace Dnp3BugTest
{
    public class Dnp3PendingMessage
    {
        public string Name;
        public double Value;
        public DateTime timestamp = DateTime.UtcNow;
    }

    public class Dnp3EventRecorder
    {
        private object listLock = new object();
        private List<Dnp3PendingMessage> pending = new List<Dnp3PendingMessage>();
        private int errors;
        private int total = 0;
        public int Errors => errors;
        public int Total => total;
        private bool removeDuplicated;

        public Dnp3EventRecorder(bool removeDuplicated = true)
        {
            this.removeDuplicated = removeDuplicated;
        }

        public void AddEvent(string name, IConvertible value)
        {
            lock (listLock)
            {
                pending.Add(new Dnp3PendingMessage { Name = name, Value = Convert.ToDouble(value) });
            }
        }

        public bool CheckEvent(string name, IConvertible value)
        {
            lock (listLock)
            {
                ++total;
                bool found = false;
                for (int i = pending.Count - 1; i >= 0; --i)
                {
                    var v = pending[i];

                    if (removeDuplicated && found && v.Name.Equals(name))
                    {
                        pending.RemoveAt(i);
                    }
                    else if (v.Name.Equals(name) && v.Value == Convert.ToDouble(value))
                    {
                        found = true;
                        pending.RemoveAt(i);
                        if (!removeDuplicated)
                        {
                            break;
                        }
                    }
                }
                if (!found)
                    errors++;
                return found;
            }
        }

        public bool ExistsDeprecatedMessages()
        {
            lock(listLock)
            {
                DateTime now = DateTime.UtcNow;
                foreach (var m in pending)
                {
                    if ((now - m.timestamp).TotalSeconds > 30)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasValue(string name, IConvertible value)
        {
            lock (listLock)
            {
                var found = false;
                foreach(var v in pending)
                {
                    if (v.Name.Equals(name) && v.Value == Convert.ToDouble(value))
                    {
                        found = true;
                    }
                }
                return found;
            }
        }

        public int GetPending()
        {
            return pending.Count;
        }

        public override string ToString()
        {
            var list = pending.ToList().Select((x) => x.Name + " " + x.Value + ";");
            var result = "";
            foreach(var l in list)
            {
                result += l;
            }

            return result;
        }
    }
}
