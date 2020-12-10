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
            public void Test(Type type, string sol1, string sol2)
            {
                var instance = Activator.CreateInstance(type) as BaseDay;

                Assert.Equal(sol1, instance.Solve_1());
                Assert.Equal(sol2, instance.Solve_2());
            }
        }
    }
}
