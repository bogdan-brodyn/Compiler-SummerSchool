// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public class Parser
{
    private readonly List<Token> tokens;
    private int position = 0;

    public Parser(List<Token> tokens) => this.tokens = tokens;

    private Token CurrentToken
    {
        get
        {
            if (this.position < this.tokens.Count)
            {
                return this.tokens[this.position];
            }

            return Token.Empty;
        }
    }

    public SyntaxTree Parse()
    {
        var (program, errorOccured) = this.ParseStatements();
        if (errorOccured)
        {
            throw new InvalidDataException(this.GetErrorMessage("id or keyword"));
        }

        return program;
    }

    private (SyntaxTree, bool) ParseStatements()
    {
        var (statements, errorOccured) = this.GetStatementsList();
        var statementsTree = new SyntaxTree(Token.Semicolon, statements);
        return (statementsTree, errorOccured);
    }

    private (List<SyntaxTree>, bool) GetStatementsList()
    {
        var parsedStatements = new List<SyntaxTree>();
        if (this.position >= this.tokens.Count)
        {
            return (parsedStatements, false);
        }

        var statement = this.ParseStatement();
        if (statement == null)
        {
            return (parsedStatements, true);
        }

        parsedStatements.Add(statement);
        var (statements, errorOccured) = this.GetStatementsList();
        parsedStatements.AddRange(statements);
        return (parsedStatements, errorOccured);
    }

    private SyntaxTree? ParseStatement()
    {
        if (this.CurrentToken.Type == TokenType.Id)
        {
            return this.ParseAssignment();
        }
        else if (this.CurrentToken.IsKeyword("if"))
        {
            return this.ParseIf();
        }
        else if (this.CurrentToken.IsKeyword("while"))
        {
            return this.ParseWhile();
        }

        return null;
    }

    private SyntaxTree ParseAssignment()
    {
        var idTree = new SyntaxTree(this.CurrentToken);
        ++this.position;
        if (!this.CurrentToken.IsOperator(":="))
        {
            throw new InvalidDataException(this.GetErrorMessage("':='"));
        }

        var expressionTree = this.ParseExpression();
        if (this.CurrentToken.Type != TokenType.Semicolon)
        {
            throw new InvalidDataException(this.GetErrorMessage("';'"));
        }

        ++this.position;
        return new SyntaxTree(Token.Assignment, idTree, expressionTree);
    }

    private SyntaxTree ParseIf()
    {
        var conditionTree = this.ParseCondition();
        if (!this.CurrentToken.IsKeyword("then"))
        {
            throw new InvalidDataException(this.GetErrorMessage("'then'"));
        }

        ++this.position;
        var (thenTree, _) = this.ParseStatements();
        var elseTree = this.ParseElse();
        if (elseTree == null)
        {
            return new SyntaxTree(Token.If, conditionTree, thenTree);
        }

        return new SyntaxTree(Token.If, conditionTree, thenTree, elseTree);
    }

    private SyntaxTree ParseWhile()
    {
        var conditionTree = this.ParseCondition();
        if (!this.CurrentToken.IsKeyword("do"))
        {
            throw new InvalidDataException(this.GetErrorMessage("'do'"));
        }

        ++this.position;
        var (doTree, _) = this.ParseStatements();
        if (!this.CurrentToken.IsKeyword("done"))
        {
            throw new InvalidDataException(this.GetErrorMessage("'done'"));
        }

        ++this.position;
        return new SyntaxTree(Token.While, conditionTree, doTree);
    }

    private SyntaxTree ParseCondition()
    {
        ++this.position;
        if (this.CurrentToken.Type != TokenType.LeftParenthesis)
        {
            throw new InvalidDataException(this.GetErrorMessage("'('"));
        }

        return this.ParseParExpression();
    }

    private SyntaxTree? ParseElse()
    {
        SyntaxTree? elseTree = null;
        if (this.CurrentToken.IsKeyword("else"))
        {
            ++this.position;
            (elseTree, _) = this.ParseStatements();
        }

        if (!this.CurrentToken.IsKeyword("fi"))
        {
            throw new InvalidDataException(this.GetErrorMessage("'fi'"));
        }

        ++this.position;
        return elseTree;
    }

    private SyntaxTree ParseExpression()
    {
        var termTree = this.ParseTerm(true);
        return this.ParseExpressionExtra(termTree);
    }

    private SyntaxTree ParseTerm(bool makeRecursiveCall)
    {
        ++this.position;
        SyntaxTree operand;
        if (this.CurrentToken.Type == TokenType.LeftParenthesis)
        {
            operand = this.ParseParExpression();
        }
        else if (this.CurrentToken.IsConstOrId())
        {
            operand = new SyntaxTree(this.CurrentToken);
            ++this.position;
        }
        else
        {
            throw new InvalidDataException(this.GetErrorMessage("const or id"));
        }

        return makeRecursiveCall ? this.ParseTermExtra(operand) : operand;
    }

    private SyntaxTree ParseExpressionExtra(SyntaxTree leftOperand)
    {
        if (this.CurrentToken.IsOperator("+") || this.CurrentToken.IsOperator("-"))
        {
            var operation = this.CurrentToken;
            var rightOperand = this.ParseTerm(true);
            leftOperand = new SyntaxTree(operation, leftOperand, rightOperand);
            return this.ParseExpressionExtra(leftOperand);
        }

        return leftOperand;
    }

    private SyntaxTree ParseTermExtra(SyntaxTree leftOperand)
    {
        if (this.CurrentToken.IsOperator("*") || this.CurrentToken.IsOperator("/"))
        {
            var operation = this.CurrentToken;
            var rightOperand = this.ParseTerm(false);
            leftOperand = new SyntaxTree(operation, leftOperand, rightOperand);
            return this.ParseTermExtra(leftOperand);
        }

        return leftOperand;
    }

    private SyntaxTree ParseParExpression()
    {
        var expressionTree = this.ParseExpression();
        if (this.CurrentToken.Type != TokenType.RightParenthesis)
        {
            throw new InvalidDataException(this.GetErrorMessage("')'"));
        }

        ++this.position;
        return expressionTree;
    }

    private string GetErrorMessage(string expectedToken)
        => $"Invalid syntax: {expectedToken} expected, but got {this.CurrentToken}";
}
