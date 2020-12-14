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

            // Replacing string with StringBuilder makes very little difference, so sticking with the simplest
            static ICollection<long> DecodeAddress(string address)
            {
                var possibleAddressList = new List<string>();

                possibleAddressList.AddRange(address.Last() == 'X'
                    ? new[] { "0", "1" }
                    : new[] { $"{address.Last()}" });        // Convert char to string!

                foreach (var ch in address.Reverse().Skip(1))
                {
                    var toAdd = new List<string>();
                    var toRemove = new List<string>();

                    switch (ch)
                    {
                        case 'X':
                            foreach (var possibleAddress in possibleAddressList)
                            {
                                toAdd.AddRange(new[] {
                                    possibleAddress + '0',
                                    possibleAddress + '1'});
                                toRemove.Add(possibleAddress);
                            }
                            break;
                        default:
                            possibleAddressList = possibleAddressList.ConvertAll(add => add + ch);
                            break;
                    }

                    toAdd.ForEach(sb => possibleAddressList.Add(sb));
                    toRemove.ForEach(sb => possibleAddressList.Remove(sb));
                }

                return possibleAddressList.ConvertAll(str => Convert.ToInt64(string.Join(string.Empty, str.Reverse()), 2));
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
