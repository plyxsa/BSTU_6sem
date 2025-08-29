using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    class Program
    {
        public static void Main()
        {
            // Раздел для задания а
            Console.WriteLine("\n==================== ЗАДАНИЕ A ====================\n");

            string danishText = FileReader.ReadTextFromFile("danish.txt");
            string kazakhText = FileReader.ReadTextFromFile("kazakh.txt");
            string belarusText = FileReader.ReadTextFromFile("belarusText.txt");

            char[] DanishAlphabet = "abcdefghijklmnopqrstuvwxyzæøå".ToCharArray();
            char[] KazakhAlphabet = "аәбвгғдеёжзийкқлмнңоөпрстуұүфхһцчшщъыіьэюя".ToCharArray();
            char[] BelarusAlphabet = "абвгдзеёжзійклмнопрстуўфхцчджшщъыьэюя".ToCharArray();

            // Вывод для датского языка
            Console.WriteLine($"1. Датский язык:");
            Console.WriteLine("----------------------------------------------------");
            double danishEntropy = EntropyCalculator.CalculateEntropy(danishText, DanishAlphabet, "frequency_data_danish.xlsx");
            Console.WriteLine($"Энтропия для датского текста: {danishEntropy:F4}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------\n");

            // Вывод для казахского языка
            Console.WriteLine($"2. Казахский язык:");
            Console.WriteLine("----------------------------------------------------");
            double kazakhEntropy = EntropyCalculator.CalculateEntropy(kazakhText, KazakhAlphabet, "frequency_data_kazakh.xlsx");
            Console.WriteLine($"Энтропия для казахского текста: {kazakhEntropy:F4}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------\n");

            // Вывод для казахского языка
            //Console.WriteLine($"Белорусский язык:");
            //Console.WriteLine("----------------------------------------------------");
            //double BelarusEntropy = EntropyCalculator.CalculateEntropy(belarusText, BelarusAlphabet, "frequency_data_belarus.xlsx");
            //Console.WriteLine($"Энтропия для белоруссого текста текста: {BelarusEntropy:F4}");
            //Console.ResetColor();
            //Console.WriteLine("----------------------------------------------------\n");


            // Раздел для задания б
            Console.WriteLine("\n==================== ЗАДАНИЕ B ====================\n");

            string danishTextBinary = FileReader.ReadTextFromFile("danishbinary.txt");
            string kazakhTextBinary = FileReader.ReadTextFromFile("kazakhbinary.txt");

            char[] BinaryAlphabet = "01".ToCharArray();

            // Вывод для датского бинарного текста
            Console.WriteLine($"1. Датский язык (бинарный):");
            Console.WriteLine("----------------------------------------------------");
            double danishEntropyBinary = EntropyCalculator.CalculateEntropy(danishTextBinary, BinaryAlphabet, "frequency_data_danish_binary.xlsx");
            Console.WriteLine($"Энтропия для датского бинарного текста: {danishEntropyBinary:F4}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------\n");

            // Вывод для казахского бинарного текста
            Console.WriteLine($"2. Казахский язык (бинарный):");
            Console.WriteLine("----------------------------------------------------");
            double kazakhEntropyBinary = EntropyCalculator.CalculateEntropy(kazakhTextBinary, BinaryAlphabet, "frequency_data_kazakh_binary.xlsx");
            Console.WriteLine($"Энтропия для казахского бинарного текста: {kazakhEntropyBinary:F4}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------\n");

            // Раздел для задания в
            Console.WriteLine("\n==================== ЗАДАНИЕ В ====================\n");

            string danishFIO = "Lopatniuk Polina Vecheslavovna";
            string kazakhFIO = "Лопатнюк Полина Вечеславовна";

            string danishFIOBinary = "01010110 01101111 01100100 01100011 01101000 01111001 01110100 01110011 00100000 01000001 01101110 01100001 01110011 01110100 01100001 01110011 01101001 01111001 01100001 00100000 01010110 01101001 01110100 01100001 01101100 01101001 01100101 01110110 01101110 01100001";
            string kazakhFIOBinary = "11010000 10010010 11010000 10111110 11010000 10110100 11010001 10000111 11010000 10111000 11010001 10000110 00100000 11010000 10010000 11010000 10111101 11010000 10110000 11010001 10000001 11010001 10000010 11010000 10110000 11010001 10000001 11010000 10111000 11010001 10001111 00100000 11010000 10010010 11010000 10111000 11010001 10000010 11010000 10110000 11010000 10111011 11010000 10111000 11010000 10110101 11010000 10110010 11010000 10111101 11010000 10110000";

            double danishInformationAmount = EntropyCalculator.CalculateInformationAmount(danishFIO, danishEntropy, DanishAlphabet);
            double kazakhInformationAmount = EntropyCalculator.CalculateInformationAmount(kazakhFIO, kazakhEntropy, KazakhAlphabet);

            double danishInformationAmountBinary = EntropyCalculator.CalculateInformationAmount(danishFIOBinary, danishEntropyBinary, BinaryAlphabet);
            double kazakhInformationAmountBinary = EntropyCalculator.CalculateInformationAmount(danishFIOBinary, danishEntropyBinary, BinaryAlphabet);

            // Вывод количества информации в ФИО
            Console.WriteLine($"3. Количество информации в ФИО:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Количество информации в ФИО на датском: {danishInformationAmount:F4} бит");
            Console.WriteLine($"Количество информации в ФИО на казахском: {kazakhInformationAmount:F4} бит");

            Console.WriteLine($"Количество информации в ФИО на датском (бинарный): {danishInformationAmountBinary:F4}  бит");
            Console.WriteLine($"Количество информации в ФИО на казахском (бинарный): {kazakhInformationAmountBinary:F4}  бит");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------\n");

            // Раздел для задания г
            Console.WriteLine("\n==================== ЗАДАНИЕ Г ====================\n");

            double EffectiveEntropy = 1 - EntropyCalculator.EffectiveEntropy(0.1);
            Console.WriteLine($"4. Количество информации с ошибкой 0.1:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Количество информации в ФИО на датском с ошибкой 0.1: {danishInformationAmountBinary * EffectiveEntropy:F4} бит");
            Console.WriteLine($"Количество информации в ФИО на казахском с ошибкой 0.1: {kazakhInformationAmountBinary * EffectiveEntropy:F4} бит");

            EffectiveEntropy = 1 - EntropyCalculator.EffectiveEntropy(0.5);
            Console.WriteLine($"5. Количество информации с ошибкой 0.5:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Количество информации в ФИО на датском с ошибкой 0.5: {danishInformationAmountBinary * EffectiveEntropy:F4} бит");
            Console.WriteLine($"Количество информации в ФИО на казахском с ошибкой 0.5: {kazakhInformationAmountBinary * EffectiveEntropy:F4} бит");

            EffectiveEntropy = 1 - EntropyCalculator.EffectiveEntropy(1.0);
            Console.WriteLine($"6. Количество информации с ошибкой 1.0:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"Количество информации в ФИО на датском с ошибкой 1.0: {danishInformationAmountBinary * EffectiveEntropy:F4} бит");
            Console.WriteLine($"Количество информации в ФИО на казахском с ошибкой 1.0: {kazakhInformationAmountBinary * EffectiveEntropy:F4} бит");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------\n");
        }
    }
}
