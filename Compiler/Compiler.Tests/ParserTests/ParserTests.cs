// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public class ParserTests
{
    private const string CorrectSamplesPath = "ParserTests/TestSamples/Correct";
    private const string IncorrectSamplesPath = "ParserTests/TestSamples/Incorrect";

    public static IEnumerable<TestCaseData> CorrectTestCases()
    {
        foreach (var dir in Directory.EnumerateDirectories(CorrectSamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var text = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            var expectedJson = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".json")));
            yield return new TestCaseData(text, expectedJson, Path.GetFileName(dir));
        }
    }

    public static IEnumerable<TestCaseData> IncorrectTestCases()
    {
        foreach (var dir in Directory.EnumerateDirectories(IncorrectSamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var text = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            yield return new TestCaseData(text, Path.GetFileName(dir));
        }
    }

    [TestCaseSource(nameof(CorrectTestCases))]
    public void Test_CorrectCases(string text, string expectedJson, string testName)
    {
        var tokens = Lexer.Analyze(text);
        var ast = Parser.Parse(tokens);
        var options = new JsonSerializerOptions() { WriteIndented = true };
        var actualJson = JsonSerializer.Serialize(ast, options);
        Assert.That(actualJson, Is.EqualTo(expectedJson), $"Test: {testName} failed!");
    }

    [TestCaseSource(nameof(IncorrectTestCases))]
    public void Test_IncorrectCases(string text, string testName)
    {
        var tokens = Lexer.Analyze(text);
        Assert.Throws<InvalidSyntaxException>(() => Parser.Parse(tokens), $"Test: {testName} failed!");
    }
}
