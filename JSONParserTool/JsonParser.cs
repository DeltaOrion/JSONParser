using JSONParserTool.Error;
using JSONParserTool.Lexer;
using JSONParserTool.Model;
using JSONParserTool.Parser;

namespace JSONParserTool;

public class JsonParser
{

    public JsonNode Parse(TextReader reader)
    {
        //1. Perform lexical analysis
        var lexer = new JsonLexer();
        lexer.Lexer(reader);

        if (lexer.GetError() != null)
        {
            HandleError(lexer.GetError());
        }

        //2. Parse the result to an object tree

        IParser? initialParser = null;
        var analysis = lexer.GetAnalysis();
        if (analysis.Count == 0)
        {
            HandleError(new ParserError(ErrorReason.ExpectedStartOfObject, 0));
        }

        initialParser = new ValueParser();
        var callStack = new Stack<IParser>();

        initialParser.Parent = null;
        initialParser.Key = null;
        initialParser.CallStack = callStack;
        initialParser.Pointer = 0;

        callStack.Push(initialParser);

        IParser parser = null;
        while (callStack.Any())
        {
            parser = callStack.Pop();
            if (parser.ShouldResume)
            {
                parser.Resume(analysis);
            }
            else
            {
                parser.Parse(analysis);
            }

            if (parser.Error != null)
            {
                HandleError(parser.Error);
            }
        }


        if (parser.Pointer < lexer.GetAnalysis().Count)
        {
            HandleError(new ParserError(ErrorReason.UnexpectedToken, analysis[parser.Pointer].LineNumber));
        }

        return initialParser.Result;
    }

    private void HandleError(ParserError error)
    {
        //normally you would throw a prettier error depending on what actually occurred
        throw new Exception($"Invalid JSON String. Error {error.Reason}, occurred at line {error.LineNumber}");
    }
}