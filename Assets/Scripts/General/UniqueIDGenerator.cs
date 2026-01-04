using System;
using System.Threading;

namespace TimeLine
{
    /// <summary>
    /// Утилита для генерации максимально надежных уникальных идентификаторов.
    /// </summary>
    public class UniqueIDGenerator
    {
        private static long lastTimestamp = DateTime.UtcNow.Ticks;
        private static int counter = 0;
        private const int MAX_COUNTER = 10000; // Лимит генераций в одну и ту же единицу времени

        public static string GenerateUniqueID()
        {
            long timestampPart;
            int countPart;

            // Блокировка (lock) нужна, чтобы ID не дублировались при работе в несколько потоков
            lock (typeof(UniqueIDGenerator))
            {
                long now = DateTime.UtcNow.Ticks;
            
                if (now == lastTimestamp)
                {
                    if (++counter >= MAX_COUNTER)
                    {
                        // Если лимит превышен, ждем 1 мс, чтобы время обновилось
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

            // Добавляем немного рандома для финальной уникальности
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 4);
            string randomPart = UnityEngine.Random.Range(0, 1000000).ToString("D6");
        
            // Сборка итоговой строки в 16-ричном и десятичном форматах
            return $"{timestampPart:X}{countPart:D4}{guidPart}{randomPart}";
        }
    }
}