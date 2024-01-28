using JSONParserTool.Error;
using JSONParserTool.Lexer;
using JSONParserTool.Model;

namespace JSONParserTool.Parser;

public interface IParser
{
    public void Parse(List<JsonLexeme> tokens);

    public void Resume(List<JsonLexeme> tokens);

    public ParserError? Error { get; }

    public JsonNode Result { get; }
    
    public int Pointer { get; set; }
    
    public Stack<IParser> CallStack { get; set; }

    public string Key { get; set; }

    public JsonNode? Parent { get; set; }
    
    public bool ShouldResume { get; }
}