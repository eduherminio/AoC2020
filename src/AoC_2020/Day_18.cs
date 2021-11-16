using AoCHelper;

namespace AoC_2020
{
    public class Day_18 : BaseDay
    {
        private readonly List<string> _reversedLines;

        public Day_18()
        {
            _reversedLines = ParseInput().ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            long results = 0;

            foreach (var line in _reversedLines)
            {
                results += CalculateLine(line, (expression) => expression.SolvePart1());
            }

            return new(results.ToString());
        }

        public override ValueTask<string> Solve_2()
        {
            long results = 0;

            foreach (var line in _reversedLines)
            {
                results += CalculateLine(line, (expression) => expression.SolvePart2());
            }

            return new(results.ToString());
        }

        internal static long CalculateLine(string line, Func<Expression, long> solveMethod)
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
                        if (innerRight.OperandLeft!.OperandLeft is null && innerRight.OperandLeft!.Value is null)
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

                            if (innerRight?.OperandLeft!.OperandLeft is null && innerRight?.OperandLeft!.Value is null)
                            {
                                innerRight!.OperandLeft = expression;
                            }
                            else
                            {
                                Expression? iter = innerRight;
                                while (iter.OperandRight is not null)
                                {
                                    iter = iter.OperandRight;
                                }

                                iter.OperandRight = new Expression { OperandLeft = new Expression { Value = solveMethod(expression) } };
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

                            innerRight.OperandLeft = new Expression { Value = solveMethod(expression) };
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

            var result = solveMethod(main);
            //Console.WriteLine(result);

            return result;
        }

        internal static string ReverseLineAndAddSpaces(string line)
        {
            return string.Join("", line.Reverse())
                .Replace(")", "/")
                .Replace("(", " )")
                .Replace("/", "( ");
        }

        private IEnumerable<string> ParseInput()
        {
            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                yield return ReverseLineAndAddSpaces(line);
            }
        }

        internal enum Operator { None, Plus, Times }

        internal class Expression
        {
            public long? Value { get; set; }

            public long? Multiplier { get; set; }

            public Expression? OperandLeft { get; set; }

            public Expression? OperandRight { get; set; }

            public Operator Operator { get; set; }

            public long SolvePart1()
            {
                return Operator switch
                {
                    Operator.Plus => OperandLeft!.SolvePart1() + (OperandRight?.SolvePart1() ?? 0),
                    Operator.Times => OperandLeft!.SolvePart1() * (OperandRight?.SolvePart1() ?? 1),
                    _ => Value ?? OperandLeft!.SolvePart1()
                };
            }

            public long SolvePart2()
            {
                long result = 0;

                result += Operator switch
                {
                    Operator.None => ((Value ?? 0) * (Multiplier ?? 1)) + (OperandLeft?.SolvePart2() ?? 0),
                    Operator.Plus => PlusOperator(),
                    Operator.Times => TimesOperator(),
                    _ => throw new SolvingException()
                };

                return result;

                long PlusOperator()
                {
                    AddOrCreateValue(OperandRight, (Value ?? 0) + OperandLeft!.SolvePart2());

                    return OperandRight!.SolvePart2();
                }

                long TimesOperator()
                {
                    if (OperandRight?.Operator == Operator.Times)
                    {
                        AddOrCreateValue(OperandLeft, Value ?? 0);
                        AddOrCreateMultiplier(OperandLeft, Multiplier ?? 1);

                        AddOrCreateMultiplier(OperandRight, OperandLeft!.SolvePart2());

                        return OperandRight?.SolvePart2() ?? 1;
                    }
                    else
                    {
                        AddOrCreateValue(OperandLeft, Value ?? 0);
                        AddOrCreateMultiplier(OperandLeft, Multiplier ?? 1);

                        return OperandLeft!.SolvePart2() * (OperandRight?.SolvePart2() ?? 1);
                    }
                }

                static void AddOrCreateValue(Expression? expression, long value)
                {
                    if (expression is null)
                    {
                        throw new SolvingException();
                    }

                    if (expression.Value is null)
                    {
                        expression.Value = value;
                    }
                    else
                    {
                        expression.Value += value;
                    }
                }

                static void AddOrCreateMultiplier(Expression? expression, long multiplier)
                {
                    if (expression is null)
                    {
                        throw new SolvingException();
                    }

                    if (expression.Multiplier is null)
                    {
                        expression.Multiplier = multiplier;
                    }
                    else
                    {
                        expression.Multiplier *= multiplier;
                    }
                }
            }
        }
    }
}
