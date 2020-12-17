using SheepTools.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020.GameOfLife
{
    public static class GameOfLife
    {
        public static void Mutate(HashSet<Point> alivePoints)
        {
            var pointsToActivate = new HashSet<Point>(alivePoints.Count);
            var pointsToDeactivate = new HashSet<Point>(alivePoints.Count);
            var neighbours = new Dictionary<Point, int>(10 * alivePoints.Count);

            foreach (var point in alivePoints)
            {
                var pointNeighbours = MutatePoint(point, true, alivePoints, pointsToActivate, pointsToDeactivate);

                pointNeighbours.ForEach(p =>
                {
                    if (neighbours.TryGetValue(p.point, out var value))
                    {
                        neighbours[p.point] = ++value;
                    }
                    else
                    {
                        neighbours[p.point] = 1;
                    }
                });
            }

            foreach (var neighbourPair in neighbours.Where(pair => pair.Value >= 3 && !alivePoints.Contains(pair.Key)))
            {
                MutatePoint(neighbourPair.Key, false, alivePoints, pointsToActivate, pointsToDeactivate);
            }

            pointsToActivate.ForEach(p => alivePoints.Add(p));
            pointsToDeactivate.ForEach(p => alivePoints.Remove(p));

            static IEnumerable<(Point point, bool isAlive)> MutatePoint(Point point, bool isAlive,
                HashSet<Point> alivePoints, HashSet<Point> pointsToActivate, HashSet<Point> pointsToDeactivate)
            {
                var pointNeighbours = point.Neighbours()
                    .Select(p => (point: p, alivePoints.Contains(p)));

                GameOfLife.MutatePoint(point, isAlive, pointNeighbours, pointsToActivate, pointsToDeactivate);

                return pointNeighbours;
            }
        }

        private static void MutatePoint(Point point, bool isAlive, IEnumerable<(Point point, bool isActive)> neighbours, HashSet<Point> toActivate, HashSet<Point> toDeactivate)
        {
            var activeNeighboursCount = neighbours.Count(pair => pair.isActive);

            if (isAlive)
            {
                if (activeNeighboursCount != 2 && activeNeighboursCount != 3)
                {
                    toDeactivate.Add(point);
                }
            }
            else
            {
                if (activeNeighboursCount == 3)
                {
                    toActivate.Add(point);
                }
            }
        }
    }
}
