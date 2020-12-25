using AoCHelper;
using FileParser;
using SheepTools;
using SheepTools.Extensions;
using SheepTools.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AoC_2020
{
    public class Day_20 : BaseDay
    {
        private readonly List<Piece> _pieces;
        private const string BinarySeaMonsterPattern = @"
00000000000000000010
10000110000110000111
01001001001001001000";

        public Day_20()
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
            //return Part2_Search();

            var sideLength = Convert.ToInt32(Math.Sqrt(_pieces.Count));

            var pieceNeighbours = ExtractPieceNeighboursDictionary();

            //var corners = pieceNeighbours.Where(node => node.Value.Count == 2).ToList();
            //Debug.Assert(corners.Count == 4);

            var countourList = ExtractContours(sideLength, pieceNeighbours);

            var orderedContourList = OrderContours(countourList);

            var puzzle = PutTogetherPuzzle(orderedContourList);

            var bitMatrix = RemoveBordersAndJoin(puzzle);

            // Example result -> Need to flip it upside down
            //var str = new BitMatrix(bitMatrix.FlipUpsideDown()).ToString();

            var allPossibleMatrixes = new List<BitMatrix>
            {
                bitMatrix,
                new BitMatrix(bitMatrix.RotateClockwise()),
                new BitMatrix(bitMatrix.RotateAnticlockwise()),
                new BitMatrix(bitMatrix.Rotate180())
            };

            allPossibleMatrixes.AddRange(allPossibleMatrixes.ConvertAll(matrix => new BitMatrix(matrix.FlipLeftRight())));

            var totalOnes = allPossibleMatrixes[0].ToString().Count(ch => ch == '1');
            var onesInMonster = BinarySeaMonsterPattern.Count(ch => ch == '1');

            var monsters = FindMonsters(allPossibleMatrixes);

            var onesNotInMonsters = totalOnes - (onesInMonster * monsters);

            return onesNotInMonsters.ToString();
        }

        private static int FindMonsters(List<BitMatrix> allPossibleMatrixes)
        {
            var monsterBitArrayHits = new Dictionary<BitArray, int>();
            var monsterBitArrayList = new List<BitArray>();
            var monsterLines = BinarySeaMonsterPattern.Split(new[] { "\n", "\r\n" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

            for (int lineIndex = 0; lineIndex < monsterLines.Count; ++lineIndex)
            {
                var line = monsterLines[lineIndex];
                var boolList = new List<bool>(line.Length);

                for (int x = 0; x < line.Length; ++x)
                {
                    if (line[x] == '1')
                    {
                        boolList.Add(true);
                    }
                    else
                    {
                        boolList.Add(false);
                    }
                }

                monsterBitArrayList.Add(new BitArray(boolList.ToArray()));
                monsterBitArrayHits.Add(monsterBitArrayList.Last(), boolList.Count(b => b == true));
                boolList.Clear();
            }

            var seaMonsterX = monsterLines[0].Length;
            var seaMonsterY = monsterLines.Count;          // 3

            var totalSeaMonsters = 0;
            foreach (var matrix in allPossibleMatrixes)
            {
                if (totalSeaMonsters > 0)
                {
                    break;
                }

                int maxY = matrix.Content.Count;
                int maxX = matrix.Content[0].Count;

                for (int y = 0; y < maxY - seaMonsterY; ++y)
                {
                    var lines = new List<string>(seaMonsterY);
                    for (int localY = 0; localY < seaMonsterY; ++localY)
                    {
                        lines.Add(matrix.Content[y + localY].ToBitString());
                    }
                    //var matrix1 = matrix.Content[y + 0].ToBitString().AsSpan();
                    //var matrix2 = matrix.Content[y + 1].ToBitString().AsSpan();
                    //var matrix3 = matrix.Content[y + 2].ToBitString().AsSpan();

                    for (int x = 0; x < maxX - seaMonsterX; ++x)
                    {
                        var seaRegions = new List<string>(seaMonsterY);
                        for (int localY = 0; localY < seaMonsterY; ++localY)
                        {
                            seaRegions.Add(lines[localY].Substring(x, seaMonsterX));
                        }

                        var seaRegionsBitArrayList = seaRegions.ConvertAll(str => new BitArray(str.Select(c => c == '1').ToArray()));

                        // And them, and the result should be equals to the line of the monster
                        var monsters =
                            seaRegionsBitArrayList
                                .Zip(monsterBitArrayList)
                                .All(pair =>
                                {
                                    var result = pair.First.And(pair.Second);

                                    return result.ToBitString() == pair.Second.ToBitString();
                                });

                        if (monsters)
                        {
                            ++totalSeaMonsters;
                        }
                        // seaRegions | bitmonster
                        //var sea1 = matrix1(x + 0, seaMonsterX);
                        //var firstLine = new BitArray()



                        //var contentToSearch =
                    }
                }

            }

            if (totalSeaMonsters == 0)
            {
                throw new SolvingException();
            }

            return totalSeaMonsters;
        }

        /// <summary>
        /// Returns a dictionary with each Piece Id as key and a dictionary of possible neighbours Pieces and their possible shared sides as value
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, Dictionary<Piece, HashSet<string>>> ExtractPieceNeighboursDictionary()
        {
            return _pieces.ToDictionary(piece => piece.Id, piece =>
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
        }

        /// <summary>
        /// Extracts all the groups of sides (named contours) by depth level:
        /// first the real puzzle sides, second the sides if we dont' consider those previous sides, etc.
        /// </summary>
        /// <param name="sideLength"></param>
        /// <param name="pieceNeighbours"></param>
        /// <returns></returns>
        private List<List<List<(Piece Piece, IntPoint Position)>>> ExtractContours(int sideLength, Dictionary<int, Dictionary<Piece, HashSet<string>>> pieceNeighbours)
        {
            List<List<List<(Piece Piece, IntPoint Position)>>>? countourList = new();
            List<KeyValuePair<int, Dictionary<Piece, HashSet<string>>>>? totalSides = new();

            var contourCount = 0;
            while (true)
            {
                var sides = pieceNeighbours.Except(totalSides).Where(node => node.Value.Count <= 3).ToList();

                Debug.Assert(
                    sides.Count ==
                        (2 * (sideLength - (2 * contourCount)))
                        + ((2 * (sideLength - (2 * contourCount) - 2)) > 0
                            ? (2 * (sideLength - (2 * contourCount) - 2))
                            : 0)
                    || sides.Single().Value.Count == 0  // Piece in the center
                            );

                ++contourCount;

                var sideKeys = sides.ConvertAll(s => s.Key);

                var sideNeighbours = _pieces
                    .Where(p => sideKeys.Contains(p.Id))
                    .ToDictionary(piece => piece.Id, piece =>
                    {
                        var candidateNeighbours = new Dictionary<Piece, HashSet<string>>();

                        foreach (var otherPiece in _pieces.Where(p => sideKeys.Contains(p.Id)).Except(new[] { piece }))
                        {
                            var sharedSides = piece.PossibleSides.Intersect(otherPiece.PossibleSides);
                            if (sharedSides.Any())
                            {
                                candidateNeighbours.Add(otherPiece, sharedSides.ToHashSet());
                            }
                        }
                        return candidateNeighbours;
                    });

                if (sideNeighbours.Count == 0)
                {
                    break;
                }
                else if (sideNeighbours.Count == 1)
                {
                    countourList.Add(new List<List<(Piece Piece, IntPoint Position)>>
                    {
                        new List<(Piece Piece, IntPoint Position)>
                        {
                            (_pieces.Single(p => p.Id == sideNeighbours.Single().Key), new IntPoint((sideLength / 2) + 1, (sideLength / 2) + 1))
                        }
                    });

                    break;
                }
                else
                {
                    countourList.Add(GetSides(
                        pieceNeighbours,
                        sideNeighbours,
                        _pieces.Where(p => sideKeys.Contains(p.Id)).ToList(),
                        sideLength));
                }

                foreach (var pair in pieceNeighbours)
                {
                    foreach (var side in countourList.Last())
                    {
                        var ids = side.ConvertAll(p => p.Piece.Id);

                        pair.Value.RemoveAll(pair => ids.Contains(pair.Key.Id));
                    }
                }

                totalSides.AddRange(sides);
            }

            return countourList;
        }

        private static List<List<(Piece Piece, IntPoint Position)>> OrderContours(List<List<List<(Piece Piece, IntPoint Position)>>> countourList)
        {
            var result = new List<List<(Piece Piece, IntPoint Position)>>();

            var startingPoint = new IntPoint(0, 0); // It doesn't really matter for the inned ones, we'll re-number them anyway
            foreach (var contour in countourList)
            {
                result.Add(OrderSides(contour, startingPoint));
            }

            return result;
        }

        private static List<(Piece Piece, IntPoint Position)> OrderSides(List<List<(Piece Piece, IntPoint Position)>> contour, IntPoint startingPoint)
        {
            if (contour.Count == 1)
            {
                return new[] { contour.First().First() }.ToList();
            }

            List<List<(Piece Piece, IntPoint Position)>> orderedSides = new List<List<(Piece Piece, IntPoint Position)>>();

            List<(Piece Piece, IntPoint Position)> result = new();

            // First side
            result.Add((contour[0][0].Piece, startingPoint));
            OrderSide(contour, orderedSides, result);

            // Second side
            OrderSide(contour, orderedSides, result);

            // Third side
            OrderSide(contour, orderedSides, result);

            // Fourth side
            OrderSide(contour, orderedSides, result);

            Debug.Assert(result[0].Piece.Id == result[^1].Piece.Id);
            Debug.Assert(result[0].Position == result[^1].Position);

            result.RemoveAt(result.Count - 1);

            return result;

            static void OrderSide(
                List<List<(Piece Piece, IntPoint Position)>> contour,
                List<List<(Piece Piece, IntPoint Position)>> usedSides,
                List<(Piece Piece, IntPoint Position)> result)
            {
                var corner = result.Last().Piece.Id;
                var side = contour.Except(usedSides).First(s => s[0].Piece.Id == corner || s[^1].Piece.Id == corner);

                usedSides.Add(side);

                if (corner != side[0].Piece.Id)
                {
                    side.Reverse();
                }

                Direction? initialDirection = null;
                for (int i = 1; i < side.Count; ++i)
                {
                    var previousPiece = result.Last();
                    var piece = side[i];

                    var sharedBorder = previousPiece.Piece.PossibleSides
                        .Intersect(piece.Piece.PossibleSides)
                        .First();

                    var previousPieceDirection = previousPiece.Piece.GetSideDirection(sharedBorder);

                    if (initialDirection is null)
                    {
                        initialDirection = previousPieceDirection;
                    }

                    var thisDirection = (Direction)(((int)(previousPieceDirection) + 2) % 4);
                    var mutatedPiece = piece.Piece.MutateToHave(sharedBorder, thisDirection);
                    result.Add((mutatedPiece, previousPiece.Position.Move(previousPieceDirection)));

                    Debug.Assert(previousPieceDirection == initialDirection);
                }
            }
        }

        /// <summary>
        /// Extract sides (Piece groups) from <paramref name="allSidePieces"/>.
        /// Uses breath first search (BFS)
        /// </summary>
        /// <param name="allPiecesWithNeighbours">All nodes</param>
        /// <param name="sidePiecesWithNeighbours"></param>
        /// <param name="allSidePieces"></param>
        /// <param name="sideLength">Total puzzle sideLength</param>
        /// <returns></returns>
        internal static List<List<(Piece Piece, IntPoint Position)>> GetSides(
            Dictionary<int, Dictionary<Piece, HashSet<string>>> allPiecesWithNeighbours,
            Dictionary<int, Dictionary<Piece, HashSet<string>>> sidePiecesWithNeighbours,
            List<Piece> allSidePieces, int sideLength)
        {
            var corners = allPiecesWithNeighbours.Where(node => node.Value.Count == 2).ToList();

            var possibleSides = new List<List<(Piece Piece, IntPoint Position)>>();

            for (int i = 0; i < corners.Count; ++i)
            {
                var initialPiece = allSidePieces.Single(p => p.Id == corners[i].Key);

                var otherCornerIds = corners.ConvertAll(node => node.Key);
                otherCornerIds.Remove(initialPiece.Id);

                var queue = new Queue<(Piece Piece, int ParentIndex, Direction ParentDirection)>();
                queue.Enqueue((initialPiece, -1, Direction.Up));

                var solutions = new Dictionary<int, List<(Piece Node, IntPoint Position)>>();

                Piece currentPiece = initialPiece;
                var currentSolution = new List<(Piece Node, IntPoint Position)> { (currentPiece, new IntPoint(0, 0)) };

                var expandedNodes = 0;

                var childrenToExpand = new Dictionary<int, int>(sidePiecesWithNeighbours.Select(n => new KeyValuePair<int, int>(n.Key, 0)));

                //var sw = new Stopwatch();
                //sw.Start();

                //var maxDepth = 1;

                while (queue.Count > 0)
                {
                    ++expandedNodes;
                    //if (expandedNodes % 250_000 == 0)
                    //{
                    //    Console.WriteLine($"\tTime: {0.001 * sw.ElapsedMilliseconds:F3}");
                    //    Console.WriteLine($"\tIndex: {expandedNodes}, queue: {queue.Count}");
                    //}

                    var current = queue.Dequeue();

                    currentPiece = current.Piece;
                    var parentIndex = current.ParentIndex;
                    var parentDirection = current.ParentDirection;

                    if (parentIndex != -1)
                    {
                        var previousPiece = solutions[parentIndex].Last();

                        var currentPosition = previousPiece.Position.Move(parentDirection);

                        currentSolution = solutions[parentIndex].Append((currentPiece, currentPosition)).ToList();

                        //if (currentSolution.Count > maxDepth)
                        //{
                        //    maxDepth = currentSolution.Count;
                        //    Console.WriteLine($"New depth: {maxDepth}");
                        //}

                        var minX = currentSolution.Min(sol => sol.Position.X);
                        var maxX = currentSolution.Max(sol => sol.Position.X);
                        var minY = currentSolution.Min(sol => sol.Position.Y);
                        var maxY = currentSolution.Max(sol => sol.Position.Y);

                        bool outsideOfSquare =
                            Math.Abs(maxX - minX) >= sideLength
                            || Math.Abs(maxY - minY) >= sideLength
                            || (Math.Abs(currentPosition.Y) != 0 && Math.Abs(currentPosition.Y) != sideLength)
                                    && (Math.Abs(currentPosition.X) != 0 && Math.Abs(currentPosition.X) != sideLength)
                            || (Math.Abs(currentPosition.X) != 0 && Math.Abs(currentPosition.X) != sideLength)
                                && (Math.Abs(currentPosition.Y) != 0 && Math.Abs(currentPosition.Y) != sideLength)
                            || (maxX > 0 && minX < 0)
                            || (minX < 0 && maxX > 0)
                            || (maxY > 0 && minY < 0)
                            || (minY < 0 && maxY > 0);

                        bool overlayingPieces = currentSolution.Select(s => s.Position).Distinct().Count() != currentSolution.Count
                            || currentSolution.Count > allSidePieces.Count;

                        if (outsideOfSquare || overlayingPieces)
                        {
                            continue;
                        }
                    }

                    solutions.Add(expandedNodes, currentSolution);

                    if (otherCornerIds.Contains(currentPiece.Id)) // Complete side
                    {
                        possibleSides.Add(currentSolution);
                    }

                    foreach (var candidate in GetCandidates(currentPiece, expandedNodes, sidePiecesWithNeighbours, currentSolution))
                    {
                        queue.Enqueue(candidate);
                        ++childrenToExpand[currentPiece.Id];
                    }
                }
            }

            var stringIds = possibleSides.Select((side, index) => (
                    side,
                    string.Join('|', side.Select(pair => pair.Piece.Id))))
                .ToList();

            var distinct = new HashSet<string>();
            var distinctItems = new List<List<(Piece Piece, IntPoint Position)>>();
            foreach (var pair in stringIds)
            {
                if (!distinct.Contains(pair.Item2) && !distinct.Contains(string.Join('|', pair.Item2.Split('|').Reverse())))
                {
                    distinct.Add(pair.Item2);
                    distinctItems.Add(pair.side);
                }
            }

            if (distinctItems.Count == 4)
            {
                return distinctItems;
            }

            throw new SolvingException();
        }

        /// <summary>
        /// Given a possible solution to <see cref="GetSides"/>, extracts the nodes to expand (possible continuations)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="index"></param>
        /// <param name="nodes"></param>
        /// <param name="solution"></param>
        /// <returns></returns>
        private static IEnumerable<(Piece Piece, int ParentIndex, Direction ParentDirection)> GetCandidates(Piece piece, int index,
            Dictionary<int, Dictionary<Piece, HashSet<string>>> nodes,
            List<(Piece Node, IntPoint Position)> solution)
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

        private List<(Piece Piece, IntPoint Position)> PutTogetherPuzzle(List<List<(Piece Piece, IntPoint Position)>> orderedContoursList)
        {
            var pieceNeighbours = ExtractPieceNeighboursDictionary();

            // Place pieces in the right place, without worrying about its orientation
            List<(Piece Piece, IntPoint Position)> puzzle = ComposePuzzleWithUnrotatedPieces(orderedContoursList, pieceNeighbours);

            // Rotate them
            puzzle = RotatePuzzlePiecesToCompletePuzzle(puzzle);

            return puzzle;

            static List<(Piece Piece, IntPoint Position)> ComposePuzzleWithUnrotatedPieces(List<List<(Piece Piece, IntPoint Position)>> orderedContoursList, Dictionary<int, Dictionary<Piece, HashSet<string>>> pieceNeighbours)
            {
                var puzzle = new List<(Piece Piece, IntPoint Position)>();

                for (int contourIndex = 1; contourIndex < orderedContoursList.Count; ++contourIndex)
                {
                    var contour = orderedContoursList[contourIndex];
                    var idsToAdd = contour.ConvertAll(p => p.Piece.Id);

                    var previousContourPiecesCount = orderedContoursList[contourIndex - 1].Count;
                    var minX = orderedContoursList[contourIndex - 1].Min(p => p.Position.X);
                    var maxX = orderedContoursList[contourIndex - 1].Max(p => p.Position.X);
                    var minY = orderedContoursList[contourIndex - 1].Min(p => p.Position.Y);
                    var maxY = orderedContoursList[contourIndex - 1].Max(p => p.Position.Y);

                    if (contourIndex == 1)
                    {
                        orderedContoursList[0] = orderedContoursList[0]
                            .OrderBy(p => p.Position.Y)
                            .ThenBy(p => p.Position.X)
                            .Select(pair =>
                                (pair.Piece, new IntPoint(pair.Position.X - minX, pair.Position.Y - minY)))
                            .ToList();

                        puzzle.AddRange(orderedContoursList[0]);

                        maxX -= minX;
                        minX = 0;
                        maxY -= minY;
                        minY = 0;
                    }

                    var offset = puzzle.Count - previousContourPiecesCount;
                    var skiped = 0;
                    for (int i = 0; i < previousContourPiecesCount; ++i)
                    {
                        var placedPiece = puzzle[offset + i];

                        // Skip corners and one of the squares next to the corner, since corners don't have relationship with this frame
                        // and there are two pieces next to corners that have relationship to the same square
                        if (
                               (placedPiece.Position.X == maxX && placedPiece.Position.Y >= maxY - 1)
                            || (placedPiece.Position.X == minX && placedPiece.Position.Y <= minY + 1)
                            || (placedPiece.Position.X == maxX && placedPiece.Position.Y <= minY + 1)
                            || (placedPiece.Position.X == minX && placedPiece.Position.Y >= maxY - 1))
                        {
                            ++skiped;
                            continue;
                        }

                        var pieceToAdd = pieceNeighbours[placedPiece.Piece.Id].Keys.Single(p => idsToAdd.Contains(p.Id));

                        var x = placedPiece.Position.X;
                        var y = placedPiece.Position.Y;

                        if (placedPiece.Position.X == minX)
                        {
                            x = placedPiece.Position.X + 1;
                        }
                        else if (placedPiece.Position.X == maxX)
                        {
                            x = placedPiece.Position.X - 1;
                        }

                        if (placedPiece.Position.Y == minY)
                        {
                            y = placedPiece.Position.Y + 1;
                        }
                        else if (placedPiece.Position.Y == maxY)
                        {
                            y = placedPiece.Position.Y - 1;
                        }

                        orderedContoursList[contourIndex][(i - 1 - skiped) < 0 ? 0 : (i - 1 - skiped)] = (pieceToAdd, new IntPoint(x, y));
                        puzzle.Add((pieceToAdd, new IntPoint(x, y)));
                    }

                    if (idsToAdd.Count == 1)
                    {
                        puzzle.Remove(puzzle.Last());
                    }

                    orderedContoursList[contourIndex] = puzzle.TakeLast(orderedContoursList[contourIndex].Count).ToList();
                }

                return puzzle;
            }

            static List<(Piece Piece, IntPoint Position)> RotatePuzzlePiecesToCompletePuzzle(List<(Piece Piece, IntPoint Position)> puzzle)
            {
                var orderedPuzzle = puzzle.OrderBy(p => p.Position.Y).ThenBy(p => p.Position.X).ToList();

                var pieceNeighbours = orderedPuzzle
                    .ToDictionary(
                        pair => pair.Piece.Id,
                        pair => orderedPuzzle.Where(otherPair => pair.Position.DistanceTo(otherPair.Position) == 1).ToList());

                var solutionIds = new List<int>();
                var solution = new List<(Piece Piece, IntPoint Position)>();

                void AddSolution((Piece Piece, IntPoint Position) pair)
                {
                    solutionIds.Add(pair.Piece.Id);
                    solution.Add(pair);
                }

                AddSolution(orderedPuzzle[0]);

                for (int i = 1; i < orderedPuzzle.Count; ++i)
                {
                    var currentPiece = orderedPuzzle[i];
                    var placedNeighbourIds = pieceNeighbours[currentPiece.Piece.Id].Where(n => solutionIds.Contains(n.Piece.Id)).Select(n => n.Piece.Id).ToList();
                    var placedNeighbours = solution.Where(p => placedNeighbourIds.Contains(p.Piece.Id)).ToList();

                    var candidatePieces = new List<(Piece piece, string sharedSide, (Piece piece, IntPoint position) neighbour)>();

                    foreach (var neighbour in placedNeighbours)
                    {
                        var sharedSides = neighbour.Piece.FreeSides.Select(pair => pair.side).Intersect(currentPiece.Piece.PossibleSides);

                        foreach (var sharedSide in sharedSides)
                        {
                            var neighbourDirection = neighbour.Piece.FreeSides.Single(pair => pair.side == sharedSide).direction;

                            var ownDirection = GetOppositeDirection(neighbourDirection);

                            var transformedPiece = currentPiece.Piece.MutateToHave(sharedSide, ownDirection);
                            candidatePieces.Add((transformedPiece, sharedSide, neighbour));
                        }
                    }

                    // Choosing those ones that have shared sides with all neighbours, once transformed
                    var realCandidates = candidatePieces.Where(piece =>
                        placedNeighbours.All(n =>
                            piece.piece.FreeSides.Select(pair => pair.side)
                                .Intersect(n.Piece.FreeSides.Select(pair => pair.side))
                                .Any()))
                        .ToList();

                    if (realCandidates.Count == 0)
                    {
                        throw new SolvingException(":(");
                    }
                    else if (realCandidates.Count == 1)
                    {
                        AddSolution((realCandidates[0].piece, currentPiece.Position));
                    }
                    else
                    {
                        // Rotate and see if they really match
                        var realRealCandidates = new List<Piece>();
                        foreach (var candidate in realCandidates)
                        {
                            foreach (var neighbour in placedNeighbours.Except(new[] { candidate.neighbour }))
                            {
                                var sharedSides = candidate.piece.FreeSides.Select(pair => pair.side).Intersect(neighbour.Item1.FreeSides.Select(pair => pair.side));

                                var sharedPairsAlreadyInPlace = neighbour.Item1.FreeSides.Where(pair => sharedSides.Contains(pair.side));

                                if (sharedPairsAlreadyInPlace.All(pair => candidate.piece.GetSideDirection(pair.side) == GetOppositeDirection(pair.direction)))
                                {
                                    realRealCandidates.Add(candidate.piece);
                                }
                            }
                        }

                        AddSolution((realRealCandidates[0], currentPiece.Position));  // Si hay varios, son iguales
                    }
                }

                return solution;
            }
        }

        private BitMatrix RemoveBordersAndJoin(List<(Piece Piece, IntPoint Position)> puzzle)
        {
            var sideLength = Convert.ToInt32(Math.Sqrt(puzzle.Count));
            var pieceSideLength = puzzle[0].Piece.Left.Length;

            var solution = new BitMatrix(new List<BitArray>(sideLength));

            puzzle = puzzle
                .OrderByDescending(p => p.Position.Y)   // We want to start from the top left corner.
                .ThenBy(p => p.Position.X).ToList();

            var prev = puzzle[0].Position;

            var boolRows = new List<List<bool>>(RangeHelpers.GenerateRange(0, pieceSideLength - 3).Select(_ => new List<bool>()));
            //var boolRows = new List<List<bool>>(RangeHelpers.GenerateRange(0, pieceSideLength - 1).Select(_ => new List<bool>()));    // Not removing anything

            foreach ((Piece piece, IntPoint position) in puzzle)
            {
                var rows = piece.Matrix.Content;

                // Comment to not remove anything
                rows.Remove(rows.First());
                rows.Remove(rows.Last());

                if (position.Y != prev.Y)
                {
                    solution.Content.AddRange(boolRows.Select(l => new BitArray(l.ToArray())));
                    boolRows.ForEach(row => row.Clear());
                }

                for (int rowIndex = 0; rowIndex < rows.Count; ++rowIndex)
                {
                    var bitArray = rows[rowIndex];

                    //var end = bitArray.Length;        // Not removing anything
                    //for (int i = 0; i < end; ++i)
                    //{
                    //    boolRows[rowIndex].Add(bitArray[i]);
                    //}

                    var end = bitArray.Length - 1;
                    for (int i = 1; i < end; ++i)
                    {
                        boolRows[rowIndex].Add(bitArray[i]);
                    }
                }

                prev = position;
            }

            solution.Content.AddRange(boolRows.Select(l => new BitArray(l.ToArray())));

            return solution;
        }

        private static Direction GetOppositeDirection(Direction neighbourDirection)
        {
            return (Direction)(((int)neighbourDirection + 2) % 4);
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
        public BitMatrix Matrix { get; private set; } = null!;

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
        public ICollection<Direction> FreeDirections { get; set; } = null!;

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

        public List<BitArray> Content => Matrix.Content;

        public Piece(int id, List<BitArray> content)
        {
            Id = id;
            SetBitMatrix(content);
        }

        private void SetBitMatrix(List<BitArray> content)
        {
            Matrix = new BitMatrix(content);

            _top = Matrix.Content[0].ToBitString();
            _topReversed = ReverseString(_top);

            _bottom = Matrix.Content.Last().ToBitString();
            _bottomReversed = ReverseString(_bottom);

            _left = new BitArray(Matrix.Content.Select(arr => arr[0]).ToArray()).ToBitString();
            _leftReversed = ReverseString(_left);

            _right = new BitArray(Matrix.Content.Select(arr => arr[^1]).ToArray()).ToBitString();
            _rightReversed = ReverseString(_right);

            PossibleSides = new HashSet<string> {
                    _top, _topReversed,
                    _bottom, _bottomReversed,
                    _left, _leftReversed,
                    _right, _rightReversed,
                };

            FreeDirections = new List<Direction> { Direction.Up, Direction.Down, Direction.Right, Direction.Left };
        }

        /// <summary>
        /// [Placed]
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public Direction GetSideDirection(string side)
        {
            if (side == _top)
            {
                return Direction.Up;
            }

            if (side == _bottom)
            {
                return Direction.Down;
            };

            if (side == Left)
            {
                return Direction.Left;
            }

            if (side == Right)
            {
                return Direction.Right;
            }

            throw new SolvingException();
        }

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

        public Piece RotateClockwise() => new Piece(Id, Matrix.RotateClockwise());

        public Piece RotateAnticlockwise() => new Piece(Id, Matrix.RotateAnticlockwise());

        public Piece Rotate180() => new Piece(Id, Matrix.Rotate180());

        public Piece FlipUpsideDown() => new Piece(Id, Matrix.FlipUpsideDown());

        public Piece FlipLeftRight() => new Piece(Id, Matrix.FlipLeftRight());

        public override string ToString()
        {
            return Matrix.ToString();
        }

        internal static string ReverseString(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    public static class StringHelpers
    {
        public static string ReverseString(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);

            return new string(charArray);
        }
    }
}

