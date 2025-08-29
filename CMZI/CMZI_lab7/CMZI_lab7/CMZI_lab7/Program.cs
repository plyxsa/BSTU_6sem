using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; // Для вывода

public static class PolynomialUtils
{
    // Деление полиномов (для CRC/синдрома)
    public static int[] Divide(int[] dividend, int[] divisor)
    {
        int[] currentDividend = (int[])dividend.Clone();
        int divisorDegree = divisor.Length - 1;
        // int currentDegree = currentDividend.Length - 1; // Не используется

        if (divisorDegree < 0) throw new ArgumentException("Divisor cannot be empty.");
        if (currentDividend.Length < divisor.Length) // Если делимое короче делителя, остаток - само делимое (если не считать ведущие нули)
        {
            // Для корректного остатка длиной divisorDegree
            int[] shortRemainder = new int[divisorDegree];
            int lenToCopy = Math.Min(currentDividend.Length, divisorDegree);
            int startCopyIdxDividend = Math.Max(0, currentDividend.Length - divisorDegree);
            int startCopyIdxRemainder = Math.Max(0, divisorDegree - currentDividend.Length);
            Array.Copy(currentDividend, startCopyIdxDividend, shortRemainder, startCopyIdxRemainder, lenToCopy);
            return shortRemainder;
        }


        for (int i = 0; i <= currentDividend.Length - divisor.Length; i++)
        {
            if (currentDividend[i] == 1)
            {
                for (int j = 0; j < divisor.Length; j++)
                {
                    currentDividend[i + j] ^= divisor[j];
                }
            }
        }

        // Остаток
        int[] remainder = new int[divisorDegree];
        Array.Copy(currentDividend, currentDividend.Length - divisorDegree, remainder, 0, divisorDegree);
        return remainder;
    }
}

public class CyclicCode
{
    private int k; // Длина информационного слова
    private int n; // Длина кодового слова
    private int[] generatorPolynomial; // G(x)
    private int r; // Длина проверочной части

    // Для (10,7) с G(x) = x^3 + x + 1 = 1011
    public CyclicCode(int k_val = 7, int[] genPoly = null)
    {
        this.k = k_val;
        this.generatorPolynomial = genPoly ?? new int[] { 1, 0, 1, 1 }; // x^3 + x + 1
        this.r = this.generatorPolynomial.Length - 1;
        this.n = this.k + this.r;
    }

    public int K => k;
    public int N => n;

    public int[] Encode(int[] messageBlock)
    {
        if (messageBlock.Length != k)
            throw new ArgumentException($"Длина информационного блока должна быть {k}");

        int[] augmentedMessage = new int[n];
        Array.Copy(messageBlock, augmentedMessage, k);
        // нули для x^r уже там по умолчанию

        int[] remainder = PolynomialUtils.Divide(augmentedMessage, generatorPolynomial);

        int[] codeword = new int[n];
        Array.Copy(messageBlock, codeword, k);
        Array.Copy(remainder, 0, codeword, k, r);
        return codeword;
    }

    // Возвращает true, если блок удалось "исправить" (была 1 ошибка или 0)
    // Возвращает исправленный блок через out параметр
    public bool DecodeAndCorrect(int[] receivedBlock, int[] originalCodewordForComparison, out int[] correctedBlock)
    {
        if (receivedBlock.Length != n)
            throw new ArgumentException($"Длина принятого блока должна быть {n}");

        correctedBlock = (int[])receivedBlock.Clone(); // Начнем с того, что исправленный блок равен принятому
        int[] syndrome = PolynomialUtils.Divide(receivedBlock, generatorPolynomial);

        bool allZeroSyndrome = syndrome.All(bit => bit == 0);

        if (allZeroSyndrome)
        {
            // Синдром 0, ошибок не обнаружено (или они такие, что синдром 0)
            return true;
        }
        else
        {
            // Ошибка обнаружена. "Исправляем" только если это была одна ошибка в кодовом слове.
            int diffCount = 0;
            int errorPos = -1;
            for (int i = 0; i < n; i++)
            {
                if (receivedBlock[i] != originalCodewordForComparison[i])
                {
                    diffCount++;
                    errorPos = i;
                }
            }

            if (diffCount == 1)
            {
                // Это была действительно одна ошибка, "исправляем" ее
                correctedBlock[errorPos] = 1 - correctedBlock[errorPos];
                // Проверяем, стал ли синдром нулевым после нашего "исправления"
                int[] newSyndrome = PolynomialUtils.Divide(correctedBlock, generatorPolynomial);
                return newSyndrome.All(bit => bit == 0); // Успех, если синдром стал 0
            }
            // Если ошибок было больше одной, или синдром не 0, но это не одна ошибка,
            // то мы считаем, что исправить не удалось. correctedBlock останется равным receivedBlock.
            return false;
        }
    }
    public int[] CalculateSyndrome(int[] receivedBlock)
    {
        if (receivedBlock.Length != n)
            throw new ArgumentException($"Длина принятого блока должна быть {n}");
        return PolynomialUtils.Divide(receivedBlock, generatorPolynomial);
    }
}

