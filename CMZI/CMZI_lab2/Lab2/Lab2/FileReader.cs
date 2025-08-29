using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    class FileReader
    {
        public static string ReadTextFromFile(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);

                if (fileInfo.Length < 10 * 1024)
                {
                    throw new Exception("Ошибка: файл слишком мал (меньше 10 КБ). Добавьте больше данных");
                }

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
                return string.Empty;
            }
        }
    }
}