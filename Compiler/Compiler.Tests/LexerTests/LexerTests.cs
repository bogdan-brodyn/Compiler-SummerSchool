// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public class LexerTests
{
    private const string SamplesPath = "LexerTests/TestSamples";

    public static IEnumerable<TestCaseData> Sample()
    {
        foreach (var dir in Directory.EnumerateDirectories(SamplesPath))
        {
            var sampleFiles = Directory.GetFiles(dir);
            var text = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".txt")));
            var jsonString = File.ReadAllText(sampleFiles.First(file => file.EndsWith(".json")));
            var options = new JsonSerializerOptions() { WriteIndented = true };
            List<Token>? expectedTokens = JsonSerializer.Deserialize<List<Token>>(jsonString, options);
            if (expectedTokens is null)
            {
                throw new JsonException("Json Serializtion failed!");
            }

            yield return new TestCaseData(text, expectedTokens, Path.GetFileName(dir));
        }
    }

    [TestCaseSource(nameof(Sample))]
    public void Test(string text, List<Token> expectedTokens, string testName)
    {
        var actualTokens = Lexer.Analyze(text);
        Assert.That(actualTokens, Is.EqualTo(expectedTokens).UsingPropertiesComparer(), $"Test: {testName} failed!");
    }
}
