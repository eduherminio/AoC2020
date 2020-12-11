using AoCHelper;
using FileParser;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_11 : BaseDay
    {
        private SeatingSystem _system;

        public Day_11()
        {
            _system = ParseInput();
        }

        public override string Solve_1()
        {
            return "";

            var rounds = 0;

            Console.Clear(); _system.Print();
            while (_system.Mutate())
            {
                _system.Print();

                ++rounds;
                Console.WriteLine(rounds);
            }

            return _system.OccupiedSeats.ToString();
        }

        public override string Solve_2()
        {
            var rounds = 0;

            Console.Clear(); _system.Print();
            while (_system.Mutate())
            {
                _system.Print();

                ++rounds;
                Console.WriteLine(rounds);
            }

            return _system.OccupiedSeats.ToString();
        }

        private SeatingSystem ParseInput()
        {
            static SeatingStatus GetSeatingStatus(char ch) => ch switch
            {
                '.' => SeatingStatus.Floor,
                'L' => SeatingStatus.Empty,
                '#' => SeatingStatus.Occupied,
                _ => throw new SolvingException()
            };

            static IEnumerable<SeatingLocation> ParseSeatingLocations(string inputFilePath)
            {
                var lines = File.ReadAllLines(inputFilePath);
                for (int y = 0; y < lines.Length; y++)
                {
                    for (int x = 0; x < lines[y].Length; x++)
                    {
                        yield return new SeatingLocation(x, y, GetSeatingStatus(lines[y][x]));
                    }
                }
            }

            return new SeatingSystem(ParseSeatingLocations(InputFilePath).ToList());
        }

        public class SeatingSystem
        {
            private readonly HashSet<SeatingLocation> _floorLocations;
            private readonly HashSet<SeatingLocation> _seatingLocations;
            public int OccupiedSeats => _seatingLocations.Count(l => l.Status == SeatingStatus.Occupied);

            public SeatingLocation? this[int x, int y]
            {
                get { return _seatingLocations.SingleOrDefault(location => location.X == x && location.Y == y); }
            }

            public SeatingSystem(ICollection<SeatingLocation> seatingLocations)
            {
                _floorLocations = seatingLocations.Where(location => location.Status == SeatingStatus.Floor).ToHashSet();
                _seatingLocations = seatingLocations.Except(_floorLocations).ToHashSet();

                // Part 1
                foreach (var seat in _seatingLocations)
                {
                    seat.AdjacentLocationIndexesPart1().ForEach(pair =>
                    {
                        var adjacent = this[pair.x, pair.y];
                        if (adjacent is not null)
                        {
                            seat.AdjacentSeats.Add(adjacent);
                        }
                    });
                }

                // Part 2

                foreach (var seat in _seatingLocations)
                {
                    seat.SetAdjacentLocations(_seatingLocations);
                }
            }

            public bool Mutate()
            {
                var plannedMutations = _seatingLocations.Where(seat => seat.PlanMutation())
                    .ToList();   // Relevant .ToList(), to make sure all planning happens here.

                if (plannedMutations.Count > 0)
                {
                    plannedMutations.ForEach(seat => seat.ApplyMutation());
                    return true;
                }

                return false;
            }

            public static int Offset = 0;
            public void Print()
            {
                foreach (var seat in _floorLocations.Union(_seatingLocations))
                {
                    Console.CursorLeft = seat.X;
                    Console.CursorTop = seat.Y + Offset;
                    Console.Write(seat.Status switch
                    {
                        SeatingStatus.Floor => '.',
                        SeatingStatus.Empty => 'L',
                        SeatingStatus.Occupied => '#'
                    });
                }
                Offset += 15;
            }
        }

        public enum SeatingStatus { Floor, Empty, Occupied };

        public class SeatingLocation
        {
            private SeatingStatus? _next;

            public int X { get; }
            public int Y { get; }
            public SeatingStatus Status { get; set; }
            public IList<SeatingLocation> AdjacentSeats { get; }

            public IReadOnlyList<(int x, int y)> AdjacentLocationIndexesPart1() => new List<(int, int)>
            {
                (X + 1, Y),
                (X - 1, Y),
                (X, Y + 1),
                (X, Y - 1),
                (X + 1, Y + 1),
                (X + 1, Y - 1),
                (X - 1, Y + 1),
                (X - 1, Y - 1)
            };

            public SeatingLocation(int x, int y, SeatingStatus status)
            {
                X = x;
                Y = y;
                Status = status;

                AdjacentSeats = new List<SeatingLocation>();
            }

            public bool PlanMutation()
            {
                if (Status == SeatingStatus.Empty && !AdjacentSeats.Any(s => s.Status == SeatingStatus.Occupied))
                {
                    _next = SeatingStatus.Occupied;
                }
                //else if (Status == SeatingStatus.Occupied && AdjacentSeats.Count(s => s.Status == SeatingStatus.Occupied) >= 4) // Part 1
                else if (Status == SeatingStatus.Occupied && AdjacentSeats.Count(s => s.Status == SeatingStatus.Occupied) >= 5) // Part 2
                {
                    _next = SeatingStatus.Empty;
                }

                return _next is not null;
            }

            public void ApplyMutation()
            {
                Status = _next ?? throw new SolvingException();
                _next = null;
            }

            public void SetAdjacentLocations(HashSet<SeatingLocation> otherSeats)
            {
                AddNonDiagonalAdjacents(otherSeats);

                AddDiagonalAdjacents(otherSeats);

                void AddNonDiagonalAdjacents(HashSet<SeatingLocation> otherSeats)
                {
                    var nonDiagonalSeats = otherSeats.Where(seat => seat.X == X ^ seat.Y == Y);

                    SeatingLocation? left = null;
                    SeatingLocation? right = null;
                    SeatingLocation? top = null;
                    SeatingLocation? bottom = null;

                    foreach (var seat in nonDiagonalSeats)
                    {
                        if (seat.X == X)    // Vertical
                        {
                            if (seat.Y < Y)
                            {
                                if (seat.Y > top?.Y || top is null)
                                {
                                    top = seat;
                                }
                            }
                            else
                            {
                                if (seat.Y < bottom?.Y || bottom is null)
                                {
                                    bottom = seat;
                                }
                            }
                        }
                        else // Horizontal
                        {
                            if (seat.X < X)
                            {
                                if (seat.X > left?.X || left is null)
                                {
                                    left = seat;
                                }
                            }
                            else
                            {
                                if (seat.X < right?.X || right is null)
                                {
                                    right = seat;
                                }
                            }
                        }
                    }

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                    AdjacentSeats.AddRange(new[] { left, right, top, bottom }.Where(i => i is not null));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                }

                void AddDiagonalAdjacents(HashSet<SeatingLocation> otherSeats)
                {
                    var diagonalSeats = otherSeats.Where(seat =>
                    {
                        var dx = Math.Abs(X - seat.X);
                        var dy = Math.Abs(Y - seat.Y);

                        return dx == dy && X != seat.X && Y != seat.Y;
                    });

                    SeatingLocation? topLeft = null;
                    SeatingLocation? topRight = null;
                    SeatingLocation? bottomLeft = null;
                    SeatingLocation? bottomRight = null;

                    foreach (var diagonal in diagonalSeats)
                    {
                        var dx = Math.Abs(X - diagonal.X);

                        if (diagonal.X < X)
                        {
                            if (diagonal.Y < Y)
                            {
                                if (diagonal.X > topLeft?.X || topLeft is null)
                                {
                                    topLeft = diagonal;
                                }
                            }
                            else
                            {
                                if (diagonal.X > bottomLeft?.X || bottomLeft is null)
                                {
                                    bottomLeft = diagonal;
                                }
                            }
                        }
                        else
                        {
                            if (diagonal.Y < Y)
                            {
                                if (diagonal.X < topRight?.X || topRight is null)
                                {
                                    topRight = diagonal;
                                }
                            }
                            else
                            {
                                if (diagonal.X < bottomRight?.X || bottomRight is null)
                                {
                                    bottomRight = diagonal;
                                }
                            }
                        }
                    }

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                    AdjacentSeats.AddRange(new[] { topLeft, topRight, bottomLeft, bottomRight }.Where(i => i is not null));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                }
            }
        }
    }
}
