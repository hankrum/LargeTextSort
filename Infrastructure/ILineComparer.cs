namespace Infrastructure
{
    public interface ILineComparer
    {
        public List<string> Sort(List<string> lines);
        public int Compare(string line1, string line2);
    }
}
