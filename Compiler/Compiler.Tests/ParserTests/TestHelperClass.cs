// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler.Tests;

using System.Text.Json;

public static class TestHelperClass
{
    public static int Dfs(SyntaxTree node, StreamWriter streamWriter, int ver)
    {
        streamWriter.WriteLine($"\t{ver} [label = \"{node.RootToken.Attribute ?? node.RootToken.Type.ToString()}\"]");
        int k = ver + 1;
        foreach (var child in node.Children)
        {
            streamWriter.WriteLine($"\t{ver} -- {k}");
            k = Dfs(child, streamWriter, k) + 1;
        }

        return k;
    }

    public static void TestHelper()
    {
        foreach (var d in Directory.EnumerateDirectories("C:/Users/Legion/Compiler-SummerSchool/Compiler/Compiler.Tests/ParserTests/TestSamples"))
        {
            var f = Directory.GetFiles(d).First(f => f.EndsWith(".txt"));
            try
            {
                var tokens = Lexer.Analyze(File.ReadAllText(f));
                SyntaxTree ast = new Parser(tokens).Parse();

                var json = JsonSerializer.Serialize(ast, new JsonSerializerOptions() { WriteIndented = true });
                var jsonPath = Path.ChangeExtension(f, "json");
                using StreamWriter streamWriter1 = new StreamWriter(jsonPath);
                streamWriter1.WriteLine(json);

                var graph = "graph.txt";
                graph = Path.ChangeExtension(f, null) + graph;
                using StreamWriter streamWriter = new StreamWriter(graph);
                streamWriter.WriteLine("graph {");
                int ver = 0;
                Dfs(ast, streamWriter, ver);
                streamWriter.WriteLine("}");

                var svgPath = Path.ChangeExtension(f, "svg");
                if (!File.Exists(svgPath))
                {
                    File.Create(svgPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}; {Path.GetFileName(f)}");
            }
        }
    }
}
