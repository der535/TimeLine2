using System;
using System.Threading;

namespace TimeLine.LevelEditor.GeneralServices
{
    /// <summary>
    /// Утилита для генерации максимально надежных уникальных идентификаторов.
    /// </summary>
    public class UniqueIDGenerator
    {
        private static long lastTimestamp = DateTime.UtcNow.Ticks;
        private static int counter = 0;
        private const int MAX_COUNTER = 10000;

        // Создаем системный генератор случайных чисел
        private static readonly System.Random _sysRandom = new System.Random();

        public static string GenerateUniqueID()
        {
            long timestampPart;
            int countPart;
            int randomInt;

            // Блокировка (lock) защищает и счетчик, и System.Random (который не потокобезопасен)
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
                
                // Генерируем число внутри lock для безопасности
                randomInt = _sysRandom.Next(0, 1000000);
            }

            // Используем Guid и наше случайное число
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 4);
            string randomPart = randomInt.ToString("D6");
        
            return $"{timestampPart:X}{countPart:D4}{guidPart}{randomPart}";
        }
    }
}