using System;
using System.Runtime.Serialization;

namespace AoC_2020.AssemblyComputer
{
    [Serializable]
    public class InfiniteLoopException : Exception
    {
        public long LastAccumulatorValue { get; set; }

        public InfiniteLoopException()
        {
        }

        public InfiniteLoopException(string? message) : base(message)
        {
        }

        public InfiniteLoopException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InfiniteLoopException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
