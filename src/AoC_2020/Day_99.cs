using AoCHelper;
using FileParser;
using SheepTools.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nodes = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<AoC_2020.Piece, System.Collections.Generic.HashSet<string>>>;

namespace AoC_2020
{
    public class Day_99 : BaseDay
    {
        private readonly List<Piece> _pieces;

        public Day_99()
        {
            _pieces = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            var pieceWithCandidateNeighbours = _pieces.Select(piece =>
                 {
                     var candidateNeighbours = new Dictionary<Piece, HashSet<string>>();

                     foreach (var otherPiece in _pieces.Except(new[] { piece }))
                     {
                         var sharedSides = piece.PossibleSides.Intersect(otherPiece.PossibleSides);
                         if (sharedSides.Any())
                         {
                             candidateNeighbours.Add(otherPiece, sharedSides.ToHashSet());
                         }
                     }
                     return (piece, candidateNeighbours);
                 });

            var candidateCorners = pieceWithCandidateNeighbours.Where(tuple => tuple.candidateNeighbours.Count == 2);

            return candidateCorners.Aggregate((long)1, (total, corner) => total * corner.piece.Id)
                .ToString();
        }

        public override string Solve_2()
        {
            var nodes = _pieces.ToDictionary(piece => piece.Id, piece =>
            {
                var candidateNeighbours = new Dictionary<Piece, HashSet<string>>();

                foreach (var otherPiece in _pieces.Except(new[] { piece }))
                {
                    var sharedSides = piece.PossibleSides.Intersect(otherPiece.PossibleSides);
                    if (sharedSides.Any())
                    {
                        candidateNeighbours.Add(otherPiece, sharedSides.ToHashSet());
                    }
                }
                return candidateNeighbours;
            });

            var sol = DepthFirstSearch(nodes, _pieces);

            var orderedPuzzle = sol
                    .OrderBy(sol => sol.Position.Y)
                    .ThenBy(sol => sol.Position.X)
                    .ToList();

            var sideLength = Convert.ToInt32(Math.Sqrt(nodes.Count));

            for (int i = 0; i < orderedPuzzle.Count; ++i)
            {
                if (i % sideLength == 0)
                {
                    Console.WriteLine();
                }

                Console.Write($"{orderedPuzzle[i].Piece.Id} ");
            }

            return string.Join(",", orderedPuzzle.Select(s => s.Piece.Id));
        }

