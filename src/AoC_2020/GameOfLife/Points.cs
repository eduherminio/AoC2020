using SheepTools;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020.GameOfLife
{
    public abstract record Point()
    {
        public abstract Point[] Neighbours();
    }

    public record Point2D(int X, int Y) : Point
    {
        public override Point2D[] Neighbours()
        {
            var result = NeighboursIncludingThis().ToList();
            result.Remove(this);

#if DEBUG
            Ensure.Count(26, result);
#endif

            return result.ToArray();
        }

        private IEnumerable<Point2D> NeighboursIncludingThis()
        {
            for (int x = X - 1; x <= X + 1; ++x)
            {
                for (int y = Y - 1; y <= Y + 1; ++y)
                {
                    yield return new Point2D(x, y);
                }
            }
        }

        public override string ToString() => $"[{X}, {Y}]";
    }

    public record Point3D(int X, int Y, int Z) : Point
    {
        public override Point3D[] Neighbours()
        {
            var result = NeighboursIncludingThis().ToList();
            result.Remove(this);

#if DEBUG
            Ensure.Count(26, result);
#endif

            return result.ToArray();
        }

        private IEnumerable<Point3D> NeighboursIncludingThis()
        {
            for (int x = X - 1; x <= X + 1; ++x)
            {
                for (int y = Y - 1; y <= Y + 1; ++y)
                {
                    for (int z = Z - 1; z <= Z + 1; ++z)
                    {
                        yield return new Point3D(x, y, z);
                    }
                }
            }
        }

        public override string ToString() => $"[{X}, {Y}, {Z}]";
    }

    public record Point4D(int X, int Y, int Z, int W) : Point3D(X, Y, Z)
    {
        public override Point4D[] Neighbours()
        {
            var result = NeighboursIncludingThis().ToList();
            result.Remove(this);

#if DEBUG
            Ensure.Count(80, result);
#endif

            return result.ToArray();
        }

        private IEnumerable<Point4D> NeighboursIncludingThis()
        {
            for (int x = X - 1; x <= X + 1; ++x)
            {
                for (int y = Y - 1; y <= Y + 1; ++y)
                {
                    for (int z = Z - 1; z <= Z + 1; ++z)
                    {
                        for (int w = W - 1; w <= W + 1; ++w)
                        {
                            yield return new Point4D(x, y, z, w);
                        }
                    }
                }
            }
        }

        public override string ToString() => $"[{X}, {Y}, {Z}, {W}]";
    }
}
