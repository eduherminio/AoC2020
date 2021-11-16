// Other 'proper' solution:
// * By @jabadia applying Chinese Remainder Theorem -> https://github.com/jabadia/adventOfCode2020/blob/main/d13/d13p2.py
//   Chinese Remainder theorem itself: http://matesup.cl/portal/revista/2007/4.pdf

using AoCHelper;
using SheepTools;
using System.Diagnostics;

namespace AoC_2020
{
    public class Day_13 : BaseDay
    {
        private readonly long _earliestDeparture;
        private readonly List<long> _busFrequency;

        public Day_13()
        {
            (_earliestDeparture, _busFrequency) = ParseInput();
        }

        public override ValueTask<string> Solve_1()
        {
            var validFrequencies = _busFrequency.Where(freq => freq != -1).ToList();
            for (uint delta = 0; delta < _earliestDeparture; ++delta)
            {
                foreach (var frequency in validFrequencies)
                {
                    if ((_earliestDeparture + delta) % frequency == 0)
                    {
                        return new($"{frequency * delta}");
                    }
                }
            }

            throw new SolvingException();
        }

        public override ValueTask<string> Solve_2() => new(Part2_mariomka());

        /// <summary>
        /// ~96 min
        /// </summary>
        /// <returns></returns>
        public string Part2_Bruteforce_SingleThread()
        {
            var requirements = ExtractRequirements()
                .OrderByDescending(req => req.freq)     // Micro-optimization #2: fail as fast as possible
                .ToList();

            var max = requirements[0].freq;
            var maxIndex = requirements[0].index;

            var finalRequirements = requirements
                .Skip(1)    // Micro-optimization #3: no need to check the step
                .Select(pair => (pair.freq, index: pair.index - maxIndex))
                .ToList();

            var lcm = requirements.Select(req => (ulong)req.freq).LeastCommonMultiple();
            var longLimit = lcm > long.MaxValue ? long.MaxValue : Convert.ToInt64(lcm);

            return Calculate(1, finalRequirements, max, limit: longLimit).ToString();
        }

        /// <summary>
        /// Micro-optimization #1
        /// [f: A, i: X] && [f: B, i: A] => [f: A * B, i: A]
        /// Example:
        /// [f: 7, i: 0] && [f: 19, i: 7] => [f: 7 * 19, i:7]
        /// </summary>
        /// <returns></returns>
        private List<(long freq, int index)> ExtractRequirements()
        {
            var initialRequirements = _busFrequency
                .Select((freq, index) => (freq, index))
                .Where(pair => pair.freq != -1)
                .ToList();

            var redundantRequirements = new List<(long, int)>();
            var newRequirements = new List<(long, int)>();

            foreach (var req in initialRequirements)
            {
                var other = initialRequirements
                    .Except(new[] { req })
                    .Where(req2 => req2.index != 0 && req2.index % req.freq == 0)
                    .ToList();

                if (other.Count > 0)
                {
                    other.Add(req);
                    redundantRequirements.AddRange(other);

                    var newIndex = other.Max(req => req.index);
                    var newFrequency = other.Aggregate((long)1, (total, current) => total * current.freq);

                    newRequirements.Add((newFrequency, newIndex));
                }
            }

            return initialRequirements
                .Except(redundantRequirements)
                .Concat(newRequirements)
                .ToList();
        }

        private static long Calculate(long n, List<(long freq, int index)> hardRequirements, long max, long offset = 0, long limit = long.MaxValue)
        {
            var increment = max * n;
            var start = (long)0;

            if (offset != 0)
            {
                for (long i = offset; i < long.MaxValue; ++i)
                {
                    if (i % max == 0)
                    {
                        start = i;
                        break;
                    }
                }
            }

            long t = start + increment;

            // Monitoring
            long prevPrintedT = 0;
            int n_prevPrintedT = 1;
            var sw = new Stopwatch();
            sw.Start();
            // Monitoring end

            while (true)
            {
                // Monitoring
                if (t - offset >= 10 * prevPrintedT)
                {
                    if (t >= limit) { return -1; }
                    prevPrintedT = t - offset;

                    Console.WriteLine($"Task {n:D3}:\t[{n_prevPrintedT:D2}]: {0.001 * sw.ElapsedMilliseconds:F2}s\t-->\t{t}");
                    ++n_prevPrintedT;
                }
                // Monitoring end

                if (hardRequirements.Any(req => (t + req.index) % req.freq != 0))
                {
                    t += increment;
                }
                else
                {
                    return t;
                }
            }
        }

        /// <summary>
        /// Solution by @JavierLight (https://gist.github.com/JavierLight/660bdef0b5694d48d7d6fa1e9f559eea)
        /// Visualization: https://streamable.com/tojflp
        /// </summary>
        /// <returns></returns>
        public string Part2_JavierLight()
        {
            var busOffsetsDictionary = new Dictionary<int, long>();
            int minutesOffset = 0;
            foreach (var busId in _busFrequency)
            {
                if (busId != -1)
                {
                    busOffsetsDictionary.Add(minutesOffset, busId);
                }
                minutesOffset++;
            }

            long timestamp = 0;
            long leastCommon = busOffsetsDictionary.First().Value;

            // Tried brute force first, step size was too small and was getting forever :S
            // All ids are prime, so the least common can be accumulated!
            // Until the next timestamp is found, by using the same timestamp as step size we can assume all previous conditions are met (kind of a memoization)
            foreach (var busOffsetKvp in busOffsetsDictionary.Skip(1))
            {
                var offset = busOffsetKvp.Value;
                bool foundNewCommon = false;

                while (!foundNewCommon)
                {
                    timestamp += leastCommon;
                    // Instead of checking that all offsets match, let's go one by one, accumulating the least common to speed up processing
                    if ((timestamp + busOffsetKvp.Key) % busOffsetKvp.Value == 0)
                    {
                        leastCommon *= offset;
                        foundNewCommon = true;
                    }
                }
            }

            return timestamp.ToString();
        }

        /// <summary>
        /// Solution by @mariomka (https://github.com/mariomka/AdventOfCode2020/blob/master/day13/src/lib.rs)
        /// Visualization: https://streamable.com/tojflp
        /// </summary>
        /// <returns></returns>
        public string Part2_mariomka()
        {
            long timestamp = 0;
            int bus_index = 1;
            var increment = _busFrequency[0];

            while (true)
            {
                if (_busFrequency[bus_index] == -1)
                {
                    ++bus_index;
                    continue;
                }

                if ((timestamp + bus_index) % _busFrequency[bus_index] == 0)
                {
                    increment *= _busFrequency[bus_index];
                    ++bus_index;
                }

                if (bus_index >= _busFrequency.Count)
                {
                    break;
                }

                timestamp += increment;
            }

            return timestamp.ToString();
        }

        private (long, List<long>) ParseInput()
        {
            var lines = File.ReadAllLines(InputFilePath);

            var min = long.Parse(lines[0]);
            var frequencies = lines[1].Replace("x", $"{-1}").Split(',').Select(long.Parse);

            return (min, frequencies.ToList());
        }
    }
}
