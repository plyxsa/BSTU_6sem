using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

public class BWTResult
{
    public string TransformedText { get; set; }
    public int OriginalIndex { get; set; } // 0-based index
}

public class BurrowsWheelerTransform
{
    public static BWTResult Encode(string text)
    {
        Console.WriteLine($"--- Запуск ПРЯМОГО преобразования BWT для строки: '{text}' (сдвиги ВПРАВО) ---");

        if (string.IsNullOrEmpty(text))
        {
            Console.WriteLine("[ПРЕДУПРЕЖДЕНИЕ] Входная строка пуста или null. Возвращаем пустой результат.");
            return new BWTResult { TransformedText = "", OriginalIndex = -1 };
        }

        int n = text.Length;
        List<string> rotations = new List<string>(n);
        string currentRotation = text;

        Console.WriteLine("\n[ШАГ 1] Генерация циклических сдвигов (ротаций) ВПРАВО:");
        for (int i = 0; i < n; i++)
        {
            rotations.Add(currentRotation);
            Console.WriteLine($"    Сдвиг {i + 1,2}: {currentRotation}"); // i+1 для 1-based нумерации сдвигов

            if (n > 0) // Эта проверка избыточна, т.к. n = text.Length и мы уже проверили string.IsNullOrEmpty
            {
                char lastChar = currentRotation[n - 1];
                string restOfString = currentRotation.Substring(0, n - 1);
                currentRotation = lastChar + restOfString;
            }
        }

        Console.WriteLine("\n[ШАГ 2] Лексикографическая сортировка сдвигов:");
        rotations.Sort(StringComparer.Ordinal); // Используем Ordinal для консистентности с поиском

        Console.WriteLine("    Отсортированная матрица (M):");
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"      [{i.ToString().PadLeft(2)}] {rotations[i]} {(rotations[i].Equals(text, StringComparison.Ordinal) ? "<- Исходная строка" : "")}");
        }

        Console.WriteLine("\n[ШАГ 3] Извлечение последнего столбца (L):");
        StringBuilder transformedTextBuilder = new StringBuilder(n);
        for (int i = 0; i < n; i++)
        {
            transformedTextBuilder.Append(rotations[i][n - 1]);
        }
        string transformedText = transformedTextBuilder.ToString();
        Console.WriteLine($"    => Последний столбец (L): '{transformedText}'");

        Console.WriteLine("\n[ШАГ 4] Поиск 0-based индекса исходной строки (I) в отсортированной матрице:");
        int originalIndex = -1;
        for (int i = 0; i < n; i++)
        {
            if (rotations[i].Equals(text, StringComparison.Ordinal))
            {
                originalIndex = i;
                break;
            }
        }

        if (originalIndex != -1)
        {
            Console.WriteLine($"    => Исходная строка '{text}' найдена. Индекс (I): {originalIndex} (это {originalIndex + 1}-я строка в M)");
        }
        else
        {
            // Этого не должно произойти, если текст не пустой, т.к. исходная строка всегда будет одним из сдвигов
            Console.WriteLine($"    [ОШИБКА!] Исходная строка '{text}' НЕ найдена в отсортированной матрице! Это неожиданно.");
        }

        Console.WriteLine("--- Прямое преобразование BWT завершено. ---\n");
        return new BWTResult { TransformedText = transformedText, OriginalIndex = originalIndex };
    }

    public static string DecodeMatrixMethod(string transformedText, int originalIndex) // originalIndex is 0-based
    {
        Console.WriteLine($"--- Запуск ОБРАТНОГО преобразования BWT (метод матрицы) для L='{transformedText}', I={originalIndex} ({originalIndex + 1}-я строка для извлечения) ---");

        if (string.IsNullOrEmpty(transformedText))
        {
            Console.WriteLine("[ПРЕДУПРЕЖДЕНИЕ] Входная L-строка пуста или null. Возвращаем пустую строку.");
            return "";
        }
        if (originalIndex < 0 || originalIndex >= transformedText.Length)
        {
            Console.WriteLine($"[ОШИБКА] Некорректный индекс I={originalIndex}. Должен быть в диапазоне [0, {transformedText.Length - 1}]. Возвращаем пустую строку.");
            return "";
        }


        int n = transformedText.Length;
        Console.WriteLine($"  Длина L-строки: {n}");

        List<string> matrixRows = new List<string>(n);
        for (int i = 0; i < n; ++i)
        {
            matrixRows.Add("");
        }

        Console.WriteLine("\n[ПРОЦЕСС] Построение исходной матрицы (M) справа налево:");

        for (int j = 0; j < n; j++) // j - номер столбца, который мы "приклеиваем" (от 0 до n-1)
        {
            Console.WriteLine($"\n  Итерация {j + 1}/{n} (добавляем {j + 1}-й символ к каждой строке):");

            Console.WriteLine("    1. Префиксация L-столбца к текущим строкам:");
            List<string> tempRows = new List<string>(n);
            for (int i = 0; i < n; i++)
            {
                string newRow = transformedText[i] + matrixRows[i];
                tempRows.Add(newRow);
                // Для очень длинных строк вывод каждой строки может быть избыточен
                // if (n < 15) Console.WriteLine($"      '{transformedText[i]}' + \"{matrixRows[i].PadRight(j, '.')}\"  =>  \"{newRow}\"");
                Console.WriteLine($"       '{transformedText[i]}' + \"{matrixRows[i]}\"  =>  \"{newRow}\"");
            }
            matrixRows = tempRows;

            Console.WriteLine("\n    2. Лексикографическая сортировка строк:");
            matrixRows.Sort(StringComparer.Ordinal);

            Console.WriteLine("    Текущее состояние строк матрицы (отсортировано):");
            // Для очень длинных строк вывод матрицы на каждой итерации может быть избыточен
            // if (n < 15)
            // {
            for (int i = 0; i < n; i++)
            {
                Console.Write($"      [{i.ToString().PadLeft(2)}]: \"{matrixRows[i]}\"");
                if (i == originalIndex) Console.Write(" <-- Потенциальная исходная строка на этой итерации, если бы это была последняя");
                Console.WriteLine();
            }
            // } else if (j == n-1) { // Показать только финальную матрицу, если N большое
            //      Console.WriteLine("      (Промежуточная матрица опущена для краткости...)");
            // }
        }

        Console.WriteLine("\n--- Построение матрицы завершено. ---");

        // originalIndex уже проверен на корректность в начале метода
        string decodedText = matrixRows[originalIndex];
        Console.WriteLine($"\n[РЕЗУЛЬТАТ ДЕКОДИРОВАНИЯ] Извлекаем строку по 0-based индексу I={originalIndex} ({originalIndex + 1}-я строка): '{decodedText}'");
        Console.WriteLine("--- Обратное преобразование BWT завершено. ---\n");
        return decodedText;
    }
}

