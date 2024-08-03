// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public static class Parser
{
    private static List<Token>? tokensToParse;
    private static int position;
    private static int currentLine;

    private static Token CurrentToken
    {
        get
        {
            if (tokensToParse != null && position < tokensToParse.Count)
            {
                var token = tokensToParse[position];
                currentLine = token.Line;
                return token;
            }

            return Token.Empty;
        }
    }

    public static SyntaxTree Parse(List<Token> tokens)
    {
        tokensToParse = tokens;
        (position, currentLine) = (0, 0);

        var (program, errorOccured) = ParseStatements();
        if (errorOccured)
        {
            throw new InvalidDataException(GetErrorMessage("id or keyword"));
        }

        return program;
    }

    private static (SyntaxTree, bool) ParseStatements()
    {
        var (statements, errorOccured) = GetStatementsList();
        var statementsTree = new SyntaxTree(Token.Semicolon, statements);
        return (statementsTree, errorOccured);
    }

    private static (List<SyntaxTree>, bool) GetStatementsList()
    {
        var parsedStatements = new List<SyntaxTree>();
        if (CurrentToken.Type == TokenType.Empty)
        {
            return (parsedStatements, false);
        }

        var statement = ParseStatement();
        if (statement == null)
        {
            return (parsedStatements, true);
        }

        parsedStatements.Add(statement);
        var (statements, errorOccured) = GetStatementsList();
        parsedStatements.AddRange(statements);
        return (parsedStatements, errorOccured);
    }

    private static SyntaxTree? ParseStatement()
    {
        if (CurrentToken.Type == TokenType.Id)
        {
            return ParseAssignment();
        }
        else if (CurrentToken.IsKeyword("if"))
        {
            return ParseIf();
        }
        else if (CurrentToken.IsKeyword("while"))
        {
            return ParseWhile();
        }

        return null;
    }

    private static SyntaxTree ParseAssignment()
    {
        var idTree = new SyntaxTree(CurrentToken);
        ++position;
        if (!CurrentToken.IsOperator(":="))
        {
            throw new InvalidDataException(GetErrorMessage("':='"));
        }

        var expressionTree = ParseExpression();
        if (CurrentToken.Type != TokenType.Semicolon)
        {
            throw new InvalidDataException(GetErrorMessage("';'"));
        }

        ++position;
        return new SyntaxTree(Token.Assignment, idTree, expressionTree);
    }

    private static SyntaxTree ParseIf()
    {
        var conditionTree = ParseCondition();
        if (!CurrentToken.IsKeyword("then"))
        {
            throw new InvalidDataException(GetErrorMessage("'then'"));
        }

        ++position;
        var (thenTree, _) = ParseStatements();
        var elseTree = ParseElse();
        if (elseTree == null)
        {
            return new SyntaxTree(Token.If, conditionTree, thenTree);
        }

        return new SyntaxTree(Token.If, conditionTree, thenTree, elseTree);
    }

    private static SyntaxTree ParseWhile()
    {
        var conditionTree = ParseCondition();
        if (!CurrentToken.IsKeyword("do"))
        {
            throw new InvalidDataException(GetErrorMessage("'do'"));
        }

        ++position;
        var (doTree, _) = ParseStatements();
        if (!CurrentToken.IsKeyword("done"))
        {
            throw new InvalidDataException(GetErrorMessage("'done'"));
        }

        ++position;
        return new SyntaxTree(Token.While, conditionTree, doTree);
    }

    private static SyntaxTree ParseCondition()
    {
        ++position;
        if (CurrentToken.Type != TokenType.LeftParenthesis)
        {
            throw new InvalidDataException(GetErrorMessage("'('"));
        }

        return ParseParExpression();
    }

    private static SyntaxTree? ParseElse()
    {
        SyntaxTree? elseTree = null;
        if (CurrentToken.IsKeyword("else"))
        {
            ++position;
            (elseTree, _) = ParseStatements();
        }

        if (!CurrentToken.IsKeyword("fi"))
        {
            throw new InvalidDataException(GetErrorMessage("'fi'"));
        }

        ++position;
        return elseTree;
    }

    private static SyntaxTree ParseExpression()
    {
        var termTree = ParseTerm(true);
        return ParseExpressionExtra(termTree);
    }

    private static SyntaxTree ParseTerm(bool makeRecursiveCall)
    {
        ++position;
        SyntaxTree operand;
        if (CurrentToken.Type == TokenType.LeftParenthesis)
        {
            operand = ParseParExpression();
        }
        else if (CurrentToken.IsConstOrId())
        {
            operand = new SyntaxTree(CurrentToken);
            ++position;
        }
        else
        {
            throw new InvalidDataException(GetErrorMessage("const or id"));
        }

        return makeRecursiveCall ? ParseTermExtra(operand) : operand;
    }

    private static SyntaxTree ParseExpressionExtra(SyntaxTree leftOperand)
    {
        if (CurrentToken.IsOperator("+") || CurrentToken.IsOperator("-"))
        {
            var operation = CurrentToken;
            var rightOperand = ParseTerm(true);
            leftOperand = new SyntaxTree(operation, leftOperand, rightOperand);
            return ParseExpressionExtra(leftOperand);
        }

        return leftOperand;
    }

    private static SyntaxTree ParseTermExtra(SyntaxTree leftOperand)
    {
        if (CurrentToken.IsOperator("*") || CurrentToken.IsOperator("/"))
        {
            var operation = CurrentToken;
            var rightOperand = ParseTerm(false);
            leftOperand = new SyntaxTree(operation, leftOperand, rightOperand);
            return ParseTermExtra(leftOperand);
        }

        return leftOperand;
    }

    private static SyntaxTree ParseParExpression()
    {
        var expressionTree = ParseExpression();
        if (CurrentToken.Type != TokenType.RightParenthesis)
        {
            throw new InvalidDataException(GetErrorMessage("')'"));
        }

        ++position;
        return expressionTree;
    }

    private static string GetErrorMessage(string expectedToken)
        => $"Line ({currentLine})\nInvalid syntax: {expectedToken} expected, but got {CurrentToken}";
}
