// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

using System.Text.Json.Serialization;

public class Token(TokenType type, string? attribute = null)
{
    public TokenType Type { get; } = type;

    public string? Attribute { get; } = attribute;
}

[JsonConverter(typeof(JsonStringEnumConverter<TokenType>))]
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
