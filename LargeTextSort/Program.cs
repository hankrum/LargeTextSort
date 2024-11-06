using LargeTextSort;
using System.Diagnostics;

Console.WriteLine("Large File Sorter");

var timer = new Stopwatch();
timer.Start();
Console.WriteLine($"Started: {DateTime.Now}");

var sorter = new LargeFileSorter();
sorter.SortFile("Test-150GB.txt", "SortedTest-150GB.txt");
// await sorter.SortFileAsync("Test-15GB.txt", "SortedTest-15GB.txt", new CancellationToken());

timer.Stop();

Console.WriteLine($"Sorted file created.");
Console.WriteLine($"Time taken: {timer.Elapsed}");

