using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.IO;

public class LZ77Coder
{
    private int dictionarySizeN1;
    private int lookaheadBufferSizeN2;
    private bool enableVerboseOutput;
    private char defaultFillChar = '0';

    public LZ77Coder(int n1, int n2, bool verbose = false)
    {
        if (n1 <= 0) throw new ArgumentException("Размер словаря (n1) должен быть положительным.");
        if (n2 < 1 && n2 != 0) throw new ArgumentException("Размер буфера упреждения (n2) должен быть >= 1, если не 0 (для правила Q_max = N2-1).");
        dictionarySizeN1 = n1;
        lookaheadBufferSizeN2 = n2;
        enableVerboseOutput = verbose;
    }

    public struct Triplet
    {
        public int P { get; } // Смещение НАЗАД от текущей позиции (1-based). P=0 если нет совпадения.
        public int Q { get; } // Длина совпадения. Q=0 если нет совпадения.
        public char S { get; } // Следующий символ.

        public Triplet(int p, int q, char s)
        {
            P = p; // P - это смещение назад
            Q = q;
            S = s;
        }

        public override string ToString()
        {
            char displayS = S;
            // Если S - это маркер конца (P=0,Q=0,S=defaultFillChar), то в выводе можно это отметить
            if (S == '0' && P == 0 && Q == 0)
            {
                // displayS = '§'; // или другое обозначение EOF, если defaultFillChar может быть в данных
            }
            else if (S == '\0') // Если \0 используется как внутренний маркер, а не defaultFillChar
            {
                // displayS = '?';
            }
            return $"({P},{Q},'{displayS}')"; // Убрал Replace для \0, чтобы видеть его, если он не defaultFillChar
        }
    }

