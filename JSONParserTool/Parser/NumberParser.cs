using JSONParserTool.Error;
using JSONParserTool.Lexer;

namespace JSONParserTool.Parser;

public class NumberParser
{
    public ParserError? Error { get; private set; }

    public double? Result { get; private set; }

    public void Parse(JsonLexeme token)
    {
        try
        {
            var result = ParseNumber(token.Value);
            if (double.IsPositiveInfinity(result) || double.IsPositiveInfinity(result) || double.IsNaN(result))
            {
                Error = new ParserError(ErrorReason.NumberOverflow, token.LineNumber);
                return;
            }

            Result = result;
        }
        catch (Exception ex)
        {
            Error = new ParserError(ErrorReason.BadNumber, token.LineNumber);
        }
    }

    private double ParseNumber(string? stoi)
    {
        bool negative = false;

        if (stoi == null)
        {
            throw new Exception();
        }

        var stoiArr = stoi.ToCharArray();
        int i = 0;
        if (stoiArr[0] == JsonDictionary.Minus)
        {
            negative = true;
            i++;
        }

        var intPart = new List<char>();
        var fractionPart = new List<char>();
        var exponentPart = new List<char>();

        var state = State.IntPart;
        var isFraction = false;
        var isExponent = false;
        var isLeadingZero = false;

        var negativeExponent = false;
        var seenExponentSign = false;

        for (;i < stoiArr.Length;i++)
        {
            var token = stoiArr[i];
            if (token == JsonDictionary.DecimalPoint)
            {
                if ((state == State.IntPart && !intPart.Any()) ||
                    (state != State.IntPart && state != State.LeadingDecimalZeroPart))
                {
                    throw new Exception();
                }

                state = State.FracPart;
                isFraction = true;
                continue;
            }

            if (token == JsonDictionary.e || token == JsonDictionary.capitalE)
            {
                if (state == State.ExponentPart ||
                    (state == State.FracPart && !fractionPart.Any())
                    || (state == State.IntPart && !intPart.Any()))
                {
                    throw new Exception();
                }

                state = State.ExponentPart;
                isExponent = true;
                continue;
            }

            if (token == JsonDictionary.Plus || token == JsonDictionary.Minus)
            {
                if (state != State.ExponentPart || seenExponentSign)
                {
                    throw new Exception();
                }

                if (token == JsonDictionary.Minus)
                {
                    negativeExponent = true;
                }

                seenExponentSign = true;
                continue;
            }

            if (token < JsonDictionary.Zero || token > JsonDictionary.Nine)
            {
                throw new Exception();
            }

            switch (state)
            {
                case State.LeadingDecimalZeroPart:
                    throw new Exception();
                case State.IntPart:
                {
                    if (token == JsonDictionary.Zero && !intPart.Any())
                    {
                        state = State.LeadingDecimalZeroPart;
                        isLeadingZero = true;
                        continue;
                    }

                    intPart.Add(token);
                    break;
                }
                case State.FracPart:
                    fractionPart.Add(token);
                    break;
                case State.ExponentPart:
                    exponentPart.Add(token);
                    break;
            }
        }

        if (isExponent && !exponentPart.Any() || isFraction && !fractionPart.Any() ||
            (!intPart.Any() && !isLeadingZero))
        {
            throw new Exception();
        }

        double number = 0;
        var currExponent = 1;
        for (int j = intPart.Count - 1; j >= 0; j--)
        {
            number += currExponent * (intPart[j] - JsonDictionary.Zero);
            currExponent *= 10;
        }

        var currExponentFrac = 0.1;
        for (int j = fractionPart.Count - 1; j >= 0; j--)
        {
            number += currExponentFrac * (fractionPart[j] - JsonDictionary.Zero);
            currExponentFrac /= 10;
        }

        var exponentPartValue = 0;
        currExponent = 1;

        for (int j = exponentPart.Count - 1; j >= 0; j--)
        {
            exponentPartValue += currExponent * (exponentPart[j] - JsonDictionary.Zero);
            currExponent *= 10;
        }
        
        if (isExponent)
        {
            if (negativeExponent)
            {
                exponentPartValue *= -1;
            }
            
            number *= Math.Pow(10, exponentPartValue);
        }

        if (negative)
        {
            number *= -1;
        }

        return number;
    }

    private enum State
    {
        FracPart,
        IntPart,
        LeadingDecimalZeroPart,
        ExponentPart
    }
}