class Program
{
    static void Main(string[] args)
    {
        //string inputText = "полина";
        //string inputText = "лопатнюк";
        //string inputText = "самообороноспособность";
        string inputText = "110100011000000111010000101100001101000010111100";


        Console.WriteLine($"Исходный текст для BWT: \"{inputText}\" (длина: {inputText.Length})\n");
        Console.WriteLine("===================================================");

        Stopwatch encodeStopwatch = new Stopwatch();
        encodeStopwatch.Start();
        BWTResult encodedResult = BurrowsWheelerTransform.Encode(inputText);
        encodeStopwatch.Stop();
        TimeSpan encodeElapsedTime = encodeStopwatch.Elapsed;

        Console.WriteLine("===================================================");
        Console.WriteLine("=== Результаты Прямого BWT ===");
        if (!string.IsNullOrEmpty(encodedResult.TransformedText))
        {
            Console.WriteLine($"  L (Преобразованный текст): \"{encodedResult.TransformedText}\"");
            if (encodedResult.OriginalIndex != -1)
            {
                Console.WriteLine($"  I (0-based индекс исходной строки): {encodedResult.OriginalIndex} (соответствует {encodedResult.OriginalIndex + 1}-й строке в отсортированной матрице)");
            }
            else
            {
                Console.WriteLine($"  I (0-based индекс исходной строки): [ОШИБКА ПОИСКА]");
            }
        }
        else
        {
            Console.WriteLine("  Кодирование не произведено (пустая или null входная строка).");
        }
        Console.WriteLine($"\n[ВРЕМЯ] Прямое BWT: {encodeElapsedTime.TotalMilliseconds:F4} мс ({encodeElapsedTime.Ticks} тиков)");
        Console.WriteLine("===================================================\n");


        TimeSpan decodeElapsedTime = TimeSpan.Zero;
        string decodedText = "";

        if (encodedResult.OriginalIndex != -1 && !string.IsNullOrEmpty(encodedResult.TransformedText))
        {
            Console.WriteLine("===================================================");
            Stopwatch decodeStopwatch = new Stopwatch();
            decodeStopwatch.Start();
            decodedText = BurrowsWheelerTransform.DecodeMatrixMethod(encodedResult.TransformedText, encodedResult.OriginalIndex);
            decodeStopwatch.Stop();
            decodeElapsedTime = decodeStopwatch.Elapsed;
            Console.WriteLine("===================================================");

            Console.WriteLine("=== Результаты Обратного BWT (Матричный метод) ===");
            Console.WriteLine($"  Восстановленный текст: \"{decodedText}\"");
            Console.WriteLine($"\n[ВРЕМЯ] Обратное BWT: {decodeElapsedTime.TotalMilliseconds:F4} мс ({decodeElapsedTime.Ticks} тиков)");
            Console.WriteLine("===================================================\n");

            Console.WriteLine("===================================================");
            Console.WriteLine("=== Итоговая проверка и сводка по времени ===");
            Console.WriteLine($"  Исходный текст:         \"{inputText}\"");
            Console.WriteLine($"  Восстановленный текст:  \"{decodedText}\"");
            bool isMatch = inputText == decodedText;
            Console.WriteLine($"  Проверка совпадения:    {(isMatch ? "УСПЕХ (тексты совпадают)" : "!!! ОШИБКА (тексты НЕ СОВПАДАЮТ) !!!")}");

            Console.WriteLine($"\n  Сводка по времени (для '{inputText}', длина: {inputText.Length}):");
            Console.WriteLine($"    Прямое преобразование:   {encodeElapsedTime.TotalMilliseconds:F4} мс");
            Console.WriteLine($"    Обратное преобразование: {decodeElapsedTime.TotalMilliseconds:F4} мс");
            if (encodeElapsedTime.Ticks == 0 && decodeElapsedTime.Ticks == 0 && !string.IsNullOrEmpty(inputText)) // Very fast operations
            {
                Console.WriteLine("  Оба преобразования выполнены очень быстро (менее одного тика таймера).");
            }
            else if (encodeElapsedTime > decodeElapsedTime)
            {
                Console.WriteLine("  > Прямое преобразование заняло больше времени.");
            }
            else if (decodeElapsedTime > encodeElapsedTime)
            {
                Console.WriteLine("  > Обратное преобразование заняло больше времени.");
            }
            else
            {
                Console.WriteLine("  > Время выполнения примерно одинаково.");
            }
            Console.WriteLine("===================================================");
        }
        else if (string.IsNullOrEmpty(inputText))
        {
            Console.WriteLine("[ИНФО] Декодирование не выполнялось, так как входная строка была пустой.");
        }
        else
        {
            Console.WriteLine("[ОШИБКА] Декодирование не выполнено из-за ошибки на этапе кодирования (не найден индекс I).");
        }


        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}