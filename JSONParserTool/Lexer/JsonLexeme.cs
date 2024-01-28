using System.Text.Json;

namespace JSONParserTool.Lexer;

public class JsonLexeme
{
    public string? Value { get; set; }

    public LexicalToken Type { get; set; }

    public int LineNumber { get; set; }

    public static JsonLexeme Seperator(LexicalToken tokenType, int lineNumber)
    {
        return new JsonLexeme()
        {
            Type = tokenType,
            LineNumber = lineNumber,
            Value = null
        };
    }

    public static JsonLexeme Literal(string value, int lineNumber, LexicalToken tokenType)
    {
        var literals = new[]
        {
            LexicalToken.NullLiteral, LexicalToken.NumberLiteral, LexicalToken.TrueLiteral, LexicalToken.FalseLiteral,
            LexicalToken.StringLiteral, LexicalToken.None
        };

        if (!literals.Contains(tokenType))
        {
            throw new Exception();
        }
        
        return new JsonLexeme()
        {
            Type = tokenType,
            LineNumber = lineNumber,
            Value = value
        };
    }
}