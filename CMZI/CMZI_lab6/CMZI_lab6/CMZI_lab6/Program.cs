namespace CMZI_lab6
{
    class Program
    {
        static void Main()
        {
            int r = 5; // Количество избыточных символов
            int n = 10; // Длина кодового слова 1101101011
            int k = n - r; // Длина информационного слова

            // Порождающий полином: x^5 + x^4 + x^3 + x^2 + 1 => 1 1 1 1 0 1
            int[] g = { 1, 1, 1, 1, 0, 1 };

            CodingDecoding codingDecoding = new CodingDecoding(k, r, g);

            Console.WriteLine($"Количество избыточных символов кода, r={r}\nПолином");
            Console.WriteLine(codingDecoding.GetGeneratorPolynomial().ToString());
            codingDecoding.PrintGeneratorMatrix();
            codingDecoding.PrintCanonicalGeneratorMatrix();
            codingDecoding.PrintHMatrix();


            // Ввод информационного слова
            Console.Write("\nВведите информационное слово длины " + k + ": ");
            string input = Console.ReadLine();
            int[] infoWord = input.Select(c => c - '0').ToArray();

            // Генерация кодового слова
            int[] codeword = codingDecoding.Encode(infoWord);
            Console.WriteLine("Кодовое слово Xn: " + string.Join("", codeword));

            // Анализ с 0, 1, 2 ошибками
            for (int i = 0; i <= 2; i++)
            {
                codingDecoding.AnalyzeWithErrorCount(codeword, i);
            }
        }
    }
}

