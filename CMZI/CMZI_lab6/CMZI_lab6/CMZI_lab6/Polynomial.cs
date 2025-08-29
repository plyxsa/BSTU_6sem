using System;
using System.Linq; // Для TrimTrailingZeros

namespace CMZI_lab6
{
    public class Polynomial
    {
        public int Degree { get; private set; } // Сделаем set private, чтобы Degree обновлялась при операциях
        public int[] Coefficients { get; private set; } // Сделаем set private

        public Polynomial(int[] coefficients)
        {
            if (coefficients == null || coefficients.Length == 0)
            {
                // Можно создать нулевой полином или выбросить исключение
                Coefficients = new int[] { 0 }; // Нулевой полином
                Degree = 0;
            }
            else
            {
                // Удаляем завершающие нули, так как они не влияют на значение полинома, но влияют на степень
                Coefficients = TrimTrailingZeros(coefficients);
                Degree = Coefficients.Length - 1;
            }
        }

        // Вспомогательный метод для удаления завершающих нулей
        private int[] TrimTrailingZeros(int[] coeffs)
        {
            int lastNonZero = coeffs.Length - 1;
            while (lastNonZero > 0 && coeffs[lastNonZero] == 0)
            {
                lastNonZero--;
            }
            if (lastNonZero == 0 && coeffs[0] == 0) // Случай нулевого полинома
            {
                return new int[] { 0 };
            }
            int[] trimmed = new int[lastNonZero + 1];
            Array.Copy(coeffs, trimmed, lastNonZero + 1);
            return trimmed;
        }

        // --- Арифметические операции в GF(2) ---

        // Сложение/Вычитание (эквивалентно в GF(2))
        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            int maxLength = Math.Max(a.Coefficients.Length, b.Coefficients.Length);
            int[] resultCoeffs = new int[maxLength];

            for (int i = 0; i < maxLength; i++)
            {
                int coeffA = (i < a.Coefficients.Length) ? a.Coefficients[i] : 0;
                int coeffB = (i < b.Coefficients.Length) ? b.Coefficients[i] : 0;
                resultCoeffs[i] = coeffA ^ coeffB; // XOR для сложения в GF(2)
            }

            return new Polynomial(resultCoeffs);
        }

        // Умножение
        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            if (a.IsZero() || b.IsZero()) return new Polynomial(new int[] { 0 }); // Оптимизация для нулевого полинома

            int[] resultCoeffs = new int[a.Degree + b.Degree + 1]; // Максимальная возможная степень

            for (int i = 0; i <= a.Degree; i++)
            {
                if (a.Coefficients[i] == 1) // Если коэффициент 1, просто прибавляем (XORим) смещенный второй полином
                {
                    for (int j = 0; j <= b.Degree; j++)
                    {
                        if (b.Coefficients[j] == 1)
                        {
                            resultCoeffs[i + j] ^= 1; // XOR для сложения произведений в GF(2)
                        }
                    }
                }
            }

            return new Polynomial(resultCoeffs);
        }

        // Проверка на нулевой полином
        public bool IsZero()
        {
            return Coefficients.Length == 1 && Coefficients[0] == 0;
        }


        // Деление с остатком (Возвращает остаток) - Это метод Mod
        public Polynomial Mod(Polynomial divisor)
        {
            if (divisor.IsZero()) throw new DivideByZeroException("Делитель не может быть нулевым полиномом.");
            if (this.IsZero()) return new Polynomial(new int[] { 0 }); // 0 mod g(x) = 0

            // Создаем копию делимого, с которой будем работать
            int[] remainderCoeffs = (int[])this.Coefficients.Clone();
            int currentDegree = this.Degree;

            // Выравниваем массивы, чтобы работать с общей длиной
            // При делении полинома A на полином B, мы вычитаем B, умноженный на x^k, из A.
            // Это делается до тех пор, пока степень остатка не станет меньше степени делителя.
            // В GF(2) "вычитание" - это XOR.
            // Для простоты, будем работать с массивом coefficients и отслеживать текущую старшую степень.

            while (currentDegree >= divisor.Degree && !AllZeros(remainderCoeffs, currentDegree + 1))
            {
                // Находим старший ненулевой коэффициент текущего остатка
                while (currentDegree > 0 && remainderCoeffs[currentDegree] == 0 && currentDegree >= divisor.Degree)
                {
                    currentDegree--;
                }
                if (currentDegree < divisor.Degree) break; // Остаток меньше делителя

                // На сколько нужно сдвинуть делитель? Разница степеней.
                int shift = currentDegree - divisor.Degree;

                // Создаем полином-делитель, сдвинутый на 'shift' позиций
                // Это эквивалентно умножению divisor на x^shift
                int[] shiftedDivisorCoeffs = new int[currentDegree + 1]; // Создаем массив нужной длины
                for (int i = 0; i <= divisor.Degree; i++)
                {
                    if (divisor.Coefficients[i] == 1)
                    {
                        shiftedDivisorCoeffs[i + shift] = 1;
                    }
                }

                // "Вычитаем" (XORим) сдвинутый делитель из остатка
                for (int i = 0; i < shiftedDivisorCoeffs.Length; i++)
                {
                    if (i < remainderCoeffs.Length)
                    {
                        remainderCoeffs[i] ^= shiftedDivisorCoeffs[i];
                    }
                    // Нет else, т.к. shiftedDivisorCoeffs[i] будет 0, если выходит за границы remainderCoeffs (которые меньше)
                }

                // Обновляем текущую старшую степень остатка
                while (currentDegree > 0 && remainderCoeffs[currentDegree] == 0)
                {
                    currentDegree--;
                }
            }

            // Возвращаем остаток как новый полином
            // Нужно обрезать массив remainderCoeffs до текущей старшей степени + 1
            int[] finalRemainderCoeffs = TrimTrailingZeros(remainderCoeffs.Take(currentDegree + 1).ToArray());

            return new Polynomial(finalRemainderCoeffs);
        }

        // Вспомогательная функция для проверки, не стали ли все коэффициенты до определенной степени нулями
        private bool AllZeros(int[] coeffs, int upToDegree)
        {
            for (int i = 0; i < upToDegree; i++)
            {
                if (i < coeffs.Length && coeffs[i] != 0) return false;
            }
            return true;
        }


        // --- Переопределения и другие методы ---

        // Возвращает коэффициенты (альтернатива GetCoefficients)
        // public int[] GetCoefficients() { return Coefficients; } // Можно добавить, но публичное свойство лучше

        public override string ToString()
        {
            if (IsZero()) return "0";

            string polynomialString = "";
            // Идем от старшей степени к младшей
            for (int i = Degree; i >= 0; i--)
            {
                if (Coefficients[i] == 1)
                {
                    if (!string.IsNullOrEmpty(polynomialString))
                    {
                        polynomialString += " + ";
                    }
                    if (i == 0)
                    {
                        polynomialString += "1";
                    }
                    else if (i == 1)
                    {
                        polynomialString += "x";
                    }
                    else
                    {
                        polynomialString += $"x^{i}"; // Используем x^i для степени i
                    }
                }
            }
            return polynomialString;
        }
    }
}