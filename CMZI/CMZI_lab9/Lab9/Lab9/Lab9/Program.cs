using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// --- Класс для хранения информации о символе ---
public class SymbolInfo
{
    public char Symbol { get; set; }
    public double Frequency { get; set; }
    public double Probability { get; set; }
    public string Code { get; set; } = "";

    public SymbolInfo(char symbol, double frequency)
    {
        Symbol = symbol;
        Frequency = frequency;
    }

    public override string ToString()
    {
        return $"'{Symbol}': F={Frequency:F0}, P={Probability:F5}, C='{Code}'";
    }
}

// --- Узел для дерева Хаффмана ---
public class HuffmanNode : IComparable<HuffmanNode>
{
    public char Symbol { get; set; }
    public double Probability { get; set; }
    public HuffmanNode LeftChild { get; set; }
    public HuffmanNode RightChild { get; set; }

    public HuffmanNode(char symbol, double probability)
    {
        Symbol = symbol;
        Probability = probability;
    }

    public HuffmanNode(HuffmanNode left, HuffmanNode right)
    {
        Symbol = '\0'; // Внутренние узлы
        Probability = left.Probability + right.Probability;
        LeftChild = left;
        RightChild = right;
    }

    public bool IsLeaf() => LeftChild == null && RightChild == null;

    public int CompareTo(HuffmanNode other)
    {
        if (other == null) return 1;
        int probabilityComparison = Probability.CompareTo(other.Probability);
        if (probabilityComparison != 0)
        {
            return probabilityComparison;
        }
        if (IsLeaf() && other.IsLeaf())
        {
            return Symbol.CompareTo(other.Symbol);
        }
        return 0;
    }
}

// --- Кодировщик Шеннона-Фано ---
public class ShannonFanoEncoder
{
    public Dictionary<char, string> Codes { get; private set; }
    private Dictionary<string, char> _reverseCodes;
    private List<SymbolInfo> _symbolInfos;

    public ShannonFanoEncoder()
    {
        Codes = new Dictionary<char, string>();
        _reverseCodes = new Dictionary<string, char>();
        _symbolInfos = new List<SymbolInfo>();
    }

