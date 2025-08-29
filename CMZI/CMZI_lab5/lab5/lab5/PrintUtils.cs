using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab5
{
    public static class PrintUtils // Вспомогательный класс для вывода
    {
        public static string FormatBinaryVector(IEnumerable<int> vector)
        {
            return string.Join("", vector);
        }

        public static void PrintVector(IEnumerable<int> vector, string label)
        {
            Console.WriteLine($"{label}: [{FormatBinaryVector(vector)}]");
        }

        public static void PrintMatrix(int[,] matrix, string label)
        {
            Console.WriteLine($"{label}:");
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                Console.Write("  [");
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(matrix[i, j]);
                    if (j < cols - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }
        }

        public static void PrintMatrix(int[,,] matrix, string label)
        {
            Console.WriteLine($"{label}:");
            int dim1 = matrix.GetLength(0); // k1
            int dim2 = matrix.GetLength(1); // k2
            int dim3 = matrix.GetLength(2); // z
            for (int k = 0; k < dim3; k++)
            {
                Console.WriteLine($"  Уровень {k}:");
                for (int i = 0; i < dim1; i++)
                {
                    Console.Write("    [");
                    for (int j = 0; j < dim2; j++)
                    {
                        Console.Write(matrix[i, j, k]);
                        if (j < dim2 - 1) Console.Write(", ");
                    }
                    Console.WriteLine("]");
                }
            }
        }
    }
}
