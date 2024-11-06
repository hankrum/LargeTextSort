
using Infrastructure;
using LargeTextSort.Infrastructure;

namespace Tests
{
    public class Tests
    {
        private readonly ILineComparer _lineComparer = new LineComparer();

        [Test]
        public void IsResultFileSorted()
        {
            var a = string.Compare("1230", "1234");

            var bufferSize = 1024 * 1024;
            var filename = "SortedTest-150GB.txt";
            byte[] buffer = new byte[bufferSize];
            using var reader = new StreamReader($"{Path.Combine(Constants.BasePath, filename)}");

            string line = reader.ReadLine();
            var lastLine = string.Empty;
            var lastNotCompared = string.Empty;
            string next;

            int count = 0;
            while ((next = reader.ReadLine()) != null)
            {
                Assert.LessOrEqual(_lineComparer.Compare(line, next), 0, $"**{line}\n****{next}\n{count}");
                line = next;
                count++;
            }

            Assert.AreEqual(Constants.LinesNumber, count + 1);
        }
    }
}