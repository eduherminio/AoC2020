using AoCHelper;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_11 : BaseDay
    {
        public override string Solve_1()
        {
            var system = new SeatingSystem(ParseInput(isPart2: false).ToList(), isPart2: false);

            while (system.Mutate()) { /*_system.Print();*/ }

            return system.OccupiedSeats.ToString();
        }

        public override string Solve_2()
        {
            var system = new SeatingSystem(ParseInput(isPart2: true).ToList(), isPart2: true);

            while (system.Mutate()) { /*_system.Print();*/ }

            return system.OccupiedSeats.ToString();
        }

        private IEnumerable<SeatingLocation> ParseInput(bool isPart2)
        {
            var lines = File.ReadAllLines(InputFilePath);

            var seats = new List<List<SeatingLocation>>(lines.Length);
            for (int y = 0; y < lines.Length; y++)
            {
                seats.Add(new List<SeatingLocation>(lines[y].Length));
                for (int x = 0; x < lines[y].Length; x++)
                {
                    seats[y].Add(new SeatingLocation(x, y, GetSeatingStatus(lines[y][x])));
                }
            }

            if (!isPart2)
            {
                PopulatePart1Adjacents(seats);
            }

            return seats.SelectMany(seat => seat);

            static SeatingStatus GetSeatingStatus(char ch) => ch switch
            {
                '.' => SeatingStatus.Floor,
                'L' => SeatingStatus.Empty,
                '#' => SeatingStatus.Occupied,
                _ => throw new SolvingException()
            };

            static void PopulatePart1Adjacents(List<List<SeatingLocation>> seats)
            {
                for (int y = 0; y < seats.Count; y++)
                {
                    for (int x = 0; x < seats[y].Count; x++)
                    {
                        var current = seats[y][x];

                        if (x + 1 < seats[y].Count)
                        {
                            seats[y][x + 1].AdjacentSeats.Add(current);

                            if (y + 1 < seats.Count)
                            {
                                seats[y + 1][x + 1].AdjacentSeats.Add(current);
                            }
                            if (y - 1 >= 0)
                            {
                                seats[y - 1][x + 1].AdjacentSeats.Add(current);
                            }
                        }

                        if (x - 1 >= 0)
                        {
                            seats[y][x - 1].AdjacentSeats.Add(current);

                            if (y + 1 < seats.Count)
                            {
                                seats[y + 1][x - 1].AdjacentSeats.Add(current);
                            }
                            if (y - 1 >= 0)
                            {
                                seats[y - 1][x - 1].AdjacentSeats.Add(current);
                            }
                        }

                        if (y + 1 < seats.Count)
                        {
                            seats[y + 1][x].AdjacentSeats.Add(current);
                        }

                        if (y - 1 >= 0)
                        {
                            seats[y - 1][x].AdjacentSeats.Add(current);
                        }
                    }
                }
            }
        }

        public class SeatingSystem
        {
            private readonly bool _isPart2;
            private readonly HashSet<SeatingLocation> _seatingLocations;

            public int OccupiedSeats => _seatingLocations.Count(l => l.Status == SeatingStatus.Occupied);

            public SeatingSystem(List<SeatingLocation> seatingLocations, bool isPart2)
            {
                _isPart2 = isPart2;
                _seatingLocations = seatingLocations.Where(location => location.Status != SeatingStatus.Floor).ToHashSet();

                if (_isPart2)
                {
                    _seatingLocations.ForEach(seat => seat.SetAdjacentSeats_Part2(_seatingLocations));
                }
            }

            public bool Mutate()
            {
                var numberOfOccupiedAdjacentsToFree = _isPart2 ? 5 : 4;
                var plannedMutations = _seatingLocations.Where(seat => seat.PlanMutation(numberOfOccupiedAdjacentsToFree))
                    .ToList();   // Relevant .ToList(), to make sure all planning happens here.

                if (plannedMutations.Count > 0)
                {
                    plannedMutations.ForEach(seat => seat.ApplyMutation());
                    return true;
                }

                return false;
            }

            //public static int Offset = 0;
            //public void Print()
            //{
            //    foreach (var seat in _floorLocations.Union(_seatingLocations))
            //    {
            //        Console.CursorLeft = seat.X;
            //        Console.CursorTop = seat.Y + Offset;
            //        Console.Write(seat.Status switch
            //        {
            //            SeatingStatus.Floor => '.',
            //            SeatingStatus.Empty => 'L',
            //            SeatingStatus.Occupied => '#',
            //            _ => throw new SolvingException()
            //        });
            //    }
            //    Offset += 15;
            //}
        }

        public enum SeatingStatus { Floor, Empty, Occupied };

        public class SeatingLocation
        {
            private SeatingStatus? _next;

            public int X { get; }
            public int Y { get; }
            public SeatingStatus Status { get; set; }
            public IList<SeatingLocation> AdjacentSeats { get; }

            public SeatingLocation(int x, int y, SeatingStatus status)
            {
                X = x;
                Y = y;
                Status = status;

                AdjacentSeats = new List<SeatingLocation>();
            }

            public bool PlanMutation(int numberOfOccupiedAdjacentsToFree)
            {
                if (Status == SeatingStatus.Empty && !AdjacentSeats.Any(s => s.Status == SeatingStatus.Occupied))
                {
                    _next = SeatingStatus.Occupied;
                }
                else if (Status == SeatingStatus.Occupied && AdjacentSeats.Count(s => s.Status == SeatingStatus.Occupied) >= numberOfOccupiedAdjacentsToFree)
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

            public void SetAdjacentSeats_Part2(HashSet<SeatingLocation> otherSeats)
            {
                AdjacentSeats.Clear();

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
                        else    // Horizontal
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
