using AoCHelper;
using SheepTools;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_25 : BaseDay
    {
        private readonly int _doorPublicKey;
        private readonly int _cardPublicKey;

        public Day_25()
        {
            (_doorPublicKey, _cardPublicKey) = ParseInput();
        }

        public override string Solve_1()
        {
            var doorLoopSize = CalculateLoopSize(7, _doorPublicKey);
            var cardLoopSize = CalculateLoopSize(7, _cardPublicKey);

            var encryptionKey1 = TransformSubjectNumber(_cardPublicKey, doorLoopSize);

#if DEBUG
            var encryptionKey2 = TransformSubjectNumber(_doorPublicKey, cardLoopSize);

            if (encryptionKey1 != encryptionKey2)
            {
                throw new SolvingException();
            }
#endif

            return encryptionKey1.ToString();
        }

        public override string Solve_2() => string.Empty;

        private static int CalculateLoopSize(int subject, int publicKey)
        {
            int loopSize = 0;
            long tempSubject = 1;

            while (tempSubject != publicKey)
            {
                tempSubject = MutateSubjectNumber(tempSubject, subject);
                ++loopSize;
            }

            return loopSize;
        }

        private static long TransformSubjectNumber(int subject, int loopSize)
        {
            long tempSubject = 1;

            for (int i = 1; i <= loopSize; ++i)
            {
                tempSubject = MutateSubjectNumber(tempSubject, subject);
            }

            return tempSubject;
        }

        private static long MutateSubjectNumber(long previousSubject, int originalSubject)
        {
            return (previousSubject * originalSubject) % 20201227;
        }

        private (int DoorPublicKey, int CardPublicKey) ParseInput()
        {
            var rawInput = File.ReadAllLines(InputFilePath).Select(int.Parse);
            Ensure.Count(2, rawInput);

            return (rawInput.First(), rawInput.Last());
        }
    }
}
