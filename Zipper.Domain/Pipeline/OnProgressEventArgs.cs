namespace Zipper.Domain.Pipeline
{
    public class OnProgressEventArgs
    {
        public string Message { get; }
        
        public long Progress { get; }

        public OnProgressEventArgs(long progress, string message)
        {
            Progress = progress;
            Message = message;
        }
    }
}