using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static AoC_2020.Day_20_initial_attempt;

namespace AoC_2020.Test
{
    public class BitMatrix_Tests
    {
        [Theory]
        [MemberData(nameof(FlipUpsideDownData))]
        public void FlipUpsideDown(List<BitArray> original, List<BitArray> expectedResult)
        {
            var result = new BitMatrix(original).FlipUpsideDown();

            foreach (var (first, second) in expectedResult.Zip(result))
            {
                Assert.Equal(first, second);
            }
        }

        [Theory]
        [MemberData(nameof(FlipLeftRightData))]
        public void FlipLeftRight(List<BitArray> original, List<BitArray> expectedResult)
        {
            var result = new BitMatrix(original).FlipLeftRight();

            foreach (var (first, second) in expectedResult.Zip(result))
            {
                Assert.Equal(first, second);
            }
        }

        [Theory]
        [MemberData(nameof(RotateClockwiseData))]
        public void RotateClockwise(List<BitArray> original, List<BitArray> expectedResult)
        {
            var result = new BitMatrix(original).RotateClockwise();

            foreach (var (first, second) in expectedResult.Zip(result))
            {
                Assert.Equal(first, second);
            }
        }

        [Theory]
        [MemberData(nameof(RotateAnticlockwiseData))]
        public void RotateAnticlockwise(List<BitArray> original, List<BitArray> expectedResult)
        {
            var result = new BitMatrix(original).RotateAnticlockwise();

            foreach (var (first, second) in expectedResult.Zip(result))
            {
                Assert.Equal(first, second);
            }
        }

        [Theory]
        [MemberData(nameof(Rotate180Data))]
        public void Rotate180(List<BitArray> original, List<BitArray> expectedResult)
        {
            var result = new BitMatrix(original).Rotate180();

            foreach (var (first, second) in expectedResult.Zip(result))
            {
                Assert.Equal(first, second);
            }
        }

        public static IEnumerable<object[]> FlipUpsideDownData()
        {
            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, true, false }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, true, true }),
                    new BitArray( new [] { false, false, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { false, false, true }),
                    new BitArray( new [] { true, true, true }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, true, false })
                }
            };

            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, true, false }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { false, false, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { false, false, true }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, true, false })
                }
            };
        }

        public static IEnumerable<object[]> FlipLeftRightData()
        {
            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, true, false }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, false, true }),
                    new BitArray( new [] { true , false, false })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { false, true, true}),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, false, true }),
                    new BitArray( new [] { false, false, true })
                }
            };

            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, true, false, false }),
                    new BitArray( new [] { false, false, true, false }),
                    new BitArray( new [] { false, true, true, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { false, false, true, true }),
                    new BitArray( new [] { false, true, false, false }),
                    new BitArray( new [] { true, true, true, false })
                }
            };
        }

        public static IEnumerable<object[]> RotateClockwiseData()
        {
            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, false, true }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { false, false, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { false, false, true}),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, false, true })
                }
            };

            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, false, true, false }),
                    new BitArray( new [] { false, true, false, true}),
                    new BitArray( new [] { false, false, true, true }),
                    new BitArray( new [] { true, true, false, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { true, false, false, true }),
                    new BitArray( new [] { true, false, true, false }),
                    new BitArray( new [] { false, true, false, true }),
                    new BitArray( new [] { true, true, true, false })
                }
            };
        }

        public static IEnumerable<object[]> RotateAnticlockwiseData()
        {
            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { false, false, true}),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, false, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { true, false, true }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { false, false, true })
                }
            };

            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, false, false, true }),
                    new BitArray( new [] { true, false, true, false }),
                    new BitArray( new [] { false, true, false, true }),
                    new BitArray( new [] { true, true, true, false })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { true, false, true, false }),
                    new BitArray( new [] { false, true, false, true}),
                    new BitArray( new [] { false, false, true, true }),
                    new BitArray( new [] { true, true, false, true })
                }
            };
        }

        public static IEnumerable<object[]> Rotate180Data()
        {
            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { false, false, true}),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, false, true })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { true, false, true }),
                    new BitArray( new [] { false, false, false }),
                    new BitArray( new [] { true, false, false })
                }
            };

            yield return new object[]
            {
                new List<BitArray>
                {
                    new BitArray( new [] { true, false, false, false }),
                    new BitArray( new [] { true, false, true, false }),
                    new BitArray( new [] { false, true, false, true }),
                    new BitArray( new [] { true, true, true, false })
                },

                new List<BitArray>
                {
                    new BitArray( new [] { false, true, true, true}),
                    new BitArray( new [] { true, false, true, false}),
                    new BitArray( new [] { false, true, false, true }),
                    new BitArray( new [] { false, false, false, true })
                }
            };
        }
    }
}
