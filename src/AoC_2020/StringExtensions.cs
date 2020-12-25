using System;

namespace AoC_2020
{
    public static class StringExtensions
    {
        public static string ReverseString(this string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);

            return new string(charArray);
        }
    }
}
