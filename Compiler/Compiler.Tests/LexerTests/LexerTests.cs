// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public class LexerTests
{
    private const string SamplesPath = "LexerTests/LexerTestSamples";

    private const string SamplesAnswers = "LexerTests/LexerTestAnswers";

    [Test]
    public void Test()
    {
        foreach (var testFile in Directory.EnumerateFiles(SamplesPath))
        {
            var text = File.ReadAllText(testFile);
            var actualTokens = Lexer.Analyze(text);
            var jsonStringPath = Path.Combine(SamplesAnswers, Path.ChangeExtension(Path.GetFileName(testFile), "json"));
            var jsonString = File.ReadAllText(jsonStringPath);
            var options = new JsonSerializerOptions() { WriteIndented = true };
            List<Token>? expectedTokens = JsonSerializer.Deserialize<List<Token>>(jsonString, options);
            if (expectedTokens is null)
            {
                throw new JsonException("Json Serializtion failed!");
            }

            Assert.That(actualTokens, Is.EqualTo(expectedTokens).UsingPropertiesComparer(), $"Test: {testFile} failed");
        }
    }
}
