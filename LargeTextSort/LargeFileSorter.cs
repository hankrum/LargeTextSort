using Infrastructure;
using LargeTextSort.Models;
using System.Text;

namespace LargeTextSort
{
    public class LargeFileSorter
    {
        private readonly ILineComparer _comparer;
        private readonly SorterOptions _options;

        /// <summary>
        /// The Sorter sorts text files using the following algorithm:
        /// The file is split in a number of smaller files that can be sorted in memory
        /// and these sorted files are further merged in the result file
        /// Both synchronous and asynchronous methods for sorting can be used
        /// </summary>
        /// <param name="comparer">A comparer with compare and sort methods can be provided as parameter or the default comparer will be used</param>
        /// <param name="options">Sorter options can be provided as parameter or the default options will be used</param>
        public LargeFileSorter(ILineComparer? comparer = null, SorterOptions? options = null)
        {
            _comparer = comparer ?? new LineComparer();
            _options = options ?? new SorterOptions();
        }

        public void SortFile(string inputFilePath, string outputFilePath)
        {
            string tempDirectory = Path.Combine(_options.BasePath, "FileSortTemp");
            inputFilePath = Path.Combine(_options.BasePath, inputFilePath);
            Directory.CreateDirectory(tempDirectory);

            // Step 1: Split the file into sorted chunks
            List<string> tempFiles = SplitAndSortChunks(inputFilePath, tempDirectory);

            // var tempFiles = Directory.GetFiles(tempDirectory).ToList();

            // Step 2: Merge sorted chunks
            MergeChunks(tempFiles, outputFilePath);

            // Cleanup
            Directory.Delete(tempDirectory, true);
        }

        public async Task SortFileAsync(string inputFilePath, string outputFilePath, CancellationToken ct)
        {
            string tempDirectory = Path.Combine(_options.BasePath, "FileSortTemp");
            inputFilePath = Path.Combine(_options.BasePath, inputFilePath);
            Directory.CreateDirectory(tempDirectory);

            // Step 1: Split the file into sorted chunks
            List<string> tempFiles = await SplitAndSortChunksAsync(inputFilePath, tempDirectory, ct);

            tempFiles = Directory.GetFiles(tempDirectory).ToList();

            // Step 2: Merge sorted chunks
            await MergeChunksAsync(tempFiles, outputFilePath, ct);

            // Cleanup
            Directory.Delete(tempDirectory, true);
        }

        private List<string> SplitAndSortChunks(string inputFilePath, string tempDirectory)
        {
            var tempFiles = new List<string>();
            var lines = new List<string>();

            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);

