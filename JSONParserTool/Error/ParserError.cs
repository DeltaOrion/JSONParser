namespace JSONParserTool.Error;

public class ParserError
{
    public int LineNumber { get; set; }

    public ErrorReason Reason { get; set; }

    public ParserError(ErrorReason reason, int lineNumber)
    {
        LineNumber = lineNumber;
        Reason = reason;
    }
}

public enum ErrorReason
{
    UnrecognisedEscapeSequence,
    BadCompileConstant,
    UnexpectedEndOfSequence,
    ExpectedEndOfArray,
    UnexpectedToken,
    ExpectedEndOfObject,
    ExpectedValue,
    DuplicateKey,
    BadNumber,
    NumberOverflow,
    ExpectedStartOfObject
}