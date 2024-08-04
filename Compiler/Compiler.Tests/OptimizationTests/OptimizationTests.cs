// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public class OptimizationTests
{
    private const string CorrectSamplesPath = "OptimizationTests/TestSamples/Correct";
    private const string IncorrectSamplesPath = "OptimizationTests/TestSamples/Incorrect";

    public static IEnumerable<TestCaseData> CorrectTestCases()
    {
        foreach (var dir in Directory.EnumerateDirectories(CorrectSamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var sourceCode = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            var optimizedCode = File.ReadAllText(sampleFiles.First(file => file.EndsWith("Optimized.txt")));
            yield return new TestCaseData(sourceCode, optimizedCode, Path.GetFileName(dir));
        }
    }

    public static IEnumerable<TestCaseData> IncorrectTestCases()
    {
        foreach (var dir in Directory.EnumerateDirectories(IncorrectSamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var sourceCode = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            yield return new TestCaseData(sourceCode, Path.GetFileName(dir));
        }
    }

    [TestCaseSource(nameof(CorrectTestCases))]
    public void Test_CorrectCases(string sourceCode, string optimizedCode, string testName)
    {
        var actualTree = Parser.Parse(Lexer.Analyze(sourceCode));
        var expectedTree = Parser.Parse(Lexer.Analyze(optimizedCode));
        actualTree.Optimize();

        var options = new JsonSerializerOptions() { WriteIndented = true };
        var actualJson = JsonSerializer.Serialize(actualTree, options);
        var expectedJson = JsonSerializer.Serialize(expectedTree, options);

        Assert.That(actualJson, Is.EqualTo(expectedJson), $"Test: {testName} failed!");
    }

    [TestCaseSource(nameof(IncorrectTestCases))]
    public void Test_IncorrectCases(string sourceCode, string testName)
    {
        var ast = Parser.Parse(Lexer.Analyze(sourceCode));
        Assert.Throws<CompilerException>(ast.Optimize, $"Test: {testName} failed!");
    }
}
