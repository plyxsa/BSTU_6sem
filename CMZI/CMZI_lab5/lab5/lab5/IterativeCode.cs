using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab5
{
    public class IterativeCode
    {
        public int K1 { get; }
        public int K2 { get; }
        public int? Z { get; }
        public int NumParityGroups { get; }
        public int K { get; }
        public int R { get; private set; }
        public int N { get; private set; }

        private int[] _informationWord;
        private int[] _codewordXn;
        private int[] _receivedWordYn;
        private int[] _correctedWordYnPrime;

        private int[,] _matrix2D;
        private int[,,] _matrix3D;

        private int[] _parityGroup1;
        private int[] _parityGroup2;
        private int[] _parityGroup3;
        private int[] _parityGroup4;

        private readonly Random _random = new Random();
        private const int MAX_DECODING_ITERATIONS = 10;

        private readonly MatrixOperations _matrixOperations;
        private readonly ParityCalculator _parityCalculator;
        private readonly SyndromeCalculator _syndromeCalculator;
        private readonly ErrorCorrector _errorCorrector;

        public IterativeCode(int k1, int k2, int? z, int numParityGroups)
        {
            K1 = k1;
            K2 = k2;
            Z = z;
            NumParityGroups = numParityGroups;

            if (Z.HasValue)
            {
                K = K1 * K2 * Z.Value;
                _matrix3D = new int[K1, K2, Z.Value];
            }
            else
            {
                K = K1 * K2;
                _matrix2D = new int[K1, K2];
            }

            CalculateRedundancy();
            N = K + R;

            _matrixOperations = new MatrixOperations(K1, K2, Z, _matrix2D, _matrix3D);
            _parityCalculator = new ParityCalculator();
            _syndromeCalculator = new SyndromeCalculator(K1, K2, Z, _parityCalculator);
            _errorCorrector = new ErrorCorrector(K1, K2, Z);
        }

        private void CalculateRedundancy()
        {
            R = 0;
            if (!Z.HasValue)
            {
                if (NumParityGroups >= 1) R += K1;
                if (NumParityGroups >= 2) R += K2;
            }
            else
            {
                if (NumParityGroups >= 1) R += K2 * Z.Value;
                if (NumParityGroups >= 2) R += K1 * Z.Value;
                if (NumParityGroups >= 3) R += K1 * K2;
                if (NumParityGroups >= 4) R += 1;
            }
        }

        public int[] Encode(int[] informationWord)
        {
            if (informationWord == null || informationWord.Length != K)
            {
                throw new ArgumentException($"Информационное слово должно иметь длину {K}.");
            }
            _informationWord = (int[])informationWord.Clone();

            _matrixOperations.FillMatrix(_informationWord);

            CalculateParityBits();

            FormCodewordXn();

            return (int[])_codewordXn.Clone();
        }

        private void CalculateParityBits()
        {
            if (_matrix2D != null)
            {
                _parityGroup1 = new int[K1];
                for (int i = 0; i < K1; i++)
                {
                    int[] row = Enumerable.Range(0, K2).Select(j => _matrix2D[i, j]).ToArray();
                    _parityGroup1[i] = _parityCalculator.CalculateParity(row);
                }

                _parityGroup2 = new int[K2];
                for (int j = 0; j < K2; j++)
                {
                    int[] col = Enumerable.Range(0, K1).Select(i => _matrix2D[i, j]).ToArray();
                    _parityGroup2[j] = _parityCalculator.CalculateParity(col);
                }
                _parityGroup3 = null;
                _parityGroup4 = null;
            }
            else
            {
                _parityGroup1 = new int[K2 * Z.Value];
                int p1Idx = 0;
                for (int k = 0; k < Z.Value; k++)
                {
                    for (int j = 0; j < K2; j++)
                    {
                        _parityGroup1[p1Idx++] = _parityCalculator.CalculateParity(Enumerable.Range(0, K1).Select(i => _matrix3D[i, j, k]));
                    }
                }

                _parityGroup2 = new int[K1 * Z.Value];
                int p2Idx = 0;
                for (int k = 0; k < Z.Value; k++)
                {
                    for (int i = 0; i < K1; i++)
                    {
                        _parityGroup2[p2Idx++] = _parityCalculator.CalculateParity(Enumerable.Range(0, K2).Select(j => _matrix3D[i, j, k]));
                    }
                }

                _parityGroup3 = new int[K1 * K2];
                int p3Idx = 0;
                for (int i = 0; i < K1; i++)
                {
                    for (int j = 0; j < K2; j++)
                    {
                        _parityGroup3[p3Idx++] = _parityCalculator.CalculateParity(Enumerable.Range(0, Z.Value).Select(k => _matrix3D[i, j, k]));
                    }
                }

                _parityGroup4 = new int[1];
                _parityGroup4[0] = _parityCalculator.CalculateParity(_informationWord);
            }
        }

        private void FormCodewordXn()
        {
            var codewordList = new List<int>(_informationWord);
            if (_parityGroup1 != null) codewordList.AddRange(_parityGroup1);
            if (_parityGroup2 != null) codewordList.AddRange(_parityGroup2);
            if (_parityGroup3 != null) codewordList.AddRange(_parityGroup3);
            if (_parityGroup4 != null) codewordList.AddRange(_parityGroup4);
            _codewordXn = codewordList.ToArray();

            if (_codewordXn.Length != N)
            {
                Console.WriteLine($"Предупреждение: Вычисленная длина кодового слова ({_codewordXn.Length}) не соответствует ожидаемой N ({N}). R={R}");
                N = _codewordXn.Length;
            }
        }

        public int[] IntroduceErrors(int errorCount)
        {
            if (_codewordXn == null)
            {
                throw new InvalidOperationException("Кодирование должно быть выполнено до внесения ошибок.");
            }
            if (errorCount < 0) errorCount = 0;
            if (errorCount > N) errorCount = N;


            _receivedWordYn = (int[])_codewordXn.Clone();

            if (errorCount == 0) return (int[])_receivedWordYn.Clone();


            var indicesToFlip = new HashSet<int>();
            while (indicesToFlip.Count < errorCount)
            {
                indicesToFlip.Add(_random.Next(N));
            }

            foreach (int index in indicesToFlip)
            {
                _receivedWordYn[index] = 1 - _receivedWordYn[index];
            }

            return (int[])_receivedWordYn.Clone();
        }

        public int[] Decode()
        {
            if (_receivedWordYn == null)
            {
                throw new InvalidOperationException("Ошибки должны быть внесены (или Yn установлен) до декодирования.");
            }

            _correctedWordYnPrime = (int[])_receivedWordYn.Clone();

            int[,] currentMatrix2D = null;
            int[,,] currentMatrix3D = null;

            int[] currentData = _correctedWordYnPrime.Take(K).ToArray();
            int[] receivedP1 = _correctedWordYnPrime.Skip(K).Take(_parityGroup1?.Length ?? 0).ToArray();
            int[] receivedP2 = _correctedWordYnPrime.Skip(K + (_parityGroup1?.Length ?? 0)).Take(_parityGroup2?.Length ?? 0).ToArray();
            int[] receivedP3 = _correctedWordYnPrime.Skip(K + (_parityGroup1?.Length ?? 0) + (_parityGroup2?.Length ?? 0)).Take(_parityGroup3?.Length ?? 0).ToArray();
            int[] receivedP4 = _correctedWordYnPrime.Skip(K + (_parityGroup1?.Length ?? 0) + (_parityGroup2?.Length ?? 0) + (_parityGroup3?.Length ?? 0)).Take(_parityGroup4?.Length ?? 0).ToArray();


            if (!Z.HasValue)
            {
                currentMatrix2D = new int[K1, K2];
                _matrixOperations.FillMatrix2DFromData(currentData, currentMatrix2D);
            }
            else
            {
                currentMatrix3D = new int[K1, K2, Z.Value];
                _matrixOperations.FillMatrix3DFromData(currentData, currentMatrix3D);
            }


            for (int iter = 0; iter < MAX_DECODING_ITERATIONS; iter++)
            {
                bool correctionMade = false;

                int[] syndrome1 = null, syndrome2 = null, syndrome3 = null, syndrome4 = null;
                if (!Z.HasValue)
                {
                    syndrome1 = _syndromeCalculator.CalculateSyndrome2D(currentMatrix2D, receivedP1, 1);
                    syndrome2 = _syndromeCalculator.CalculateSyndrome2D(currentMatrix2D, receivedP2, 2);
                }
                else
                {
                    syndrome1 = _syndromeCalculator.CalculateSyndrome3D(currentMatrix3D, receivedP1, 1);
                    syndrome2 = _syndromeCalculator.CalculateSyndrome3D(currentMatrix3D, receivedP2, 2);
                    syndrome3 = _syndromeCalculator.CalculateSyndrome3D(currentMatrix3D, receivedP3, 3);
                    syndrome4 = _syndromeCalculator.CalculateSyndrome3D(currentMatrix3D, receivedP4, 4);
                }

                bool allZero = (syndrome1?.All(s => s == 0) ?? true) &&
                               (syndrome2?.All(s => s == 0) ?? true) &&
                               (syndrome3?.All(s => s == 0) ?? true) &&
                               (syndrome4?.All(s => s == 0) ?? true);

                if (allZero)
                {
                    break;
                }

                if (!Z.HasValue && currentMatrix2D != null)
                {
                    correctionMade = _errorCorrector.CorrectErrors2D(currentMatrix2D, syndrome1, syndrome2);
                }
                else if (Z.HasValue && currentMatrix3D != null)
                {
                    correctionMade = _errorCorrector.CorrectErrors3D(currentMatrix3D, syndrome1, syndrome2, syndrome3);
                }


                if (!correctionMade && !allZero)
                {
                    break;
                }
                if (iter == MAX_DECODING_ITERATIONS - 1 && !allZero)
                {
                }

            }

            if (!Z.HasValue)
            {
                CalculateParityBitsFromMatrix(currentMatrix2D);
                currentData = _matrixOperations.FlattenMatrix(currentMatrix2D);
            }
            else
            {
                CalculateParityBitsFromMatrix(currentMatrix3D);
                currentData = _matrixOperations.FlattenMatrix(currentMatrix3D);
            }

            var correctedList = new List<int>(currentData);
            if (_parityGroup1 != null) correctedList.AddRange(_parityGroup1);
            if (_parityGroup2 != null) correctedList.AddRange(_parityGroup2);
            if (_parityGroup3 != null) correctedList.AddRange(_parityGroup3);
            if (_parityGroup4 != null) correctedList.AddRange(_parityGroup4);
            _correctedWordYnPrime = correctedList.ToArray();


            return (int[])_correctedWordYnPrime.Clone();
        }

        private void CalculateParityBitsFromMatrix(int[,] matrix)
        {
            if (NumParityGroups >= 1)
            {
                _parityGroup1 = new int[K1];
                for (int i = 0; i < K1; i++)
                    _parityGroup1[i] = _parityCalculator.CalculateParity(Enumerable.Range(0, K2).Select(j => matrix[i, j]));
            }
            if (NumParityGroups >= 2)
            {
                _parityGroup2 = new int[K2];
                for (int j = 0; j < K2; j++)
                    _parityGroup2[j] = _parityCalculator.CalculateParity(Enumerable.Range(0, K1).Select(i => matrix[i, j]));
            }
            _parityGroup3 = null;
            _parityGroup4 = null;
        }

        private void CalculateParityBitsFromMatrix(int[,,] matrix)
        {
            if (NumParityGroups >= 1)
            {
                _parityGroup1 = new int[K2 * Z.Value];
                int p1Idx = 0;
                for (int k = 0; k < Z.Value; k++)
                    for (int j = 0; j < K2; j++)
                        _parityGroup1[p1Idx++] = _parityCalculator.CalculateParity(Enumerable.Range(0, K1).Select(i => matrix[i, j, k]));
            }

            if (NumParityGroups >= 2)
            {
                _parityGroup2 = new int[K1 * Z.Value];
                int p2Idx = 0;
                for (int k = 0; k < Z.Value; k++)
                    for (int i = 0; i < K1; i++)
                        _parityGroup2[p2Idx++] = _parityCalculator.CalculateParity(Enumerable.Range(0, K2).Select(j => matrix[i, j, k]));
            }

            if (NumParityGroups >= 3)
            {
                _parityGroup3 = new int[K1 * K2];
                int p3Idx = 0;
                for (int i = 0; i < K1; i++)
                    for (int j = 0; j < K2; j++)
                        _parityGroup3[p3Idx++] = _parityCalculator.CalculateParity(Enumerable.Range(0, Z.Value).Select(k => matrix[i, j, k]));
            }

            if (NumParityGroups >= 4)
            {
                _parityGroup4 = new int[1];
                int[] currentData = _matrixOperations.FlattenMatrix(matrix);
                _parityGroup4[0] = _parityCalculator.CalculateParity(currentData);
            }
        }

        public bool AnalyzeCorrection()
        {
            if (_codewordXn == null || _correctedWordYnPrime == null)
            {
                return false;
            }
            if (_codewordXn.Length != _correctedWordYnPrime.Length)
            {
                Console.WriteLine($"Невозможно проанализировать: Длина не соответствует Xn={_codewordXn.Length}, Yn'={_correctedWordYnPrime.Length}");
                return false;
            }
            return _codewordXn.SequenceEqual(_correctedWordYnPrime);
        }

        public int[] GetInformationWord() => (int[])_informationWord?.Clone();
        public int[] GetCodewordXn() => (int[])_codewordXn?.Clone();
        public int[] GetReceivedWordYn() => (int[])_receivedWordYn?.Clone();
        public int[] GetCorrectedWordYnPrime() => (int[])_correctedWordYnPrime?.Clone();
        public int[,] GetMatrix2D() => (int[,])_matrix2D?.Clone();
        public int[,,] GetMatrix3D() => (int[,,])_matrix3D?.Clone();
        public int[] GetParityGroup(int groupNum)
        {
            switch (groupNum)
            {
                case 1: return (int[])_parityGroup1?.Clone();
                case 2: return (int[])_parityGroup2?.Clone();
                case 3: return (int[])_parityGroup3?.Clone();
                case 4: return (int[])_parityGroup4?.Clone();
                default: return null;
            }
        }
    }
}
