using Microsoft.VisualBasic;
using System;

namespace Lab4
{
    class Program
    {
        static void Main()
        {
            string inputText = FileReader.ReadTextFromFile("input.txt");
            int[] Xk = FileReader.TextToBinaryArray(inputText);

            Console.WriteLine("\n[Исходное сообщение]");
            Console.WriteLine($"Текст: {inputText}");
            Console.WriteLine($"Бинарный вид: {string.Join("", Xk)}");
            Console.WriteLine($"Длина сообщения (k): {Xk.Length} бит");

            Console.WriteLine("\n" + new string('=', 50));

            int[,]? H = Hemming.GenerateMatrix(Xk.Length);

            if (H == null)
            {
                Console.WriteLine("Ошибка генерации матрицы H");
                return;
            }

            Console.WriteLine("\n[Проверочная матрица H]");
            Hemming.PrintMatrix(H);

            Console.WriteLine("\n" + new string('=', 50));

            int[] Xr;
            int[] Xn = Hemming.SolveHamming(H, Xk, out Xr);

            Console.WriteLine("\n[Результат кодирования]");
            Console.WriteLine($"Информационное слово (Xk): {string.Join("", Xk)}");
            Console.WriteLine($"Избыточные биты (Xr):    {string.Join("", Xr)}");
            Console.WriteLine($"Кодовое слово (Xn):      {string.Join("", Xn)}");

            // Создаём три варианта принятого слова
            int[] Yn1 = new int[Xn.Length];
            int[] Yn2 = new int[Xn.Length];
            int[] Yn3 = new int[Xn.Length];
            Xn.CopyTo(Yn1, 0);
            Xn.CopyTo(Yn2, 0);
            Xn.CopyTo(Yn3, 0);

            // Вносим ошибки
            Hemming.ChangeValue(Yn2);  // 1 ошибка
            Hemming.ChangeValue(Yn3);  // 2 ошибки
            Hemming.ChangeValue(Yn3);

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("\n[Тестирование декодирования]");

            Console.WriteLine("\n1. Без ошибок:");
            Console.WriteLine(new string('-', 30));
            Hemming.CheckYn(Yn1, Xk.Length, H);

            Console.WriteLine("\n2. С одной ошибкой:");
            Console.WriteLine(new string('-', 30));
            Hemming.CheckYn(Yn2, Xk.Length, H);

            Console.WriteLine("\n3. С двумя ошибками:");
            Console.WriteLine(new string('-', 30));
            Hemming.CheckYn(Yn3, Xk.Length, H);

            Console.WriteLine("\n" + new string('=', 50));
        }
    }
}