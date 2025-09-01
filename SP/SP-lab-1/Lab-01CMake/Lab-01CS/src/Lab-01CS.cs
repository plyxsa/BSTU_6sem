using System;

class Program
{
    static void Main()
    {
        Console.Write("Enter number x: ");
        double x = double.Parse(Console.ReadLine());

        double x2 = x * x;
        double x4 = x2 * x2;
        double x8 = x4 * x4;
        double x16 = x8 * x8;
        double x17 = x16 * x;
        double x5 = x4 * x;

        Console.WriteLine($"x^2  = {x2}");
        Console.WriteLine($"x^5  = {x5}");
        Console.WriteLine($"x^17 = {x17}");
    }
}