    public List<Triplet> Encode(string inputText)
    {
        List<Triplet> encodedTriplets = new List<Triplet>();
        StringBuilder searchBuffer = new StringBuilder(new string(defaultFillChar, dictionarySizeN1));
        int currentInputPosition = 0;
        int step = 1;

        if (enableVerboseOutput)
        {
            Console.WriteLine("\n--- НАЧАЛО КОДИРОВАНИЯ (P - смещение назад, Q_max <= N2-1 если N2>0) ---");
            Console.WriteLine($"Параметры: Окно поиска (n1)={dictionarySizeN1}, Буфер упреждения (n2)={lookaheadBufferSizeN2}");
            Console.WriteLine($"Символ по умолчанию для окна: '{defaultFillChar}'");
            Console.WriteLine($"Входная строка: \"{inputText}\"");
            Console.WriteLine($"Начальное окно поиска: [{searchBuffer.ToString()}]");
        }

        while (currentInputPosition < inputText.Length)
        {
            if (enableVerboseOutput)
            {
                Console.WriteLine($"\nШаг {step}:");
                Console.WriteLine($"  Окно поиска:         [{searchBuffer.ToString()}]");
            }

            int lookaheadActualSize = Math.Min(lookaheadBufferSizeN2, inputText.Length - currentInputPosition);
            string lookaheadBuffer = "";
            if (lookaheadActualSize > 0)
            {
                lookaheadBuffer = inputText.Substring(currentInputPosition, lookaheadActualSize);
            }

            if (enableVerboseOutput)
            {
                string remainingInput = (currentInputPosition + lookaheadActualSize < inputText.Length) ?
                                      inputText.Substring(currentInputPosition + lookaheadActualSize) : "";
                Console.WriteLine($"  Буфер упреждения:    [{lookaheadBuffer.PadRight(Math.Max(lookaheadBuffer.Length, lookaheadBufferSizeN2), ' ')}] Остаток: [{remainingInput}]");
            }

            int bestMatchP_OffsetBackward = 0; // Смещение НАЗАД (1-based)
            int bestMatchLengthQ = 0;

            // Максимальная длина Q для поиска, чтобы S остался в lookaheadBuffer
            int max_q_to_search_for = 0;
            if (lookaheadActualSize > 0 && lookaheadBufferSizeN2 > 0)
            {
                max_q_to_search_for = lookaheadActualSize - 1; // Q_max = N2-1
            }
            if (max_q_to_search_for < 0) max_q_to_search_for = 0;


            if (lookaheadBuffer.Length > 0 && max_q_to_search_for >= 0) // Если Q может быть хотя бы 0 (для (0,0,S))
            {
                // Ищем САМОЕ ДЛИННОЕ совпадение.
                // Если несколько совпадений максимальной длины, выбираем то, у которого P (смещение назад) МЕНЬШЕ.
                // (т.е. самое "свежее", самое правое вхождение в searchBuffer)
                for (int q = max_q_to_search_for; q >= 1; q--) // Перебираем длины от большей к меньшей
                {
                    if (q == 0 && bestMatchLengthQ > 0) continue; // Если уже нашли совпадение, нет смысла проверять Q=0

                    string patternToFind = lookaheadBuffer.Substring(0, q);
                    int foundAt_SB_0based_Index = -1;

                    // Ищем последнее (самое правое) вхождение этого паттерна в searchBuffer
                    for (int i = searchBuffer.Length - q; i >= 0; i--)
                    {
                        bool match = true;
                        for (int k = 0; k < q; k++)
                        {
                            if (searchBuffer[i + k] != patternToFind[k])
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            foundAt_SB_0based_Index = i;
                            break; // Нашли самое правое для текущей длины q
                        }
                    }

                    if (foundAt_SB_0based_Index != -1)
                    {
                        bestMatchLengthQ = q;
                        bestMatchP_OffsetBackward = searchBuffer.Length - foundAt_SB_0based_Index; // P - смещение назад
                        goto MatchFoundLoopExit; // Нашли самое длинное + самое правое для него, выходим
                    }
                }
            }
        MatchFoundLoopExit:;

            char symbolS;
            int symbolsToShift;

            if (lookaheadActualSize == 0) // Достигли конца inputText в предыдущем шаге
            {
                symbolS = defaultFillChar; // Это будет маркер конца (0,0,EOF)
                bestMatchLengthQ = 0;
                bestMatchP_OffsetBackward = 0;
                symbolsToShift = 0; // Уже нечего сдвигать из inputText
                if (encodedTriplets.Count > 0 && encodedTriplets.Last().P == 0 && encodedTriplets.Last().Q == 0 && encodedTriplets.Last().S == defaultFillChar)
                {
                    if (enableVerboseOutput) Console.WriteLine("  Предотвращение двойного EOF.");
                    break;
                }
            }
            else
            {
                // bestMatchLengthQ не превышает max_q_to_search_for (т.е. N2-1 или меньше)
                // Значит, lookaheadBuffer[bestMatchLengthQ] всегда валиден, если lookaheadActualSize > bestMatchLengthQ
                if (bestMatchLengthQ < lookaheadActualSize)
                {
                    symbolS = lookaheadBuffer[bestMatchLengthQ];
                }
                else
                {
                    // Это случай, когда Q = N2-1 и lookaheadActualSize = N2. S = lookaheadBuffer[N2-1]
                    // Или если lookaheadActualSize = bestMatchLengthQ (например, Q=0, lookaheadActualSize=0 - уже обработано)
                    // Если bestMatchLengthQ == lookaheadActualSize, то S должен быть EOF, так как мы использовали весь буфер для Q
                    // Но по правилу Q_max = N2-1, эта ситуация не должна возникнуть, если N2>0.
                    // Если N2=0 (или N2=1 -> max_q=0), то Q=0, S=lookahead[0].
                    symbolS = defaultFillChar; // Запасной вариант, если что-то пошло не так с логикой S
                    if (lookaheadActualSize > bestMatchLengthQ)
                    { // Это условие должно быть истинно по правилу Q <= N2-1
                        symbolS = lookaheadBuffer[bestMatchLengthQ];
                    }
                    else if (lookaheadActualSize == bestMatchLengthQ && lookaheadActualSize > 0)
                    {
                        // Эта ветка не должна достигаться при Q_max = N2-1. Если достиглась, S - это lookaheadBuffer[Q-1]
                        // и следующий символ (реальный S) должен был бы быть defaultFillChar,
                        // но так как Q заняло весь буфер, то S и есть defaultFillChar.
                        // Это не совсем так. Если Q = lookaheadActualSize - 1, то S = lookaheadActualSize[Q].
                        // Если Q = 0, то S = lookaheadBuffer[0].
                        if (bestMatchLengthQ == 0 && lookaheadActualSize > 0)
                        {
                            symbolS = lookaheadBuffer[0];
                        }
                        else
                        {
                            // Если Q > 0 и Q == max_q_to_search_for == lookaheadActualSize - 1
                            // то S = lookaheadBuffer[Q]
                            symbolS = lookaheadBuffer[bestMatchLengthQ];
                        }
                    }
                }
                symbolsToShift = bestMatchLengthQ + 1;
            }

            encodedTriplets.Add(new Triplet(bestMatchP_OffsetBackward, bestMatchLengthQ, symbolS));

            if (enableVerboseOutput)
            {
                string matchStr = bestMatchLengthQ > 0 ? inputText.Substring(currentInputPosition, bestMatchLengthQ) : "НЕТ";
                Console.WriteLine($"  Найдено совпадение: \"{matchStr}\" (P_назад={bestMatchP_OffsetBackward}, Q={bestMatchLengthQ}). Следующий символ S='{symbolS}'");
                Console.WriteLine($"  Выведена триада: {encodedTriplets.Last()}");
            }

            if (symbolsToShift > 0 && currentInputPosition < inputText.Length)
            {
                int actualCharsToProcessFromInput = Math.Min(symbolsToShift, inputText.Length - currentInputPosition);
                if (actualCharsToProcessFromInput > 0)
                {
                    string processedChars = inputText.Substring(currentInputPosition, actualCharsToProcessFromInput);
                    searchBuffer.Append(processedChars);
                }
            }

            if (searchBuffer.Length > dictionarySizeN1)
            {
                searchBuffer.Remove(0, searchBuffer.Length - dictionarySizeN1);
            }

            currentInputPosition += symbolsToShift;
            step++;

            bool isLastMeaningfulTriplet = (symbolS == defaultFillChar &&
                                           (bestMatchP_OffsetBackward != 0 || bestMatchLengthQ != 0) &&
                                            currentInputPosition >= inputText.Length);
            bool isExplicitEOF = (symbolS == defaultFillChar &&
                                  bestMatchP_OffsetBackward == 0 &&
                                  bestMatchLengthQ == 0 &&
                                  currentInputPosition >= inputText.Length);


            if (isExplicitEOF)
            {
                if (enableVerboseOutput) Console.WriteLine("  Завершение: последняя триада (0,0,EOF).");
                break;
            }
            if (symbolsToShift == 0 && lookaheadActualSize == 0)
            {
                if (enableVerboseOutput) Console.WriteLine("  Завершение: lookahead был пуст, нечего сдвигать.");
                // Проверим, не нужно ли добавить (0,0,EOF)
                if (encodedTriplets.Last().S != defaultFillChar || encodedTriplets.Last().P != 0 || encodedTriplets.Last().Q != 0)
                {
                    encodedTriplets.Add(new Triplet(0, 0, defaultFillChar));
                    if (enableVerboseOutput) Console.WriteLine($"  Добавлена финальная триада: {encodedTriplets.Last()}");
                }
                break;
            }
            if (isLastMeaningfulTriplet)
            { // Если S стал EOF потому что Q дошло до конца строки
                if (enableVerboseOutput) Console.WriteLine("  Завершение: S стал EOF из-за конца строки после Q.");
                break;
            }
        }
        if (enableVerboseOutput) Console.WriteLine("--- КОНЕЦ КОДИРОВАНИЯ ---");
        return encodedTriplets;
    }

