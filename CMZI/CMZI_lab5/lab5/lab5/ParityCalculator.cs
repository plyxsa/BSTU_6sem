using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab5
{
    public class ParityCalculator
    {
        public int CalculateParity(IEnumerable<int> bits)
        {
            return bits.Aggregate(0, (acc, bit) => acc ^ bit);
        }
    }
}
