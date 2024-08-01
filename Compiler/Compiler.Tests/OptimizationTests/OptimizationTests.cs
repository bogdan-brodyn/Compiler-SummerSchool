// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public class OptimizationTests
{
    private const string SamplesPath = "OptimizationTests/TestSamples";

    public static IEnumerable<TestCaseData> Sample()
    {
        foreach (var dir in Directory.EnumerateDirectories(SamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var sourceCode = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            var optimizedCode = File.ReadAllText(sampleFiles.First(file => file.EndsWith("Optimized.txt")));
            yield return new TestCaseData(sourceCode, optimizedCode, Path.GetFileName(dir));
        }
    }

    [TestCaseSource(nameof(Sample))]
    public void Test(string sourceCode, string optimizedCode, string testName)
    {
        var actualTree = Parser.Parse(Lexer.Analyze(sourceCode));
        var expectedTree = Parser.Parse(Lexer.Analyze(optimizedCode));
        actualTree.Optimize();

        var options = new JsonSerializerOptions() { WriteIndented = true };
        var actualJson = JsonSerializer.Serialize(actualTree, options);
        var expectedJson = JsonSerializer.Serialize(expectedTree, options);

        Assert.That(actualJson, Is.EqualTo(expectedJson), $"Test: {testName} failed!");
    }
}
