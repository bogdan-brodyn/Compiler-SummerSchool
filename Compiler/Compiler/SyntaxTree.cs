// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public class SyntaxTree
{
    public SyntaxTree(Token rootToken, List<SyntaxTree>? children = null)
    {
        this.RootToken = rootToken;
        if (children == null)
        {
            this.Children = new List<SyntaxTree>();
        }
        else
        {
            this.Children = children;
        }
    }

    public SyntaxTree(Token rootToken, params SyntaxTree[] children)
    {
        this.RootToken = rootToken;
        this.Children = new List<SyntaxTree>(children);
    }

    public Token RootToken { get; private set; }

    public List<SyntaxTree> Children { get; private set; }

    private SyntaxTree LeftChild
    {
        get
        {
            if (this.Children.Count != 2)
            {
                throw new InvalidOperationException();
            }

            return this.Children[0];
        }
    }

    private SyntaxTree RightChild
    {
        get
        {
            if (this.Children.Count != 2)
            {
                throw new InvalidOperationException();
            }

            return this.Children[1];
        }
    }

    public void Optimize()
    {
        if (this.RootToken.Type == TokenType.Semicolon)
        {
            this.OptimizeStatements();
        }
    }

    private void OptimizeStatements()
    {
        var idValues = new Dictionary<string, string>();
        for (int i = 0; i < this.Children.Count; ++i)
        {
            var statementTree = this.Children[i];
            if (statementTree.RootToken.IsOperator(":="))
            {
                statementTree.OptimizeAssignment(idValues);
            }
            else if (statementTree.RootToken.IsKeyword("if"))
            {
                var (replaceStatement, statementsToInsert) = statementTree.OptimizeIf(idValues);
                this.ReplaceStatementAfterOptimization(
                    replaceStatement, statementsToInsert, idValues, ref i);
            }
            else if (statementTree.RootToken.IsKeyword("while"))
            {
                var removeStatement = statementTree.OptimizeWhile(idValues);
                this.ReplaceStatementAfterOptimization(
                    removeStatement, null, idValues, ref i);
            }
        }
    }

    private void OptimizeAssignment(Dictionary<string, string> idValues)
    {
        var idTree = this.LeftChild;
        var expressionTree = this.RightChild;

        string id = idTree.RootToken.Attribute ?? throw new InvalidOperationException();
        expressionTree.OptimizeExpression(idValues);

        if (expressionTree.RootToken.Type == TokenType.Const)
        {
            string value = expressionTree.RootToken.Attribute ?? throw new InvalidOperationException();
            idValues[id] = value;
        }
    }

    private (bool, List<SyntaxTree>?) OptimizeIf(Dictionary<string, string> idValues)
    {
        var conditionTree = this.Children[0];
        var thenTree = this.Children[1];
        var elseTree = this.Children.Count == 3 ? this.Children[2] : null;

        conditionTree.OptimizeExpression(idValues);
        thenTree.Optimize();
        elseTree?.Optimize();

        if (conditionTree.RootToken.Type == TokenType.Const)
        {
            int value = conditionTree.RootToken.ParseConstAttribute();
            if (value > 0)
            {
                return (true, thenTree.Children);
            }

            return (true, elseTree?.Children);
        }

        return (false, null);
    }

    private bool OptimizeWhile(Dictionary<string, string> idValues)
    {
        var conditionTree = this.Children[0];
        var doTree = this.Children[1];

        conditionTree.OptimizeExpression(idValues);
        doTree.Optimize();

        if (conditionTree.RootToken.Type == TokenType.Const)
        {
            int value = conditionTree.RootToken.ParseConstAttribute();
            if (value <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private void ReplaceStatementAfterOptimization(
        bool replaceStatement,
        List<SyntaxTree>? statementsToInsert,
        Dictionary<string, string> idValues,
        ref int position)
    {
        if (replaceStatement)
        {
            this.Children.RemoveAt(position);
            if (statementsToInsert != null)
            {
                this.Children.InsertRange(position, statementsToInsert);
            }

            --position;
        }
        else
        {
            idValues.Clear();
        }
    }

    private void OptimizeExpression(Dictionary<string, string> idValues)
    {
        if (this.RootToken.IsConstOrId())
        {
            this.SubstituteConstValue(idValues);
            return;
        }

        this.LeftChild.OptimizeExpression(idValues);
        this.RightChild.OptimizeExpression(idValues);

        if (this.LeftChild.RootToken.Type == TokenType.Const &&
            this.RightChild.RootToken.Type == TokenType.Const)
        {
            this.RootToken = this.GetResultToken(
                this.LeftChild.RootToken, this.RightChild.RootToken);
            this.Children.Clear();
        }
    }

    private void SubstituteConstValue(Dictionary<string, string> idValues)
    {
        if (this.RootToken.Type == TokenType.Id)
        {
            string id = this.RootToken.Attribute ?? throw new InvalidOperationException();
            bool idFound = idValues.TryGetValue(id, out string? value);
            if (idFound)
            {
                this.RootToken = new Token(TokenType.Const, value);
            }
        }
    }

    private Token GetResultToken(Token leftOperand, Token rightOperand)
    {
        int leftNumber = leftOperand.ParseConstAttribute();
        int rightNumber = rightOperand.ParseConstAttribute();
        int result = this.GetResultOfOperation(leftNumber, rightNumber);
        return new Token(TokenType.Const, result.ToString());
    }

    private int GetResultOfOperation(int leftOperand, int rightOperand)
    {
        return this.RootToken.Attribute switch
        {
            "+" => leftOperand + rightOperand,
            "-" => leftOperand - rightOperand,
            "*" => leftOperand * rightOperand,
            "/" => leftOperand / rightOperand,
            _ => throw new InvalidOperationException("Unknown operation"),
        };
    }
}
