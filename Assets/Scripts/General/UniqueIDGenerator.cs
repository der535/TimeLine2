using System;
using System.Threading;

namespace TimeLine
{
    public class UniqueIDGenerator
    {
        private static long lastTimestamp = DateTime.UtcNow.Ticks;
        private static int counter = 0;
        private const int MAX_COUNTER = 10000;

        public static string GenerateUniqueID()
        {
            long timestampPart;
            int countPart;

            lock (typeof(UniqueIDGenerator))
            {
                long now = DateTime.UtcNow.Ticks;
            
                if (now == lastTimestamp)
                {
                    if (++counter >= MAX_COUNTER)
                    {
                        while (now == lastTimestamp)
                        {
                            Thread.Sleep(1);
                            now = DateTime.UtcNow.Ticks;
                        }
                        counter = 0;
                    }
                }
                else
                {
                    counter = 0;
                }
            
                lastTimestamp = now;
                timestampPart = now;
                countPart = counter;
            }

            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 4);
            string randomPart = UnityEngine.Random.Range(0, 1000000).ToString("D6");
        
            return $"{timestampPart:X}{countPart:D4}{guidPart}{randomPart}";
        }
    }
}