        internal static List<(Piece Piece, IntPoint Position)> DepthFirstSearch(Nodes nodes, List<Piece> pieces)
        {
            var sideLength = Convert.ToInt32(Math.Sqrt(nodes.Count));

            var corners = nodes.Where(n => n.Value.Count == 2);

            var initialPiece = pieces.Single(p => p.Id == corners.First().Key);

            var stack = new Stack<(Piece Piece, int ParentIndex, Direction ParentDirection)>();
            stack.Push((initialPiece, -1, Direction.Up));

            var solutions = new Dictionary<int, List<(Piece Node, IntPoint Position)>>();

            Piece currentPiece = initialPiece;
            var currentSolution = new List<(Piece Node, IntPoint Position)> { (currentPiece, new IntPoint(0, 0)) };

            var expandedNodes = 0;
            bool success = false;

            var childrenToExpand = new Dictionary<int, int>(nodes.Select(n => new KeyValuePair<int, int>(n.Key, 0)));

            var sw = new Stopwatch();
            sw.Start();

            while (stack.Count > 0)
            {
                ++expandedNodes;
                if (expandedNodes % 250_000 == 0)
                {
                    Console.WriteLine($"\tTime: {0.001 * sw.ElapsedMilliseconds:F3}");
                    Console.WriteLine($"\tIndex: {expandedNodes}, stack: {stack.Count}");
                }

                var current = stack.Pop();

                currentPiece = current.Piece;
                var parentIndex = current.ParentIndex;
                var parentDirection = current.ParentDirection;

                if (parentIndex != -1)
                {
                    var previousPiece = solutions[parentIndex].Last();

                    var currentPosition = previousPiece.Position.Move(parentDirection);

                    // Modifying FreeDirections
                    // TODO: Investigate how to make this work, since it modifies all the copies of that piece.
                    // Does it matter? The only permanent ones are the initial ones, and we only check the latest one appended:
                    // never going to be affected?

                    //if (parentIndex != 0 || parentIndex != -1)
                    //{
                    //    switch (parentDirection)
                    //    {
                    //        case Direction.Up:
                    //            //previousPiece.Node.FreeDirections.Remove(Direction.Up);
                    //            currentPiece.FreeDirections.Remove(Direction.Down);
                    //            break;
                    //        case Direction.Down:
                    //            //previousPiece.Node.FreeDirections.Remove(Direction.Down);
                    //            currentPiece.FreeDirections.Remove(Direction.Up);
                    //            break;
                    //        case Direction.Left:
                    //            //previousPiece.Node.FreeDirections.Remove(Direction.Left);
                    //            currentPiece.FreeDirections.Remove(Direction.Right);
                    //            break;
                    //        case Direction.Right:
                    //            //previousPiece.Node.FreeDirections.Remove(Direction.Right);
                    //            currentPiece.FreeDirections.Remove(Direction.Left);
                    //            break;
                    //        default:
                    //            throw new SolvingException();
                    //    }
                    //}

                    currentSolution = solutions[parentIndex].Append((currentPiece, currentPosition)).ToList();

                    var minX = currentSolution.Min(sol => sol.Position.X);
                    var maxX = currentSolution.Max(sol => sol.Position.X);
                    var minY = currentSolution.Min(sol => sol.Position.Y);
                    var maxY = currentSolution.Max(sol => sol.Position.Y);

                    bool outsideOfSquare =
                        Math.Abs(maxX - minX) >= sideLength
                        || Math.Abs(maxY - minY) >= sideLength;

                    bool overlayingPieces = currentSolution.Select(s => s.Position).Distinct().Count() != currentSolution.Count
                        || currentSolution.Count > pieces.Count;

                    if (outsideOfSquare || overlayingPieces)
                    {
                        if (parentIndex != -1 && --childrenToExpand[solutions[parentIndex].Last().Node.Id] == 0)
                        {
                            solutions[parentIndex].Last().Node.FreeDirections =
                                new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                        }
                        continue;
                    }
                }

                solutions.Add(expandedNodes, currentSolution);

                if (currentSolution.Count == sideLength * sideLength)
                {
                    success = true;
                    break;
                }

                foreach (var candidate in GetCandidates(currentPiece, expandedNodes, nodes, currentSolution))
                {
                    stack.Push(candidate);
                    ++childrenToExpand[currentPiece.Id];
                }

                if (parentIndex != -1 && --childrenToExpand[solutions[parentIndex].Last().Node.Id] == 0)
                {
                    solutions[parentIndex].Last().Node.FreeDirections =
                        new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                }
            }

            var sols = solutions.Select(s => string.Join(",", s.Value.Select(p => p.Node.Id))).ToList();

            Console.WriteLine(sols);

            return success
                ? solutions[expandedNodes]
                : throw new SolvingException($"{nameof(DepthFirstSearch)} couldn't find a solution");
        }

        private static IEnumerable<(Piece Piece, int ParentIndex, Direction ParentDirection)> GetCandidates(Piece piece, int index, Nodes nodes, List<(Piece Node, IntPoint Position)> solution)
        {
            var freeSides = piece.FreeSides.ToList();

            var usedIds = solution.ConvertAll(sol => sol.Node.Id);
            foreach (var candidatePiece in nodes[piece.Id].Where(pair => !usedIds.Contains(pair.Key.Id)))
            {
                if (candidatePiece.Key.Id == piece.Id) continue;

                var validSharedSideList = freeSides
                    .Where(pair => candidatePiece.Value.Contains(pair.side));

                foreach (var (direction, side) in validSharedSideList)
                {
                    var directionInCandidate = (Direction)(((int)direction + 2) % 4);

                    var mutatedCandidate = candidatePiece.Key.MutateToHave(side, directionInCandidate);

                    Debug.Assert(direction switch
                    {
                        Direction.Up => piece.Top == mutatedCandidate.Bottom,
                        Direction.Down => piece.Bottom == mutatedCandidate.Top,
                        Direction.Left => piece.Left == mutatedCandidate.Right,
                        Direction.Right => piece.Right == mutatedCandidate.Left,
                        _ => false
                    });

                    yield return (mutatedCandidate, index, direction);
                }
            }
        }

        private IEnumerable<Piece> ParseInput()
        {
            foreach (var pieceBlock in ParsedFile.ReadAllGroupsOfLines(InputFilePath))
            {
                var pieceId = int.Parse(pieceBlock[0].Split(" ")[1][..^1]);

                var bitArrayList = new List<BitArray>();
                foreach (var line in pieceBlock.Skip(1))
                {
                    bitArrayList.Add(new BitArray(line.Select(ch => ch == '#').ToArray()));
                }

                yield return new Piece(pieceId, bitArrayList);
            }
        }
    }

    internal class Piece
    {
        private BitMatrix _matrix = null!;