    public string Decode(List<Triplet> encodedTriplets)
    {
        StringBuilder decodedText = new StringBuilder();
        StringBuilder searchBuffer = new StringBuilder(new string(defaultFillChar, dictionarySizeN1));
        int step = 1;

        if (enableVerboseOutput)
        {
            Console.WriteLine("\n--- НАЧАЛО ДЕКОДИРОВАНИЯ (P - смещение назад) ---");
            Console.WriteLine($"Параметры: Окно поиска (n1)={dictionarySizeN1}, Символ по умолчанию='{defaultFillChar}'");
            Console.WriteLine($"Начальное окно поиска: [{searchBuffer.ToString()}]");
        }

        for (int tripletIndex = 0; tripletIndex < encodedTriplets.Count; tripletIndex++)
        {
            Triplet triplet = encodedTriplets[tripletIndex];
            if (enableVerboseOutput)
            {
                Console.WriteLine($"\nШаг {step}:");
                Console.WriteLine($"  Окно поиска: [{searchBuffer.ToString()}]");
                Console.WriteLine($"  Принята триада: {triplet}");
            }

            int p_offset_backward = triplet.P;
            int q = triplet.Q;
            char s = triplet.S;

            if (p_offset_backward == 0 && q == 0 && s == defaultFillChar && tripletIndex == encodedTriplets.Count - 1)
            {
                if (enableVerboseOutput) Console.WriteLine("  Обнаружен явный маркер конца потока (0,0,EOF_char). Завершение декодирования.");
                break;
            }

            if (q > 0)
            {
                if (p_offset_backward == 0)
                {
                    if (enableVerboseOutput) Console.WriteLine($"  ПРЕДУПРЕЖДЕНИЕ: Невалидная триада с P=0 и Q>0. Пропуск копирования.");
                }
                else if (p_offset_backward > searchBuffer.Length)
                {
                    if (enableVerboseOutput) Console.WriteLine($"  ОШИБКА ДЕКОДИРОВАНИЯ: Смещение P={p_offset_backward} больше длины окна {searchBuffer.Length}.");
                }
                else
                {
                    for (int i = 0; i < q; i++)
                    {
                        char charToCopy = searchBuffer[searchBuffer.Length - p_offset_backward];
                        decodedText.Append(charToCopy);
                        searchBuffer.Append(charToCopy);
                        if (searchBuffer.Length > dictionarySizeN1)
                        {
                            searchBuffer.Remove(0, 1);
                        }
                    }
                    if (enableVerboseOutput) Console.WriteLine($"  Скопировано из окна (длина {q}, смещение назад {p_offset_backward})");
                }
            }

            // Добавляем S, если это не последняя триада, которая была (P>0 или Q>0) и S=EOF
            bool isLastTriplet = tripletIndex == encodedTriplets.Count - 1;
            bool sIsPaddingEOFOnLast = (s == defaultFillChar && isLastTriplet && (p_offset_backward > 0 || q > 0));

            if (!sIsPaddingEOFOnLast) // Не добавляем S, если это defaultFillChar на последней "содержательной" триаде
            {
                decodedText.Append(s);
                searchBuffer.Append(s);
                if (searchBuffer.Length > dictionarySizeN1)
                {
                    searchBuffer.Remove(0, 1);
                }
                if (enableVerboseOutput) Console.WriteLine($"  Добавлен символ S: '{s}'");
            }
            else
            {
                if (enableVerboseOutput) Console.WriteLine($"  Символ S ('{s}') на последней значащей триаде является заполнителем EOF, не добавляется.");
            }


            if (enableVerboseOutput) Console.WriteLine($"  Восстановленный текст (часть): ...{decodedText.ToString().Substring(Math.Max(0, decodedText.Length - 30))}");
            step++;
            if (sIsPaddingEOFOnLast)
            { // Если это была последняя значащая триада с S=EOF
                if (enableVerboseOutput) Console.WriteLine("  Завершение: Обработан S=EOF на последней значащей триаде.");
                break;
            }
        }
        if (enableVerboseOutput) Console.WriteLine("--- КОНЕЦ ДЕКОДИРОВАНИЯ ---");
        return decodedText.ToString();
    }
}

