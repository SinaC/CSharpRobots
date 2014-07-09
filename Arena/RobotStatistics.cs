using System.Collections.Generic;

namespace Arena
{
    public class RobotStatistics
    {
        public Dictionary<string, int> Values { get; private set; }

        public RobotStatistics()
        {
            Values = new Dictionary<string, int>();
        }

        public void Set(string key, int value)
        {
            lock(Values)
            {
                Values[key] = value;
            }
        }

        public void Increment(string key)
        {
            lock(Values)
            {
                if (Values.ContainsKey(key))
                    Values[key]++;
                else
                    Values[key] = 1;
            }
        }

        public void Add(string key, int value)
        {
            lock(Values)
            {
                if (Values.ContainsKey(key))
                    Values[key] += value;
                else
                    Values[key] = value;
            }
        }
    }
}
