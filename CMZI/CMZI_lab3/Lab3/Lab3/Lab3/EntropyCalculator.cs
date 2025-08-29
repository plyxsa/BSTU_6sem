namespace Lab3
{
    class EntropyCalculator
    {
        public static double CalculateInformationAmount(string text, double entropy, char[] alphabet)
        {
            text = new string(text.ToLower().Where(c => alphabet.Contains(c)).ToArray());
            return entropy * text.Length;
        }

        public static double CalculateEntropyHartly(char[] alphabet)
        {
            return Math.Log2(alphabet.Length);
        }

        public static double CalculateEntropy(string text, char[] alphabet)
        {
            text = new string(text.ToLower().Where(c => alphabet.Contains(c)).ToArray());
            int textLength = text.Length;

            if (textLength < 100)
            {
                Console.WriteLine("Текст слишком маленький для того, чтобы рассчитать энтропию.");
                return -1;
            }

            var frequency = new Dictionary<char, int>();
            foreach (var c in text)
            {
                if (frequency.ContainsKey(c))
                    frequency[c]++;
                else
                    frequency[c] = 1;
            }

            double entropy = 0;
            foreach (var kvp in frequency)
            {
                double probability = (double)kvp.Value / textLength;
                entropy += probability * Math.Log2(probability);
            }

            return -entropy;
        }

        public static double EffectiveEntropy(double p)
        {
            if (p == 0 || p == 1) return 0;
            double q = 1 - p;
            return -p * Math.Log2(p) - q * Math.Log2(q);
        }

        public static double AlphabetRedundancy(double entropyShannon, double entropyHartly)
        {
            return (1 - entropyShannon / entropyHartly) * 100;
        }
    }
}
