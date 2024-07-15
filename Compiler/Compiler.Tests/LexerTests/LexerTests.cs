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

    [Test]
    public void Test()
    {
        foreach (var testFile in Directory.EnumerateFiles(SamplesPath))
        {
            var text = File.ReadAllText(testFile);
            var actualTokens = Lexer.Analyze(text);
            var jsonString = File.ReadAllText(Path.ChangeExtension(testFile, "json"));
            LexerTestAnswer? expected = JsonSerializer.Deserialize<LexerTestAnswer>(jsonString);
            if (expected is null)
            {
                throw new JsonException("Json Serializtion failed!");
            }

            Assert.That(actualTokens, Is.EqualTo(expected.Tokens).UsingPropertiesComparer(), $"Test: {testFile} failed");
        }
    }
}
