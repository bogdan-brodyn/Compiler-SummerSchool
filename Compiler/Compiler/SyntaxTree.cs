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

    private static bool TreeHasNoStatements(SyntaxTree? tree)
        => tree == null || tree.Children.Count == 0;

    private void OptimizeStatements()
    {
        var idValues = new Dictionary<string, string>();
        for (int i = 0; i < this.Children.Count; ++i)
        {
            var statementTree = this.Children[i];
            var optimizationData = statementTree.OptimizeStatement(idValues);
            this.OptimizeUsingData(optimizationData, idValues, ref i);
        }
    }

    private OptimizationData OptimizeStatement(Dictionary<string, string> idValues)
    {
        if (this.RootToken.IsOperator(":="))
        {
            return this.OptimizeAssignment(idValues);
        }
        else if (this.RootToken.IsKeyword("if"))
        {
            return this.OptimizeIf(idValues);
        }
        else if (this.RootToken.IsKeyword("while"))
        {
            return this.OptimizeWhile();
        }

        throw new InvalidOperationException("Unexpected token");
    }

    private OptimizationData OptimizeAssignment(Dictionary<string, string> idValues)
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

        return new OptimizationData(Optimization.None, null);
    }

    private OptimizationData OptimizeIf(Dictionary<string, string> idValues)
    {
        var conditionTree = this.Children[0];
        var thenTree = this.Children[1];
        var elseTree = this.Children.Count == 3 ? this.Children[2] : null;

        conditionTree.OptimizeExpression(idValues);
        thenTree.Optimize();
        elseTree?.Optimize();

        if (TreeHasNoStatements(thenTree) && TreeHasNoStatements(elseTree))
        {
            return new OptimizationData(Optimization.ReplaceStatement, null);
        }

        if (conditionTree.RootToken.Type == TokenType.Const)
        {
            int value = conditionTree.RootToken.ParseConstAttribute();
            if (value > 0)
            {
                return new OptimizationData(Optimization.ReplaceStatement, thenTree.Children);
            }

            return new OptimizationData(Optimization.ReplaceStatement, elseTree?.Children);
        }

        return new OptimizationData(Optimization.ClearIdValues, null);
    }

    private OptimizationData OptimizeWhile()
    {
        var conditionTree = this.Children[0];
        var doTree = this.Children[1];

        conditionTree.OptimizeExpression(null);
        doTree.Optimize();

        if (TreeHasNoStatements(doTree))
        {
            return new OptimizationData(Optimization.ReplaceStatement, null);
        }

        if (conditionTree.RootToken.Type == TokenType.Const)
        {
            int value = conditionTree.RootToken.ParseConstAttribute();
            if (value > 0)
            {
                return new OptimizationData(Optimization.RemoveFollowingStatements, null);
            }

            return new OptimizationData(Optimization.ReplaceStatement, null);
        }

        return new OptimizationData(Optimization.ClearIdValues, null);
    }

    private void OptimizeUsingData(
        OptimizationData data,
        Dictionary<string, string> idValues,
        ref int position)
    {
        switch (data.Optimization)
        {
            case Optimization.None:
                return;
            case Optimization.ClearIdValues:
                idValues.Clear();
                return;
            case Optimization.ReplaceStatement:
                this.ReplaceStatement(data.StatementsToInsert, ref position);
                return;
            case Optimization.RemoveFollowingStatements:
                this.Children.RemoveRange(position + 1, this.Children.Count - position - 1);
                return;
        }
    }

    private void ReplaceStatement(List<SyntaxTree>? statementsToInsert, ref int position)
    {
        this.Children.RemoveAt(position);
        if (statementsToInsert != null)
        {
            this.Children.InsertRange(position, statementsToInsert);
        }

        --position;
    }

    private void OptimizeExpression(Dictionary<string, string>? idValues)
    {
        if (this.RootToken.IsConstOrId())
        {
            this.SubstituteWithConstValue(idValues);
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

    private void SubstituteWithConstValue(Dictionary<string, string>? idValues)
    {
        if (this.RootToken.Type == TokenType.Id && idValues != null)
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

    private class OptimizationData(Optimization optimization, List<SyntaxTree>? statementsToInsert)
    {
        public Optimization Optimization { get; } = optimization;

        public List<SyntaxTree>? StatementsToInsert { get; } = statementsToInsert;
    }
}

public enum Optimization
{
    None,
    ClearIdValues,
    ReplaceStatement,
    RemoveFollowingStatements,
}
