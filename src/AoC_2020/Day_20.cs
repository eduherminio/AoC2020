using AoCHelper;
using FileParser;
using SheepTools.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_20 : BaseDay
    {
        private readonly List<Tile> _input;

        public Day_20()
        {
            _input = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            //var sideTileList = _input.SelectMany(tile => Modification.GetValues(typeof(Modification)).Cast<Modification>().Select(mod => new Node(tile, mod))).ToList();
            var sideTileList = _input.SelectMany(tile => new[] {
                 tile,
                 tile.FlipUpsideDown(),
                 tile.FlipLeftRight()
            }).ToList();

            int solutionSquareSide = Convert.ToInt32(Math.Sqrt(_input.Count));
            var result = BreathFirstSearch(sideTileList, solutionSquareSide);

            return result.ToString();
        }

        internal static long BreathFirstSearch(List<Tile> nodes, int solutionSquareSide)
        {
            var groups = nodes.GroupBy(node => node.Id)
                .ToList();

            // If the selected one is not in a corner, there's no guarantee that going one by one using only the previous one we'll find a way
            var sw = new Stopwatch();
            sw.Start();
            for (int groupIndex = 0; groupIndex < groups.Count; ++groupIndex)
            {
                Console.WriteLine($"Time: {0.001 * sw.ElapsedMilliseconds:F3} Group: {groups[groupIndex].Key}");

                var initialGroup = groups[groupIndex];

                //var index = -1;
                var expandedNodes = 0;

                //var queue = new List<(Tile Node, int Parent, string SharedSide)>(initialGroup.Select(node => (node, index, string.Empty)));
                var queue = new Queue<(Tile Node, int Parent, string SharedSide)>(initialGroup.Select(node => (node, -1, string.Empty)));

                //var solutions = new List<List<(Tile Node, IntPoint Position)>>();
                var solutions = new Dictionary<int, List<(Tile Node, IntPoint Position)>>();

                bool success = false;
                //index = 0;

                //while (index < queue.Count)
                while (queue.Count > 0)
                {
                    ++expandedNodes;
                    if (expandedNodes % 250_000 == 0)
                    {
                        Console.WriteLine($"\tTime: {0.001 * sw.ElapsedMilliseconds:F3}");
                        Console.WriteLine($"\tIndex: {expandedNodes}, queue: {queue.Count}");
                    }

                    List<(Tile Node, IntPoint Position)> currentSolution;
                    //var currentTuple = queue[index];
                    var currentTuple = queue.Dequeue();

                    var parentIndex = currentTuple.Parent;
                    var currentNode = currentTuple.Node;
                    var sharedSide = currentTuple.SharedSide;

                    // Calculate current solution from parent solution
                    if (parentIndex == -1)
                    {
                        currentSolution = new List<(Tile Node, IntPoint Position)> { (currentNode, new IntPoint(0, 0)) };
                    }
                    else
                    {
                        var previousPiece = solutions[parentIndex].Last();

                        var relativePosition = previousPiece.Node.GetSideDirection(sharedSide);
                        currentNode.Transform(relativePosition, sharedSide);
                        var currentPosition = previousPiece.Position.Move(relativePosition);

                        currentSolution = solutions[parentIndex].Append((currentNode, currentPosition)).ToList();
                    }
                    //solutions.Add(currentSolution);
                    //solutions.Add(expandedNodes, currentSolution);

                    var minX = currentSolution.Min(sol => sol.Position.X);
                    var maxX = currentSolution.Max(sol => sol.Position.X);
                    var minY = currentSolution.Min(sol => sol.Position.Y);
                    var maxY = currentSolution.Max(sol => sol.Position.Y);

                    if (
                        currentSolution.Select(s => s.Position).Distinct().Count() != currentSolution.Count
                        || Math.Abs(maxX - minX) >= solutionSquareSide
                        || Math.Abs(maxY - minY) >= solutionSquareSide)
                    {
                        //++index;
                        //currentSolution.Clear();
                        //currentNode.Tile.MarkAsInvalid();
                        continue;
                    }

                    solutions.Add(expandedNodes, currentSolution);

                    if (currentSolution.Count == solutionSquareSide * solutionSquareSide)
                    {
                        success = true;
                        break;
                    }

                    //foreach (var candidate in GetCandidates(currentNode, index, nodes, currentSolution))
                    foreach (var candidate in GetCandidates(currentNode, expandedNodes, nodes, currentSolution))
                    {
                        //queue.Add(candidate);
                        queue.Enqueue(candidate);
                    }

                    //++index;
                }

                if (success)
                {
                    //var finalSolution = solutions[index];
                    var finalSolution = solutions[expandedNodes];
                    var minX = finalSolution.Min(sol => sol.Position.X);
                    var maxX = finalSolution.Max(sol => sol.Position.X);
                    var minY = finalSolution.Min(sol => sol.Position.Y);
                    var maxY = finalSolution.Max(sol => sol.Position.Y);

                    var corners = finalSolution
                        .Where(sol =>
                            (sol.Position.X == minX && sol.Position.Y == minY)
                            || (sol.Position.X == minX && sol.Position.Y == maxY)
                            || (sol.Position.X == maxX && sol.Position.Y == minY)
                            || (sol.Position.X == maxX && sol.Position.Y == maxY))
                        .ToList();

                    Debug.Assert(corners.Count == 4);

                    return corners.Aggregate((long)1, (total, item) => total * item.Node.Id);
                }
            }

            throw new SolvingException("No solution found");
        }

        internal static long DepthFirstSearch(List<Tile> nodes)
        {
            return 0;
        }

        internal static IEnumerable<(Tile, int, string)> GetCandidates(Tile currentNode, int index, List<Tile> nodes, List<(Tile Node, IntPoint Position)> solution)
        {
            foreach (var candidate in nodes.Where(node => !solution.Select(s => s.Node.Id).Contains(node.Id)))
            {
                foreach (var side in candidate.Sides.Intersect(currentNode.Sides))
                {
                    yield return (candidate, index, side);
                }
            }
        }

        public string FalseHopesForPart1()
        {
            int side = Convert.ToInt32(Math.Sqrt(_input.Count));
            var result = new List<List<Tile>>(side);
            for (int i = 0; i < side; ++i)
            {
                result.Add(new List<Tile>(side));
            }


            var tileSides =
                _input.Select(tile => (
                    tile,
                    candidateNeighbours: new List<(Tile, string sharedSide)>(),
                    sides: new[] {
                        tile.Content[0].ToBitString(),
                        tile.Content[0].Reverse().ToBitString(),

                        tile.Content.Last().ToBitString(),
                        tile.Content.Last().Reverse().ToBitString(),

                        new BitArray(tile.Content.Select(arr => arr[0]).ToArray()).ToBitString(),
                        new BitArray(tile.Content.Select(arr => arr[0]).Reverse().ToArray()).ToBitString(),

                        new BitArray(tile.Content.Select(arr => arr[^1]).ToArray()).ToBitString(),
                        new BitArray(tile.Content.Select(arr => arr[^1]).Reverse().ToArray()).ToBitString()
                    }))
                .ToList();

            foreach (var tuple in tileSides)
            {
                foreach (var candidate in tileSides.Except(new[] { tuple }))
                {
                    foreach (var possibleSharedSide in candidate.sides.Intersect(tuple.sides))
                    {
                        tuple.candidateNeighbours.Add((candidate.tile, possibleSharedSide));
                    }
                }
            }

            var candidateCorners = tileSides.Where(t => t.candidateNeighbours.Count >= 2);
            var candidateBorders = tileSides.Where(t => t.candidateNeighbours.Count >= 3);

            var solution = string.Empty;

            return solution;
        }

        public override string Solve_2()
        {
            var solution = string.Empty;

            return solution;
        }

        private IEnumerable<Tile> ParseInput()
        {
            foreach (var tileBlock in ParsedFile.ReadAllGroupsOfLines(InputFilePath))
            {
                var tileId = int.Parse(tileBlock[0].Split(" ")[1][..^1]);

                var bitArrayList = new List<BitArray>();
                foreach (var line in tileBlock.Skip(1))
                {
                    bitArrayList.Add(new BitArray(line.Select(ch => ch == '#').ToArray()));
                }

                yield return new Tile(tileId, bitArrayList);
            }
        }

        internal class Tile
        {
            private BitMatrix _matrix = null!;
            private string _top = null!;
            private string _topReversed = null!;
            private string _bottom = null!;
            private string _bottomReversed = null!;
            private string _left = null!;
            private string _leftReversed = null!;
            private string _right = null!;
            private string _rightReversed = null!;
            private Dictionary<Direction, Tile> _rotated = new Dictionary<Direction, Tile>();

            public int Id { get; private init; }

            public HashSet<string> Sides { get; private set; } = null!;

            public List<BitArray> Content => _matrix.Content;

            public Tile(int id, List<BitArray> content)
            {
                Id = id;
                SetBitMatrix(content);
            }

            private void SetBitMatrix(List<BitArray> content)
            {
                _matrix = new BitMatrix(content);

                _top = _matrix.Content[0].ToBitString();
                _topReversed = ReverseString(_top);

                _bottom = _matrix.Content.Last().ToBitString();
                _bottomReversed = ReverseString(_bottom);

                _left = new BitArray(_matrix.Content.Select(arr => arr[0]).ToArray()).ToBitString();
                _leftReversed = ReverseString(_left);

                _right = new BitArray(_matrix.Content.Select(arr => arr[^1]).ToArray()).ToBitString();
                _rightReversed = ReverseString(_right);

                Sides = new HashSet<string> {
                    _top, _topReversed,
                    _bottom, _bottomReversed,
                    _left, _leftReversed,
                    _right, _rightReversed,
                };
            }

            //public Tile GetModifiedTile(Modification mod)
            //{
            //    return mod switch
            //    {
            //        Modification.Normal => this,
            //        Modification.FlipUpsideDown => FlipUpsideDown(),
            //        Modification.FlipLeftRight => FlipLeftRight(),
            //        _ => throw new SolvingException()
            //    };
            //}

            public Direction GetSideDirection(string side)
            {
                //var top = Content[0].ToBitString();
                //if (side == top || side == ReverseString(top))
                //{
                //    return Direction.Up;
                //}

                //var bottom = Content.Last().ToBitString();
                //if (side == bottom || side == ReverseString(bottom))
                //{
                //    return Direction.Down;
                //}

                //var left = new BitArray(Content.Select(arr => arr[0]).ToArray()).ToBitString();
                //if (side == left || side == ReverseString(left))
                //{
                //    return Direction.Left;
                //}

                //var right = new BitArray(Content.Select(arr => arr[^1]).ToArray()).ToBitString();
                //if (side == right || side == ReverseString(right))
                //{
                //    return Direction.Right;
                //}

                //throw new SolvingException();

                if (side == _top || side == _topReversed)
                {
                    return Direction.Up;
                }

                if (side == _bottom || side == _bottomReversed)
                {
                    return Direction.Down;
                }

                if (side == _left || side == _leftReversed)
                {
                    return Direction.Left;
                }

                if (side == _right || side == _rightReversed)
                {
                    return Direction.Right;
                }

                throw new SolvingException();
            }

            //private HashSet<string> CalculateSides()
            //{
            //    var top = Content[0].ToBitString();
            //    var bottom = Content.Last().ToBitString();
            //    var left = new BitArray(Content.Select(arr => arr[0]).ToArray()).ToBitString();
            //    var right = new BitArray(Content.Select(arr => arr[^1]).ToArray()).ToBitString();

            //    return new HashSet<string>(new[] {
            //        top, ReverseString(top),
            //        bottom, ReverseString(bottom),
            //        left, ReverseString(left),
            //        right, ReverseString(right)
            //        //Content[0].ToBitString(),
            //        //Content.Last().ToBitString(),
            //        //new BitArray(Content.Select(arr => arr[0]).ToArray()).ToBitString(),
            //        //new BitArray(Content.Select(arr => arr[^1]).ToArray()).ToBitString()
            //    });
            //}

            public void Transform(Direction direction, string side)
            {
                if (_rotated.Count == 0)
                {
                    _rotated = new Dictionary<Direction, Tile>
                    {
                        [Direction.Up] = this,
                        [Direction.Left] = RotateAnticlockwise(),
                        [Direction.Right] = RotateClockwise(),
                        [Direction.Down] = Rotate180()
                    };
                }

                foreach (var rotation in _rotated)
                {
                    if (rotation.Value.GetSideDirection(side) == direction)
                    {
                        SetBitMatrix(rotation.Value.Content);
                    }
                }
                //while (GetSideDirection(side) != direction)
                //{
                //    SetBitMatrix(_matrix.RotateClockwise());
                //}
            }

            public Tile RotateClockwise() => new Tile(Id, _matrix.RotateClockwise());

            public Tile RotateAnticlockwise() => new Tile(Id, _matrix.RotateAnticlockwise());

            public Tile Rotate180() => new Tile(Id, _matrix.Rotate180());

            public Tile FlipUpsideDown() => new Tile(Id, _matrix.FlipUpsideDown());

            public Tile FlipLeftRight() => new Tile(Id, _matrix.FlipLeftRight());

            public override string ToString()
            {
                return _matrix.ToString();
            }

            private static string ReverseString(string str)
            {
                char[] charArray = str.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }
        }

        //internal enum Modification { Normal, FlipUpsideDown, FlipLeftRight }

        //internal record Node(Tile Tile);
    }
}
