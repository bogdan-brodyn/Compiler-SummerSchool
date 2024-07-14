// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public class Token(TokenType type, string? attribute = null)
{
    public static Token Semicolon => new (TokenType.Semicolon);

    public static Token Assignment => new (TokenType.Operator, "=");

    public static Token If => new (TokenType.Keyword, "if");

    public static Token While => new (TokenType.Keyword, "while");

    public TokenType Type { get; } = type;

    public string? Attribute { get; } = attribute;
}

public enum TokenType
{
    Id,
    Const,
    Keyword,
    Operator,
    Semicolon,
    LeftParenthesis,
    RightParenthesis,
}
