using AoCHelper;
using FileParser;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC_2020
{
    public class Day_14 : BaseDay
    {
        private const int MaskLength = 36;

        private readonly List<Instruction> _input;

        public Day_14()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            var memory = new Dictionary<string, long>();

            foreach (var instruction in _input)
            {
                foreach (var addressValuePair in instruction.AddressValueDictionary)
                {
                    memory[addressValuePair.Key] = ApplyMask(addressValuePair.Value, instruction.Mask);
                }
            }

            return memory.Values.Sum().ToString();

            static long ApplyMask(long value, string mask)
            {
                var valueAsBinaryString = Convert.ToString(value, 2);
                valueAsBinaryString = new string('0', MaskLength - valueAsBinaryString.Length) + valueAsBinaryString;

                var result = valueAsBinaryString.Zip(mask, (originalBit, maskBit) =>
                {
                    return maskBit switch
                    {
                        'X' => originalBit,
                        _ => maskBit
                    };
                });

                return Convert.ToInt64(string.Join("", result), 2);
            }
        }

        public override string Solve_2()
        {
            var memory = new Dictionary<long, long>();

            foreach (var instruction in _input)
            {
                foreach (var addressValuePair in instruction.AddressValueDictionary)
                {
                    var address = ApplyMask(long.Parse(addressValuePair.Key), instruction.Mask);

                    foreach (var add in DecodeAddress(address))
                    {
                        memory[add] = addressValuePair.Value;
                    }
                }
            }

            return memory.Values.Sum().ToString();

            static string ApplyMask(long address, string mask)
            {
                var addressAsBinaryString = Convert.ToString(address, 2);
                addressAsBinaryString = new string('0', MaskLength - addressAsBinaryString.Length) + addressAsBinaryString;

                var result = addressAsBinaryString.Zip(mask, (originalBit, maskBit) =>
                {
                    return maskBit switch
                    {
                        '0' => originalBit,
                        _ => maskBit
                    };
                });

                return string.Join("", result);
            }

            static ICollection<long> DecodeAddress(string address)
            {
                var possibleAddressList = new HashSet<StringBuilder>();

                possibleAddressList.AddRange(address.Last() == 'X'
                    ? new[] { new StringBuilder("0"), new StringBuilder("1") }
                    : new[] { new StringBuilder($"{address.Last()}") });        // Convert char to string!

                foreach (var ch in address.Reverse().Skip(1))
                {
                    var toAdd = new List<StringBuilder>();
                    var toRemove = new List<StringBuilder>();

                    switch (ch)
                    {
                        case 'X':
                            foreach (var possibleAddress in possibleAddressList)
                            {
                                toAdd.AddRange(new[] {
                                    new StringBuilder(possibleAddress.ToString()).Append('0'),
                                    new StringBuilder(possibleAddress.ToString()).Append('1') });
                                toRemove.Add(possibleAddress);
                            }
                            break;
                        default:
                            possibleAddressList.ForEach(add => add.Append(ch));
                            break;
                    }

                    toAdd.ForEach(sb => possibleAddressList.Add(sb));
                    toRemove.ForEach(sb => possibleAddressList.Remove(sb));
                }

                return possibleAddressList.Select(sb => Convert.ToInt64(string.Join(string.Empty, sb.ToString().Reverse()), 2)).ToList();
            }
        }

        private List<Instruction> ParseInput()
        {
            var instructionList = new List<Instruction>();
            foreach (var line in new ParsedFile(InputFilePath))
            {
                var first = line.NextElement<string>();

                if (first == "mask")
                {
                    instructionList.Add(new Instruction(line.LastElement<string>()));
                }
                else
                {
                    instructionList.Last().AddressValueDictionary[first[4..^1]] = line.LastElement<int>();
                }
            }

            return instructionList;
        }

        private record Instruction(string Mask)
        {
            public Dictionary<string, int> AddressValueDictionary { get; } = new();
        }
    }
}
