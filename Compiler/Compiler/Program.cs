// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            WriteInfo();
            return;
        }

        if (args.Length != 2)
        {
            WriteArgumentErrorMessage();
            return;
        }

        var sourcePath = args[0];
        var destinationPath = args[1];
        Compile(sourcePath, destinationPath);
    }

    private static void WriteInfo()
    {
        Console.WriteLine(
            "Compiler of simple imperative language for RISC-V architecture\n" +
            "For more info see: https://github.com/bogdan-brodyn/Compiler-SummerSchool/\n\n" +
            "To compile your program use arguments: {source file path} {destination file path}");
    }

    private static void WriteArgumentErrorMessage()
    {
        Console.WriteLine(
            "Error: incorrect arguments\n" +
            "Expected: {source file path} {destination file path}");
    }

    private static string ReadFile(string path)
    {
        using var reader = new StreamReader(path);
        return reader.ReadToEnd();
    }

    private static void Compile(string sourcePath, string destinationPath)
    {
        try
        {
            var sourceCode = ReadFile(sourcePath);
            var tokens = Lexer.Analyze(sourceCode);
            var ast = Parser.Parse(tokens);
            ast.Optimize();
            AssemblerCodeGenerator.Generate(destinationPath, ast);
        }
        catch (SystemException e) when (e is CompilerException ||
            e is FileNotFoundException || e is DirectoryNotFoundException)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}