// Класс Program остается таким же
// ... (вставьте ваш класс Program сюда) ...
class Program
{
    static readonly (int n1, int n2)[] windowSizeCombinations = new (int, int)[]
    {
        (15, 13), (32, 16), (64, 32), (128, 64),
        (256, 128), (512, 256), (1024, 512), (60,40)
    };

    struct TestResult
    {
        public int N1;
        public int N2;
        public long EncodeTimeMs;
        public long DecodeTimeMs;
        public int TripletCount;
        public int OriginalLength;
        public double CompressionRatio;
        public bool IsCorrect;
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("--- Демонстрационный этап ---");
        string demoInputText = "2000302013020130313031303130313333333";
        int demoN1 = 15;
        int demoN2 = 13;
        bool demoVerbose = true;

        Console.WriteLine($"Демо-строка (S): \"{demoInputText}\" (длина: {demoInputText.Length})");
        Console.WriteLine($"Окна для демо: n1={demoN1} (поиск), n2={demoN2} (упреждение)");
        LZ77Coder demoCoder = new LZ77Coder(demoN1, demoN2, demoVerbose);

        Console.WriteLine("\nКодирование демо-строки:");
        List<LZ77Coder.Triplet> demoEncodedData = demoCoder.Encode(demoInputText);
        Console.Write("Закодированные триады (P,Q,S): ");
        foreach (var triplet in demoEncodedData) { Console.Write(triplet + " "); }
        Console.WriteLine($"\nВсего триад: {demoEncodedData.Count}");

