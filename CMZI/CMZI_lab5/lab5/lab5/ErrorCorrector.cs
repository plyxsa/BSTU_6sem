using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab5
{
    public class ErrorCorrector
    {
        private readonly int _k1;
        private readonly int _k2;
        private readonly int? _z;

        public ErrorCorrector(int k1, int k2, int? z)
        {
            _k1 = k1;
            _k2 = k2;
            _z = z;
        }

        public bool CorrectErrors2D(int[,] matrix, int[] syndrome1, int[] syndrome2)
        {
            bool correctionMade = false;
            for (int i = 0; i < _k1; i++)
            {
                for (int j = 0; j < _k2; j++)
                {
                    int failingChecks = 0;
                    if (syndrome1 != null && syndrome1[i] == 1) failingChecks++;
                    if (syndrome2 != null && syndrome2[j] == 1) failingChecks++;

                    if (failingChecks >= 2)
                    {
                        matrix[i, j] = 1 - matrix[i, j];
                        correctionMade = true;
                    }
                }
            }
            return correctionMade;
        }

        public bool CorrectErrors3D(int[,,] matrix, int[] syndrome1, int[] syndrome2, int[] syndrome3)
        {
            bool correctionMade = false;
            for (int i = 0; i < _k1; i++)
            {
                for (int j = 0; j < _k2; j++)
                {
                    for (int k = 0; k < _z.Value; k++)
                    {

                        int failingChecks = 0;
                        if (syndrome1 != null && syndrome1[k * _k2 + j] == 1) failingChecks++;
                        if (syndrome2 != null && syndrome2[k * _k1 + i] == 1) failingChecks++;
                        if (syndrome3 != null && syndrome3[i * _k2 + j] == 1) failingChecks++;

                        if (failingChecks >= 2)
                        {
                            matrix[i, j, k] = 1 - matrix[i, j, k];
                            correctionMade = true;
                        }
                    }
                }
            }
            return correctionMade;
        }
    }
}
