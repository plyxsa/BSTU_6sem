using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ArithmeticCoding
{
    public class SymbolInfo
    {
        public char Symbol { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }
        public decimal Probability { get; set; }
    }

    public ArithmeticCoding() { }

    public static Dictionary<char, decimal> GetProbabilities(string text)
    {
        var counts = new Dictionary<char, int>();
        if (string.IsNullOrEmpty(text)) return counts.ToDictionary(kvp => kvp.Key, kvp => 0m);

        foreach (char c in text)
        {
            if (counts.ContainsKey(c))
                counts[c]++;
            else
                counts[c] = 1;
        }

        var probabilities = new Dictionary<char, decimal>();
        int totalLength = text.Length;
        foreach (var pair in counts)
        {
            probabilities[pair.Key] = (decimal)pair.Value / totalLength;
        }
        return probabilities;
    }

    public static List<SymbolInfo> BuildSymbolInfoTable(Dictionary<char, decimal> charProbabilities)
    {
        var symbolInfoTable = new List<SymbolInfo>();
        decimal cumulativeLow = 0m;
        var sortedSymbols = charProbabilities.Keys.OrderBy(c => c).ToList();

        for (int i = 0; i < sortedSymbols.Count; i++)
        {
            char symbol = sortedSymbols[i];
            decimal probability = charProbabilities[symbol];

            if (i == sortedSymbols.Count - 1 && sortedSymbols.Count > 1)
            {
                if (cumulativeLow < 1.0m)
                {
                    probability = 1.0m - cumulativeLow;
                }
                if (probability < 0) probability = 0;
            }
            else if (sortedSymbols.Count == 1)
            {
                probability = 1.0m;
            }

            symbolInfoTable.Add(new SymbolInfo
            {
                Symbol = symbol,
                Probability = probability,
                Low = cumulativeLow,
                High = cumulativeLow + probability
            });
            cumulativeLow += probability;
        }

        if (symbolInfoTable.Any())
        {
            var lastSymbol = symbolInfoTable.Last();
            if (lastSymbol.High != 1.0m && Math.Abs(lastSymbol.High - 1.0m) < 0.000000000000001m)
            {
                lastSymbol.High = 1.0m;
            }
            if (lastSymbol.High < 1.0m && sortedSymbols.Count > 0)
            {
                if (lastSymbol.Low < 1.0m)
                {
                    lastSymbol.High = 1.0m;
                    lastSymbol.Probability = 1.0m - lastSymbol.Low;
                }
            }
            if (symbolInfoTable.Count == 1)
            {
                lastSymbol.Low = 0m;
                lastSymbol.High = 1.0m;
                lastSymbol.Probability = 1.0m;
            }
        }
        return symbolInfoTable;
    }

    public decimal Encode(string textToEncode, List<SymbolInfo> symbolInfoTable, bool verbose = false)
    {
        decimal low = 0.0m;
        decimal high = 1.0m;

        if (verbose) Console.WriteLine("\n>>> НАЧАЛО ПРОЦЕССА КОДИРОВАНИЯ <<<");
        if (verbose) Console.WriteLine($"Исходный интервал: Low = {low:F28}, High = {high:F28}");

        int step = 1;
        foreach (char symbol in textToEncode)
        {
            decimal currentRange = high - low;
            SymbolInfo sInfo = symbolInfoTable.FirstOrDefault(s => s.Symbol == symbol);

            if (sInfo == null) throw new ArgumentException($"Символ '{symbol}' не найден в таблице вероятностей.");

            decimal oldLow = low;
            decimal oldHigh = high;

            high = low + currentRange * sInfo.High;
            low = low + currentRange * sInfo.Low;

            if (verbose)
            {
                Console.WriteLine($"\n--- Шаг кодирования {step} ---");
                Console.WriteLine($"  Кодируемый символ: '{sInfo.Symbol}' (Базовый диапазон: [{sInfo.Low:F6} - {sInfo.High:F6}])");
                Console.WriteLine($"  Предыдущий интервал: [{oldLow:F28} - {oldHigh:F28}], Диапазон: {currentRange:F28}");
                Console.WriteLine($"  Новый интервал:     [{low:F28} - {high:F28}]");
            }
            step++;
        }
        if (verbose) Console.WriteLine("\n>>> КОНЕЦ ПРОЦЕССА КОДИРОВАНИЯ <<<\n");
        return low;
    }

    public string Decode(decimal encodedValue, int originalLength, List<SymbolInfo> symbolInfoTable, bool verbose = false)
    {
        StringBuilder decodedText = new StringBuilder();
        decimal low = 0.0m;
        decimal high = 1.0m;
        const decimal EPSILON = 1e-27m;

        if (verbose) Console.WriteLine("\n>>> НАЧАЛО ПРОЦЕССА ДЕКОДИРОВАНИЯ <<<");
        if (verbose) Console.WriteLine($"Исходное значение для декодирования: {encodedValue:F28}");
        if (verbose) Console.WriteLine($"Начальный интервал: Low = {low:F28}, High = {high:F28}");

        for (int i = 0; i < originalLength; i++)
        {
            decimal currentRange = high - low;

            if (verbose) Console.WriteLine($"\n--- Шаг декодирования {i + 1} ---");
            if (verbose) Console.WriteLine($"  Текущий интервал: [{low:F28} - {high:F28}], Диапазон: {currentRange:F28}");

            if (currentRange <= 0 || currentRange < EPSILON * symbolInfoTable.Count * 1e-5m)
            {
                Console.WriteLine($"  ВНИМАНИЕ: Критически малый или нулевой диапазон ({currentRange}). Декодирование может быть неточным.");
                decimal valueInCurrentRangeForEmergency = (currentRange > 0) ? (encodedValue - low) / currentRange : 0;
                SymbolInfo emergencySymbol = symbolInfoTable
                    .OrderBy(s => Math.Abs(valueInCurrentRangeForEmergency - s.Low))
                    .ThenBy(s => s.Symbol)
                    .First();
                decodedText.Append(emergencySymbol.Symbol);
                if (verbose) Console.WriteLine($"  Аварийно выбран символ '{emergencySymbol.Symbol}' из-за малого диапазона. Масштаб. значение: {valueInCurrentRangeForEmergency:F28}");

                if (currentRange > 0)
                {
                    decimal temp_high_emergency = low + currentRange * emergencySymbol.High;
                    low = low + currentRange * emergencySymbol.Low;
                    high = temp_high_emergency;
                }
                else
                {
                    if (verbose) Console.WriteLine("  Диапазон равен нулю, дальнейшее декодирование невозможно стандартным методом.");
                    if (i < originalLength - 1 && verbose)
                    {
                        Console.WriteLine($"  ПРЕДУПРЕЖДЕНИЕ: Цикл будет продолжен, но результаты могут быть неверными.");
                    }
                }
                continue;
            }

            decimal valueInCurrentRange = (encodedValue - low) / currentRange;
            if (verbose) Console.WriteLine($"  Масштабированное значение (encodedValue - Low) / Range = {valueInCurrentRange:F28}");

            SymbolInfo foundSymbol = null;
            foreach (var sInfo in symbolInfoTable)
            {
                bool isMatch = false;
                if (valueInCurrentRange >= sInfo.Low - EPSILON)
                {
                    if (Math.Abs(sInfo.High - 1.0m) < EPSILON)
                    {
                        if (valueInCurrentRange <= sInfo.High + EPSILON)
                        {
                            isMatch = true;
                        }
                    }
                    else
                    {
                        if (valueInCurrentRange < sInfo.High - EPSILON)
                        {
                            isMatch = true;
                        }
                    }
                }

                if (isMatch)
                {
                    foundSymbol = sInfo;
                    break;
                }
            }

            if (foundSymbol == null)
            {
                if (verbose) Console.WriteLine($"  ПРЕДУПРЕЖДЕНИЕ: Символ не найден для масштабированного значения {valueInCurrentRange:F28}. EncodedValue: {encodedValue:F28}");
                SymbolInfo bestCandidate = null;
                decimal minDiff = decimal.MaxValue;

                foreach (var sInfoCand in symbolInfoTable)
                {
                    bool isContainedRelaxed = (valueInCurrentRange >= sInfoCand.Low - EPSILON && valueInCurrentRange <= sInfoCand.High + EPSILON);
                    if (isContainedRelaxed)
                    {
                        if (bestCandidate == null) bestCandidate = sInfoCand;

                        decimal midPoint = sInfoCand.Low + (sInfoCand.High - sInfoCand.Low) / 2;
                        decimal diff = Math.Abs(valueInCurrentRange - midPoint);
                        if (diff < minDiff)
                        {
                            minDiff = diff;
                        }
                    }
                }

                if (bestCandidate != null)
                {
                    foundSymbol = bestCandidate;
                    if (verbose) Console.WriteLine($"  Аварийно (расширенный поиск) выбран символ '{foundSymbol.Symbol}'");
                }
                else
                {
                    foundSymbol = symbolInfoTable.OrderBy(s => Math.Abs(valueInCurrentRange - s.Low)).First();
                    if (verbose) Console.WriteLine($"  Аварийно (ближайший по L0) выбран символ '{foundSymbol.Symbol}'");
                }
            }

            if (foundSymbol == null)
            {
                throw new InvalidOperationException($"Критическая ошибка декодирования: не удалось найти символ на шаге {i + 1}. " +
                                                    $"EncodedValue={encodedValue:F28}, Low={low:F28}, High={high:F28}, Range={currentRange:F28}, ValueInCurrentRange={valueInCurrentRange:F28}");
            }

            decodedText.Append(foundSymbol.Symbol);
            if (verbose) Console.WriteLine($"  Найден символ: '{foundSymbol.Symbol}' (Базовый диапазон: [{foundSymbol.Low:F6} - {foundSymbol.High:F6}])");

            decimal oldLow = low;
            high = low + currentRange * foundSymbol.High;
            low = oldLow + currentRange * foundSymbol.Low;

            if (verbose) Console.WriteLine($"  Обновленный интервал: [{low:F28} - {high:F28}]");

            if (low > high || Math.Abs(low - high) < 1e-28m && i < originalLength - 1)
            {
                if (verbose) Console.WriteLine($"  ПРЕДУПРЕЖДЕНИЕ: Невалидный или слишком узкий интервал после обновления: Low={low:F28}, High={high:F28}. Следующая итерация может быть неточной.");
            }
        }
        if (verbose) Console.WriteLine("\n>>> КОНЕЦ ПРОЦЕССА ДЕКОДИРОВАНИЯ <<<\n");
        return decodedText.ToString();
    }
}