        Console.WriteLine("\nДекодирование демо-строки:");
        string demoDecodedText = demoCoder.Decode(demoEncodedData);
        Console.WriteLine($"Декодированный текст: \"{demoDecodedText}\"");
        Console.WriteLine($"Корректность: {(demoInputText == demoDecodedText ? "УСПЕШНО" : "ОШИБКА")}");
        Console.WriteLine("--- Конец демонстрационного этапа ---\n");

        Console.WriteLine("Нажмите Enter для основного этапа...");
        Console.ReadLine();

        Console.WriteLine("\n--- Основной этап (задания 2-3) ---");
        string inputTextFromFile = "";
        string filePath = "D:\\Uni\\CMZI\\CMZI_lab10\\Lab10\\Lab10\\Lab10\\inputText.txt";

        try
        {
            inputTextFromFile = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            Console.WriteLine($"Загружен текст из файла {filePath}, длина: {inputTextFromFile.Length} символов.\n");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл {filePath} не найден. Используется тестовая строка для основного этапа.");
            inputTextFromFile = "Это_длинный_текстовый_файл_для_демонстрации_работы_LZ77_с_разными_размерами_окон_и_для_проверки_эффективности_сжатия_и_скорости_кодирования_и_декодирования_000aaa111bbb000ccc.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            return;
        }

