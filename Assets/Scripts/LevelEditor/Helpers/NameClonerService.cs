using System.Text.RegularExpressions;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;

namespace TimeLine.LevelEditor.NameCloner
{
    public static class NameClonerService
    {
        public static string Clone(string originalName, IMaxObjectIndexDataReading maxObjectIndexDataReading)
        {
            
            // Регулярное выражение ищет " (Clone " + число + ")" в конце строки
            string pattern = @" \(Clone (\d+)\)$";
            Match match = Regex.Match(originalName, pattern);

            if (match.Success)
            {
                // Заменяем старый суффикс на новый с увеличенным числом
                return Regex.Replace(originalName, pattern, " (Clone " + maxObjectIndexDataReading.GetNextIndex() + ")");
            }
            else
            {
                // Если суффикса нет, просто добавляем (Clone 1)
                return originalName + " (Clone 1)";
            }
        }
    }
}