using JSONParserTool.Error;
using JSONParserTool.Lexer;
using JSONParserTool.Model;

namespace JSONParserTool.Parser;

public class ValueParser : AbstractParser
{
    private ResumeType _resumeType = ResumeType.None;

    public override void Parse(List<JsonLexeme> tokens)
    {
        JsonValue? value = null;
        var lineNumber = (Pointer <= 0 || Pointer - 1 >= tokens.Count) ? 0 : tokens[Pointer - 1].LineNumber;

        if (Pointer >= tokens.Count)
        {
            Error = new ParserError(ErrorReason.ExpectedValue, lineNumber);
            return;
        }

        var token = tokens[Pointer];
        switch (token.Type)
        {
            case LexicalToken.TrueLiteral:
                value = new JsonValue(JsonValueType.True, true);
                Pointer++;
                break;
            case LexicalToken.FalseLiteral:
                value = new JsonValue(JsonValueType.False, false);
                Pointer++;
                break;
            case LexicalToken.NullLiteral:
                value = new JsonValue(JsonValueType.Null);
                Pointer++;
                break;
            case LexicalToken.StringLiteral:
                value = new JsonValue(JsonValueType.String, token.Value);
                Pointer++;
                break;
            case LexicalToken.NumberLiteral:
                var numberParser = new NumberParser();
                numberParser.Parse(token);
                if (numberParser.Error != null)
                {
                    Error = numberParser.Error;
                    return;
                }

                value = new JsonValue(JsonValueType.Number, numberParser.Result);
                Pointer++;
                break;
            case LexicalToken.BeginObject:
                AddToCallStack(new ObjectParser(), Pointer, Parent, Key);
                _resumeType = ResumeType.Object;
                return;
            case LexicalToken.BeginArray:
                AddToCallStack(new ArrayParser(), Pointer, Parent, Key);
                _resumeType = ResumeType.Array;
                return;
            default:
                Error = new ParserError(ErrorReason.UnexpectedToken, lineNumber);
                break;
        }

        Result = new JsonNode(Key, Parent, value);
    }

    public override void Resume(List<JsonLexeme> tokens)
    {
        JsonValue? value = null;
        switch (_resumeType)
        {
            case ResumeType.Array:
                value = new JsonValue(JsonValueType.Array, Child.Result.AsArray());
                break;
            case ResumeType.Object:
                value = new JsonValue(JsonValueType.Object, Child.Result.AsObject());
                break;
            default:
                throw new Exception();
        }

        _resumeType = ResumeType.None;
        Pointer = Child.Pointer;
        Result = new JsonNode(Key, Parent, value);
    }

    private enum ResumeType
    {
        Object,
        Array,
        None
    };
}