using AoCHelper;
using FileParser;
using SheepTools.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_99 : BaseDay
    {
        private readonly List<Tile> _pieces;

        public Day_99()
        {
            _pieces = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            var tileSides =
                _pieces.ConvertAll(tile => (
                    tile,
                    candidateNeighbours: new Dictionary<Tile, HashSet<string>>(),
                    tile.Sides));

            foreach (var tuple in tileSides)
            {
                foreach (var (tile, candidateNeighbours, Sides) in tileSides.Except(new[] { tuple }))
                {
                    var sharedSides = Sides.Intersect(tuple.Sides);

                    if (sharedSides.Any())
                    {
                        tuple.candidateNeighbours.Add(tile, sharedSides.ToHashSet());
                    }
                }
            }

            var candidateCorners = tileSides.Where(tuple => tuple.candidateNeighbours.Count == 2);
            var candidateNonCornerSides = tileSides.Where(tuple => tuple.candidateNeighbours.Count == 3);
            var middle = tileSides.Where(tuple => tuple.candidateNeighbours.Count > 3);

            return candidateCorners.Aggregate((long)1, (total, corner) => total * corner.tile.Id)
                .ToString();
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
            //        _rotated = new Dictionary<Direction, Tile>
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

            //public Tile Transform(Direction direction, string side)
            //{
            //    if (_rotated.Count == 0)
            //    {
            //        _rotated = new Dictionary<Direction, Tile>
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
    }
}

