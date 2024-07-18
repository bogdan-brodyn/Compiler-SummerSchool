// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

using System.Text;

public static class Lexer
{
    private static readonly string[] Keywords = { "if", "then", "else", "fi", "while", "do", "done" };

    private enum State
    {
        InitialState,
        ConstReadingState,
        IdOrKeywordReadingState,
        OperatorReadingState,
        AssignmentStartReadingState,
        AssignmentEndReadingState,
        SemicolonReadingState,
        LeftParenthesisReadingState,
        RightParenthesisReadingState,
    }

    public static List<Token> Analyze(string input)
    {
        var tokens = new List<Token>();
        var currentState = State.InitialState;
        var attributeAcc = new StringBuilder();
        for (var i = 0; i < input.Length; ++i)
        {
            var inputChar = input[i];
            var newState = SwitchState(currentState, inputChar);
            RespondStateChange(currentState, inputChar, newState, tokens, attributeAcc);
            currentState = newState;
        }

        RespondStateChange(currentState, ' ', State.InitialState, tokens, attributeAcc);
        return tokens;
    }

    private static void RespondStateChange(
        State currentState, char inputChar, State newState, List<Token> tokens, StringBuilder attributeAcc)
    {
        if (currentState == State.ConstReadingState && currentState != newState)
        {
            tokens.Add(new Token(TokenType.Const, attributeAcc.ToString()));
            attributeAcc.Clear();
        }

        if (currentState == State.IdOrKeywordReadingState && currentState != newState)
        {
            var attribute = attributeAcc.ToString();
            tokens.Add(new Token(
                Keywords.Contains(attribute) ? TokenType.Keyword : TokenType.Id, attribute));
            attributeAcc.Clear();
        }

        switch (newState)
        {
            case State.ConstReadingState:
            case State.IdOrKeywordReadingState:
                attributeAcc.Append(inputChar);
                break;
            case State.OperatorReadingState:
                tokens.Add(new Token(TokenType.Operator, inputChar.ToString()));
                break;
            case State.AssignmentEndReadingState:
                tokens.Add(new Token(TokenType.Operator, ":="));
                break;
            case State.SemicolonReadingState:
                tokens.Add(new Token(TokenType.Semicolon));
                break;
            case State.LeftParenthesisReadingState:
                tokens.Add(new Token(TokenType.LeftParenthesis));
                break;
            case State.RightParenthesisReadingState:
                tokens.Add(new Token(TokenType.RightParenthesis));
                break;
        }
    }

#pragma warning disable SA1503 // Braces should not be omitted
    private static State SwitchState(State currentState, char ch)
    {
        switch (currentState)
        {
            case State.InitialState:
            case State.OperatorReadingState:
            case State.SemicolonReadingState:
            case State.AssignmentEndReadingState:
            case State.LeftParenthesisReadingState:
            case State.RightParenthesisReadingState:
                if (ch == ' ' || ch == '\r' || ch == '\t' || ch == '\n') return State.InitialState;
                if (ch >= '0' && ch <= '9') return State.ConstReadingState;
                if (ch >= 'a' && ch <= 'z') return State.IdOrKeywordReadingState;
                if (ch == '+' || ch == '*' || ch == '-' || ch == '/') return State.OperatorReadingState;
                if (ch == ':') return State.AssignmentStartReadingState;
                if (ch == ';') return State.SemicolonReadingState;
                if (ch == '(') return State.LeftParenthesisReadingState;
                if (ch == ')') return State.RightParenthesisReadingState;
                throw new Exception();
            case State.ConstReadingState:
                if (ch == ' ' || ch == '\r' || ch == '\t' || ch == '\n') return State.InitialState;
                if (ch >= '0' && ch <= '9') return State.ConstReadingState;
                if (ch == '+' || ch == '*' || ch == '-' || ch == '/') return State.OperatorReadingState;
                if (ch == ':') return State.AssignmentStartReadingState;
                if (ch == ';') return State.SemicolonReadingState;
                if (ch == '(') return State.LeftParenthesisReadingState;
                if (ch == ')') return State.RightParenthesisReadingState;
                throw new Exception();
            case State.IdOrKeywordReadingState:
                if (ch == ' ' || ch == '\r' || ch == '\t' || ch == '\n') return State.InitialState;
                if (ch >= '0' && ch <= '9') return State.IdOrKeywordReadingState;
                if (ch >= 'a' && ch <= 'z') return State.IdOrKeywordReadingState;
                if (ch == '+' || ch == '*' || ch == '-' || ch == '/') return State.OperatorReadingState;
                if (ch == ':') return State.AssignmentStartReadingState;
                if (ch == ';') return State.SemicolonReadingState;
                if (ch == '(') return State.LeftParenthesisReadingState;
                if (ch == ')') return State.RightParenthesisReadingState;
                throw new Exception();
            case State.AssignmentStartReadingState:
                if (ch == '=') return State.AssignmentEndReadingState;
                throw new Exception();
            default:
                throw new Exception();
        }
    }
#pragma warning restore SA1503 // Braces should not be omitted
}
