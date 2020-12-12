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

        private static readonly Dictionary<ShipAction, Func<Instruction, (IntPoint Position, Direction Direction), (IntPoint, Direction)>> Movements = new()
        {
            [ShipAction.Down] = (ins, state) => (state.Position.Move(Direction.Down, ins.Value), state.Direction),
            [ShipAction.Up] = (ins, state) => (state.Position.Move(Direction.Up, ins.Value), state.Direction),
            [ShipAction.Left] = (ins, state) => (state.Position.Move(Direction.Left, ins.Value), state.Direction),
            [ShipAction.Right] = (ins, state) => (state.Position.Move(Direction.Right, ins.Value), state.Direction),

            [ShipAction.TurnLeft] = (_, state) => (state.Position, state.Direction.TurnLeft()),
            [ShipAction.TurnRight] = (_, state) => (state.Position, state.Direction.TurnRight()),
            [ShipAction.Turn180] = (_, state) => (state.Position, state.Direction.Turn180()),

            [ShipAction.KeepMovin] = (ins, state) => (state.Position.Move(state.Direction, ins.Value), state.Direction)
        };

        private static readonly Dictionary<ShipAction, Func<Instruction, IntPoint, IntPoint, (IntPoint ship, IntPoint waypoint)>> MovementsPart2 = new()
        {
            [ShipAction.Down] = (ins, ship, waypoint) => (ship, waypoint.Move(Direction.Down, ins.Value)),
            [ShipAction.Up] = (ins, ship, waypoint) => (ship, waypoint.Move(Direction.Up, ins.Value)),
            [ShipAction.Left] = (ins, ship, waypoint) => (ship, waypoint.Move(Direction.Left, ins.Value)),
            [ShipAction.Right] = (ins, ship, waypoint) => (ship, waypoint.Move(Direction.Right, ins.Value)),

            [ShipAction.TurnLeft] = (_, ship, waypoint) => (ship, waypoint.RotateCounterclockwise(ship, 90, isRadians: false)),
            [ShipAction.TurnRight] = (_, ship, waypoint) => (ship, waypoint.RotateClockwise(ship, 90, isRadians: false)),
            [ShipAction.Turn180] = (_, ship, waypoint) => (ship, waypoint.RotateClockwise(ship, 180, isRadians: false)),

            [ShipAction.KeepMovin] = (ins, ship, waypoint) =>
            {
                var xIncrement = ins.Value * (waypoint.X - ship.X);
                var yIncrement = ins.Value * (waypoint.Y - ship.Y);

                return (new IntPoint(ship.X + xIncrement, ship.Y + yIncrement),
                        new IntPoint(waypoint.X + xIncrement, waypoint.Y + yIncrement));
            }
        };

        public Day_12()
        {
            _instructions = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            var initialState = (Position: new IntPoint(0, 0), Direction: Direction.Right);
            var shipState = initialState;

            foreach (var instruction in _instructions)
            {
                shipState = Movements[instruction.Action](instruction, shipState);
            }

            return initialState.Position.ManhattanDistance(shipState.Position).ToString();
        }

        public override string Solve_2()
        {
            var initialShipPosition = new IntPoint(0, 0);
            var initialWaypoint = new IntPoint(10, 1);

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

        private enum ShipAction
        {
            TurnLeft, TurnRight, Turn180, Up, Down, Right, Left, KeepMovin
        }

        private record Instruction(ShipAction Action, int Value);
    }
}
