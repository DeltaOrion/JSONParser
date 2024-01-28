using System.Globalization;
using JSONParserTool.Error;

namespace JSONParserTool.Lexer;

public class JsonLexer
{
    private LexerState? _state;
    private List<JsonLexeme> _lexemes;
    private List<char> _grouping;
    private int _lineNumber;

    private bool _stringEscaped;
    private int _fourDigitHexEscapeIndex = -1;
    private char[] _fourDigitEscapeSequence;

    private ParserError _error;

    public void Lexer(TextReader reader)
    {
        _lexemes = new List<JsonLexeme>();
        int value = reader.Read();
        _state = null;
        _grouping = new List<char>();

        _lineNumber = 1;
        _stringEscaped = false;
        _fourDigitHexEscapeIndex = -1;
        _fourDigitEscapeSequence = new char[4];

        while (value != -1)
        {
            if (_error != null)
            {
                break;
            }

            var character = (char)value;
            if (_state == LexerState.StringLiteral)
            {
                HandleString(character);
            }
            else
            {
                HandleProgram(character);
            }

            value = reader.Read();
        }

        if (_state == LexerState.StringLiteral && _error == null)
        {
            _error = new ParserError(ErrorReason.UnexpectedEndOfSequence, _lineNumber);
        }

        AddLexeme();
    }

    private void HandleProgram(char character)
    {
        switch (character)
        {
            //case 1 - special structural token
            case JsonDictionary.BeginArray:
                AddSeparator(LexicalToken.BeginArray);
                break;
            case JsonDictionary.EndArray:
                AddSeparator(LexicalToken.EndArray);
                break;
            case JsonDictionary.BeginObject:
                AddSeparator(LexicalToken.BeginObject);
                break;
            case JsonDictionary.EndObject:
                AddSeparator(LexicalToken.EndObject);
                break;
            case JsonDictionary.ValueSeparator:
                AddSeparator(LexicalToken.ValueSeparator);
                break;
            case JsonDictionary.NameSeparator:
                AddSeparator(LexicalToken.NameSeparator);
                break;
            //case 2. white space
            case JsonDictionary.NewLine:
            case JsonDictionary.CarriageReturn:
            case JsonDictionary.HorizontalTab:
            case JsonDictionary.Space:
                //we want to group together all whitespace before continuing
                if (_state != LexerState.Whitespace)
                {
                    AddLexeme();
                }

                if (character == JsonDictionary.NewLine)
                {
                    _lineNumber++;
                }

                _grouping.Add(character);
                _state = LexerState.Whitespace;
                break;
            case JsonDictionary.QuotationMark:
                if (_state != LexerState.StringLiteral)
                {
                    AddLexeme();
                }

                _stringEscaped = false;
                _state = LexerState.StringLiteral;
                break;
            default:
                //another token
                if (_state != LexerState.Literal)
                {
                    AddLexeme();
                }

                _grouping.Add(character);
                _state = LexerState.Literal;
                break;
        }
    }

    private void HandleString(char character)
    {
        if (_stringEscaped)
        {
            HandleEscaped(character);
        }
        else
        {
            HandleUnescaped(character);
        }
    }

    private void HandleUnescaped(char character)
    {
        if (character == JsonDictionary.ReverseSolidus)
        {
            _stringEscaped = true;
        }
        else if (character == JsonDictionary.QuotationMark)
        {
            _lexemes.Add(JsonLexeme.Literal(new string(_grouping.ToArray(), 0, _grouping.Count),
                _lineNumber, LexicalToken.StringLiteral));
            _state = null;
        }
        else if ((character >= 0x20 && character <= 0x21) || (character >= 0x23 && character <= 0x5b) ||
                 character >= 0x5D)
        {
            _grouping.Add(character);
        }
        else
        {
            _error = new ParserError(ErrorReason.BadCompileConstant, _lineNumber);
        }
    }

    private void HandleEscaped(char character)
    {
        _stringEscaped = false;

        if (_fourDigitHexEscapeIndex != -1)
        {
            HandleFourDigitEscapeCharacter(character);
            return;
        }

        switch (character)
        {
            case JsonDictionary.QuotationMark:
                _grouping.Add(JsonDictionary.QuotationMark);
                break;
            case JsonDictionary.ReverseSolidus:
                _grouping.Add(JsonDictionary.ReverseSolidus);
                break;
            case JsonDictionary.Solidus:
                _grouping.Add(JsonDictionary.Solidus);
                break;
            case 'b':
                _grouping.Add(JsonDictionary.Backspace);
                break;
            case 'f':
                _grouping.Add(JsonDictionary.FormFeed);
                break;
            case 'n':
                _grouping.Add(JsonDictionary.NewLine);
                break;
            case 'r':
                _grouping.Add(JsonDictionary.CarriageReturn);
                break;
            case 't':
                _grouping.Add(JsonDictionary.HorizontalTab);
                break;
            case 'u':
                _stringEscaped = true;
                if (_fourDigitHexEscapeIndex == -1)
                {
                    _fourDigitHexEscapeIndex = 0;
                }
                else
                {
                    _error = new ParserError(ErrorReason.UnrecognisedEscapeSequence, _lineNumber);
                }

                break;
            default:
                _error = new ParserError(ErrorReason.UnrecognisedEscapeSequence, _lineNumber);
                break;
        }
    }

    private void HandleFourDigitEscapeCharacter(char character)
    {
        _stringEscaped = true;
        _fourDigitEscapeSequence[_fourDigitHexEscapeIndex] = character;
        _fourDigitHexEscapeIndex++;
        if (_fourDigitHexEscapeIndex != 4) return;

        _stringEscaped = false;
        
        if (!int.TryParse(new string(_fourDigitEscapeSequence, 0, _fourDigitEscapeSequence.Length),
                NumberStyles.HexNumber, null, out var intEscape))
        {
            _error = new ParserError(ErrorReason.UnrecognisedEscapeSequence, _lineNumber);
        }
        else
        {
            _grouping.Add((char)intEscape);
        }

        _fourDigitHexEscapeIndex = -1;
    }

    private void AddSeparator(LexicalToken token)
    {
        AddLexeme();
        _lexemes.Add(JsonLexeme.Seperator(token, _lineNumber));
        _state = LexerState.Structure;
    }

    private void AddLexeme()
    {
        switch (_state)
        {
            case LexerState.Whitespace:
            case LexerState.Structure:
            case null:
                break;
            case LexerState.StringLiteral:
            case LexerState.Literal:
                var literal = new string(_grouping.ToArray(), 0, _grouping.Count);
                LexicalToken token;
                if (_state == LexerState.StringLiteral)
                {
                    token = LexicalToken.StringLiteral;
                }
                else
                {
                    token = literal switch
                    {
                        "true" => LexicalToken.TrueLiteral,
                        "false" => LexicalToken.FalseLiteral,
                        "null" => LexicalToken.NullLiteral,
                        _ => LexicalToken.NumberLiteral
                    };
                }

                _lexemes.Add(JsonLexeme.Literal(literal, _lineNumber, token));
                break;
        }

        _grouping.Clear();
    }

    public ParserError? GetError()
    {
        return _error;
    }

    public List<JsonLexeme> GetAnalysis()
    {
        return _lexemes;
    }

    private enum LexerState
    {
        Whitespace,
        Literal,
        StringLiteral,
        Structure
    }
}