public class Lab11
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        bool enableVerboseOutput = true;

        string message1 = "самообороноспособность";
        Console.WriteLine($"############################################################");
        Console.WriteLine($"### ОБРАБОТКА СООБЩЕНИЯ 1: '{message1}' ###");
        Console.WriteLine($"############################################################");
        Dictionary<char, decimal> probs1 = ArithmeticCoding.GetProbabilities(message1);
        List<ArithmeticCoding.SymbolInfo> symbolTable1 = ArithmeticCoding.BuildSymbolInfoTable(probs1);

        Console.WriteLine("\n*** Таблица вероятностей и диапазонов L0, H0 для Сообщения 1 ***");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine($"{"Символ",-8} | {"Вероятность",-15} | {"L0",-30} | {"H0",-30}");
        Console.WriteLine("--------------------------------------------------------------------------------");
        foreach (var sInfo in symbolTable1.OrderBy(s => s.Symbol))
        {
            Console.WriteLine($"{sInfo.Symbol,-8} | {sInfo.Probability,-15:F6} | {sInfo.Low,-30:F28} | {sInfo.High,-30:F28}");
        }
        Console.WriteLine("--------------------------------------------------------------------------------");

        ArithmeticCoding coder1 = new ArithmeticCoding();
        decimal encoded1 = coder1.Encode(message1, symbolTable1, enableVerboseOutput);
        Console.WriteLine($"\nИТОГ КОДИРОВАНИЯ для '{message1}':");
        Console.WriteLine($"  Закодированное значение: {encoded1:F28}");

        string decoded1 = coder1.Decode(encoded1, message1.Length, symbolTable1, enableVerboseOutput);
        Console.WriteLine($"\nИТОГ ДЕКОДИРОВАНИЯ для '{message1}':");
        Console.WriteLine($"  Декодированное сообщение: '{decoded1}'");
        Console.WriteLine($"  Сообщения совпадают: {(message1 == decoded1 ? "ДА" : "НЕТ")}");
        Console.WriteLine(new string('=', 70));


        string part1_msg2 = "самообороноспособность";
        string part2_msg2 = "малосимпатичный";
        string message2 = part1_msg2 + part2_msg2;

        Console.WriteLine($"\n\n############################################################");
        Console.WriteLine($"### ОБРАБОТКА СООБЩЕНИЯ 2: '{message2}' ###");
        Console.WriteLine($"############################################################");
        Dictionary<char, decimal> probs2 = ArithmeticCoding.GetProbabilities(message2);
        List<ArithmeticCoding.SymbolInfo> symbolTable2 = ArithmeticCoding.BuildSymbolInfoTable(probs2);

        Console.WriteLine("\n*** Таблица вероятностей и диапазонов L0, H0 для Сообщения 2 ***");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine($"{"Символ",-8} | {"Вероятность",-15} | {"L0 (кумулятивно)",-30} | {"H0 (кумулятивно)",-30}");
        Console.WriteLine("--------------------------------------------------------------------------------");
        foreach (var sInfo in symbolTable2.OrderBy(s => s.Symbol))
        {
            Console.WriteLine($"{sInfo.Symbol,-8} | {sInfo.Probability,-15:F6} | {sInfo.Low,-30:F28} | {sInfo.High,-30:F28}");
        }
        Console.WriteLine("--------------------------------------------------------------------------------");

        ArithmeticCoding coder2 = new ArithmeticCoding();
        decimal encoded2 = coder2.Encode(message2, symbolTable2, enableVerboseOutput);
        Console.WriteLine($"\nИТОГ КОДИРОВАНИЯ для '{message2}':");
        Console.WriteLine($"  Закодированное значение: {encoded2:F28}");

        string decoded2 = coder2.Decode(encoded2, message2.Length, symbolTable2, enableVerboseOutput);
        Console.WriteLine($"\nИТОГ ДЕКОДИРОВАНИЯ для '{message2}':");
        Console.WriteLine($"  Декодированное сообщение: '{decoded2}'");
        Console.WriteLine($"  Сообщения совпадают: {(message2 == decoded2 ? "ДА" : "НЕТ")}");
        Console.WriteLine(new string('=', 70));

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}