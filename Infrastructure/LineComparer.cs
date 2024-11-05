namespace Infrastructure
{
    public class LineComparer : ILineComparer
    {
        public  int Compare(string line1, string line2)
        {
            var (num1, text1) = ParseLine(line1);
            var (num2, text2) = ParseLine(line2);

            int comparison = string.Compare(text1, text2, StringComparison.CurrentCulture);

            if (comparison == 0)
            {
                comparison = num1.CompareTo(num2);
            }

            return comparison;
        }

        public List<string> Sort(List<string> lines)
        {
            lines = lines.AsParallel()
                         .OrderBy(line => ParseLine(line).Item2) // Sort by string part
                         .ThenBy(line => ParseLine(line).Item1)  // Then by number part
                         .ToList();
            return lines;
        }

        private (int, string) ParseLine(string line)
        {
            int separatorIndex = line.IndexOf(". ");
            int number = int.Parse(line.Substring(0, separatorIndex));
            string text = line.Substring(separatorIndex + 2);
            return (number, text);
        }

        //private (string, string) ParseLine(string line)
        //{
        //    int separatorIndex = line.IndexOf(". ");
        //    string number = line.Substring(0, separatorIndex);
        //    string text = line.Substring(separatorIndex + 2);
        //    return (number, text);
        //}
    }
}
