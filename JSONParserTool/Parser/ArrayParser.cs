using JSONParserTool.Error;
using JSONParserTool.Lexer;
using JSONParserTool.Model;

namespace JSONParserTool.Parser;

public class ArrayParser : AbstractParser
{
    private JsonLexeme? _lastToken = null;
    private bool _endOfMember = false;
    private int _index = 0;

    private JsonNode _arrayNode;
    private List<JsonNode> _list;

    private ResumeType _resumeType;

    public ArrayParser()
    {
        _resumeType = ResumeType.None;
        _list = new List<JsonNode>();
        var jsonArray = new JsonArray(_list);
        var jsonArrayValue = new JsonValue(JsonValueType.Array, jsonArray);
        _arrayNode = new JsonNode(Key, Parent, jsonArrayValue);
    }

    public override void Parse(List<JsonLexeme> tokens)
    {
        while (Pointer < tokens.Count)
        {
            var token = tokens[Pointer];
            switch (token.Type)
            {
                case LexicalToken.BeginArray:
                    if (_lastToken != null && _lastToken.Type != LexicalToken.BeginArray &&
                        _lastToken.Type != LexicalToken.ValueSeparator)
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    if (_lastToken != null && (_lastToken.Type == LexicalToken.BeginArray ||
                                               _lastToken.Type == LexicalToken.ValueSeparator))
                    {
                        AddToCallStack(new ValueParser(), Pointer, _arrayNode, _index.ToString());
                        _resumeType = ResumeType.Value;
                        return;
                    }

                    Pointer++;
                    break;
                case LexicalToken.EndArray:
                    if (_lastToken == null || (_lastToken.Type != LexicalToken.BeginArray && !_endOfMember))
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    _arrayNode.Key = Key;
                    _arrayNode.Parent = Parent;
                    Result = _arrayNode;
                    Pointer++;
                    return;
                case LexicalToken.ValueSeparator:
                    if (!_endOfMember)
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    Pointer++;
                    _index++;
                    _endOfMember = false;
                    break;
                default:
                    if (_lastToken == null || (_lastToken.Type != LexicalToken.BeginArray &&
                                               _lastToken.Type != LexicalToken.ValueSeparator))
                    {
                        Error = new ParserError(ErrorReason.UnexpectedToken, token.LineNumber);
                        return;
                    }

                    _resumeType = ResumeType.Value;
                    AddToCallStack(new ValueParser(), Pointer, _arrayNode, _index.ToString());
                    return;
            }

            _lastToken = token;
        }

        Error = new ParserError(ErrorReason.ExpectedEndOfArray, _lastToken?.LineNumber ?? 0);
    }

    public override void Resume(List<JsonLexeme> tokens)
    {
        switch (_resumeType)
        {
            case ResumeType.Value:
                _list.Add(Child.Result);
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
        None,
        Value
    }
}