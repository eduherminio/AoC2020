using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC_2020
{
    internal class BitMatrix
    {
        public List<BitArray> Content { get; set; }

        public BitMatrix(List<BitArray> content)
        {
            Content = content;
        }

        public virtual List<BitArray> RotateClockwise()
        {
            var length = Content.Count;
            var result = new List<BitArray>(length);

            for (int i = 0; i < length; ++i)
            {
                result.Add(new BitArray(
                    Content.Select(arr => arr[i])
                    .Reverse()
                    .ToArray()));
            }

            return result;
        }

        public virtual List<BitArray> RotateAnticlockwise()
        {
            var length = Content[0].Count;
            var result = new List<BitArray>(length);

            for (int i = 0; i < length; ++i)
            {
                result.Add(new BitArray(
                    Content.Select(arr => arr[length - i - 1])
                    .ToArray()));
            }

            return result;
        }

        public virtual List<BitArray> Rotate180()
        {
            return new BitMatrix(FlipUpsideDown()).FlipLeftRight();
        }

        public virtual List<BitArray> FlipUpsideDown()
        {
            return Enumerable.Reverse(Content).ToList();
        }

        public virtual List<BitArray> FlipLeftRight()
        {
            var length = Content[0].Count;

            return Content.ConvertAll(original =>
            {
                int mid = length / 2;
                var newRow = new BitArray(original);
                for (int i = 0; i < mid; ++i)
                {
                    newRow[i] = original[length - i - 1];
                    newRow[length - i - 1] = original[i];
                }

                return newRow;
            });
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var item in Content)
            {
                foreach (var bit in item)
                {
                    sb.Append((bool)bit ? "1" : "0");
                }

                sb.Append(Environment.NewLine);
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }

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
