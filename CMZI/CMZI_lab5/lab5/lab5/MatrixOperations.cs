using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab5
{
    public class MatrixOperations
    {
        private readonly int _k1;
        private readonly int _k2;
        private readonly int? _z;
        private readonly int[,] _matrix2D;
        private readonly int[,,] _matrix3D;

        public MatrixOperations(int k1, int k2, int? z, int[,] matrix2D, int[,,] matrix3D)
        {
            _k1 = k1;
            _k2 = k2;
            _z = z;
            _matrix2D = matrix2D;
            _matrix3D = matrix3D;
        }

        public void FillMatrix(int[] data)
        {
            int index = 0;
            if (_matrix2D != null)
            {
                for (int i = 0; i < _k1; i++)
                {
                    for (int j = 0; j < _k2; j++)
                    {
                        _matrix2D[i, j] = data[index++];
                    }
                }
            }
            else
            {
                for (int k = 0; k < _z.Value; k++)
                {
                    for (int i = 0; i < _k1; i++)
                    {
                        for (int j = 0; j < _k2; j++)
                        {
                            _matrix3D[i, j, k] = data[index++];
                        }
                    }
                }
            }
        }

        public void FillMatrix2DFromData(int[] data, int[,] matrix)
        {
            int index = 0;
            for (int i = 0; i < _k1; i++)
                for (int j = 0; j < _k2; j++)
                    matrix[i, j] = data[index++];
        }

        public void FillMatrix3DFromData(int[] data, int[,,] matrix)
        {
            int index = 0;
            for (int k = 0; k < _z.Value; k++)
                for (int i = 0; i < _k1; i++)
                    for (int j = 0; j < _k2; j++)
                        matrix[i, j, k] = data[index++];
        }

        public int[] FlattenMatrix(int[,] matrix)
        {
            int k = _k1 * _k2;
            int[] data = new int[k];
            int index = 0;
            for (int i = 0; i < _k1; i++)
                for (int j = 0; j < _k2; j++)
                    data[index++] = matrix[i, j];
            return data;
        }

        public int[] FlattenMatrix(int[,,] matrix)
        {
            int k = _k1 * _k2 * _z.Value;
            int[] data = new int[k];
            int index = 0;
            for (int k1 = 0; k1 < _z.Value; k1++)
                for (int i = 0; i < _k1; i++)
                    for (int j = 0; j < _k2; j++)
                        data[index++] = matrix[i, j, k1];
            return data;
        }
    }
}