        private string _top = null!;
        private string _bottom = null!;
        private string _left = null!;
        private string _right = null!;

        private string _topReversed = null!;
        private string _bottomReversed = null!;
        private string _leftReversed = null!;
        private string _rightReversed = null!;

        public string Top => _top;

        public string Bottom => _bottom;

        public string Left => _left;

        public string Right => _right;

        public int Id { get; private init; }

        /// <summary>
        /// [Non placed] Sides as outcome of rotating and/or flipping the piece
        /// </summary>
        public HashSet<string> PossibleSides { get; private set; } = null!;

        /// <summary>
        /// [Placed] Free directions of a piece that has already been placed
        /// </summary>
        public ICollection<Direction> FreeDirections { get; set; }

        /// <summary>
        /// [Placed] Free sides of a piece that has already been placed, each one corresponding to one <see cref="FreeDirections"/>
        /// </summary>
        public IEnumerable<(Direction direction, string side)> FreeSides
        {
            get
            {
                foreach (var dir in FreeDirections)
                {
                    yield return (
                        dir,
                        dir switch
                        {
                            Direction.Up => _top,
                            Direction.Down => _bottom,
                            Direction.Left => _left,
                            Direction.Right => _right,
                            _ => throw new SolvingException()
                        });
                }
            }
        }

        public List<BitArray> Content => _matrix.Content;

        public Piece(int id, List<BitArray> content)
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

            PossibleSides = new HashSet<string> {
                    _top, _topReversed,
                    _bottom, _bottomReversed,
                    _left, _leftReversed,
                    _right, _rightReversed,
                };

            FreeDirections = new List<Direction> { Direction.Up, Direction.Down, Direction.Right, Direction.Left };
        }

        //public Direction GetSideDirection(string side)
        //{
        //    //var top = Content[0].ToBitString();
        //    //if (side == top || side == ReverseString(top))
        //    //{
        //    //    return Direction.Up;
        //    //}

        //    //var bottom = Content.Last().ToBitString();
        //    //if (side == bottom || side == ReverseString(bottom))
        //    //{
        //    //    return Direction.Down;
        //    //}

        //    //var left = new BitArray(Content.Select(arr => arr[0]).ToArray()).ToBitString();
        //    //if (side == left || side == ReverseString(left))
        //    //{
        //    //    return Direction.Left;
        //    //}

        //    //var right = new BitArray(Content.Select(arr => arr[^1]).ToArray()).ToBitString();
        //    //if (side == right || side == ReverseString(right))
        //    //{
        //    //    return Direction.Right;
        //    //}

        //    //throw new SolvingException();



        //    if (side == _top || side == _topReversed)
        //    {
        //        return Direction.Up;
        //    }

        //    if (side == _bottom || side == _bottomReversed)
        //    {
        //        return Direction.Down;
        //    }

        //    if (side == _left || side == _leftReversed)
        //    {
        //        return Direction.Left;
        //    }

        //    if (side == _right || side == _rightReversed)
        //    {
        //        return Direction.Right;
        //    }

        //    throw new SolvingException();

        //    if (_rotated.Count == 0)
        //    {
        //        _rotated = new Dictionary<Direction, Piece>
        //        {
        //            [Direction.Left] = RotateAnticlockwise(),
        //            [Direction.Right] = RotateClockwise(),
        //            [Direction.Down] = Rotate180(),
        //            [Direction.Up] = this,
        //        };
        //    }

        //    var tops = _rotated.SelectMany(r => new[] { r.Value._top, r.Value._topReversed });
        //    var bottoms = _rotated.SelectMany(r => new[] { r.Value._bottom, r.Value._bottomReversed });
        //    var lefts = _rotated.SelectMany(r => new[] { r.Value._left, r.Value._leftReversed });
        //    var rights = _rotated.SelectMany(r => new[] { r.Value._right, r.Value._rightReversed });

        //    if (tops.Contains(side))
        //    {
        //        return Direction.Up;
        //    }

        //    if (bottoms.Contains(side))
        //    {
        //        return Direction.Down;
        //    }

        //    if (lefts.Contains(side))
        //    {
        //        return Direction.Left;
        //    }

        //    if (rights.Contains(side))
        //    {
        //        return Direction.Right;
        //    }

        //    throw new SolvingException();
        //}

        //public Piece Transform(Direction direction, string side)
        //{
        //    if (_rotated.Count == 0)
        //    {
        //        _rotated = new Dictionary<Direction, Piece>
        //        {
        //            [Direction.Left] = RotateAnticlockwise(),
        //            [Direction.Right] = RotateClockwise(),
        //            [Direction.Down] = Rotate180(),
        //            [Direction.Up] = this,
        //        };
        //    }