    public void BuildCodes(List<SymbolInfo> symbolInfos)
    {
        _symbolInfos = new List<SymbolInfo>(symbolInfos);
        if (!_symbolInfos.Any()) return;

        foreach (var si in _symbolInfos)
        {
            if (si.Frequency == 0) si.Frequency = 1;
        }

        double totalFrequency = _symbolInfos.Sum(s => s.Frequency);
        if (totalFrequency > 0)
        {
            foreach (var si in _symbolInfos)
            {
                si.Probability = si.Frequency / totalFrequency;
            }
        }
        else if (_symbolInfos.Count == 1)
        {
            _symbolInfos.First().Probability = 1.0;
        }

        var sortedSymbols = _symbolInfos.OrderByDescending(s => s.Probability).ThenBy(s => s.Symbol).ToList();

        Console.WriteLine("\nСимволы (вероятность) для Шеннона-Фано:");
        sortedSymbols.ForEach(s => Console.WriteLine($"  '{s.Symbol}': {s.Probability:F5}"));

        Codes.Clear();
        _reverseCodes.Clear();

        AssignCodesRecursive(sortedSymbols, "");

        foreach (var si in _symbolInfos.Where(s => !string.IsNullOrEmpty(s.Code)))
        {
            if (!Codes.ContainsKey(si.Symbol))
            {
                Codes.Add(si.Symbol, si.Code);
            }
            if (!_reverseCodes.ContainsKey(si.Code))
            {
                _reverseCodes.Add(si.Code, si.Symbol);
            }
            else if (_reverseCodes[si.Code] != si.Symbol)
            {
                Console.WriteLine($"  Предупреждение (Ш-Ф): Конфликт кода '{si.Code}'. Уже для '{_reverseCodes[si.Code]}', текущий '{si.Symbol}'.");
            }
        }

        if (sortedSymbols.Count == 1 && string.IsNullOrEmpty(Codes.GetValueOrDefault(sortedSymbols[0].Symbol)))
        {
            string singleSymbolCode = "0";
            if (!Codes.ContainsKey(sortedSymbols[0].Symbol)) Codes.Add(sortedSymbols[0].Symbol, singleSymbolCode);
            if (!_reverseCodes.ContainsKey(singleSymbolCode)) _reverseCodes.Add(singleSymbolCode, sortedSymbols[0].Symbol);

            var originalSymbolInfo = _symbolInfos.FirstOrDefault(si => si.Symbol == sortedSymbols[0].Symbol);
            if (originalSymbolInfo != null) originalSymbolInfo.Code = singleSymbolCode;
        }


        Console.WriteLine("\nИтоговые коды (Шеннон-Фано):");
        foreach (var codeEntry in Codes.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"  '{codeEntry.Key}': {codeEntry.Value}");
        }
    }

    private void AssignCodesRecursive(List<SymbolInfo> symbols, string currentCode)
    {
        if (!symbols.Any()) return;

        Console.WriteLine($"\nРекурсия (Ш-Ф): [{string.Join(", ", symbols.Select(s => $"'{s.Symbol}'({s.Probability:F3})"))}], префикс: '{currentCode}'");

        if (symbols.Count == 1)
        {
            var originalSymbolInfo = _symbolInfos.FirstOrDefault(si => si.Symbol == symbols[0].Symbol);
            if (originalSymbolInfo != null)
            {
                originalSymbolInfo.Code = string.IsNullOrEmpty(currentCode) ? "0" : currentCode;
            }
            Console.WriteLine($"  Лист (Ш-Ф): '{symbols[0].Symbol}' -> {(string.IsNullOrEmpty(currentCode) ? "0" : currentCode)}");
            return;
        }
        if (symbols.Count == 0) return;

        int splitIndex = FindSplitPoint(symbols);
        Console.WriteLine($"  Разделение по индексу (Ш-Ф): {splitIndex}");

        List<SymbolInfo> group1 = new List<SymbolInfo>();
        List<SymbolInfo> group2 = new List<SymbolInfo>();

        for (int i = 0; i < symbols.Count; i++)
        {
            if (i <= splitIndex) group1.Add(symbols[i]);
            else group2.Add(symbols[i]);
        }

        if (!group1.Any() || !group2.Any())
        {
            Console.WriteLine($"  Предупреждение (Ш-Ф): Неэффективное разделение для [{string.Join(", ", symbols.Select(s => $"'{s.Symbol}'"))}]. Аварийное присвоение.");
            if (symbols.Count > 0)
            {
                for (int i = 0; i < symbols.Count; ++i)
                {
                    string emergencyCode = currentCode + (i < symbols.Count / 2 ? "0" : "1");
                    if (symbols.Count == 1) emergencyCode = currentCode;

                    var originalSymbolInfo = _symbolInfos.FirstOrDefault(si => si.Symbol == symbols[i].Symbol);
                    if (originalSymbolInfo != null) originalSymbolInfo.Code = emergencyCode;
                    Console.WriteLine($"    Аварийно (Ш-Ф): '{symbols[i].Symbol}' -> '{emergencyCode}'");
                }
            }
            return;
        }

        Console.WriteLine($"  Группа 1 (Ш-Ф, код + '0'): [{string.Join(", ", group1.Select(s => $"'{s.Symbol}'"))}]");
        AssignCodesRecursive(group1, currentCode + "0");

        Console.WriteLine($"  Группа 2 (Ш-Ф, код + '1'): [{string.Join(", ", group2.Select(s => $"'{s.Symbol}'"))}]");
        AssignCodesRecursive(group2, currentCode + "1");
    }

    private int FindSplitPoint(List<SymbolInfo> symbols)
    {
        if (symbols.Count <= 1) return 0;
        double totalProbability = symbols.Sum(s => s.Probability);
        double minDifference = double.MaxValue;
        int bestSplitIndex = 0;
        for (int i = 0; i < symbols.Count - 1; i++)
        {
            double sumGroup1 = 0;
            for (int j = 0; j <= i; j++) sumGroup1 += symbols[j].Probability;
            double difference = Math.Abs(sumGroup1 - (totalProbability - sumGroup1));
            if (difference < minDifference)
            {
                minDifference = difference;
                bestSplitIndex = i;
            }
        }
        return bestSplitIndex;
    }

    public string Encode(string source)
    {
        if (Codes == null || !Codes.Any())
        {
            Console.WriteLine("Ошибка кодирования (Ш-Ф): Таблица кодов пуста.");
            return null;
        }
        StringBuilder encodedSource = new StringBuilder();
        Console.WriteLine($"\nКодирование (Ш-Ф): \"{source}\"");
        foreach (char c in source)
        {
            if (Codes.TryGetValue(c, out string code))
            {
                encodedSource.Append(code);
                Console.WriteLine($"  '{c}' -> {code}");
            }
            else
            {
                Console.WriteLine($"  Предупреждение (Ш-Ф): Символ '{c}' не найден. Пропуск.");
            }
        }
        Console.WriteLine($"  Результат кодирования (Ш-Ф): {encodedSource.ToString()}");
        return encodedSource.ToString();
    }

    public string Decode(string encodedSource)
    {
        if (_reverseCodes == null || !_reverseCodes.Any())
        {
            Console.WriteLine("Ошибка декодирования (Ш-Ф): Обратная таблица кодов пуста.");
            return null;
        }
        StringBuilder decodedSource = new StringBuilder();
        StringBuilder currentCode = new StringBuilder();
        Console.WriteLine($"\nДекодирование (Ш-Ф): {encodedSource}");
        foreach (char bit in encodedSource)
        {
            currentCode.Append(bit);
            Console.Write($"  Текущий код: {currentCode}");
            if (_reverseCodes.TryGetValue(currentCode.ToString(), out char decodedChar))
            {
                decodedSource.Append(decodedChar);
                Console.WriteLine($" -> Символ: '{decodedChar}'");
                currentCode.Clear();
            }
            else
            {
                Console.WriteLine(" (поиск)");
            }
        }
        if (currentCode.Length > 0)
        {
            Console.WriteLine($"  Предупреждение (Ш-Ф): Остались необработанные биты: {currentCode}");
        }
        Console.WriteLine($"  Результат декодирования (Ш-Ф): \"{decodedSource.ToString()}\"");
        return decodedSource.ToString();
    }

    public static List<SymbolInfo> GetStaticFrequencies()
    {
        var frequencies = new List<SymbolInfo>
        {
            new SymbolInfo('а', 1120), new SymbolInfo('ә', 110), new SymbolInfo('б', 186),
            new SymbolInfo('в', 4), new SymbolInfo('г', 78), new SymbolInfo('ғ', 118),
            new SymbolInfo('д', 380), new SymbolInfo('е', 670), new SymbolInfo('ё', 1),
            new SymbolInfo('ж', 120), new SymbolInfo('з', 160), new SymbolInfo('и', 136),
            new SymbolInfo('й', 84), new SymbolInfo('к', 188), new SymbolInfo('қ', 321),
            new SymbolInfo('л', 438), new SymbolInfo('м', 298), new SymbolInfo('н', 580),
            new SymbolInfo('ң', 104), new SymbolInfo('о', 206), new SymbolInfo('ө', 70),
            new SymbolInfo('п', 96), new SymbolInfo('р', 496), new SymbolInfo('с', 300),
            new SymbolInfo('т', 560), new SymbolInfo('у', 126), new SymbolInfo('ұ', 72),
            new SymbolInfo('ү', 74), new SymbolInfo('ф', 6), new SymbolInfo('х', 32),
            new SymbolInfo('һ', 1), new SymbolInfo('ц', 2), new SymbolInfo('ч', 1),
            new SymbolInfo('ш', 64), new SymbolInfo('щ', 1), new SymbolInfo('ъ', 1),
            new SymbolInfo('ы', 518), new SymbolInfo('і', 372), new SymbolInfo('ь', 1),
            new SymbolInfo('э', 20), new SymbolInfo('ю', 1), new SymbolInfo('я', 32)
        };
        return frequencies.Where(s => s.Frequency > 0).ToList();
    }

    public static List<SymbolInfo> GetDynamicFrequencies(string source)
    {
        var charCounts = new Dictionary<char, int>();
        foreach (char c in source)
        {
            if (charCounts.ContainsKey(c)) charCounts[c]++;
            else charCounts[c] = 1;
        }
        return charCounts.Select(kvp => new SymbolInfo(kvp.Key, kvp.Value)).ToList();
    }
}

