namespace Lab3
{
    class XOR
    {
        public static byte[] XORBuffers(byte[] a, byte[] b)
        {
            byte[] result = new byte[Math.Max(a.Length, b.Length)];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)((i < a.Length ? a[i] : 0) ^ (i < b.Length ? b[i] : 0));
            }

            return result;
        }

        public static string ToBinary(byte[] data)
        {
            if (data == null || data.Length == 0) return string.Empty;
            return string.Join(" ", data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }
    }
}
