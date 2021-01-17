using System;
using System.IO;

namespace Zipper.Tests.Utils
{
    public class TempFile : IDisposable
    {
        public string FullPath { get; }

        public void Dispose()
        {
            try
            {
                if (File.Exists(FullPath))
                    File.Delete(FullPath);
            }
            catch 
            {
                // Suppress any errors.
            }
        }
        
        public TempFile()
        {
            FullPath = Path.GetTempFileName();
        }
    }
}