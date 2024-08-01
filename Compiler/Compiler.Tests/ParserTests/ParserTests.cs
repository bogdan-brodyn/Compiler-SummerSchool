// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public class ParserTests
{
    private const string SamplesPath = "ParserTests/TestSamples";

    public static IEnumerable<TestCaseData> Sample()
    {
        foreach (var dir in Directory.EnumerateDirectories(SamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var text = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            var jsonString = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".json")));
            yield return new TestCaseData(text, jsonString, Path.GetFileName(dir));
        }
    }

    [TestCaseSource(nameof(Sample))]
    public void Test(string text, string expectedJson, string testName)
    {
        var tokens = Lexer.Analyze(text);
        var ast = Parser.Parse(tokens);
        var options = new JsonSerializerOptions() { WriteIndented = true };
        var actualJson = JsonSerializer.Serialize(ast, options);
        Assert.That(actualJson, Is.EqualTo(expectedJson), $"Test: {testName} failed!");
    }
}
