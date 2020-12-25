using SheepTools.Extensions;
using SheepTools.Model;

namespace AoC_2020
{
    public static class DirectionExtensions
    {
        public static Direction Opposite(this Direction direction) => direction.Turn180();
    }
}
