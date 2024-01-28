using JSONParserTool.Error;
using JSONParserTool.Lexer;
using JSONParserTool.Model;

namespace JSONParserTool.Parser;

public abstract class AbstractParser : IParser
{
    public abstract void Parse(List<JsonLexeme> tokens);

    public abstract void Resume(List<JsonLexeme> tokens);

    protected void AddToCallStack(IParser parser, int pointer, JsonNode parent, string key)
    {
        CallStack.Push(this);
        parser.Pointer = pointer;
        parser.Parent = parent;
        parser.Key = key;
        parser.CallStack = CallStack;
        CallStack.Push(parser);
        Child = parser;
        ShouldResume = true;
    }

    public ParserError? Error { get; protected set; }
    public JsonNode Result { get; protected set; }
    public int Pointer { get; set; }
    public Stack<IParser> CallStack { get; set; }
    
    public string Key { get; set; }
    public JsonNode Parent { get; set; }
    public bool ShouldResume { get; set; }

    protected IParser Child { get; private set; }
}