        //    if (GetSideDirection(side) == direction) return this;

        //    foreach (var rotation in _rotated)
        //    {
        //        if (rotation.Value.GetSideDirection(side) == direction)
        //        {
        //            SetBitMatrix(rotation.Value.Content);

        //            return rotation.Value;          // TODO remove
        //        }
        //    }

        //    return null;
        //    //throw new SolvingException();

        //    //while (GetSideDirection(side) != direction)
        //    //{
        //    //    SetBitMatrix(_matrix.RotateClockwise());
        //    //}
        //}

        /// <summary>
        /// [Non placed] Actual pieces outcome of rotating/flipping this one that have <paramref name="side"/>
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public Piece MutateToHave(string side, Direction direction)
        {
            if (direction == Direction.Up)
            {
                if (side == _top)
                {
                    return this;
                }
                else if (side == _topReversed)
                {
                    return this.FlipLeftRight();
                }
                //
                else if (side == _bottom)
                {
                    return FlipUpsideDown();
                }
                else if (side == _bottomReversed)
                {
                    return Rotate180();   // Or FlipUpsideDown().FlipLeftRight() :D
                }
                //
                else if (side == _left)
                {
                    return RotateClockwise().FlipLeftRight();
                }
                else if (side == _leftReversed)
                {
                    return RotateClockwise();
                }
                //
                else if (side == _right)
                {
                    return RotateAnticlockwise();
                }
                else if (side == _rightReversed)
                {
                    return RotateAnticlockwise().FlipLeftRight();
                }
            }

            if (direction == Direction.Down)
            {
                if (side == _top)
                {
                    return FlipUpsideDown();
                }
                else if (side == _topReversed)
                {
                    return Rotate180();   // Or FlipUpsideDown().FlipLeftRight() :D
                }
                //
                else if (side == _bottom)
                {
                    return this;
                }
                else if (side == _bottomReversed)
                {
                    return this.FlipLeftRight();
                }
                //
                else if (side == _left)
                {
                    return RotateAnticlockwise();
                }
                else if (side == _leftReversed)
                {
                    return RotateAnticlockwise().FlipLeftRight();
                }
                //
                else if (side == _right)
                {
                    return RotateClockwise().FlipLeftRight();
                }
                else if (side == _rightReversed)
                {
                    return RotateClockwise();
                }
            }

            if (direction == Direction.Right)
            {
                if (side == _top)
                {
                    return RotateClockwise();
                }
                else if (side == _topReversed)
                {
                    return RotateClockwise().FlipUpsideDown();
                }
                //
                else if (side == _bottom)
                {
                    return RotateAnticlockwise().FlipUpsideDown();
                }
                else if (side == _bottomReversed)
                {
                    return RotateAnticlockwise();
                }
                //
                else if (side == _left)
                {
                    return FlipLeftRight();
                }
                else if (side == _leftReversed)
                {
                    return Rotate180();   // Or FlipUpsideDown().FlipLeftRight() :D
                }
                //
                else if (side == _right)
                {
                    return this;
                }
                else if (side == _rightReversed)
                {
                    return this.FlipUpsideDown();
                }
            }

            if (direction == Direction.Left)
            {
                if (side == _top)
                {
                    return RotateAnticlockwise().FlipUpsideDown();
                }
                else if (side == _topReversed)
                {
                    return RotateAnticlockwise();
                }
                //
                else if (side == _bottom)
                {
                    return RotateClockwise();
                }
                else if (side == _bottomReversed)
                {
                    return RotateClockwise().FlipUpsideDown();
                }
                //
                else if (side == _left)
                {
                    return this;
                }
                else if (side == _leftReversed)
                {
                    return this.FlipUpsideDown();
                }
                //
                else if (side == _right)
                {
                    return FlipLeftRight();
                }
                else if (side == _rightReversed)
                {
                    return Rotate180();   // Or FlipUpsideDown().FlipLeftRight() :D
                }
            }

            throw new SolvingException($"Id {Id} => MutateToHave(Side {side}, Direction {direction})");
        }

        public Piece RotateClockwise() => new Piece(Id, _matrix.RotateClockwise());

        public Piece RotateAnticlockwise() => new Piece(Id, _matrix.RotateAnticlockwise());

        public Piece Rotate180() => new Piece(Id, _matrix.Rotate180());

        public Piece FlipUpsideDown() => new Piece(Id, _matrix.FlipUpsideDown());

        public Piece FlipLeftRight() => new Piece(Id, _matrix.FlipLeftRight());

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
}