        bool enableVerboseForFile = false;
        if (inputTextFromFile.Length < 250)
        {
            Console.Write("Включить подробный вывод для текста из файла? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "y")
            {
                enableVerboseForFile = true;
            }
        }
        else
        {
            Console.WriteLine("Подробный вывод для текста из файла отключен (текст >250 симв).");
        }

        List<TestResult> results = new List<TestResult>();
        Console.WriteLine("\n--- Запуск тестов для текста из файла ---");

        foreach (var (n1, n2) in windowSizeCombinations)
        {
            Console.WriteLine($"\nТестирование: Окно поиска n1={n1}, Буфер упреждения n2={n2}");
            LZ77Coder fileCoder = new LZ77Coder(n1, n2, enableVerboseForFile);

            Stopwatch stopwatchEncode = Stopwatch.StartNew();
            List<LZ77Coder.Triplet> encodedDataFile = fileCoder.Encode(inputTextFromFile);
            stopwatchEncode.Stop();

            Stopwatch stopwatchDecode = Stopwatch.StartNew();
            string decodedTextFile = fileCoder.Decode(encodedDataFile);
            stopwatchDecode.Stop();

            bool isCorrect = (inputTextFromFile == decodedTextFile);

            int bitsPerSymbol = 8;
            long originalSizeBytes = (long)inputTextFromFile.Length * bitsPerSymbol / 8;
            if (originalSizeBytes == 0 && inputTextFromFile.Length > 0) originalSizeBytes = 1;

            int bitsForP = (n1 > 0) ? (int)Math.Ceiling(Math.Log2(n1 + 1)) : 0;
            int bitsForQ = (n2 >= 0) ? (int)Math.Ceiling(Math.Log2(n2 + 1)) : 0;
            int bitsPerTripletVal = bitsForP + bitsForQ + bitsPerSymbol;

            long compressedSizeBits = (long)encodedDataFile.Count * bitsPerTripletVal;
            long compressedSizeBytes = compressedSizeBits / 8;
            if (compressedSizeBits % 8 != 0) compressedSizeBytes++;

            double compressionRatio = 0;
            if (originalSizeBytes > 0)
            {
                compressionRatio = (1.0 - (double)compressedSizeBytes / originalSizeBytes) * 100.0;
            }

            results.Add(new TestResult
            {
                N1 = n1,
                N2 = n2,
                EncodeTimeMs = stopwatchEncode.ElapsedMilliseconds,
                DecodeTimeMs = stopwatchDecode.ElapsedMilliseconds,
                TripletCount = encodedDataFile.Count,
                OriginalLength = inputTextFromFile.Length,
                CompressionRatio = compressionRatio,
                IsCorrect = isCorrect
            });

            Console.WriteLine($"  Кодирование: {stopwatchEncode.ElapsedMilliseconds} мс, Триад: {encodedDataFile.Count}");
            Console.WriteLine($"  Декодирование: {stopwatchDecode.ElapsedMilliseconds} мс, Корректно: {(isCorrect ? "Да" : "Нет!!!")}");
            Console.WriteLine($"  Размер до: {originalSizeBytes} B, после: {compressedSizeBytes} B (при {bitsPerTripletVal} бит/триада)");
            Console.WriteLine($"  Коэф. сжатия: {compressionRatio:F2}%");
            if (!isCorrect && !enableVerboseForFile) Console.WriteLine("  !!! ОБНАРУЖЕНА ОШИБКА ДЕКОДИРОВАНИЯ !!!");
        }

        Console.WriteLine("\n\n--- Итоговая таблица результатов ---");
        Console.WriteLine("===================================================================================================================");
        Console.WriteLine("|  Окно (n1,n2)  | Кодирование (мс) | Декодирование (мс) | Кол-во триад | Исход. | Сжатое | Коэф. сжатия | Корректно |");
        Console.WriteLine("|----------------|------------------|--------------------|--------------|--------|--------|--------------|-----------|");
        foreach (var res in results.OrderBy(r => r.N1).ThenBy(r => r.N2))
        {
            long originalSizeBytesRes = (long)res.OriginalLength * 8 / 8;
            if (originalSizeBytesRes == 0 && res.OriginalLength > 0) originalSizeBytesRes = 1;
            int bitsPerPRes = (res.N1 > 0) ? (int)Math.Ceiling(Math.Log2(res.N1 + 1)) : 0;
            int bitsPerQRes = (res.N2 >= 0) ? (int)Math.Ceiling(Math.Log2(res.N2 + 1)) : 0;
            int bitsTripletRes = bitsPerPRes + bitsPerQRes + 8;
            long compressedSizeBytesRes = (long)res.TripletCount * bitsTripletRes / 8;
            if (((long)res.TripletCount * bitsTripletRes) % 8 != 0) compressedSizeBytesRes++;

            Console.WriteLine($"| {res.N1,4},{res.N2,-4} | {res.EncodeTimeMs,16} | {res.DecodeTimeMs,18} | {res.TripletCount,12} | {originalSizeBytesRes,6}B | {compressedSizeBytesRes,6}B | {res.CompressionRatio,12:F2}% | {(res.IsCorrect ? "Да" : "Нет"),-9} |");
        }
        Console.WriteLine("===================================================================================================================");

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}