public class Interleaver
{
    private int rows;
    private int cols;
    private int originalDataLength; // Длина данных до паддинга

    public Interleaver(int dataLength, int numCols)
    {
        if (numCols <= 0) throw new ArgumentOutOfRangeException(nameof(numCols), "Количество столбцов должно быть положительным.");
        this.originalDataLength = dataLength;
        this.cols = numCols;
        this.rows = (int)Math.Ceiling((double)dataLength / numCols);
        if (this.rows == 0 && dataLength > 0) this.rows = 1; // Если данных меньше чем столбцов, нужна 1 строка
        else if (dataLength == 0) this.rows = 0;
    }

    public int PaddedLength => rows * cols;


    public int[] Interleave(int[] data)
    {
        if (data.Length != originalDataLength)
            throw new ArgumentException($"Входная длина данных {data.Length} не соответствует ожидаемой {originalDataLength}.");

        int[,] matrix = new int[rows, cols];
        int k_idx = 0;
        int paddedLength = rows * cols;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (k_idx < data.Length)
                    matrix[i, j] = data[k_idx++];
                else
                    matrix[i, j] = 0;
            }
        }

        int[] interleavedData = new int[paddedLength];
        k_idx = 0;
        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                interleavedData[k_idx++] = matrix[i, j];
            }
        }
        return interleavedData;
    }

    public int[] Deinterleave(int[] interleavedData) 
    {
        if (interleavedData.Length != PaddedLength)
            throw new ArgumentException($"Длина перемеженных данных {interleavedData.Length} не соответствует ожидаемой {PaddedLength}.");

        int[,] matrix = new int[rows, cols];
        int k_idx = 0;

        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                if (k_idx < interleavedData.Length)
                    matrix[i, j] = interleavedData[k_idx++];
                else
                    matrix[i, j] = 0;
            }
        }

        int[] deinterleavedData = new int[originalDataLength];
        k_idx = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (k_idx < originalDataLength)
                    deinterleavedData[k_idx++] = matrix[i, j];
                else
                    break;
            }
            if (k_idx >= originalDataLength) break;
        }
        return deinterleavedData;
    }
}

public class Lab7Simulation
{
    private static Random random = new Random();

    public static int[] GenerateRandomBits(int length)
    {
        int[] bits = new int[length];
        for (int i = 0; i < length; i++)
        {
            bits[i] = random.Next(0, 2);
        }
        return bits;
    }

    public static int[] IntroduceBurstError(int[] data, int burstLength, int position)
    {
        int[] erroredData = (int[])data.Clone();
        for (int i = 0; i < burstLength; i++)
        {
            if (position + i < erroredData.Length)
            {
                erroredData[position + i] = 1 - erroredData[position + i]; // Инвертировать бит
            }
        }
        return erroredData;
    }

    public static string ToBitString(IEnumerable<int> bits)
    {
        if (bits == null) return "null";
        return string.Join("", bits);
    }

