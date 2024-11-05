using System.Text;

namespace LargeTextGenerator
{
    public class LargeTextFileGenerator
    {
        private readonly Random _random;

        private readonly GeneratorOptions _options;

        /// <summary>
        /// The generator generates a text file of lines in the pattern: "number. text"
        /// Custom options can be defined or the default values for about of 150GB file will be used
        /// </summary>
        public LargeTextFileGenerator(GeneratorOptions options = null)
        {
            _random = new Random();
            _options = options ?? new GeneratorOptions();
            Validator.ValidateOptions(_options);
        }

        public void GenerateFile(string filePath)
        {
            filePath = Path.Combine(_options.BasePath, filePath);
            var sampleStrings = GenerateSampleStrings();
            var lineCount = _options.LinesNumber;
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    long number = _random.Next(1, _options.MaxNumberPart);
                    string text = sampleStrings[_random.Next(sampleStrings.Length)];
                    writer.WriteLine($"{number}. {text}");
                }
            }
        }

        private string[] GenerateSampleStrings()
        {
            var charsToUse = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            charsToUse = $"{charsToUse} {charsToUse.ToLower()}";

            var result = new string[_options.SampleStringMaxLength];
            for (var i = 0; i < _options.SampleStringMaxLength; i++)
            {
                var sb = new StringBuilder();

                for (var j = 0; j < _random.Next(_options.SampleStringMinLength, _options.SampleStringMaxLength); j++)
                {
                    sb.Append(charsToUse[_random.Next(charsToUse.Length)]);
                }

                result[i] = sb.ToString().Trim();
            }

            return result;
        }
    }
}
