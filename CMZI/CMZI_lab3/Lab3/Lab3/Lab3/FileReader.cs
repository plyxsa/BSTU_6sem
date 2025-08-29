namespace Lab3
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