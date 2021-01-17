namespace Zipper.Domain.Pipeline
{
    /// <summary>
    /// Delegate to handle pipeline progression. 
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Event arguments</param>
    public delegate void OnProgressHandler(object sender, OnProgressEventArgs args);

    /// <summary>
    /// Arguments for OnProgressHandler delegate.
    /// </summary>
    public class OnProgressEventArgs
    {
        /// <summary>
        /// Message.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Total progression of pipeline.
        /// </summary>
        public long Total { get; }
        
        /// <summary>
        /// Current progression of pipeline.
        /// </summary>
        public int Current { get; }

        /// <summary>
        /// Initialize instance of event arguments.
        /// </summary>
        /// <param name="current">Current progression of pipeline.</param>
        /// <param name="total">Total progression of pipeline.</param>
        /// <param name="message">Message.</param>
        public OnProgressEventArgs(int current, long total, string message)
        {
            Current = current;
            Total = total;
            Message = message;
        }
    }
}