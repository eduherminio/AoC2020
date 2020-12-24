using AoCHelper;
using System;
using Xunit;

namespace AoC_2020.Test
{
    public static class SolutionTests
    {
        public class Solutions
        {
            [Theory]
            [InlineData(typeof(Day_01), "988771", "171933104")]
            [InlineData(typeof(Day_02), "528", "497")]
            [InlineData(typeof(Day_03), "228", "6818112000")]
            [InlineData(typeof(Day_04), "216", "150")]
            [InlineData(typeof(Day_05), "919", "642")]
            [InlineData(typeof(Day_06), "6662", "3382")]
            [InlineData(typeof(Day_07), "248", "57281")]
            [InlineData(typeof(Day_08), "1563", "767")]
            [InlineData(typeof(Day_09), "57195069", "7409241")]
            [InlineData(typeof(Day_10), "2450", "32396521357312")]
            [InlineData(typeof(Day_11), "2453", "2159")]
            [InlineData(typeof(Day_12), "2879", "178986")]
            [InlineData(typeof(Day_13), "3215", "1001569619313439")]
            [InlineData(typeof(Day_14), "17765746710228", "4401465949086")]
            [InlineData(typeof(Day_15), "614", "1065")]
            [InlineData(typeof(Day_16), "28882", "1429779530273")]
            [InlineData(typeof(Day_17), "263", "1680")]
            [InlineData(typeof(Day_18), "280014646144", "9966990988262")]
            [InlineData(typeof(Day_19), "104", "314")]
            //[InlineData(typeof(Day_20), "", "")]
            [InlineData(typeof(Day_21), "2428", "bjq,jznhvh,klplr,dtvhzt,sbzd,tlgjzx,ctmbr,kqms")]
            [InlineData(typeof(Day_22), "31957", "33212")]
            [InlineData(typeof(Day_23), "47382659", "42271866720")]
            [InlineData(typeof(Day_24), "450", "")]
            public void Test(Type type, string sol1, string sol2)
            {
                var instance = Activator.CreateInstance(type) as BaseDay;

                Assert.Equal(sol1, instance.Solve_1());
                Assert.Equal(sol2, instance.Solve_2());
            }
        }
    }
}
