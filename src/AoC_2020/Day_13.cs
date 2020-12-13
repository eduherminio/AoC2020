using AoCHelper;

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

        public string Solve_2_Wink(long offset, long limit = 100000000000000)
        {
            var requirements = ExtractRequirements();

            var max = requirements[0].freq;
            var maxIndex = requirements[0].index;

            var finalRequirements = requirements
                .Skip(1)
                .Select(pair => (pair.freq, index: pair.index - maxIndex))
                .OrderByDescending(req => req.freq)
                .ToList();

            //return CalculateSimple(max, maxIndex, finalRequirements);
            var result = Calculate(1, finalRequirements, max, offset, limit).ToString();

            File.WriteAllText($"./{result}", result);
            return result;
        }

        /// <summary>
        /// ~96 min
        /// </summary>
        /// <returns></returns>
        public string Part2_Bruteforce_SingleThread()
        {
            var requirements = ExtractRequirements();

            var max = requirements[0].freq;
            var maxIndex = requirements[0].index;

            var finalRequirements = requirements
                .Skip(1)    // Micro-optimization #2: no need to check the step
                .Select(pair => (pair.freq, index: pair.index - maxIndex))
                .OrderByDescending(req => req.freq) // Micro-optimization #3: fail as fast as possible
                .ToList();

            var lcm = requirements.Select(req => (ulong)req.freq).LeastCommonMultiple();
            var longLimit = lcm > (ulong)long.MaxValue ? long.MaxValue : Convert.ToInt64(lcm);

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
            var hardRequirements = _busFrequency
                .Select((freq, index) => (freq, index))
                .Where(pair => pair.freq != long.MaxValue)
                .ToList();

            var redundantRequirements = new List<(long, int)>();
            var newRequirements = new List<(long, int)>();

            foreach (var req in hardRequirements)
            {
                var other = hardRequirements
                    .Except(new[] { req })
                    .Where(req2 => req2.index != 0 && (long)req2.index % req.freq == 0)
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

            return hardRequirements
                .Except(redundantRequirements)
                .Concat(newRequirements)
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
                .Select(pair => (pair.freq, index: pair.index - maxIndex))
                .ToList();

            var lcm = Convert.ToInt64(requirements.Select(req => (ulong)req.freq).LeastCommonMultiple());

            var taskList = new List<Task<long>>();

            var primes = GetPrimeNumbers(max).ToList();
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

                    ++n_prevPrintedT;
                    Console.WriteLine($"Task {n:D3}:\t[{n_prevPrintedT:D2}]: {0.001 * sw.ElapsedMilliseconds:F2}s\t-->\t{t}");
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

        private (long, List<long>) ParseInput()
        {
            var lines = File.ReadAllLines(InputFilePath);

            var min = long.Parse(lines[0]);
            var frequencies = lines[1].Replace("x", $"{-1}").Split(',').Select(long.Parse);

            return (min, frequencies.ToList());
        }

        /// <summary>
        /// https://davidsekar.com/algorithms/sieve-of-eratosthenes-prime
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static IEnumerable<long> GetPrimeNumbers(long n)
        {
            bool[] prime = new bool[n + 1];
            for (long i = 2; i <= n; i++)
            {
                prime[i] = true;
            }

            long limit = (int)Math.Ceiling(Math.Sqrt(n));

            for (long i = 2; i <= limit; i++)
            {
                if (prime[i])
                {
                    for (long j = i * i; j <= n; j += i)
                    {
                        prime[j] = false;
                    }
                }
            }

            for (long i = 0; i <= n; i++)
            {
                if (prime[i])
                {
                    yield return i;
                }
            }
        }
    }
}
