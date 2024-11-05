using LargeTextSort.Infrastructure;

namespace LargeTextSort.Models
{
    public class SorterOptions
    {
        /// <summary>
        /// The path where the file to be sorted is placed and where the result file will be created
        /// </summary>
        public string BasePath { get; init; } = Constants.BasePath;

        /// <summary>
        /// The number of lines from the file that can be processed in memory for the creation of the temporary sorted files
        /// </summary>
        public int MaxLinesInMemory { get; init; } = Constants.MaxLinesInMemory;

        /// <summary>
        /// The size of the buffer for the file read operations of the temporary files in bytes.
        /// Each stream will use such buffer for the temporary files when merged.
        /// The idea is this buffer to hold a maximum number of lines so to reduce read operations.
        /// This size should be bigger than the maximal length of one line set in the generator options
        /// </summary>
        public int BufferSize { get; init; } = Constants.BufferSize;

    }
}
