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
            if (this.position >= this.tokens.Count)
            {
                throw new InvalidDataException("Invalid syntax");
            }

            return this.tokens[this.position];
        }
    }

    public SyntaxTree Parse()
    {
        var (program, errorOccured) = this.ParseStatements();
        if (errorOccured)
        {
            throw new InvalidDataException("Invalid syntax: id or keyword expected");
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
        else if (this.CurrentTokenIsKeyword("if"))
        {
            return this.ParseIf();
        }
        else if (this.CurrentTokenIsKeyword("while"))
        {
            return this.ParseWhile();
        }

        return null;
    }

    private SyntaxTree ParseAssignment()
    {
        var idTree = new SyntaxTree(this.CurrentToken);
        ++this.position;
        if (!this.CurrentTokenIsOperator(":="))
        {
            throw new InvalidDataException("Invalid syntax: ':=' expected");
        }

        var expressionTree = this.ParseExpression();
        if (this.CurrentToken.Type != TokenType.Semicolon)
        {
            throw new InvalidDataException("Invalid syntax: ';' expected");
        }

        ++this.position;
        return new SyntaxTree(Token.Assignment, idTree, expressionTree);
    }

    private SyntaxTree ParseIf()
    {
        var conditionTree = this.ParseCondition();
        if (!this.CurrentTokenIsKeyword("then"))
        {
            throw new InvalidDataException("Invalid syntax: 'then' expected");
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
        if (!this.CurrentTokenIsKeyword("do"))
        {
            throw new InvalidDataException("Invalid syntax: 'do' expected");
        }

        ++this.position;
        var (doTree, _) = this.ParseStatements();
        if (!this.CurrentTokenIsKeyword("done"))
        {
            throw new InvalidDataException("Invalid syntax: 'done' expected");
        }

        ++this.position;
        return new SyntaxTree(Token.While, conditionTree, doTree);
    }

    private SyntaxTree ParseCondition()
    {
        ++this.position;
        if (this.CurrentToken.Type != TokenType.LeftParenthesis)
        {
            throw new InvalidDataException("Invalid syntax: '(' expected");
        }

        return this.ParseParExpression();
    }

    private SyntaxTree? ParseElse()
    {
        SyntaxTree? elseTree = null;
        if (this.CurrentTokenIsKeyword("else"))
        {
            ++this.position;
            (elseTree, _) = this.ParseStatements();
        }

        if (!this.CurrentTokenIsKeyword("fi"))
        {
            throw new InvalidDataException("Invalid syntax: 'fi' expected");
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
        else if (this.CurrentTokenIsConstOrId())
        {
            operand = new SyntaxTree(this.CurrentToken);
            ++this.position;
        }
        else
        {
            throw new InvalidDataException("Invalid syntax: const or id expected");
        }

        return makeRecursiveCall ? this.ParseTermExtra(operand) : operand;
    }

    private SyntaxTree ParseExpressionExtra(SyntaxTree leftOperand)
    {
        if (this.CurrentTokenIsOperator("+") || this.CurrentTokenIsOperator("-"))
        {
            var operation = this.CurrentToken;
            var rightOperand = this.ParseTerm(true);
            leftOperand = this.GetExpressionTree(operation, leftOperand, rightOperand);
            return this.ParseExpressionExtra(leftOperand);
        }

        return leftOperand;
    }

    private SyntaxTree ParseTermExtra(SyntaxTree leftOperand)
    {
        if (this.CurrentTokenIsOperator("*") || this.CurrentTokenIsOperator("/"))
        {
            var operation = this.CurrentToken;
            var rightOperand = this.ParseTerm(false);
            leftOperand = this.GetExpressionTree(operation, leftOperand, rightOperand);
            return this.ParseTermExtra(leftOperand);
        }

        return leftOperand;
    }

    private SyntaxTree ParseParExpression()
    {
        var expressionTree = this.ParseExpression();
        if (this.CurrentToken.Type != TokenType.RightParenthesis)
        {
            throw new InvalidDataException("Invalid syntax: ')' expected");
        }

        ++this.position;
        return expressionTree;
    }

    private SyntaxTree GetExpressionTree(
        Token operation, SyntaxTree leftOperand, SyntaxTree rightOperand)
    {
        var leftToken = leftOperand.RootToken;
        var rightToken = rightOperand.RootToken;
        if (leftToken.Type == TokenType.Const && rightToken.Type == TokenType.Const)
        {
            var resultToken = this.GetResultToken(operation, leftToken, rightToken);
            return new SyntaxTree(resultToken);
        }

        return new SyntaxTree(operation, leftOperand, rightOperand);
    }

    private Token GetResultToken(
        Token operation, Token leftOperand, Token rightOperand)
    {
        Func<int, int, int> method;
        switch (operation.Attribute)
        {
            case "+":
                method = (x, y) => x + y;
                break;
            case "-":
                method = (x, y) => x - y;
                break;
            case "*":
                method = (x, y) => x * y;
                break;
            case "/":
                method = (x, y) => x / y;
                break;
            default:
                throw new ArgumentException("Unknown operation");
        }

        return new Token(
            TokenType.Const,
            this.GetResultOfOperation(leftOperand, rightOperand, method));
    }

    private string GetResultOfOperation(
        Token leftOperand, Token rightOperand, Func<int, int, int> method)
    {
        var parsedLeft = int.TryParse(leftOperand.Attribute, out int number1);
        var parsedRight = int.TryParse(rightOperand.Attribute, out int number2);
        if (!parsedLeft || !parsedRight)
        {
            throw new ArgumentException("Failed to parse const token attribute");
        }

        return method(number1, number2).ToString();
    }

    private bool CurrentTokenIsKeyword(string keyword)
        => this.CurrentToken.Type == TokenType.Keyword &&
           this.CurrentToken.Attribute == keyword;

    private bool CurrentTokenIsOperator(string op)
        => this.CurrentToken.Type == TokenType.Operator &&
           this.CurrentToken.Attribute == op;

    private bool CurrentTokenIsConstOrId()
        => this.CurrentToken.Type == TokenType.Const ||
           this.CurrentToken.Type == TokenType.Id;
}
