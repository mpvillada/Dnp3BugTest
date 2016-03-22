using System;
using System.Collections.Generic;
using System.Linq;

namespace Dnp3BugTest
{
    public class RandomUtil
    {
        public static T GetRandomEnum<T>(Random random)
        {
            Array values = Enum.GetValues(typeof(T));
            var result = (T)values.GetValue(random.Next(values.Length));

            return result;
        }

        public static bool GetRandomBool(Random random)
        {
            return random.NextDouble() > 0.5 ? true : false;
        }

        public static T GetRandomFromList<T>(Random random, IEnumerable<T> list)
        {
            int index = random.Next() % list.Count();
            return list.ElementAt(index);
        }
    }

}