// --- Кодировщик Хаффмана ---
public class HuffmanEncoder
{
    public Dictionary<char, string> Codes { get; private set; }
    private Dictionary<string, char> _reverseCodes;
    private List<SymbolInfo> _symbolInfosStorage;

    public HuffmanEncoder()
    {
        Codes = new Dictionary<char, string>();
        _reverseCodes = new Dictionary<string, char>();
        _symbolInfosStorage = new List<SymbolInfo>();
    }

    public void BuildCodes(List<SymbolInfo> symbolInfosInput)
    {
        _symbolInfosStorage = symbolInfosInput.Select(si => new SymbolInfo(si.Symbol, si.Frequency)).ToList();

        if (!_symbolInfosStorage.Any()) return;

        foreach (var si in _symbolInfosStorage)
        {
            if (si.Frequency == 0) si.Frequency = 1;
        }
        double totalFrequency = _symbolInfosStorage.Sum(s => s.Frequency);
        if (totalFrequency > 0)
        {
            foreach (var si in _symbolInfosStorage)
            {
                si.Probability = si.Frequency / totalFrequency;
            }
        }
        else if (_symbolInfosStorage.Count == 1)
        {
            _symbolInfosStorage.First().Probability = 1.0;
        }

        Console.WriteLine("\nСимволы (вероятность) для Хаффмана:");
        _symbolInfosStorage.OrderBy(s => s.Probability).ThenBy(s => s.Symbol) // Сортируем для начального отображения как в Хаффмане
                           .ToList().ForEach(s => Console.WriteLine($"  '{s.Symbol}': {s.Probability:F5} (Частота: {s.Frequency})"));

        Codes.Clear();
        _reverseCodes.Clear();

        // 1. Создание листовых узлов
        var nodes = _symbolInfosStorage.Select(si => new HuffmanNode(si.Symbol, si.Probability)).ToList();
        if (nodes.Count == 0) return;

        Console.WriteLine("\n--- Построение дерева Хаффмана ---");

        // 2. Особый случай: один символ в алфавите
        if (nodes.Count == 1)
        {
            var singleSymbolNode = nodes.First();
            string code = "0";
            Console.WriteLine($"  Единственный узел: '{singleSymbolNode.Symbol}'({singleSymbolNode.Probability:F3}). Присваиваем код '{code}'.");
            Codes.Add(singleSymbolNode.Symbol, code);
            _reverseCodes.Add(code, singleSymbolNode.Symbol);
            UpdateSymbolInfoCodeInStorage(singleSymbolNode.Symbol, code);
        }
        else
        {
            // 3. Построение дерева Хаффмана
            int step = 1;
            // Используем PriorityQueue для эффективности, но для простоты и совместимости с List.Sort() оставим List
            // List<HuffmanNode> priorityQueue = new List<HuffmanNode>(nodes); 

            while (nodes.Count > 1)
            {
                nodes.Sort(); // Сортируем узлы по вероятности (и символу для стабильности)

                HuffmanNode left = nodes[0];
                HuffmanNode right = nodes[1];

                Console.WriteLine($"\n  Шаг {step++}:");
                Console.WriteLine($"    Выбираем два узла с наименьшей вероятностью:");
                Console.WriteLine($"      Левый:  {(left.IsLeaf() ? $"'{left.Symbol}'" : "Внутр. узел")} ({left.Probability:F5})");
                Console.WriteLine($"      Правый: {(right.IsLeaf() ? $"'{right.Symbol}'" : "Внутр. узел")} ({right.Probability:F5})");

                HuffmanNode parent = new HuffmanNode(left, right);
                Console.WriteLine($"    Создаем родительский узел с вероятностью: {parent.Probability:F5}");

                nodes.Remove(left);
                nodes.Remove(right);
                nodes.Add(parent);

                if (nodes.Count > 1) // Выводим текущее состояние очереди узлов, если это не последний шаг
                {
                    Console.WriteLine("    Текущие узлы в очереди (отсортированы):");
                    foreach (var node in nodes.OrderBy(n => n.Probability)) // Показываем в отсортированном виде
                    {
                        Console.WriteLine($"      - {(node.IsLeaf() ? $"'{node.Symbol}'" : "Внутр. узел")} ({node.Probability:F5})");
                    }
                }
            }

            HuffmanNode root = nodes.First();
            Console.WriteLine("\n  Дерево Хаффмана построено. Корень имеет вероятность: " + root.Probability.ToString("F5"));
            Console.WriteLine("\n--- Генерация кодов из дерева Хаффмана ---");
            // 4. Генерация кодов из дерева
            GenerateCodesRecursive(root, "");
        }

        Console.WriteLine("\nИтоговые коды (Хаффман):");
        foreach (var codeEntry in Codes.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"  '{codeEntry.Key}': {codeEntry.Value}");
        }
    }

    private void GenerateCodesRecursive(HuffmanNode node, string currentCode)
    {
        if (node == null) return;

        string nodeDescription = node.IsLeaf() ? $"Листовой узел '{node.Symbol}' ({node.Probability:F5})" : $"Внутренний узел ({node.Probability:F5})";
        Console.WriteLine($"  Обход дерева: {nodeDescription}, Текущий префикс: '{currentCode}'");

        if (node.IsLeaf())
        {
            // Для единственного символа в алфавите, код должен быть "0"
            string finalCode = (_symbolInfosStorage.Count == 1 && string.IsNullOrEmpty(currentCode)) ? "0" : currentCode;

            Codes[node.Symbol] = finalCode;
            _reverseCodes[finalCode] = node.Symbol;
            UpdateSymbolInfoCodeInStorage(node.Symbol, finalCode);
            Console.WriteLine($"    └─ Символу '{node.Symbol}' присвоен код: '{finalCode}'");
            return;
        }

        if (node.LeftChild != null)
        {
            Console.WriteLine($"    ├─ Переход к левому потомку (добавляем '0' к префиксу)");
            GenerateCodesRecursive(node.LeftChild, currentCode + "0");
        }
        if (node.RightChild != null)
        {
            Console.WriteLine($"    └─ Переход к правому потомку (добавляем '1' к префиксу)");
            GenerateCodesRecursive(node.RightChild, currentCode + "1");
        }
    }

    private void UpdateSymbolInfoCodeInStorage(char symbol, string code)
    {
        var symbolInfo = _symbolInfosStorage.FirstOrDefault(si => si.Symbol == symbol);
        if (symbolInfo != null)
        {
            symbolInfo.Code = code;
        }
    }

    // Методы Encode и Decode остаются идентичными
    public string Encode(string source)
    {
        if (Codes == null || !Codes.Any())
        {
            Console.WriteLine("Ошибка кодирования (Хаффман): Таблица кодов пуста.");
            return null;
        }
        StringBuilder encodedSource = new StringBuilder();
        Console.WriteLine($"\nКодирование (Хаффман): \"{source}\"");
        foreach (char c in source)
        {
            if (Codes.TryGetValue(c, out string code))
            {
                encodedSource.Append(code);
                Console.WriteLine($"  '{c}' -> {code}");
            }
            else
            {
                Console.WriteLine($"  Предупреждение (Хаффман): Символ '{c}' не найден. Пропуск.");
            }
        }
        Console.WriteLine($"  Результат кодирования (Хаффман): {encodedSource.ToString()}");
        return encodedSource.ToString();
    }

    public string Decode(string encodedSource)
    {
        if (_reverseCodes == null || !_reverseCodes.Any())
        {
            Console.WriteLine("Ошибка декодирования (Хаффман): Обратная таблица кодов пуста.");
            return null;
        }
        StringBuilder decodedSource = new StringBuilder();
        StringBuilder currentCode = new StringBuilder();
        Console.WriteLine($"\nДекодирование (Хаффман): {encodedSource}");
        foreach (char bit in encodedSource)
        {
            currentCode.Append(bit);
            Console.Write($"  Текущий код: {currentCode}");
            if (_reverseCodes.TryGetValue(currentCode.ToString(), out char decodedChar))
            {
                decodedSource.Append(decodedChar);
                Console.WriteLine($" -> Символ: '{decodedChar}'");
                currentCode.Clear();
            }
            else
            {
                Console.WriteLine(" (поиск)");
            }
        }
        if (currentCode.Length > 0)
        {
            Console.WriteLine($"  Предупреждение (Хаффман): Остались необработанные биты: {currentCode}");
        }
        Console.WriteLine($"  Результат декодирования (Хаффман): \"{decodedSource.ToString()}\"");
        return decodedSource.ToString();
    }
}
// --- Основная программа ---
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string inputText = "лопатнюк полина";
        Console.WriteLine($"Оригинал: \"{inputText}\" (Длина: {inputText.Length})");

        string processedInputText = inputText.Replace(" ", "").ToLower();
        Console.WriteLine($"Обработанный: \"{processedInputText}\"");

        Console.WriteLine("\n================================================");
        Console.WriteLine("КОДИРОВАНИЕ МЕТОДОМ ШЕННОНА-ФАНО");
        Console.WriteLine("================================================");

        // --- Режим A: Шеннон-Фано (Статическая вероятность) ---
        Console.WriteLine("\nРежим A: Шеннон-Фано (Статическая вероятность)");
        ShannonFanoEncoder staticShannonEncoder = new ShannonFanoEncoder();
        List<SymbolInfo> staticAlphabetData = ShannonFanoEncoder.GetStaticFrequencies();

        var relevantStaticSymbolsShannon = new List<SymbolInfo>();
        var uniqueCharsInInput = processedInputText.Distinct().ToList();

        foreach (char c in uniqueCharsInInput)
        {
            var staticInfo = staticAlphabetData.FirstOrDefault(s => s.Symbol == c);
            if (staticInfo != null)
            {
                relevantStaticSymbolsShannon.Add(new SymbolInfo(c, staticInfo.Frequency));
            }
            else
            {
                Console.WriteLine($"  Инфо (стат. Шеннон): Символ '{c}' не в статическом справочнике.");
            }
        }

        if (relevantStaticSymbolsShannon.Any())
        {
            staticShannonEncoder.BuildCodes(relevantStaticSymbolsShannon); // Передаем копию, т.к. BuildCodes ее модифицирует
            string staticShannonEncoded = staticShannonEncoder.Encode(processedInputText);
            if (staticShannonEncoded != null)
            {
                string staticShannonDecoded = staticShannonEncoder.Decode(staticShannonEncoded);
                Console.WriteLine("\nРезультаты (Шеннон-Фано, Статический режим):");
                Console.WriteLine($"  Оригинал (обр.): {processedInputText}");
                Console.WriteLine($"  Закодировано:    {staticShannonEncoded} ({staticShannonEncoded.Length} бит)");
                Console.WriteLine($"  Декодировано:    {staticShannonDecoded}");
                CalculateEfficiency(processedInputText, staticShannonEncoded, "Шеннон-Фано Статический");
            }
        }
        else
        {
            Console.WriteLine("Нет символов для статического кодирования Шеннона-Фано.");
        }

        // --- Режим B: Шеннон-Фано (Динамическая вероятность) ---
        Console.WriteLine("\nРежим B: Шеннон-Фано (Динамическая вероятность)");
        ShannonFanoEncoder dynamicShannonEncoder = new ShannonFanoEncoder();
        List<SymbolInfo> dynamicSymbolDataShannon = ShannonFanoEncoder.GetDynamicFrequencies(processedInputText);

        dynamicShannonEncoder.BuildCodes(dynamicSymbolDataShannon);
        string dynamicShannonEncoded = dynamicShannonEncoder.Encode(processedInputText);
        if (dynamicShannonEncoded != null)
        {
            string dynamicShannonDecoded = dynamicShannonEncoder.Decode(dynamicShannonEncoded);
            Console.WriteLine("\nРезультаты (Шеннон-Фано, Динамический режим):");
            Console.WriteLine($"  Оригинал (обр.): {processedInputText}");
            Console.WriteLine($"  Закодировано:    {dynamicShannonEncoded} ({dynamicShannonEncoded.Length} бит)");
            Console.WriteLine($"  Декодировано:    {dynamicShannonDecoded}");
            CalculateEfficiency(processedInputText, dynamicShannonEncoded, "Шеннон-Фано Динамический");
        }

        Console.WriteLine("\n================================================");
        Console.WriteLine("КОДИРОВАНИЕ МЕТОДОМ ХАФФМАНА");
        Console.WriteLine("================================================");

        // --- Режим C: Хаффман (Статическая вероятность) ---
        Console.WriteLine("\nРежим C: Хаффман (Статическая вероятность)");
        HuffmanEncoder staticHuffmanEncoder = new HuffmanEncoder();
        var relevantStaticSymbolsHuffman = new List<SymbolInfo>();
        foreach (char c in uniqueCharsInInput)
        {
            var staticInfo = staticAlphabetData.FirstOrDefault(s => s.Symbol == c);
            if (staticInfo != null)
            {
                relevantStaticSymbolsHuffman.Add(new SymbolInfo(c, staticInfo.Frequency));
            }
        }

        if (relevantStaticSymbolsHuffman.Any())
        {
            staticHuffmanEncoder.BuildCodes(relevantStaticSymbolsHuffman);
            string staticHuffmanEncoded = staticHuffmanEncoder.Encode(processedInputText);
            if (staticHuffmanEncoded != null)
            {
                string staticHuffmanDecoded = staticHuffmanEncoder.Decode(staticHuffmanEncoded);
                Console.WriteLine("\nРезультаты (Хаффман, Статический режим):");
                Console.WriteLine($"  Оригинал (обр.): {processedInputText}");
                Console.WriteLine($"  Закодировано:    {staticHuffmanEncoded} ({staticHuffmanEncoded.Length} бит)");
                Console.WriteLine($"  Декодировано:    {staticHuffmanDecoded}");
                CalculateEfficiency(processedInputText, staticHuffmanEncoded, "Хаффман Статический");
            }
        }
        else
        {
            Console.WriteLine("Нет символов для статического кодирования Хаффмана.");
        }

        // --- Режим D: Хаффман (Динамическая вероятность) ---
        Console.WriteLine("\nРежим D: Хаффман (Динамическая вероятность)");
        HuffmanEncoder dynamicHuffmanEncoder = new HuffmanEncoder();
        List<SymbolInfo> dynamicSymbolDataHuffman = ShannonFanoEncoder.GetDynamicFrequencies(processedInputText);

        dynamicHuffmanEncoder.BuildCodes(dynamicSymbolDataHuffman);
        string dynamicHuffmanEncoded = dynamicHuffmanEncoder.Encode(processedInputText);
        if (dynamicHuffmanEncoded != null)
        {
            string dynamicHuffmanDecoded = dynamicHuffmanEncoder.Decode(dynamicHuffmanEncoded);
            Console.WriteLine("\nРезультаты (Хаффман, Динамический режим):");
            Console.WriteLine($"  Оригинал (обр.): {processedInputText}");
            Console.WriteLine($"  Закодировано:    {dynamicHuffmanEncoded} ({dynamicHuffmanEncoded.Length} бит)");
            Console.WriteLine($"  Декодировано:    {dynamicHuffmanDecoded}");
            CalculateEfficiency(processedInputText, dynamicHuffmanEncoded, "Хаффман Динамический");
        }
    }

    static void CalculateEfficiency(string originalText, string encodedText, string modeName)
    {
        if (string.IsNullOrEmpty(originalText) || encodedText == null)
        {
            Console.WriteLine($"Невозможно рассчитать эффективность ({modeName}): нет данных.");
            return;
        }

        int originalBits = originalText.Length * 8;
        int encodedBits = encodedText.Length;

        Console.WriteLine($"\nЭффективность ({modeName}):");
        Console.WriteLine($"  Исходный (\"{originalText}\", {originalText.Length} симв. * 8 бит): {originalBits} бит");
        Console.WriteLine($"  Закодированный: {encodedBits} бит");

        if (originalBits > 0 && encodedBits > 0)
        {
            double compressionRatio = (double)encodedBits / originalBits;
            double spaceSaving = 1.0 - compressionRatio;
            Console.WriteLine($"  Коэфф. сжатия: {compressionRatio:F4} ({compressionRatio:P2})");
            Console.WriteLine($"  Экономия места: {spaceSaving:P2}");
        }
        else
        {
            Console.WriteLine("  Расчет коэфф. невозможен (0 размер).");
        }
    }
}