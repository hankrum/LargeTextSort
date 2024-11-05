using LargeTextGenerator;
using System.Diagnostics;

Console.WriteLine("Large File Generator");

Console.WriteLine($"Started: {DateTime.Now}");

var timer = new Stopwatch();
timer.Start();

var generator = new LargeTextFileGenerator();
generator.GenerateFile("Test-150GB.txt");

timer.Stop();
Console.WriteLine($"Time taken: {timer.Elapsed}");
