using AoCHelper;
using FileParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AoC_2020
{
    public class Day_18 : BaseDay
    {
        private readonly List<Expression> _input;
        private readonly List<string> _reversedLines;

        public Day_18()
        {
            //_input = ParseInput().ToList();
            _reversedLines = ParseInput2().ToList();
        }

        public override string Solve_1()
        {
            long results = 0;

            foreach (var line in _reversedLines)
            {
                results += CalculateLine(line);
            }

            return results.ToString();
        }

        internal static long CalculateLine(string line)
        {
            var items = line.Split(" ").ToList();

            var main = new Expression() { OperandLeft = new Expression() };
            Expression? innerRight = null;

            var parenthesis = new Stack<Expression>();

            for (int i = 0; i < items.Count; ++i)
            {
                var item = items[i];
                if (long.TryParse(item, out long n))
                {
                    if (innerRight is null)
                    {
                        innerRight = (parenthesis.Count > 0) ? parenthesis.Peek() : main;
                        if (innerRight.OperandLeft?.OperandLeft is null && innerRight.OperandLeft?.Value is null)
                        {
                            innerRight.OperandLeft = new Expression() { Value = n };
                        }
                        else
                        {
                            Expression? iter = innerRight;
                            while (iter.OperandRight is not null)
                            {
                                iter = iter.OperandRight;
                            }

                            iter.OperandRight = new Expression();
                            innerRight = iter.OperandRight;
                        }
                        innerRight.OperandLeft = new Expression() { Value = n };
                    }
                    else
                    {
                        innerRight.OperandRight = new Expression();
                        innerRight = innerRight.OperandRight;

                        innerRight.OperandLeft = new Expression() { Value = n };
                    }
                }
                else if (item == "+" || item == "*")
                {
                    innerRight!.Operator = item == "+" ? Operator.Plus : Operator.Times;
                }
                else if (item == "(")
                {
                    parenthesis.Push(new Expression { OperandLeft = new Expression() });
                    innerRight = null;
                }
                else
                {
                    var expression = parenthesis.Pop();
                    if (main.OperandLeft.OperandLeft is null)
                    {
                        if (parenthesis.Count > 0)
                        {
                            innerRight = parenthesis.Peek();

                            if (innerRight.OperandLeft.OperandLeft is null && innerRight.OperandLeft.Value is null)
                            {
                                innerRight.OperandLeft = expression;
                            }
                            else
                            {
                                Expression? iter = innerRight;
                                while (iter.OperandRight is not null)
                                {
                                    iter = iter.OperandRight;
                                }

                                iter.OperandRight = new Expression { OperandLeft = new Expression { Value = expression.Solve() } };
                                innerRight = iter.OperandRight;
                            }
                        }
                        else
                        {
                            if (main.OperandLeft.OperandLeft is null && main.OperandLeft.Value is null)
                            {
                                innerRight = main;
                            }
                            else
                            {
                                Expression? iter = main;
                                while (iter.OperandRight is not null)
                                {
                                    iter = iter.OperandRight;
                                }

                                iter.OperandRight = new Expression();
                                innerRight = iter.OperandRight;
                            }

                            innerRight.OperandLeft = new Expression { Value = expression.Solve() };
                        }
                    }
                    else
                    {
                        main.OperandRight = new Expression();
                        innerRight = main.OperandRight;


                        innerRight.OperandLeft = expression;
                    }
                }
            }

            var result = main.Solve();
            Console.WriteLine(result);

            return result;
        }



        public override string Solve_2()
        {
            var solution = string.Empty;

            return solution;
        }

        private IEnumerable<string> ParseInput2()
        {
            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                yield return ReverseLineAndAddSpaces(line);
            }
        }

        internal static string ReverseLineAndAddSpaces(string line)
        {
            return string.Join("", line.Reverse())
                .Replace(")", "/")
                .Replace("(", " )")
                .Replace("/", "( ");
        }

        public enum Operator { None, Plus, Times }

        public class Expression
        {
            public long? Value { get; set; }

            public Expression OperandLeft { get; set; }

            public Expression? OperandRight { get; set; }

            public Operator Operator { get; set; }

            public long Solve()
            {
                return Operator switch
                {
                    Operator.Plus => OperandLeft.Solve() + (OperandRight?.Solve() ?? 0),
                    Operator.Times => OperandLeft.Solve() * (OperandRight?.Solve() ?? 1),
                    _ => Value ?? OperandLeft.Solve()// ?? throw new SolvingException()
                };
            }

        }
    }
}
