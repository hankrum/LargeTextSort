namespace LargeTextSort.Models
{
    public class TempFile
    {
        public required string FileName { get; set; }
        public required FileStream Stream { get; set; }
        public string[]? Lines { get; set; }
        public int ChunkLineIndex { get; set; } = 0;
    }
}
