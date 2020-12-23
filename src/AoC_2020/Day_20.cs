using AoCHelper;
using FileParser;
using SheepTools.Extensions;
using SheepTools.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nodes = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<AoC_2020.Piece, System.Collections.Generic.HashSet<string>>>;

namespace AoC_2020
{
    public class Day_20 : BaseDay
    {
        private readonly List<Piece> _pieces;

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
            var solution = new List<List<Piece>>(sideLength);

            var pieceNeighbours = _pieces.ToDictionary(piece => piece.Id, piece =>
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

            var corners = pieceNeighbours.Where(node => node.Value.Count == 2).ToList();
            Debug.Assert(corners.Count == 4);

            var sides = pieceNeighbours.Where(node => node.Value.Count <= 3).ToList();

            Debug.Assert(sides.Count < 4 * sideLength);
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

            //var sideContacts = sideNeighbours.Select(pair => pair.Value.Count).ToHashSet();
            //var possibleSides = sideNeighbours.Select(pair => pair.Value.Values.Count).ToHashSet();

            var contour = PlaceSides(
                pieceNeighbours,
                sideNeighbours,
                _pieces.Where(p => sideKeys.Contains(p.Id)).ToList(),
                sideLength);

            foreach (var pair in pieceNeighbours)
            {
                foreach (var node in contour)
                {
                    pair.Value.RemoveAll(pair => pair.Key.Id == node.Piece.Id);
                }
            }

            var nextCountour = pieceNeighbours.Except(sides).Where(node => node.Value.Count <= 3).ToList();

            return "";
        }

        private string Part2_Search()
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

            //var sol = DepthFirstSearch(nodes, _pieces);
            var sol = BreathFirstSearch(nodes, _pieces);

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

