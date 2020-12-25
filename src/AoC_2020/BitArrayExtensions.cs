using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020
{
    public static class BitArrayExtensions
    {
        public static string ToBitString(this BitArray array)
        {
            return string.Join("", array.Cast<bool>().Select(bit => bit ? 1 : 0));
        }

        public static BitArray Reverse(this BitArray array)
        {
            var result = new BitArray(array);

            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                result[length - i - 1] = array[i];
                result[i] = array[length - i - 1];
            }

            return result;
        }
    }

    public class BitArrayComparer : IEqualityComparer<BitArray>
    {
        public bool Equals(BitArray? x, BitArray? y)
        {
            return x?.ToBitString() == y?.ToBitString();
        }

        public int GetHashCode(BitArray obj)
        {
            return obj.ToBitString().GetHashCode();
        }
    }
}
