// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public static class AssemblerCodeGenerator
{
    private static int nestingCounter;
    private static int ifOperatorCounter;
    private static int whileOperatorCounter;
    private static int storedBytesCounter;
    private static StreamWriter? streamWriter;
    private static Dictionary<string, int> keyValuePairs = new ();

    public static void Generate(string path, SyntaxTree syntaxTree)
    {
        using (streamWriter = new StreamWriter(path))
        {
            GenerateStatementsCode(syntaxTree);
        }
    }

    private static void WriteLine(string str)
    {
        if (streamWriter is null)
        {
            throw new InvalidOperationException();
        }

        for (var i = 0; i < nestingCounter; ++i)
        {
            streamWriter.Write("  ");
        }

        streamWriter.WriteLine(str);
    }

    private static void GenerateStatementsCode(SyntaxTree syntaxTree)
    {
        foreach (var statementSyntaxTree in syntaxTree.Children)
        {
            GenerateStatementCode(statementSyntaxTree);
        }
    }

    private static void GenerateStatementCode(SyntaxTree syntaxTree)
    {
        if (syntaxTree.RootToken.IsOperator(":="))
        {
            GenerateAssignmentCode(syntaxTree);
        }
        else if (syntaxTree.RootToken.IsKeyword("if"))
        {
            GenerateIfCode(syntaxTree);
        }
        else if (syntaxTree.RootToken.IsKeyword("while"))
        {
            GenerateWhileCode(syntaxTree);
        }
        else
        {
            throw new InvalidOperationException("Unexpected token");
        }
    }

    private static void GenerateExpressionCode(string register, SyntaxTree syntaxTree)
    {
        if (syntaxTree.RootToken.Type is TokenType.Const)
        {
            var @const = syntaxTree.RootToken.Attribute;
            ArgumentException.ThrowIfNullOrEmpty(@const);
            WriteLine($"LI {register}, {@const}");
            return;
        }

        if (syntaxTree.RootToken.Type is TokenType.Id)
        {
            var id = syntaxTree.RootToken.Attribute;
            ArgumentException.ThrowIfNullOrEmpty(id);
            var isIdContained = keyValuePairs.TryGetValue(id, out int address);
            if (!isIdContained)
            {
                throw new UndefinedVariableException(syntaxTree.RootToken.Line, id);
            }

            WriteLine($"LW {register}, {address}(x0)");
            return;
        }

        if (syntaxTree.RootToken.Type is not TokenType.Operator)
        {
            throw new InvalidOperationException();
        }

        var rd = register;
        var rs1 = "t1";
        var rs2 = "t2";

        GenerateExpressionCode(rs1, syntaxTree.LeftChild);

        var memoryAddress = storedBytesCounter;
        storedBytesCounter += 4;
        WriteLine($"SW {rs1}, {memoryAddress}(x0)");
        GenerateExpressionCode(rs2, syntaxTree.RightChild);
        WriteLine($"LW {rs1}, {memoryAddress}(x0)");

        switch (syntaxTree.RootToken.Attribute)
        {
            case "+":
                WriteLine($"ADD {rd}, {rs1}, {rs2}");
                break;
            case "-":
                WriteLine($"SUB {rd}, {rs1}, {rs2}");
                break;
            case "*":
                WriteLine($"MUL {rd}, {rs1}, {rs2}");
                break;
            case "/":
                WriteLine($"DIV {rd}, {rs1}, {rs2}");
                break;
            default:
                throw new InvalidOperationException("Unknown operation");
        }
    }

    private static void GenerateAssignmentCode(SyntaxTree syntaxTree)
    {
        var id = syntaxTree.LeftChild.RootToken.Attribute;
        ArgumentException.ThrowIfNullOrEmpty(id);
        GenerateExpressionCode("t1", syntaxTree.RightChild);

        var isIdContained = keyValuePairs.TryGetValue(id, out int memoryAddress);
        if (isIdContained)
        {
            WriteLine($"SW t1, {memoryAddress}(x0)");
        }
        else
        {
            memoryAddress = storedBytesCounter;
            WriteLine($"SW t1, {memoryAddress}(x0)");
            keyValuePairs.Add(id, memoryAddress);
            storedBytesCounter += 4;
        }
    }

    private static void GenerateIfCode(SyntaxTree syntaxTree)
    {
        GenerateExpressionCode("t1", syntaxTree.Children[0]);
        WriteLine($"BLT t1, zero, else{ifOperatorCounter}");
        WriteLine($"then{ifOperatorCounter}:");
        ++nestingCounter;
        GenerateStatementsCode(syntaxTree.Children[1]);
        WriteLine($"JAL zero, fi{ifOperatorCounter}");
        --nestingCounter;
        WriteLine($"else{ifOperatorCounter}:");
        ++nestingCounter;
        GenerateStatementsCode(syntaxTree.Children[2]);
        --nestingCounter;
        WriteLine($"fi{ifOperatorCounter}:");
        ++ifOperatorCounter;
    }

    private static void GenerateWhileCode(SyntaxTree syntaxTree)
    {
        WriteLine($"loop{whileOperatorCounter}:");
        ++nestingCounter;
        GenerateExpressionCode("t1", syntaxTree.Children[0]);
        WriteLine($"BLT t1, zero, done{whileOperatorCounter}");
        GenerateStatementsCode(syntaxTree.Children[1]);
        WriteLine($"JAL zero, loop{whileOperatorCounter}");
        --nestingCounter;
        WriteLine($"done{whileOperatorCounter}:");
        ++whileOperatorCounter;
    }
}
