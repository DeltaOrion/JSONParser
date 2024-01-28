namespace JSONParserTool.Lexer;

public enum LexicalToken
{
    StringLiteral,
    NumberLiteral,
    NullLiteral,
    FalseLiteral,
    TrueLiteral,
    BeginArray,
    EndArray,
    BeginObject,
    EndObject,
    NameSeparator,
    ValueSeparator,
    Whitespace,
    None,
}