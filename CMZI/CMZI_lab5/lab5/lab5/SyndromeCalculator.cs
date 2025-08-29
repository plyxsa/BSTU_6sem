using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab5
{
    public class SyndromeCalculator
    {
        private readonly int _k1;
        private readonly int _k2;
        private readonly int? _z;
        private readonly ParityCalculator _parityCalculator;

        public SyndromeCalculator(int k1, int k2, int? z, ParityCalculator parityCalculator)
        {
            _k1 = k1;
            _k2 = k2;
            _z = z;
            _parityCalculator = parityCalculator;
        }

        public int[] CalculateSyndrome2D(int[,] matrix, int[] receivedParity, int groupIndex)
        {
            if (receivedParity == null) return new int[0];

            int[] calculatedParity;
            int[] syndrome = new int[receivedParity.Length];

            if (groupIndex == 1)
            {
                calculatedParity = new int[_k1];
                for (int i = 0; i < _k1; i++)
                {
                    calculatedParity[i] = _parityCalculator.CalculateParity(Enumerable.Range(0, _k2).Select(j => matrix[i, j]));
                    syndrome[i] = calculatedParity[i] ^ receivedParity[i];
                }
            }
            else if (groupIndex == 2)
            {
                calculatedParity = new int[_k2];
                for (int j = 0; j < _k2; j++)
                {
                    calculatedParity[j] = _parityCalculator.CalculateParity(Enumerable.Range(0, _k1).Select(i => matrix[i, j]));
                    syndrome[j] = calculatedParity[j] ^ receivedParity[j];
                }
            }
            else
            {
                return new int[0];
            }
            return syndrome;
        }

        public int[] CalculateSyndrome3D(int[,,] matrix, int[] receivedParity, int groupIndex)
        {
            if (receivedParity == null) return new int[0];

            int[] calculatedParity;
            int[] syndrome = new int[receivedParity.Length];

            switch (groupIndex)
            {
                case 1:
                    calculatedParity = new int[_k2 * _z.Value];
                    int p1Idx = 0;
                    for (int k = 0; k < _z.Value; k++)
                    {
                        for (int j = 0; j < _k2; j++)
                        {
                            calculatedParity[p1Idx] = _parityCalculator.CalculateParity(Enumerable.Range(0, _k1).Select(i => matrix[i, j, k]));
                            syndrome[p1Idx] = calculatedParity[p1Idx] ^ receivedParity[p1Idx];
                            p1Idx++;
                        }
                    }
                    break;
                case 2:
                    calculatedParity = new int[_k1 * _z.Value];
                    int p2Idx = 0;
                    for (int k = 0; k < _z.Value; k++)
                    {
                        for (int i = 0; i < _k1; i++)
                        {
                            calculatedParity[p2Idx] = _parityCalculator.CalculateParity(Enumerable.Range(0, _k2).Select(j => matrix[i, j, k]));
                            syndrome[p2Idx] = calculatedParity[p2Idx] ^ receivedParity[p2Idx];
                            p2Idx++;
                        }
                    }
                    break;
                case 3:
                    calculatedParity = new int[_k1 * _k2];
                    int p3Idx = 0;
                    for (int i = 0; i < _k1; i++)
                    {
                        for (int j = 0; j < _k2; j++)
                        {
                            calculatedParity[p3Idx] = _parityCalculator.CalculateParity(Enumerable.Range(0, _z.Value).Select(k => matrix[i, j, k]));
                            syndrome[p3Idx] = calculatedParity[p3Idx] ^ receivedParity[p3Idx];
                            p3Idx++;
                        }
                    }
                    break;
                case 4:
                    int[] currentData = new MatrixOperations(_k1, _k2, _z, null, null).FlattenMatrix(matrix);
                    calculatedParity = new int[1];
                    calculatedParity[0] = _parityCalculator.CalculateParity(currentData);
                    syndrome[0] = calculatedParity[0] ^ receivedParity[0];
                    break;
                default:
                    return new int[0];
            }
            return syndrome;
        }
    }
}
