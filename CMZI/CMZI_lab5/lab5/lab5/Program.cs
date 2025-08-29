using System.Text;

namespace lab5
{
    class Program
    {
        static readonly Random GlobalRandom = new Random();

        static int[] GenerateRandomBinaryWord(int length)
        {
            int[] word = new int[length];
            for (int i = 0; i < length; i++)
            {
                word[i] = GlobalRandom.Next(2);
            }
            return word;
        }

        static void RunSingleDemo(IterativeCode coder, int errorCount)
        {
            Console.WriteLine($"\n Запуск единичной демонстрации (Количество ошибок = {errorCount}) ");

            int[] infoWord = GenerateRandomBinaryWord(coder.K);
            PrintUtils.PrintVector(infoWord, "Xk (Информационное слово)");

            int[] xn = coder.Encode(infoWord);
            PrintUtils.PrintVector(xn, "Xn (Исходное кодовое слово)");
            if (coder.Z == null)
                PrintUtils.PrintMatrix(coder.GetMatrix2D(), "Матрица данных (2D)");
            else
                PrintUtils.PrintMatrix(coder.GetMatrix3D(), "Матрица данных (3D)");

            PrintUtils.PrintVector(coder.GetParityGroup(1), "Вычисленная группа четности 1");
            PrintUtils.PrintVector(coder.GetParityGroup(2), "Вычисленная группа четности 2");
            if (coder.Z != null)
            {
                PrintUtils.PrintVector(coder.GetParityGroup(3), "Вычисленная группа четности 3");
                PrintUtils.PrintVector(coder.GetParityGroup(4), "Вычисленная группа четности 4");
            }

            int[] yn = coder.IntroduceErrors(errorCount);
            PrintUtils.PrintVector(yn, "Yn (Принятое слово с ошибками)");

            List<int> errorPositions = new List<int>();
            for (int i = 0; i < xn.Length; i++)
            {
                if (xn[i] != yn[i]) errorPositions.Add(i);
            }
            Console.WriteLine($"   (Ошибки внесены в позиции: {string.Join(", ", errorPositions)})");

            Console.WriteLine("\nНачало процесса декодирования...");
            int[] ynPrime = coder.Decode();
            Console.WriteLine("Процесс декодирования завершен.");
            PrintUtils.PrintVector(ynPrime, "Yn' (Исправленное слово)");

            bool success = coder.AnalyzeCorrection();
            Console.WriteLine($"\nИсправление успешно: {success}");
            if (!success)
            {
                Console.WriteLine("Сравнение:");
                Console.WriteLine($"Xn:  {PrintUtils.FormatBinaryVector(xn)}");
                Console.WriteLine($"Yn': {PrintUtils.FormatBinaryVector(ynPrime)}");

                List<int> remainingErrorPositions = new List<int>();
                for (int i = 0; i < xn.Length; i++)
                {
                    if (xn[i] != ynPrime[i]) remainingErrorPositions.Add(i);
                }
                Console.WriteLine($"   (Остались ошибки в позициях: {string.Join(", ", remainingErrorPositions)})");
            }
            Console.WriteLine("=== Конец единичной демонстрации ===");
        }

        static void RunAnalysis(IterativeCode coder, int errorMultiplicity, int numTrials)
        {
            Console.WriteLine($"\n Запуск анализа (Кратность ошибок = {errorMultiplicity}, Количество испытаний = {numTrials}) ");
            if (numTrials <= 0) return;

            int correctedCount = 0;

            int[] infoWord = GenerateRandomBinaryWord(coder.K);
            int[] xn = coder.Encode(infoWord);

            Console.Write("  Прогресс: ");
            for (int i = 0; i < numTrials; i++)
            {
                coder.IntroduceErrors(errorMultiplicity);
                coder.Decode();
                if (coder.AnalyzeCorrection())
                {
                    correctedCount++;
                }

                if ((i + 1) % (numTrials / 10 == 0 ? numTrials / 10 + 1 : numTrials / 10) == 0)
                {
                    Console.Write($"{(int)(((double)(i + 1) / numTrials) * 100)}% ");
                }
            }
            Console.WriteLine(" Готово.");

            double correctionRate = (double)correctedCount / numTrials;

            Console.WriteLine($"\nРезультаты для {errorMultiplicity} ошибок:");
            Console.WriteLine($"  Всего испытаний: {numTrials}");
            Console.WriteLine($"  Успешно исправлено: {correctedCount}");
            Console.WriteLine($"  Процент успешных исправлений: {correctionRate:P2}");
            Console.WriteLine(" Конец анализа ");
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Симуляция итеративного кода");
            Console.WriteLine("Выберите вариант:");
            Console.WriteLine("1. k=24, k1=4, k2=6");
            Console.WriteLine("2. k=24, k1=3, k2=8");
            Console.WriteLine("3. k=24, k1=3, k2=4, z=2");
            Console.WriteLine("4. k=24, k1=6, k2=2, z=2");

            Console.Write("Введите номер варианта (1, 2, 3 или 4): ");
            string choice = Console.ReadLine();

            int k1 = 0, k2 = 0;
            int? z = null;
            int numParityGroups = 0;

            switch (choice)
            {
                case "1":
                    k1 = 4; k2 = 6; z = null; numParityGroups = 2;
                    Console.WriteLine("Выбран вариант 1: k=24, k1=4, k2=6");
                    break;
                case "2":
                    k1 = 3; k2 = 8; z = null; numParityGroups = 2;
                    Console.WriteLine("Выбран вариант 2: k=24, k1=3, k2=8");
                    break;
                case "3":
                    k1 = 3; k2 = 4; z = 2; numParityGroups = 4;
                    Console.WriteLine("Выбран вариант 3: k=24, k1=3, k2=4, z=2");
                    break;
                case "4":
                    k1 = 6; k2 = 2; z = 2; numParityGroups = 4;
                    Console.WriteLine("Выбран вариант 3: k=24, k1=6, k2=2, z=2");
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Выход.");
                    return;
            }

            IterativeCode coder = new IterativeCode(k1, k2, z, numParityGroups);
            Console.WriteLine($"Параметры кода: K={coder.K}, R={coder.R}, N={coder.N}");
  
            RunSingleDemo(coder, 1);
         
            RunSingleDemo(coder, 2);

            RunSingleDemo(coder, 3);

            Console.WriteLine("\nЗапустить анализ производительности? (y/n)");
            if (Console.ReadLine().Trim().ToLower() == "y")
            {
                Console.Write("Введите количество испытаний для каждой кратности ошибок (например, 1000): ");
                int trials = 1000;
                if (!int.TryParse(Console.ReadLine(), out trials) || trials <= 0)
                {
                    trials = 1000;
                    Console.WriteLine("Неверный ввод, используется 1000 испытаний.");
                }

                RunAnalysis(coder, 1, trials);
                RunAnalysis(coder, 2, trials);
                RunAnalysis(coder, 3, trials);
            }

            Console.WriteLine("\nСимуляция завершена. Нажмите Enter для выхода.");
            Console.ReadLine();
        }
    }
}