﻿namespace Lab4
{
    class FileReader
    {
        public static string ReadTextFromFile(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
                return string.Empty;
            }

        }
        public static int[] TextToBinaryArray(string text)
        {
            List<int> binaryArray = new List<int>();

            foreach (char c in text)
            {
                int asciiValue = (int)c;
                string binaryValue = Convert.ToString(asciiValue, 2).PadLeft(8, '0');

                foreach (char bit in binaryValue)
                {
                    binaryArray.Add(int.Parse(bit.ToString()));
                }
            }

            return binaryArray.ToArray();
        }
    }
}