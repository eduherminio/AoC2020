using AoCHelper;
using SheepTools.Extensions;
using SheepTools.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_12 : BaseDay
    {
        private readonly List<Instruction> _instructions;

        private static readonly Dictionary<ShipAction, Func<Instruction, ShipState, ShipState>> Movements = new()
        {
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions - Records with pattern
            [ShipAction.Down] = (ins, state) => (state with { Position = state.Position.Move(Direction.Down, ins.Value) }),
            [ShipAction.Up] = (ins, state) => (state with { Position = state.Position.Move(Direction.Up, ins.Value) }),
            [ShipAction.Left] = (ins, state) => (state with { Position = state.Position.Move(Direction.Left, ins.Value) }),
            [ShipAction.Right] = (ins, state) => (state with { Position = state.Position.Move(Direction.Right, ins.Value) }),
            [ShipAction.TurnLeft] = (_, state) => (state with { Direction = state.Direction.TurnLeft() }),
            [ShipAction.TurnRight] = (_, state) => (state with { Direction = state.Direction.TurnRight() }),
            [ShipAction.Turn180] = (_, state) => (state with { Direction = state.Direction.Turn180() }),
            [ShipAction.KeepMovin] = (ins, state) => (state with { Position = state.Position.Move(state.Direction, ins.Value) })
#pragma warning restore S1121 // Assignments should not be made from within sub-expressions
        };

        private static readonly Dictionary<ShipAction, Func<Instruction, Point, Point, (Point, Point)>> MovementsPart2 = new()
        {
            [ShipAction.Down] = (ins, state, waypoint) => (state, waypoint.Move(Direction.Down, ins.Value)),
            [ShipAction.Up] = (ins, state, waypoint) => (state, waypoint.Move(Direction.Up, ins.Value)),
            [ShipAction.Left] = (ins, state, waypoint) => (state, waypoint.Move(Direction.Left, ins.Value)),
            [ShipAction.Right] = (ins, state, waypoint) => (state, waypoint.Move(Direction.Right, ins.Value)),

            [ShipAction.TurnLeft] = (_, state, waypoint) => (state, Rotate(state, waypoint, 0.5 * Math.PI)),
            [ShipAction.TurnRight] = (_, state, waypoint) => (state, Rotate(state, waypoint, -0.5 * Math.PI)),
            [ShipAction.Turn180] = (_, state, waypoint) => (state, Rotate(state, waypoint, Math.PI)),
            [ShipAction.KeepMovin] = (ins, state, waypoint) => (
                new Point(
                    state.X + (ins.Value * (waypoint.X - state.X)),
                    state.Y + (ins.Value * (waypoint.Y - state.Y))),
                new Point(
                    waypoint.X + (ins.Value * (waypoint.X - state.X)),
                    waypoint.Y + (ins.Value * (waypoint.Y - state.Y))))
        };

        public static Point Rotate(Point origin, Point point, double angle)
        {
            var sinAngle = Math.Sin(angle);
            var cosAngle = Math.Cos(angle);

            return new Point(
                x: (cosAngle * (point.X - origin.X))
                    - (sinAngle * (point.Y - origin.Y))
                    + origin.X,
                y: (sinAngle * (point.X - origin.X))
                    + (cosAngle * (point.Y - origin.Y))
                    + origin.Y);
        }

        public Day_12()
        {
            _instructions = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            var initialState = new ShipState(new Point(0, 0), Direction.Right);
            var shipState = initialState;

            foreach (var instruction in _instructions)
            {
                shipState = Movements[instruction.Action](instruction, shipState);
            }

            return initialState.Position.ManhattanDistance(shipState.Position).ToString();
        }

        public override string Solve_2()
        {
            var initialShipPosition = new Point(0, 0);
            var initialWaypoint = new Point(10, 1);

            var shipPosition = initialShipPosition;
            var waypoint = initialWaypoint;

            foreach (var instruction in _instructions)
            {
                (shipPosition, waypoint) = MovementsPart2[instruction.Action](instruction, shipPosition, waypoint);
            }

            return initialShipPosition.ManhattanDistance(shipPosition).ToString();
        }

        private IEnumerable<Instruction> ParseInput()
        {
            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                var action = line[0] switch
                {
                    'N' => ShipAction.Up,
                    'S' => ShipAction.Down,
                    'E' => ShipAction.Right,
                    'W' => ShipAction.Left,
                    'L' => ShipAction.TurnLeft,
                    'R' => ShipAction.TurnRight,
                    'F' => ShipAction.KeepMovin,
                    _ => throw new SolvingException()
                };

                var value = int.Parse(line[1..]);

                if (action == ShipAction.TurnLeft || action == ShipAction.TurnRight)
                {
                    action = value switch
                    {
                        0 => ShipAction.KeepMovin,
                        90 => action,
                        180 => ShipAction.Turn180,
                        270 => action == ShipAction.TurnLeft ? ShipAction.TurnRight : ShipAction.TurnLeft,
                        360 => ShipAction.KeepMovin,
                        _ => throw new SolvingException()
                    };
                }

                yield return new Instruction(action, value);
            }
        }
    }

    public enum ShipAction
    {
        TurnLeft, TurnRight, Turn180, Up, Down, Right, Left, KeepMovin
    }

    public record Instruction(ShipAction Action, int Value);

    public record ShipState(Point Position, Direction Direction);
}
