using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMZI_lab6
{
    public class CodingDecoding
    {
        private int k;
        private int r;
        private int n;
        private int[,] G;
        private int[,] H;
        private Polynomial generatorPolynomial;


        public CodingDecoding(int k, int r, int[] generatorCoefficients)
        {
            this.k = k;
            this.r = r;
            this.n = k + r;
            this.generatorPolynomial = new Polynomial(generatorCoefficients);

            //Создание порождающей матрицы
            G = GenerateGeneratorMatrix(k, r, generatorCoefficients);

            //Создание проверочной матрицы
            H = GenerateHMatrix(G, k, n);
        }


        // Генерация порождающей матрицы
        private int[,] GenerateGeneratorMatrix(int k, int r, int[] generatorCoefficients)
        {
            int n = k + r;
            int[,] G = new int[k, n];
            Polynomial generatorPolynomial = new Polynomial(generatorCoefficients);

            // Строим G в систематической форме [I_k | P]
            for (int i = 0; i < k; i++)
            {
                // Часть I_k (первые k столбцов)
                G[i, i] = 1;

                // Часть P (последние r столбцов) - остаток от деления x^(n-1-i) на g(x)

                // Создаем полином x^(n-1-i)
                int[] xPowCoeffs = new int[n]; // Длина n для полинома степени n-1
                if ((n - 1 - i) >= 0) // Проверка на случай, если n-1-i отрицательно (не должно быть при i < k <= n)
                {
                    xPowCoeffs[n - 1 - i] = 1;
                }
                Polynomial xPow = new Polynomial(xPowCoeffs);

                // Вычисляем остаток
                Polynomial remainder = xPow.Mod(generatorPolynomial);

                // Копируем коэффициенты остатка в матрицу P (последние r столбцов G)
                // Коэффициенты остатка (remainder.Coefficients) идут от x^0, x^1, ... до x^Degree.
                // Они должны попасть в столбцы k, k+1, ..., n-1, которые соответствуют степеням x^0, x^1, ..., x^(r-1) остатка.
                // Важно: Остаток всегда имеет степень < r.

                for (int j = 0; j < r; j++) // j от 0 до r-1 (индекс столбца k+j)
                {
                    if (j < remainder.Coefficients.Length) // У остатка может быть степень меньше r
                    {
                        G[i, k + j] = remainder.Coefficients[j]; // Коэффициент при x^j остатка идет в столбец k+j
                    }
                    else
                    {
                        G[i, k + j] = 0; // Если степень остатка меньше j, коэффициент при x^j равен 0
                    }
                }
            }

            return G;
        }

        // Метод GenerateHMatrix скорее всего правильный, ЕСЛИ ему передается правильная G в форме [I_k | P]
        private int[,] GenerateHMatrix(int[,] G, int k, int n)
        {
            int r = n - k;
            int[,] H = new int[r, n];

            for (int i = 0; i < r; i++) // Строки H (соответствуют I_r в конце H)
            {
                for (int j = 0; j < k; j++) // Первые k столбцов H (соответствуют P^T)
                    H[i, j] = G[j, k + i]; // Берем элемент из P (строка j, столбец i)

                H[i, k + i] = 1; // Ставим 1 на диагонали I_r
            }

            return H;
        }
        // Приведение матрицы к каноническому виду
        private void ToCanonicalForm(int[,] G, int k, int n)
        {
            Console.WriteLine("\nПреобразование в каноническую форму:");
            for (int i = 0; i < k; i++)
            {
                if (G[i, i] == 0)
                {
                    for (int j = i + 1; j < k; j++)
                    {
                        if (G[j, i] == 1)
                        {
                            
                            for (int t = 0; t < n; t++)
                                G[i, t] ^= G[j, t];
                            break;
                        }
                    }
                }

                for (int j = 0; j < k; j++)
                {
                    if (j != i && G[j, i] == 1)
                    {
                        
                        for (int t = 0; t < n; t++)
                            G[j, t] ^= G[i, t];
                    }
                }
            }
        }


        public int[] Encode(int[] infoWord)
        {
            if (infoWord == null || infoWord.Length != k)
            {
                throw new ArgumentException($"Информационное слово должно иметь длину {k}.", nameof(infoWord));
            }

            return MatrixOperations.MultiplyVectorByMatrix(infoWord, G);
        }

        public int[] ComputeSyndrome(int[] received)
        {
            int r = H.GetLength(0);
            int[] syndrome = new int[r];

            for (int i = 0; i < r; i++)
            {
                int sum = 0;
                for (int j = 0; j < H.GetLength(1); j++)
                    sum ^= H[i, j] * received[j];
                syndrome[i] = sum;
            }

            return syndrome;
        }

        public int[] CorrectSingleError(int[] syndrome, int[] received, out int errorPosition)
        {
            int r = H.GetLength(0);
            int n = H.GetLength(1);
            errorPosition = -1;

            for (int j = 0; j < n; j++)
            {
                bool match = true;
                for (int i = 0; i < r; i++)
                {
                    if (H[i, j] != syndrome[i])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    errorPosition = j;
                    break;
                }
            }

            int[] corrected = (int[])received.Clone();
            if (errorPosition != -1)
                corrected[errorPosition] ^= 1;

            return corrected;
        }
        public int[] IntroduceErrors(int[] codeword, int errorCount, out List<int> flippedPositions)
        {
            Random rand = new Random();
            int[] corrupted = (int[])codeword.Clone();
            flippedPositions = new List<int>();

            while (flippedPositions.Count < errorCount)
            {
                int pos = rand.Next(codeword.Length);
                if (!flippedPositions.Contains(pos))
                {
                    corrupted[pos] ^= 1;
                    flippedPositions.Add(pos);
                }
            }

            flippedPositions.Sort();
            return corrupted;
        }

        public void AnalyzeWithErrorCount(int[] codeword, int errorCount)
        {
            Console.WriteLine($"\n==== Анализ с {errorCount} ошибкой(ами) ====");

            List<int> errorPositions;
            int[] received = IntroduceErrors(codeword, errorCount, out errorPositions);

            Console.WriteLine("Принятое слово Yn: " + string.Join("", received));
            if (errorCount > 0)
                Console.WriteLine("Ошибки на позициях: " + string.Join(", ", errorPositions));
            else
                Console.WriteLine("Ошибок нет.");

            int[] syndrome = ComputeSyndrome(received);
            Console.WriteLine("Синдром: " + string.Join("", syndrome));

            if (syndrome.All(bit => bit == 0))
            {
                Console.WriteLine("Ошибок не обнаружено.");
            }
            else
            {
                int errorPos;
                int[] corrected = CorrectSingleError(syndrome, received, out errorPos);

                if (errorPos != -1)
                {
                    Console.WriteLine($"Обнаружена одиночная ошибка в позиции: {errorPos}");
                    Console.WriteLine("Унарный вектор ошибки En: " +
                        string.Join("", Enumerable.Range(0, n).Select(i => i == errorPos ? "1" : "0")));
                    Console.WriteLine("Исправленное слово:        " + string.Join("", corrected));
                }
                else
                {
                    Console.WriteLine("Синдром не соответствует одиночной ошибке — возможно, 2 ошибки.");
                    Console.WriteLine("Исправление невозможно.");
                }
            }
        }


        public void PrintGeneratorMatrix()
        {
            Console.WriteLine("Порождающая матрица G:");
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < n; j++)
                    Console.Write(G[i, j] + " ");
                Console.WriteLine();
            }
        }

        public void PrintCanonicalGeneratorMatrix()
        {
            ToCanonicalForm(G, k, n);
            Console.WriteLine("\nКаноническая порождающая матрица G:");
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < n; j++)
                    Console.Write(G[i, j] + " ");
                Console.WriteLine();
            }
        }

        public void PrintHMatrix()
        {
            Console.WriteLine("\nПроверочная матрица H:");
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < n; j++)
                    Console.Write(H[i, j] + " ");
                Console.WriteLine();
            }
        }

        public Polynomial GetGeneratorPolynomial()
        {
            return generatorPolynomial;
        }
    }


}
