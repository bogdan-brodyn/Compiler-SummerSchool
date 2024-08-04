// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

using System.Text.Json.Serialization;

public class Token(TokenType type, int line = 0, string? attribute = null)
{
    public static Token Semicolon => new (TokenType.Semicolon);

    public static Token Assignment => new (TokenType.Operator, attribute: ":=");

    public static Token If => new (TokenType.Keyword, attribute: "if");

    public static Token While => new (TokenType.Keyword, attribute: "while");

    public static Token Empty => new (TokenType.Empty);

    public TokenType Type { get; } = type;

    [JsonIgnore]
    public int Line { get; } = line;

    public string? Attribute { get; } = attribute;

    public int ParseConstAttribute()
    {
        if (this.Type != TokenType.Const || this.Attribute == null)
        {
            throw new InvalidOperationException();
        }

        if (!int.TryParse(this.Attribute, out int value))
        {
            throw new CompilerException(this.Line, $"Const overflow: {this.Attribute}");
        }

        return value;
    }

    public override string ToString()
        => $"{this.Type} {this.Attribute}";

    public bool IsKeyword(string keyword)
        => this.Type == TokenType.Keyword &&
           this.Attribute == keyword;

    public bool IsOperator(string op)
        => this.Type == TokenType.Operator &&
           this.Attribute == op;

    public bool IsConstOrId()
        => this.Type == TokenType.Const ||
           this.Type == TokenType.Id;
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
    Empty,
}
