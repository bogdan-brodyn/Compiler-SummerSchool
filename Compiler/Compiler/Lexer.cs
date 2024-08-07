// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

using System.Text;

public static class Lexer
{
    private static readonly HashSet<string> Keywords =
        new () { "if", "then", "else", "fi", "while", "do", "done" };

    private static int currentLine;

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
        currentLine = 1;
        var tokens = new List<Token>();
        var currentState = State.InitialState;
        var attributeAcc = new StringBuilder();
        for (var i = 0; i < input.Length; ++i)
        {
            var inputChar = input[i];
            var newState = SwitchState(currentState, inputChar);
            RespondStateChange(currentState, inputChar, newState, tokens, attributeAcc);
            currentState = newState;
            if (inputChar == '\n')
            {
                ++currentLine;
            }
        }

        RespondStateChange(currentState, ' ', State.InitialState, tokens, attributeAcc);
        return tokens;
    }

    private static void RespondStateChange(
        State currentState, char inputChar, State newState, List<Token> tokens, StringBuilder attributeAcc)
    {
        if (currentState == State.ConstReadingState && currentState != newState)
        {
            tokens.Add(new Token(TokenType.Const, currentLine, attributeAcc.ToString()));
            attributeAcc.Clear();
        }

        if (currentState == State.IdOrKeywordReadingState && currentState != newState)
        {
            var attribute = attributeAcc.ToString();
            tokens.Add(new Token(
                Keywords.Contains(attribute) ? TokenType.Keyword : TokenType.Id, currentLine, attribute));
            attributeAcc.Clear();
        }

        switch (newState)
        {
            case State.ConstReadingState:
            case State.IdOrKeywordReadingState:
                attributeAcc.Append(inputChar);
                break;
            case State.OperatorReadingState:
                tokens.Add(new Token(TokenType.Operator, currentLine, inputChar.ToString()));
                break;
            case State.AssignmentEndReadingState:
                tokens.Add(new Token(TokenType.Operator, currentLine, ":="));
                break;
            case State.SemicolonReadingState:
                tokens.Add(new Token(TokenType.Semicolon, currentLine));
                break;
            case State.LeftParenthesisReadingState:
                tokens.Add(new Token(TokenType.LeftParenthesis, currentLine));
                break;
            case State.RightParenthesisReadingState:
                tokens.Add(new Token(TokenType.RightParenthesis, currentLine));
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
                throw new InvalidTokenException(currentLine, ch);
            case State.ConstReadingState:
                if (ch == ' ' || ch == '\r' || ch == '\t' || ch == '\n') return State.InitialState;
                if (ch >= '0' && ch <= '9') return State.ConstReadingState;
                if (ch == '+' || ch == '*' || ch == '-' || ch == '/') return State.OperatorReadingState;
                if (ch == ':') return State.AssignmentStartReadingState;
                if (ch == ';') return State.SemicolonReadingState;
                if (ch == '(') return State.LeftParenthesisReadingState;
                if (ch == ')') return State.RightParenthesisReadingState;
                throw new InvalidTokenException(currentLine, ch);
            case State.IdOrKeywordReadingState:
                if (ch == ' ' || ch == '\r' || ch == '\t' || ch == '\n') return State.InitialState;
                if (ch >= '0' && ch <= '9') return State.IdOrKeywordReadingState;
                if (ch >= 'a' && ch <= 'z') return State.IdOrKeywordReadingState;
                if (ch == '+' || ch == '*' || ch == '-' || ch == '/') return State.OperatorReadingState;
                if (ch == ':') return State.AssignmentStartReadingState;
                if (ch == ';') return State.SemicolonReadingState;
                if (ch == '(') return State.LeftParenthesisReadingState;
                if (ch == ')') return State.RightParenthesisReadingState;
                throw new InvalidTokenException(currentLine, ch);
            case State.AssignmentStartReadingState:
                if (ch == '=') return State.AssignmentEndReadingState;
                throw new InvalidTokenException(currentLine, ch);
            default:
                throw new InvalidTokenException(currentLine, ch);
        }
    }
#pragma warning restore SA1503 // Braces should not be omitted
}