        internal static List<(Piece Piece, IntPoint Position)> PlaceSides(Nodes allNodes, Nodes sideNodes, List<Piece> pieces, int sideLength)
        {
            var corners = allNodes.Where(node => node.Value.Count == 2).ToList();

            var possibleSides = new List<List<(Piece Piece, IntPoint Position)>>();

            for (int i = 0; i < corners.Count; ++i)
            {
                var initialPiece = pieces.Single(p => p.Id == corners[i].Key);

                var otherCornerIds = corners.ConvertAll(node => node.Key);
                otherCornerIds.Remove(initialPiece.Id);

                var queue = new Queue<(Piece Piece, int ParentIndex, Direction ParentDirection)>();
                queue.Enqueue((initialPiece, -1, Direction.Up));

                var solutions = new Dictionary<int, List<(Piece Node, IntPoint Position)>>();

                Piece currentPiece = initialPiece;
                var currentSolution = new List<(Piece Node, IntPoint Position)> { (currentPiece, new IntPoint(0, 0)) };

                var expandedNodes = 0;
                bool success = false;

                var childrenToExpand = new Dictionary<int, int>(sideNodes.Select(n => new KeyValuePair<int, int>(n.Key, 0)));

                var sw = new Stopwatch();
                sw.Start();

                var maxDepth = 1;

                while (queue.Count > 0)
                {
                    ++expandedNodes;
                    if (expandedNodes % 250_000 == 0)
                    {
                        Console.WriteLine($"\tTime: {0.001 * sw.ElapsedMilliseconds:F3}");
                        Console.WriteLine($"\tIndex: {expandedNodes}, queue: {queue.Count}");
                    }

                    var current = queue.Dequeue();

                    currentPiece = current.Piece;
                    var parentIndex = current.ParentIndex;
                    var parentDirection = current.ParentDirection;

                    if (parentIndex != -1)
                    {
                        var previousPiece = solutions[parentIndex].Last();

                        var currentPosition = previousPiece.Position.Move(parentDirection);

                        currentSolution = solutions[parentIndex].Append((currentPiece, currentPosition)).ToList();

                        if (currentSolution.Count > maxDepth)
                        {
                            maxDepth = currentSolution.Count;
                            Console.WriteLine($"New depth: {maxDepth}");
                        }

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
                            || currentSolution.Count > pieces.Count;

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

                    if (currentSolution.Count == sideNodes.Count)
                    {
                        success = true;
                        break;
                    }


                    foreach (var candidate in GetCandidates(currentPiece, expandedNodes, sideNodes, currentSolution))
                    {
                        queue.Enqueue(candidate);
                        ++childrenToExpand[currentPiece.Id];
                    }
                }
            }

            var sol = new List<(Piece Piece, IntPoint Position)>();


            var knownSide = possibleSides.First();
            var knownCorner = knownSide.First().Piece;
            var knownCornerSides = knownCorner.FreeSides;
            var elementNextToKnownCorner = knownSide.ElementAt(1).Piece;
            var sharedBorder1 = knownCornerSides.Select(s => s.side)
                    .Intersect(elementNextToKnownCorner.FreeSides.Select(s => s.side))
                    .Single();

            knownSide.Reverse();
            sol.AddRange(knownSide);

            /////////////////////////////////////////
            knownSide = new List<(Piece Piece, IntPoint Position)>();

            var sideCandidateList = possibleSides.Where(side => side.Last().Piece.Id == knownCorner.Id || side.First().Piece.Id == knownCorner.Id);
            foreach (var candidate in sideCandidateList)
            {
                var elementNextToCandidateCorner = candidate.Last().Piece.Id == knownCorner.Id
                    ? candidate.ElementAt(candidate.Count - 2).Piece
                    : candidate.ElementAt(1).Piece;

                try
                {
                    var sharedBorder2 = knownCornerSides.Select(s => s.side)
                    .Intersect(elementNextToCandidateCorner.FreeSides.Select(s => s.side))
                    .Single();

                    var corner1SharedBorder1Direction = knownCorner.GetSideDirection(sharedBorder1);
                    var corner1SharedBorder2Direction = knownCorner.GetSideDirection(sharedBorder2);

                    if ((int)(corner1SharedBorder1Direction + 1) % 4 == (int)corner1SharedBorder2Direction
                        || (int)(corner1SharedBorder2Direction + 1) % 4 == (int)corner1SharedBorder1Direction)
                    {
                        knownSide = candidate;
                        break;
                    }
                }
                catch (Exception) { }
            }

            if (knownSide is null)
            {
                throw new SolvingException();
            }

            if (sol.Last().Piece.Id != knownSide.First().Piece.Id)
            {
                knownSide.Reverse();
            }
            sol.AddRange(knownSide);

            ////////////////////////

            knownCorner = knownSide.First().Piece.Id == knownCorner.Id
                ? knownSide.Last().Piece
                : knownSide.First().Piece;

            knownCornerSides = knownCorner.FreeSides;

            elementNextToKnownCorner = knownCorner.Id == knownSide.First().Piece.Id
                ? knownSide.ElementAt(1).Piece
                : knownSide.ElementAt(knownSide.Count - 2).Piece;

            sharedBorder1 = knownCornerSides.Select(s => s.side)
                    .Intersect(elementNextToKnownCorner.FreeSides.Select(s => s.side))
                    .Single();

            knownSide = new List<(Piece Piece, IntPoint Position)>();

            sideCandidateList = possibleSides.Where(side => side.Last().Piece.Id == knownCorner.Id || side.First().Piece.Id == knownCorner.Id);
            foreach (var candidate in sideCandidateList)
            {
                var nextToCorner2 = candidate.Last().Piece.Id == knownCorner.Id
                    ? candidate.ElementAt(candidate.Count - 2).Piece
                    : candidate.ElementAt(1).Piece;
                try
                {
                    var sharedBorder2 = knownCornerSides.Select(s => s.side)
                        .Intersect(nextToCorner2.FreeSides.Select(s => s.side))
                        .Single();

                    var corner1SharedBorder1Direction = knownCorner.GetSideDirection(sharedBorder1);
                    var corner1SharedBorder2Direction = knownCorner.GetSideDirection(sharedBorder2);

                    if ((int)(corner1SharedBorder1Direction + 1) % 4 == (int)corner1SharedBorder2Direction
                        || (int)(corner1SharedBorder2Direction + 1) % 4 == (int)corner1SharedBorder1Direction)
                    {
                        knownSide = candidate;
                        break;
                    }
                }
                catch (Exception) { }
            }

            if (knownSide is null)
            {
                throw new SolvingException();
            }

            if (sol.Last().Piece.Id != knownSide.First().Piece.Id)
            {
                knownSide.Reverse();
            }
            sol.AddRange(knownSide);

            ///////////////////////////////////
            ///

            knownCorner = knownSide.First().Piece.Id == knownCorner.Id
                ? knownSide.Last().Piece
                : knownSide.First().Piece;

            knownCornerSides = knownCorner.FreeSides;

            elementNextToKnownCorner = knownCorner.Id == knownSide.First().Piece.Id
                ? knownSide.ElementAt(1).Piece
                : knownSide.ElementAt(knownSide.Count - 2).Piece;

            sharedBorder1 = knownCornerSides.Select(s => s.side)
                    .Intersect(elementNextToKnownCorner.FreeSides.Select(s => s.side))
                    .Single();

            knownSide = new List<(Piece Piece, IntPoint Position)>();

            sideCandidateList = possibleSides.Where(side => side.Last().Piece.Id == knownCorner.Id || side.First().Piece.Id == knownCorner.Id);
            foreach (var candidate in sideCandidateList)
            {
                var nextToCorner2 = candidate.Last().Piece.Id == knownCorner.Id
                    ? candidate.ElementAt(candidate.Count - 2).Piece
                    : candidate.ElementAt(1).Piece;

                try
                {
                    var sharedBorder2 = knownCornerSides.Select(s => s.side)
                        .Intersect(nextToCorner2.FreeSides.Select(s => s.side))
                        .Single();

                    var corner1SharedBorder1Direction = knownCorner.GetSideDirection(sharedBorder1);
                    var corner1SharedBorder2Direction = knownCorner.GetSideDirection(sharedBorder2);

                    if ((int)(corner1SharedBorder1Direction + 1) % 4 == (int)corner1SharedBorder2Direction
                        || (int)(corner1SharedBorder2Direction + 1) % 4 == (int)corner1SharedBorder1Direction)
                    {
                        knownSide = candidate;
                        break;
                    }
                }
                catch (Exception) { }
            }

            if (knownSide is null)
            {
                throw new SolvingException();
            }

            if (sol.Last().Piece.Id != knownSide.First().Piece.Id)
            {
                knownSide.Reverse();
            }
            sol.AddRange(knownSide);





            // Intentando que cuadre
            //var cornerIds = corners.ConvertAll(p => p.Key);
            //Direction dir = Direction.Up;
            //var lastPiece = (Piece: sol.First().Piece, Position: new IntPoint(0, 0));

            //var finalSol = new List<(Piece Piece, IntPoint Position)>()
            //{
            //    (sol.First().Piece, new IntPoint(0,0))
            //};

            //for (int i = 1; i < sol.Count; ++i)
            //{
            //    var piece = sol.ElementAt(i);

            //    if (cornerIds.Contains(lastPiece.Piece.Id))
            //    {
            //        if (i != 1)
            //        {
            //            piece = sol[++i];
            //        }
            //        var sharedBorder = lastPiece.Piece.FreeSides.Select(s => s.side)
            //                .Intersect(piece.Piece.FreeSides.Select(s => s.side))
            //                .Single();

            //        dir = lastPiece.Piece.GetSideDirection(sharedBorder);   // Down la última???????????????????????
            //    }

            //    finalSol.Add(lastPiece);
            //    lastPiece = (sol.ElementAt(i).Piece, lastPiece.Position.Move(dir));
            //}


            var cornerIds = corners.ConvertAll(p => p.Key);
            Direction dir = Direction.Down;
            var lastPiece = (Piece: sol.First().Piece, Position: new IntPoint(0, 0));

            var finalSol = new List<(Piece Piece, IntPoint Position)>()
            {
                (sol.First().Piece, new IntPoint(0,0))
            };

            for (int i = 1; i < sol.Count; ++i)
            {
                var piece = sol.ElementAt(i);

                if (cornerIds.Contains(lastPiece.Piece.Id))
                {
                    if (i != 1)
                    {
                        piece = sol[++i];
                    }
                    var sharedBorder = lastPiece.Piece.FreeSides.Select(s => s.side)
                            .Intersect(piece.Piece.FreeSides.Select(s => s.side))
                            .Single();

                    dir = dir.TurnLeft();
                }

                finalSol.Add(lastPiece);
                lastPiece = (sol.ElementAt(i).Piece, lastPiece.Position.Move(dir));
            }



            // Alternativa:

            ;
            return finalSol;


            //var idsPositions = possibleSides.SelectMany(side => side.Select(pair => (pair.Node.Id, pair.Position))).ToList();

            //var ids = idsPositions.Select(pair => pair.Id).ToList();
            //var positions = idsPositions.Select(pair => pair.Position).ToList();

            //// Remove duplicates

            //var stringIds = possibleSides.Select((side, index) => (
            //    index,
            //    new string[]
            //        {
            //            string.Join('|', side.Select(pair => pair.Node.Id)),
            //            string.Join('|', side.Select(pair => pair.Node.Id).Reverse()),
            //        }
            //    )).ToList();

            //var distinct = new HashSet<string>();
            //var distinctIndex = new List<int>();
            //foreach (var pair in stringIds)
            //{
            //    if (!distinct.Contains(pair.Item2[0]) && !distinct.Contains(pair.Item2[1]))
            //    {
            //        distinct.AddRange(pair.Item2);
            //        distinctIndex.Add(pair.index);
            //    }
            //}

            //var distinctCandidates = possibleSides.Where((_, index) => distinctIndex.Contains(index)).ToList();


            //var sol = new List<(Piece Piece, IntPoint Position)>();

            //foreach (var side in distinctCandidates)
            //{
            //    var corner1 = side.First();

            //    var lateral1 = distinctCandidates.Except(new[] { side })
            //        .Where(s => s.First().Node.Id == corner1.Node.Id || s.Last().Node.Id == corner1.Node.Id)
            //        .Single();      // Let's pray

            //    corner1.Node.

            //    var corner2 = side.Last();
            //}


            //var sols = string.Join(",", solutions[expandedNodes].Select(s => s.Node.Id));
            //Console.WriteLine(sols);
            //Console.WriteLine($"Expanded: {expandedNodes}");

            Console.WriteLine("sols");

            //return success
            //    ? solutions[expandedNodes]
            //    : throw new SolvingException($"{nameof(DepthFirstSearch)} couldn't find a solution");

            throw new SolvingException();
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
                        || Math.Abs(maxY - minY) >= sideLength
                        || (maxX > 0 && minX < 0)
                        || (minX < 0 && maxX > 0)
                        || (maxY > 0 && minY < 0)
                        || (minY < 0 && maxY > 0);

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

            var sols = string.Join(",", solutions[expandedNodes].Select(s => s.Node.Id));

            Console.WriteLine(sols);
            Console.WriteLine($"Expanded: {expandedNodes}");

            return success
                ? solutions[expandedNodes]
                : throw new SolvingException($"{nameof(DepthFirstSearch)} couldn't find a solution");
        }

        internal static List<(Piece Piece, IntPoint Position)> BreathFirstSearch(Nodes nodes, List<Piece> pieces)
        {
            var sideLength = Convert.ToInt32(Math.Sqrt(nodes.Count));

            var corners = nodes.Where(n => n.Value.Count == 2);

            var initialPiece = pieces.Single(p => p.Id == corners.First().Key);

            var queue = new Queue<(Piece Piece, int ParentIndex, Direction ParentDirection)>();
            queue.Enqueue((initialPiece, -1, Direction.Up));

            var solutions = new Dictionary<int, List<(Piece Node, IntPoint Position)>>();

            Piece currentPiece = initialPiece;
            var currentSolution = new List<(Piece Node, IntPoint Position)> { (currentPiece, new IntPoint(0, 0)) };

            var expandedNodes = 0;
            bool success = false;

            var childrenToExpand = new Dictionary<int, int>(nodes.Select(n => new KeyValuePair<int, int>(n.Key, 0)));

            var sw = new Stopwatch();
            sw.Start();

            var maxDepth = 1;
            while (queue.Count > 0)
            {
                ++expandedNodes;
                if (expandedNodes % 250_000 == 0)
                {
                    Console.WriteLine($"\tTime: {0.001 * sw.ElapsedMilliseconds:F3}");
                    Console.WriteLine($"\tIndex: {expandedNodes}, queue: {queue.Count}");
                }

                var current = queue.Dequeue();

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

                    if (currentSolution.Count > maxDepth)
                    {
                        maxDepth = currentSolution.Count;
                        Console.WriteLine($"New depth: {maxDepth}");
                    }

                    var minX = currentSolution.Min(sol => sol.Position.X);
                    var maxX = currentSolution.Max(sol => sol.Position.X);
                    var minY = currentSolution.Min(sol => sol.Position.Y);
                    var maxY = currentSolution.Max(sol => sol.Position.Y);

                    bool outsideOfSquare =
                        Math.Abs(maxX - minX) >= sideLength
                        || Math.Abs(maxY - minY) >= sideLength
                        || (maxX > 0 && minX < 0)
                        || (minX < 0 && maxX > 0)
                        || (maxY > 0 && minY < 0)
                        || (minY < 0 && maxY > 0);

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
                    queue.Enqueue(candidate);
                    ++childrenToExpand[currentPiece.Id];
                }

                if (parentIndex != -1 && --childrenToExpand[solutions[parentIndex].Last().Node.Id] == 0)
                {
                    solutions[parentIndex].Last().Node.FreeDirections =
                        new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                }
            }

            var sols = string.Join(",", solutions[expandedNodes].Select(s => s.Node.Id));

            Console.WriteLine(sols);
            Console.WriteLine($"Expanded: {expandedNodes}");

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

        public Piece RotateClockwise() => new Piece(Id, _matrix.RotateClockwise());

        public Piece RotateAnticlockwise() => new Piece(Id, _matrix.RotateAnticlockwise());

        public Piece Rotate180() => new Piece(Id, _matrix.Rotate180());

        public Piece FlipUpsideDown() => new Piece(Id, _matrix.FlipUpsideDown());

        public Piece FlipLeftRight() => new Piece(Id, _matrix.FlipLeftRight());

        public override string ToString()
        {
            return _matrix.ToString();
        }

        internal static string ReverseString(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}

