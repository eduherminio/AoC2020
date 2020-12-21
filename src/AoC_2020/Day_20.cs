using AoCHelper;
using FileParser;
using SheepTools.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            //var result = BreathFirstSearch(sideTileList, solutionSquareSide);
            var result = BreathFirstSearchConcurrent(sideTileList, solutionSquareSide);

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

                var expandedNodes = 0;
                bool success = false;

                var queue = new Queue<(Tile Node, int Parent, string SharedSide)>(initialGroup.Select(node => (node, -1, string.Empty)));
                var solutions = new Dictionary<int, List<(Tile Node, IntPoint Position)>>();


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
                        //currentNode.Transform(relativePosition, sharedSide);
                        var opposite = (Direction)(((int)relativePosition + 2) % 4);
                        currentNode = currentNode.Transform(opposite, sharedSide);

                        if (currentNode is null)
                        {
                            continue;
                        }
                        var currentPosition = previousPiece.Position.Move(relativePosition);

                        currentSolution = solutions[parentIndex].Append((currentNode, currentPosition)).ToList();
                    }

                    var minX = currentSolution.Min(sol => sol.Position.X);
                    var maxX = currentSolution.Max(sol => sol.Position.X);
                    var minY = currentSolution.Min(sol => sol.Position.Y);
                    var maxY = currentSolution.Max(sol => sol.Position.Y);

                    if (
                        currentSolution.Select(s => s.Position).Distinct().Count() != currentSolution.Count
                        || Math.Abs(maxX - minX) >= solutionSquareSide
                        || Math.Abs(maxY - minY) >= solutionSquareSide)
                    {
                        continue;
                    }

                    solutions.Add(expandedNodes, currentSolution);

                    if (currentSolution.Count == solutionSquareSide * solutionSquareSide)
                    {
                        success = true;
                        break;
                    }

                    //foreach (var candidate in GetCandidates(currentNode, expandedNodes, nodes, currentSolution))
                    foreach (var candidate in GetCandidates_Efficient(currentNode, expandedNodes, groups, currentSolution))
                    {
                        queue.Enqueue(candidate);
                    }
                }

                if (success)
                {
                    Console.WriteLine($"Total expanded nodes: {expandedNodes}");

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

        internal static long BreathFirstSearchConcurrent(List<Tile> nodes, int solutionSquareSide)
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

                bool success = false;
                var expandedNodes = 0;

                var queue = new ConcurrentQueue<(Tile Node, int Parent, string SharedSide)>(initialGroup.Select(node => (node, -1, string.Empty)));
                var solutions = new ConcurrentDictionary<int, List<(Tile Node, IntPoint Position)>>();

                var tasks = new List<Task>();

                ThreadPool.SetMaxThreads(8, 4);

                while (!queue.IsEmpty || tasks.Any())
                {
                    if (expandedNodes % 250_000 == 0)
                    {
                        Console.WriteLine($"\tTime: {0.001 * sw.ElapsedMilliseconds:F3}");
                        Console.WriteLine($"\tIndex: {expandedNodes}, queue: {queue.Count}");
                    }
                    if (queue.TryDequeue(out var currentTuple))
                    {
                        ++expandedNodes;
                        tasks.Add(
                            Task.Run(() => Expand(solutionSquareSide, expandedNodes, currentTuple, nodes, queue, solutions)));
                    }

                    static bool Expand(int solutionSquareSide, int expandedNodes, (Tile Node, int Parent, string SharedSide) currentTuple,
                        List<Tile> nodes,
                        ConcurrentQueue<(Tile Node, int Parent, string SharedSide)> queue,
                        ConcurrentDictionary<int, List<(Tile Node, IntPoint Position)>> solutions)
                    {
                        List<(Tile Node, IntPoint Position)> currentSolution;

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

                            var opposite = (Direction)(((int)relativePosition + 2) % Enum.GetNames(typeof(Direction)).Length);
                            currentNode = currentNode.Transform(opposite, sharedSide);

                            var currentPosition = previousPiece.Position.Move(relativePosition);

                            currentSolution = solutions[parentIndex].Append((currentNode, currentPosition)).ToList();
                        }

                        var minX = currentSolution.Min(sol => sol.Position.X);
                        var maxX = currentSolution.Max(sol => sol.Position.X);
                        var minY = currentSolution.Min(sol => sol.Position.Y);
                        var maxY = currentSolution.Max(sol => sol.Position.Y);

                        if (
                            currentSolution.Select(s => s.Position).Distinct().Count() != currentSolution.Count
                            || Math.Abs(maxX - minX) >= solutionSquareSide
                            || Math.Abs(maxY - minY) >= solutionSquareSide)
                        {
                            return false;
                        }

                        solutions.TryAdd(expandedNodes, currentSolution);

                        if (currentSolution.Count == solutionSquareSide * solutionSquareSide)
                        {
                            var finalSolution = solutions[expandedNodes];

                            var corners = finalSolution
                                .Where(sol =>
                                    (sol.Position.X == minX && sol.Position.Y == minY)
                                    || (sol.Position.X == minX && sol.Position.Y == maxY)
                                    || (sol.Position.X == maxX && sol.Position.Y == minY)
                                    || (sol.Position.X == maxX && sol.Position.Y == maxY))
                                .ToList();

                            Debug.Assert(corners.Count == 4);

                            var result = corners.Aggregate((long)1, (total, item) => total * item.Node.Id);

                            Console.WriteLine(result);

                            return true;
                        }

                        foreach (var candidate in GetCandidates(currentNode, expandedNodes, nodes, currentSolution))
                        {
                            queue.Enqueue(candidate);
                        }

                        return false;
                    }
                }

                if (success)
                {
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

        internal static IEnumerable<(Tile, int, string)> GetCandidates(Tile currentNode, int index,
            List<Tile> nodes, List<(Tile Node, IntPoint Position)> solution)
        {
            foreach (var candidate in nodes.Where(node => !solution.Select(s => s.Node.Id).Contains(node.Id)))
            {
                foreach (var side in candidate.Sides.Where(s => currentNode.Sides.Contains(s)))
                {
                    yield return (candidate, index, side);
                }
            }
        }

        internal static IEnumerable<(Tile, int, string)> GetCandidates_Efficient(
            Tile currentNode, int index,
            List<IGrouping<int, Tile>> nodes, List<(Tile Node, IntPoint Position)> solution)
        {
            var usedIds = solution.Select(s => s.Node.Id);
            foreach (var candidate in nodes.Where(g => !usedIds.Contains(g.Key)).SelectMany(g => g))
            {
                foreach (var side in candidate.Sides.Where(s => currentNode.Sides.Contains(s)))
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

                if (_rotated.Count == 0)
                {
                    _rotated = new Dictionary<Direction, Tile>
                    {
                        [Direction.Left] = RotateAnticlockwise(),
                        [Direction.Right] = RotateClockwise(),
                        [Direction.Down] = Rotate180(),
                        [Direction.Up] = this,
                    };
                }

                var tops = _rotated.SelectMany(r => new[] { r.Value._top, r.Value._topReversed });
                var bottoms = _rotated.SelectMany(r => new[] { r.Value._bottom, r.Value._bottomReversed });
                var lefts = _rotated.SelectMany(r => new[] { r.Value._left, r.Value._leftReversed });
                var rights = _rotated.SelectMany(r => new[] { r.Value._right, r.Value._rightReversed });

                if (tops.Contains(side))
                {
                    return Direction.Up;
                }

                if (bottoms.Contains(side))
                {
                    return Direction.Down;
                }

                if (lefts.Contains(side))
                {
                    return Direction.Left;
                }

                if (rights.Contains(side))
                {
                    return Direction.Right;
                }

                throw new SolvingException();
            }


            public Tile Transform(Direction direction, string side)
            {
                if (_rotated.Count == 0)
                {
                    _rotated = new Dictionary<Direction, Tile>
                    {
                        [Direction.Left] = RotateAnticlockwise(),
                        [Direction.Right] = RotateClockwise(),
                        [Direction.Down] = Rotate180(),
                        [Direction.Up] = this,
                    };
                }

                if (GetSideDirection(side) == direction) return this;

                foreach (var rotation in _rotated)
                {
                    if (rotation.Value.GetSideDirection(side) == direction)
                    {
                        SetBitMatrix(rotation.Value.Content);

                        return rotation.Value;          // TODO remove
                    }
                }

                return null;
                //throw new SolvingException();

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