                    if (lines.Count >= _options.MaxLinesInMemory)
                    {
                        tempFiles.Add(SaveSortedChunk(lines, tempDirectory));
                        lines.Clear();
                    }
                }
            }

            // Sort and save any remaining lines
            if (lines.Count > 0)
            {
                tempFiles.Add(SaveSortedChunk(lines, tempDirectory));
            }

            return tempFiles;
        }

        private async Task<List<string>> SplitAndSortChunksAsync(string inputFilePath, string tempDirectory, CancellationToken ct)
        {
            var tempFiles = new List<string>();
            var lines = new List<string>();

            using (StreamReader reader = new StreamReader(inputFilePath, new FileStreamOptions { Options = FileOptions.Asynchronous }))
            {
                string? line;
                while ((line = await reader.ReadLineAsync(ct)) != null)
                {
                    lines.Add(line);

                    if (lines.Count >= _options.MaxLinesInMemory)
                    {
                        tempFiles.Add(await SaveSortedChunkAsync(lines, tempDirectory, ct));
                        lines.Clear();
                    }
                }
            }

            // Sort and save any remaining lines
            if (lines.Count > 0)
            {
                tempFiles.Add(await SaveSortedChunkAsync(lines, tempDirectory, ct));
            }

            return tempFiles;
        }

        private string SaveSortedChunk(List<string> lines, string tempDirectory)
        {
            lines = _comparer.Sort(lines);

            string tempFilePath = Path.Combine(tempDirectory, Guid.NewGuid().ToString() + ".txt");
            File.WriteAllLines(tempFilePath, lines);

            return tempFilePath;
        }

        private async Task<string> SaveSortedChunkAsync(List<string> lines, string tempDirectory, CancellationToken ct)
        {
            lines = _comparer.Sort(lines);

            string tempFilePath = Path.Combine(tempDirectory, Guid.NewGuid().ToString() + ".txt");
            await File.WriteAllLinesAsync(tempFilePath, lines, ct);

            return tempFilePath;
        }

        private void MergeChunks(List<string> tempFileNames, string outputFilePath)
        {

            outputFilePath = Path.Combine(_options.BasePath, outputFilePath);

            // If we only have one temporary file, just copy it as the sorted result
            if (tempFileNames.Count == 1)
            {
                // TODO: Make it async
                File.Copy(tempFileNames[0], outputFilePath, overwrite: true);
                return;
            }

            using var writer = new StreamWriter(outputFilePath);

            int bufferSize = _options.BufferSize;

            var tempFiles = new Dictionary<string, TempFile>();

            try
            {
                foreach (var fileName in tempFileNames)
                {
                    var streamReader = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);

                    var file = new TempFile
                    {
                        FileName = fileName,
                        Stream = streamReader,
                        ChunkLineIndex = 0
                    };

                    file = ReadChunk(file);

                    var isEOF = file.Lines == null;

                    var lines = new string[0];

                    if (isEOF)
                    {
                        continue;
                    }

                    tempFiles.Add(fileName, file);
                }

                var lineCount = 0;

                while (tempFiles.Count > 0)
                {
                    var minvalue = string.Empty;
                    var minFileName = string.Empty;

                    var tempfileNames = tempFiles.Keys.ToArray();

                    foreach (var tempFileName in tempfileNames)
                    {
                        var tempFile = tempFiles[tempFileName];

                        if (tempFile.ChunkLineIndex >= tempFile.Lines.Length - 1)
                        {
                            tempFile = ReadChunk(tempFile);
                        }

                        var line = tempFile.Lines[tempFile.ChunkLineIndex].Trim();

                        if (line == string.Empty)
                        {
                            continue;
                        }

                        if (minvalue == string.Empty || _comparer.Compare(line, minvalue) < 0)
                        {
                            minvalue = line;
                            minFileName = tempFileName;
                        }
                    }

                    if (minFileName == "")
                    {
                        var file = tempFiles.First().Value;
                        file.Stream.Dispose();
                        tempFiles.Remove(file.FileName);
                        File.Delete(file.FileName);

                        continue;
                    }

                    writer.WriteLine(minvalue);
                    tempFiles[minFileName].ChunkLineIndex++;
                    lineCount++;
                }
            }
            finally
            {
                foreach (var file in tempFiles)
                {
                    file.Value.Stream.Dispose();
                }
            }
        }

        private TempFile ReadChunk(TempFile tempFile)
        {
            int bufferSize = _options.BufferSize;

            tempFile.ChunkLineIndex = 0;

            var buffer = new byte[bufferSize];

            var bytesRead = tempFile.Stream.Read(buffer, 0, bufferSize);

            var isEOF = bytesRead <= 0;

            var lastLine = tempFile.Lines?[^1] ?? string.Empty;

            if (isEOF)
            {
                tempFile.Lines = new string[1];
                tempFile.Lines[0] = lastLine;
                return tempFile;
            }

            var textChunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var lines = textChunk.Split("\r\n");

            if (lines[^1].EndsWith("\r") || lines[^1].EndsWith("\n"))
            {
                var ls = lines.ToList();
                ls.Add(string.Empty);
                lines = [.. ls];
            }

            lines[0] = $"{lastLine.Trim(['\r', '\n'])}{lines[0].Trim('\r', '\n')}";
            tempFile.Lines = lines;

            return tempFile;
        }

        private async Task MergeChunksAsync(List<string> tempFileNames, string outputFilePath, CancellationToken ct)
        {
            outputFilePath = Path.Combine(_options.BasePath, outputFilePath);

            // If we only have one temporary file, just copy it as the sorted result
            if (tempFileNames.Count == 1)
            {
                // TODO: make it async
                File.Copy(tempFileNames[0], outputFilePath, overwrite: true);
                return;
            }

            using var writer = new StreamWriter(outputFilePath, new FileStreamOptions { Options = FileOptions.Asynchronous, Access = FileAccess.Write, Mode = FileMode.OpenOrCreate });

            int bufferSize = _options.BufferSize;

            var tempFiles = new Dictionary<string, TempFile>();

            try
            {
                foreach (var fileName in tempFileNames)
                {
                    var streamReader = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);

                    var file = new TempFile
                    {
                        FileName = fileName,
                        Stream = streamReader,
                        ChunkLineIndex = 0
                    };

                    file = await ReadChunkAsync(file, ct);

                    var isEOF = file.Lines == null;

                    if (isEOF)
                    {
                        continue;
                    }

                    tempFiles.Add(fileName, file);
                }

                while (tempFiles.Count > 0)
                {
                    var minvalue = string.Empty;
                    var minFileName = string.Empty;

                    var tempfileNames = tempFiles.Keys.ToArray();

                    foreach (var tempFileName in tempfileNames)
                    {
                        var tempFile = tempFiles[tempFileName];

                        if (tempFile.ChunkLineIndex >= tempFile.Lines.Length - 1)
                        {
                            tempFile = await ReadChunkAsync(tempFile, ct);
                        }

                        var line = tempFile.Lines[tempFile.ChunkLineIndex].Trim();

                        if (line == string.Empty)
                        {
                            continue;
                        }

                        if (minvalue == string.Empty || _comparer.Compare(line, minvalue) < 0)
                        {
                            minvalue = line;
                            minFileName = tempFileName;
                        }
                    }

                    if (minFileName == "")
                    {
                        var file = tempFiles.First().Value;
                        file.Stream.Dispose();
                        tempFiles.Remove(file.FileName);
                        File.Delete(file.FileName);

                        continue;
                    }

                    await writer.WriteLineAsync(new StringBuilder(minvalue), ct);
                    tempFiles[minFileName].ChunkLineIndex++;
                }
            }
            finally
            {
                foreach (var file in tempFiles)
                {
                    file.Value.Stream.Dispose();
                }
            }
        }

        private async Task<TempFile> ReadChunkAsync(TempFile tempFile, CancellationToken ct)
        {
            int bufferSize = _options.BufferSize;

            tempFile.ChunkLineIndex = 0;

            var buffer = new byte[bufferSize];

            var bytesRead = await tempFile.Stream.ReadAsync(buffer, 0, bufferSize, ct);

            var isEOF = bytesRead <= 0;

            var lastLine = tempFile.Lines?[^1] ?? string.Empty;

            if (isEOF)
            {
                tempFile.Lines = new string[1];
                tempFile.Lines[0] = lastLine;
                return tempFile;
            }

            var textChunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var lines = textChunk.Split("\r\n");

            if (lines[^1].EndsWith("\r") || lines[^1].EndsWith("\n"))
            {
                var ls = lines.ToList();
                ls.Add(string.Empty);
                lines = [.. ls];
            }

            lines[0] = $"{lastLine.Trim(['\r', '\n'])}{lines[0].Trim('\r', '\n')}";
            tempFile.Lines = lines;

            return tempFile;
        }
    }
}
