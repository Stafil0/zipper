namespace Zipper.Domain.Pipeline
{
    public class OnProgressEventArgs
    {
        public string Message { get; }
        
        public long Total { get; }
        
        public int Current { get; }

        public OnProgressEventArgs(int current, long total, string message)
        {
            Current = current;
            Total = total;
            Message = message;
        }
    }
}