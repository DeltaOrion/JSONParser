using JSONParserTool.Error;
using JSONParserTool.Lexer;
using JSONParserTool.Model;

namespace JSONParserTool.Parser;

public class ObjectParser : AbstractParser
{
    private bool _endOfMember;
    private string? _lastKey;
    private ResumeType _resumeType;
    private readonly JsonObject _jsonObject;
    private JsonLexeme? _lastToken;
    private readonly JsonNode _node;

    public ObjectParser()
    {
        _resumeType = ResumeType.None;
        _lastToken = null;
        _jsonObject = new JsonObject();
        var value = new JsonValue(JsonValueType.Object, _jsonObject);
        _node = new JsonNode(Key, Parent, value);
    }

    public override void Parse(List<JsonLexeme> tokens)
    {
        while (Pointer < tokens.Count)
        {
            if (Error != null)
            {
                break;
            }

            var token = tokens[Pointer];
            switch (token.Type)
            {
                case LexicalToken.BeginObject:
                    if (_lastToken != null)
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    Pointer++;
                    break;
                case LexicalToken.NameSeparator:
                    if (_lastToken == null || _lastToken.Type != LexicalToken.StringLiteral || _lastKey == null)
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    if (_jsonObject.ContainsKey(_lastKey))
                    {
                        Error = new ParserError(ErrorReason.DuplicateKey, _lastToken.LineNumber);
                        return;
                    }

                    AddToCallStack(new ValueParser(), Pointer + 1, _node, _lastKey);
                    _resumeType = ResumeType.Value;
                    return;
                case LexicalToken.StringLiteral:
                    if (_lastToken == null ||
                        (_lastToken.Type != LexicalToken.BeginObject &&
                         _lastToken.Type != LexicalToken.ValueSeparator))
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    _lastKey = token.Value;
                    Pointer++;
                    break;
                case LexicalToken.ValueSeparator:
                    if (!_endOfMember)
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    _endOfMember = false;
                    Pointer++;
                    break;
                case LexicalToken.EndObject:
                    if (_lastToken == null || (_lastToken.Type != LexicalToken.BeginObject && !_endOfMember))
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    _node.Parent = Parent;
                    _node.Key = Key;
                    Result = _node;
                    Pointer++;
                    return;
                default:
                    Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                    return;
            }

            _lastToken = token;
        }

        var finalLine = Pointer >= tokens.Count ? tokens.Count - 1 : Pointer;
        Error = new ParserError(ErrorReason.ExpectedEndOfObject, tokens[finalLine].LineNumber);
    }

    public override void Resume(List<JsonLexeme> tokens)
    {
        switch (_resumeType)
        {
            case ResumeType.Value:
                if (_lastKey == null)
                {
                    throw new Exception();
                }

                _jsonObject[_lastKey] = Child.Result;
                _endOfMember = true;
                break;
            default:
                throw new Exception();
        }

        _resumeType = ResumeType.None;
        Pointer = Child.Pointer;
        _lastToken = Pointer == 0 ? null : tokens[Pointer - 1];
        ShouldResume = false;
        
        Parse(tokens);
    }

    private enum ResumeType
    {
        Value,
        None,
    }
}