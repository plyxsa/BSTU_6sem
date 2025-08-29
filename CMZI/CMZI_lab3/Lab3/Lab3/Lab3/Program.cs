using System.Text;

namespace Lab3
{
    class Program
    {
        public static void Main()
        {
            string danishText = FileReader.ReadTextFromFile("danish.txt");
            string base64Text = FileReader.ReadTextFromFile("encoded_danish.txt");

            char[] DanishAlphabet = "abcdefghijklmnopqrstuvwxyzæøå".ToCharArray();
            char[] Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".ToCharArray();

            Console.WriteLine("\n------- Энтропия датского текста -------");
            double danishEntropy = EntropyCalculator.CalculateEntropy(danishText, DanishAlphabet);
            double danishEntropyHartly = EntropyCalculator.CalculateEntropyHartly(DanishAlphabet);
            Console.WriteLine($"Шеннон: {danishEntropy:F4}, Хартли: {danishEntropyHartly:F4}");
            Console.WriteLine($"Избыточность: {EntropyCalculator.AlphabetRedundancy(danishEntropy, danishEntropyHartly):F4}%");
            Console.WriteLine("----------------------------------------\n");

            Console.WriteLine("------- Энтропия base64-текста -------");
            double base64Entropy = EntropyCalculator.CalculateEntropy(base64Text, Base64Alphabet);
            double base64EntropyHartly = EntropyCalculator.CalculateEntropyHartly(Base64Alphabet);
            Console.WriteLine($"Шеннон: {base64Entropy:F4}, Хартли: {base64EntropyHartly:F4}");
            Console.WriteLine($"Избыточность: {EntropyCalculator.AlphabetRedundancy(base64Entropy, base64EntropyHartly):F4}%");
            Console.WriteLine("----------------------------------------\n");

            string surname = "Lopatniuk";
            string firstname = "Polina";

            byte[] surnameASCII = Encoding.ASCII.GetBytes(surname);
            byte[] firstnameASCII = Encoding.ASCII.GetBytes(firstname);

            string surnameBase64 = Convert.ToBase64String(surnameASCII);
            string firstnameBase64 = Convert.ToBase64String(firstnameASCII);

            byte[] surnameBase64Bytes = Convert.FromBase64String(surnameBase64);
            byte[] firstnameBase64Bytes = Convert.FromBase64String(firstnameBase64);

            byte[] resultASCII = XOR.XORBuffers(surnameASCII, firstnameASCII);
            byte[] resultBase64 = XOR.XORBuffers(surnameBase64Bytes, firstnameBase64Bytes);

            byte[] resultASCIIReversed = XOR.XORBuffers(resultASCII, firstnameASCII);
            byte[] resultBase64Reversed = XOR.XORBuffers(resultBase64, firstnameBase64Bytes);

            Console.WriteLine("------- ASCII Кодирование -------");
            Console.WriteLine($"{surname}: \t {string.Join(" ", surnameASCII)}");
            Console.WriteLine($"{firstname}: \t {string.Join(" ", firstnameASCII)}");
            Console.WriteLine($"{surname} (2): \t {XOR.ToBinary(surnameASCII)}");
            Console.WriteLine($"{firstname} (2): \t {XOR.ToBinary(firstnameASCII)}");
            Console.WriteLine($"XOR (2): \t {XOR.ToBinary(resultASCII)}");
            Console.WriteLine($"XOR: \t\t {string.Join(" ", resultASCII)}");
            Console.WriteLine($"Re-XOR (2): \t {XOR.ToBinary(resultASCIIReversed)}");
            Console.WriteLine($"Re-XOR: \t {string.Join(" ", resultASCIIReversed)}");
            Console.WriteLine("----------------------------------------\n");

            Console.WriteLine("------- Base64 Кодирование -------");
            Console.WriteLine($"{surname}: \t {surnameBase64}");
            Console.WriteLine($"{firstname}: \t {firstnameBase64}");
            Console.WriteLine($"a (2): \t \t {XOR.ToBinary(surnameBase64Bytes)}");
            Console.WriteLine($"b (2): \t \t {XOR.ToBinary(firstnameBase64Bytes)}");
            Console.WriteLine($"a XOR b (2): \t {XOR.ToBinary(resultBase64)}");
            Console.WriteLine($"a XOR b: \t {Convert.ToBase64String(resultBase64)}");
            Console.WriteLine($"a XOR b XOR b2:\t {XOR.ToBinary(resultBase64Reversed)}");
            Console.WriteLine($"a XOR b XOR b: \t {Convert.ToBase64String(resultBase64Reversed)}");
            Console.WriteLine("----------------------------------------\n");
        }
    }
}
