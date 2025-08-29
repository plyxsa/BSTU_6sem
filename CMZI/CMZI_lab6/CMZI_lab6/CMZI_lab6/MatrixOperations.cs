namespace CMZI_lab6
{
    public static class MatrixOperations
    {
        public static int[] MultiplyVectorByMatrix(int[] vector, int[,] matrix)
        {
            int[] result = new int[matrix.GetLength(1)];
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                int sum = 0;
                for (int i = 0; i < vector.Length; i++)
                    sum ^= vector[i] * matrix[i, j];
                result[j] = sum % 2; // Гарантируем, что результат будет 0 или 1
            }
            return result;
        }
    }
}