    public static void PrintMatrixForInterleaver(int[,] matrix, int rows, int cols, string title)
    {
        Console.WriteLine(title);
        if (rows == 0 || cols == 0)
        {
            Console.WriteLine("[Пустая матрица]");
            return;
        }
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public static int[] SimulateInterleave(int[] data, int originalDataLength, int numCols, bool verbose, out int[,] outMatrix)
    {
        int rows = (int)Math.Ceiling((double)originalDataLength / numCols);
        if (rows == 0 && originalDataLength > 0) rows = 1;
        else if (originalDataLength == 0) rows = 0;

        outMatrix = new int[rows, numCols]; // Инициализируем здесь
        int k_idx = 0;
        int paddedLength = rows * numCols;

        if (verbose) Console.WriteLine($"\n--- Начало перемежения (матрица {rows}x{numCols}) ---");
        if (verbose) Console.WriteLine($"Данные для перемежения (длина {data.Length}): {ToBitString(data)}");

        if (rows > 0 && numCols > 0) // Только если матрица не пустая
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    if (k_idx < data.Length)
                        outMatrix[i, j] = data[k_idx++];
                    else
                        outMatrix[i, j] = 0;
                }
            }
            if (verbose) PrintMatrixForInterleaver(outMatrix, rows, numCols, "Матрица после записи по строкам (с паддингом):");
        }
        else
        {
            if (verbose) Console.WriteLine("Матрица перемежения пуста (0 строк или 0 столбцов).");
        }


        int[] interleavedData = new int[paddedLength];
        k_idx = 0;
        if (rows > 0 && numCols > 0)
        {
            for (int j = 0; j < numCols; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    interleavedData[k_idx++] = outMatrix[i, j];
                }
            }
        }
        if (verbose) Console.WriteLine($"Перемеженные данные (считаны по столбцам, длина {interleavedData.Length}): {ToBitString(interleavedData)}");
        if (verbose) Console.WriteLine("--- Конец перемежения ---");
        return interleavedData;
    }

    public static int[] SimulateDeinterleave(int[] interleavedData, int originalDataLength, int numCols, bool verbose, out int[,] outMatrix)
    {
        int rows = (int)Math.Ceiling((double)originalDataLength / numCols);
        if (rows == 0 && originalDataLength > 0) rows = 1;
        else if (originalDataLength == 0) rows = 0;

        outMatrix = new int[rows, numCols]; // Инициализируем здесь
        int k_idx = 0;

        if (verbose) Console.WriteLine($"\n--- Начало деперемежения (матрица {rows}x{numCols}) ---");
        if (verbose) Console.WriteLine($"Данные для деперемежения (длина {interleavedData.Length}): {ToBitString(interleavedData)}");

        if (rows > 0 && numCols > 0)
        {
            for (int j = 0; j < numCols; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    if (k_idx < interleavedData.Length)
                        outMatrix[i, j] = interleavedData[k_idx++];
                    else
                        outMatrix[i, j] = 0;
                }
            }
            if (verbose) PrintMatrixForInterleaver(outMatrix, rows, numCols, "Матрица после записи по столбцам:");
        }
        else
        {
            if (verbose) Console.WriteLine("Матрица деперемежения пуста (0 строк или 0 столбцов).");
        }


        int[] deinterleavedData = new int[originalDataLength];
        k_idx = 0;
        if (rows > 0 && numCols > 0)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    if (k_idx < originalDataLength)
                        deinterleavedData[k_idx++] = outMatrix[i, j];
                    else
                        break;
                }
                if (k_idx >= originalDataLength) break;
            }
        }
        if (verbose) Console.WriteLine($"Деперемеженные данные (считаны по строкам, длина {deinterleavedData.Length}): {ToBitString(deinterleavedData)}");
        if (verbose) Console.WriteLine("--- Конец деперемежения ---");
        return deinterleavedData;
    }


    public static void RunTrial(int totalInfoBits, int k_ecc, int[] genPoly_ecc,
                                int interleaverCols, int burstErrorLength, bool useInterleaving,
                                out bool overallSuccess, bool verbose = false)
    {
        CyclicCode ecc = new CyclicCode(k_ecc, genPoly_ecc);
        int numInfoBlocks = (int)Math.Ceiling((double)totalInfoBits / ecc.K);
        int actualTotalInfoBits = numInfoBlocks * ecc.K;

        if (actualTotalInfoBits == 0) // Если информационных бит 0, то всегда успех
        {
            if (verbose) Console.WriteLine("Нет информационных бит для обработки. Успех по умолчанию.");
            overallSuccess = true;
            return;
        }

        List<int> originalInfoFull = GenerateRandomBits(actualTotalInfoBits).ToList();

        if (verbose) Console.WriteLine($"\nИСХОДНОЕ ИНФОРМАЦИОННОЕ СООБЩЕНИЕ ({originalInfoFull.Count} бит): {ToBitString(originalInfoFull)}");

        List<int> allCodewords = new List<int>();
        List<int[]> originalCodewordList = new List<int[]>();

        for (int i = 0; i < numInfoBlocks; i++)
        {
            int[] infoBlock = originalInfoFull.Skip(i * ecc.K).Take(ecc.K).ToArray();
            int[] codeword = ecc.Encode(infoBlock);
            allCodewords.AddRange(codeword);
            originalCodewordList.Add(codeword);
            if (verbose) Console.WriteLine($"Блок {i} инфо: {ToBitString(infoBlock)} -> кодовое слово: {ToBitString(codeword)}");
        }
        int[] dataToProcess = allCodewords.ToArray();
        int[] originalUncorruptedDataToProcess = (int[])dataToProcess.Clone();

        if (verbose) Console.WriteLine($"Полное закодированное сообщение ({dataToProcess.Length} бит): {ToBitString(dataToProcess)}");

        int[,] matrixForVerbose;
        int originalDataLengthForInterleaver = dataToProcess.Length; // Длина перед перемежением

        if (useInterleaving)
        {
            dataToProcess = SimulateInterleave(dataToProcess, originalDataLengthForInterleaver, interleaverCols, verbose, out matrixForVerbose);
        }

        int errorPosition = 0;
        if (dataToProcess.Length > 0) // Только если есть данные для повреждения
        {
            if (dataToProcess.Length > burstErrorLength)
            {
                errorPosition = random.Next(0, dataToProcess.Length - burstErrorLength + 1);
            }
            else
            {
                errorPosition = 0;
                burstErrorLength = dataToProcess.Length;
            }
            int[] dataWithErrors = IntroduceBurstError(dataToProcess, burstErrorLength, errorPosition);
            if (verbose) Console.WriteLine($"Сообщение с пакетной ошибкой (длина {burstErrorLength} на позиции {errorPosition} в {(useInterleaving ? "перемеженном" : "закодированном")} потоке): {ToBitString(dataWithErrors)}");
            dataToProcess = dataWithErrors;
        }
        else
        {
            if (verbose) Console.WriteLine("Нет данных для внесения ошибки (длина потока 0).");
        }


        if (useInterleaving)
        {
            dataToProcess = SimulateDeinterleave(dataToProcess, originalDataLengthForInterleaver, interleaverCols, verbose, out matrixForVerbose);
        }

        List<int> reconstructedInfo = new List<int>();
        overallSuccess = true;

        if (verbose) Console.WriteLine("\n--- Декодирование и коррекция блоков ECC ---");
        for (int i = 0; i < numInfoBlocks; i++)
        {
            if (i * ecc.N + ecc.N > dataToProcess.Length)
            {
                if (verbose) Console.WriteLine($"Блок {i}: Недостаточно данных для формирования полного кодового слова после (де)перемежения. Длина остатка: {dataToProcess.Length - i * ecc.N}");
                overallSuccess = false; // Не можем корректно извлечь блок
                break;
            }

            int[] receivedCodewordBlock = dataToProcess.Skip(i * ecc.N).Take(ecc.N).ToArray();
            int[] originalCodewordForComparison = originalCodewordList[i];

            int[] correctedInfoPart = new int[ecc.K];
            bool currentBlockInfoRestored;

            int[] tempCorrectedCodeword; // Переменная для хранения результата DecodeAndCorrect
            bool eccCorrectionAttemptSuccess = ecc.DecodeAndCorrect(receivedCodewordBlock, originalCodewordForComparison, out tempCorrectedCodeword);

            // Информационная часть берется из tempCorrectedCodeword
            Array.Copy(tempCorrectedCodeword, 0, correctedInfoPart, 0, ecc.K);

            // Успех восстановления инфо-части блока, если она совпадает с оригинальной инфо-частью
            int[] originalInfoBlock = originalInfoFull.Skip(i * ecc.K).Take(ecc.K).ToArray();
            currentBlockInfoRestored = correctedInfoPart.SequenceEqual(originalInfoBlock);

            if (verbose)
            {
                Console.Write($"Блок {i}: Принято: {ToBitString(receivedCodewordBlock)} (Ориг. код. слово: {ToBitString(originalCodewordForComparison)}). ");
                Console.Write($"Синдром: {ToBitString(ecc.CalculateSyndrome(receivedCodewordBlock))}. ");
                Console.Write($"Попытка ECC коррекции {(eccCorrectionAttemptSuccess ? "успешна (синдром 0 после)" : "неуспешна (синдром не 0)")}. ");
                Console.WriteLine($"Инфо-часть восстановлена: {currentBlockInfoRestored}. (Извлечено: {ToBitString(correctedInfoPart)}, Ожидалось: {ToBitString(originalInfoBlock)})");
            }

            reconstructedInfo.AddRange(correctedInfoPart);

            if (!currentBlockInfoRestored)
            {
                overallSuccess = false;
            }
        }

        // Дополнительная проверка, если количество восстановленных инфо бит не совпадает
        if (reconstructedInfo.Count != actualTotalInfoBits)
        {
            overallSuccess = false;
            if (verbose) Console.WriteLine($"ОШИБКА: Длина восстановленного инфо-сообщения ({reconstructedInfo.Count}) не совпадает с оригинальной ({actualTotalInfoBits}).");
        }


        if (verbose)
        {
            Console.WriteLine($"\nИтоговое восстановленное инфо-сообщение ({reconstructedInfo.Count} бит): {ToBitString(reconstructedInfo)}");
            Console.WriteLine($"Оригинальное инфо-сообщение ({originalInfoFull.Count} бит):   {ToBitString(originalInfoFull)}");
            Console.WriteLine($"Общий успех для этого испытания: {overallSuccess}");
            Console.WriteLine("====================================");
        }
    }

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        int totalInfoBits = 105;
        int k_ecc = 7;
        int[] G_ecc = new int[] { 1, 0, 1, 1 };
        int interleaverCols = 7;
        int[] burstErrorLengths = { 3, 6, 7 };
        int numTrials = 35;

        Console.WriteLine($"Лабораторная работа 7, Вариант 8: Циклический код (k={k_ecc}, G(x)={ToBitString(G_ecc)}), Столбцов перемежителя={interleaverCols}");
        Console.WriteLine($"Всего информационных бит: {totalInfoBits}, Испытаний для каждого случая: {numTrials}\n");

        foreach (int burstLen in burstErrorLengths)
        {
            Console.WriteLine($"--- Тестирование для длины пакетной ошибки: {burstLen} бит ---");

            int successesWithInterleaving = 0;
            for (int i = 0; i < numTrials; i++)
            {
                bool success;
                RunTrial(totalInfoBits, k_ecc, G_ecc, interleaverCols, burstLen, true, out success, verbose: (i == 0));
                if (success) successesWithInterleaving++;
            }
            double successRateWith = (double)successesWithInterleaving / numTrials * 100;
            Console.WriteLine($"С перемежением: {successesWithInterleaving} из {numTrials} успешных восстановлений ({successRateWith:F2}%)");

            int successesWithoutInterleaving = 0;
            for (int i = 0; i < numTrials; i++)
            {
                bool success;
                RunTrial(totalInfoBits, k_ecc, G_ecc, interleaverCols, burstLen, false, out success, verbose: (i == 0));
                if (success) successesWithoutInterleaving++;
            }
            double successRateWithout = (double)successesWithoutInterleaving / numTrials * 100;
            Console.WriteLine($"Без перемежения: {successesWithoutInterleaving} из {numTrials} успешных восстановлений ({successRateWithout:F2}%)\n");
        }
    }
}