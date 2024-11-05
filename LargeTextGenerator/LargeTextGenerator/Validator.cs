namespace LargeTextGenerator
{
    public static class Validator
    {
        public static void ValidateOptions(GeneratorOptions options)
        {
            if (options.SampleStringMaxLength <= options.SampleStringMinLength)
            {
                throw new ArgumentException("Min value must be lower than max value");
            }

            if (options.SampleStringsNumber >= options.LinesNumber)
            {
                throw new ArgumentException("SampleStrings number must be lower than lines number to have repetitions");
            }
        }
    }
}
