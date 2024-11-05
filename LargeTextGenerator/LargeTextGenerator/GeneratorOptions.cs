using LargeTextSort.Infrastructure;

namespace LargeTextGenerator
{
    public class GeneratorOptions
    {
        /// <summary>
        /// The path where the generated file will be placed
        /// </summary>
        public string BasePath { get; init; } = Constants.BasePath;

        /// <summary>
        /// The number of sample strings for the text part of the line. 
        /// Each line will have a random sample string. 
        /// In order to have repetitions, this number should be less than the total number of lines.
        /// </summary>
        public int SampleStringsNumber { get; init; } = Constants.SampleStringsNumber;

        /// <summary>
        /// The minimal length of the sample string
        /// </summary>
        public int SampleStringMinLength { get; init; } = Constants.SampleStringMinLength;

        /// <summary>
        /// The maximal length of the sample string
        /// </summary>
        public int SampleStringMaxLength { get; init; } = Constants.SampleStringMaxLength;

        /// <summary>
        /// The number of lines in the generated text files
        /// </summary>
        public int LinesNumber { get; init; } = Constants.LinesNumber;

        /// <summary>
        /// The maximum number in the number part of the line
        /// </summary>
        public int MaxNumberPart { get; init; } = Constants.MaxNumberPart;
    }
}
