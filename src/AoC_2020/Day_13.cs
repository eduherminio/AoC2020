using AoCHelper;
using FileParser;
using SheepTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AoC_2020
{
    public class Day_13 : BaseDay
    {
        private readonly List<long> _busFrequency;
        private readonly long _earliestDeparture;

        public Day_13()
        {
            (_earliestDeparture, _busFrequency) = ParseInput();
        }

        public override string Solve_1()
        {
            var validFrequencies = _busFrequency.Where(freq => freq != long.MaxValue).ToList();
            for (uint delta = 0; delta < _earliestDeparture; ++delta)
            {
                foreach (var frequency in validFrequencies)
                {
                    if ((_earliestDeparture + delta) % frequency == 0)
                    {
                        return $"{frequency * delta}";
                    }
                }
            }
            var solution = string.Empty;

            return solution;
        }

        public override string Solve_2() => Part2_Bruteforce_SingleThread();

        public string Solve_2_Wink(long offset)
        {
            var hardRequirements = _busFrequency
                .Select((freq, index) => (freq, index))
                .Where(pair => pair.freq != long.MaxValue)
                .ToList();

            // [A, X] && [Y, A] => [X * Y, A]
            var redundantRequirements = new List<(long, int)>();
            var newRequirements = new List<(long, int)>();

            foreach (var req in hardRequirements)
            {
                var other = hardRequirements
                    .Except(new[] { req })
                    .Where(req2 => req2.index != 0 && (long)req2.index % req.freq == 0)
                    .ToList();

                if (other.Any())
                {
                    other.Add(req);
                    redundantRequirements.AddRange(other);

                    var newIndex = other.Max(req => req.index);
                    var newFrequency = other.Aggregate((long)1, (total, current) => total * current.freq);

                    newRequirements.Add((newFrequency, newIndex));
                }
            }

            var reducedHardRequirements = hardRequirements
                .Except(redundantRequirements)
                .Concat(newRequirements)
                .OrderByDescending(req => req.Item1)
                .ToList();

            var max = reducedHardRequirements[0].Item1;
            var maxIndex = reducedHardRequirements[0].Item2;

            var finalRequirements = reducedHardRequirements
                .Skip(1)
                .Select(pair => (freq: pair.Item1, index: pair.Item2 - maxIndex))
                .ToList();

            //return CalculateSimple(max, maxIndex, finalRequirements);
            var result = Calculate(1, finalRequirements, max, offset).ToString();

            File.WriteAllText($"./{result}", result);
            return result;
        }

        /// <summary>
        /// ~96 min
        /// </summary>
        /// <returns></returns>
        public string Part2_Bruteforce_SingleThread()
        {
            List<(long, int)> reducedHardRequirements = ExtractRequirements();

            var max = reducedHardRequirements[0].Item1;
            var maxIndex = reducedHardRequirements[0].Item2;

            var finalRequirements = reducedHardRequirements
                .Skip(1)
                .Select(pair => (freq: pair.Item1, index: pair.Item2 - maxIndex))
                .ToList();

            return Calculate((long)1, finalRequirements, max).ToString();
        }

        private List<(long freq, int index)> ExtractRequirements()
        {
            var hardRequirements = _busFrequency
                .Select((freq, index) => (freq, index))
                .Where(pair => pair.freq != long.MaxValue)
                .ToList();

            // [A, X] && [Y, A] => [X * Y, A]
            var redundantRequirements = new List<(long, int)>();
            var newRequirements = new List<(long, int)>();

            foreach (var req in hardRequirements)
            {
                var other = hardRequirements
                    .Except(new[] { req })
                    .Where(req2 => req2.index != 0 && (long)req2.index % req.freq == 0)
                    .ToList();

                if (other.Any())
                {
                    other.Add(req);
                    redundantRequirements.AddRange(other);

                    var newIndex = other.Max(req => req.index);
                    var newFrequency = other.Aggregate((long)1, (total, current) => total * current.freq);

                    newRequirements.Add((newFrequency, newIndex));
                }
            }

            return hardRequirements
                .Except(redundantRequirements)
                .Concat(newRequirements)
                .OrderByDescending(req => req.Item1)
                .ToList();
        }

        /// <summary>
        /// Not working/fully tested
        /// </summary>
        /// <returns></returns>
        public string Part2_Bruteforce_MultiThread()
        {
            var requirements = ExtractRequirements();

            var max = requirements[0].freq;
            var maxIndex = requirements[0].index;

            var finalRequirements = requirements
                .Skip(1)
                .Select(pair => (freq: pair.Item1, index: pair.Item2 - maxIndex))
                .ToList();

            var lcm = Convert.ToInt64(requirements.Select(req => (ulong)req.freq).LeastCommonMultiple());

            var taskList = new List<Task<long>>();

            var primes = GetPrimeNumbers(Convert.ToInt32(max)).ToList();
            foreach (var prime in primes)
            {
                taskList.Add(Task.Run(() =>
                    Calculate(prime, finalRequirements, max)));

                //taskList.Add(Task.Run(() =>
                //Calculate(prime, finalRequirements, max, Convert.ToInt64(hardRequirements.Select(req => (ulong)req.freq).LeastCommonMultiple()))));
            }
            ThreadPool.SetMinThreads(primes.Count, 4);

            //Parallel.ForEach(primes, (n) => Calculate(n, hardRequirements, actionDict, max, maxIndex));

            var result = Task.WhenAll(taskList).Result;

            return $"{result.Min() - (long)maxIndex}";
        }

        //private long Max = 2029817890655789;
        private long Max = 100000000000000;
        private long Calculate(long n, List<(long freq, int index)> hardRequirements, long max, long offset = 0)
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

                    Console.WriteLine($"Task {n:D3}:\t[{n_prevPrintedT:D2}]: {0.001 * sw.ElapsedMilliseconds:F2}s\t-->\t{t}");
                    prevPrintedT = t - offset;
                    ++n_prevPrintedT;
                    if (t > Interlocked.Read(ref Max)) { return -1; }
                }
                // Monitoring end

                if (hardRequirements.Any(req =>
                {
                    var realIndex = t + req.index;

                    return realIndex % req.freq != 0;
                }))
                {
                    t += increment;
                    continue;
                }

                Console.WriteLine("********************************" + t + "********************************");
                if (t < Interlocked.Read(ref Max))
                {
                    Interlocked.Exchange(ref Max, t);
                    return t;
                }

                return long.MaxValue;
            }
        }

        private (long, List<long>) ParseInput()
        {
            var lines = File.ReadAllLines(InputFilePath);

            var min = long.Parse(lines[0]);
            var frequencies = lines[1].Replace("x", $"{long.MaxValue}").Split(',').Select(long.Parse);

            return (min, frequencies.ToList());
        }

        private IEnumerable<long> GetPrimeNumbers(int n)
        {
            bool[] prime = new bool[n + 1];
            for (int i = 2; i <= n; i++) prime[i] = true;

            int limit = (int)Math.Ceiling(Math.Sqrt(n));

            for (int i = 2; i <= limit; i++)
                if (prime[i])
                    for (int j = i * i; j <= n; j += i)
                        prime[j] = false;

            for (int i = 0; i <= n; i++)
                if (prime[i])
                    yield return (long)i;
        }
    }
}
