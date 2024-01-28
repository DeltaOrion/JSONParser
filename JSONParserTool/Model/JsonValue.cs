namespace JSONParserTool.Model;

public class JsonValue
{
    public JsonValueType Type { get; set; }

    public object? Value { get; set; }

    public JsonValue(JsonValueType type, object? value = null)
    {
        this.Type = type;
        this.Value = value;
        if (value is JsonValue)
        {
            throw new Exception();
        }
    }

    public bool AsBoolean()
    {
        if (Type == JsonValueType.True)
        {
            return true;
        }

        if (Type == JsonValueType.False)
        {
            return false;
        }

        throw new Exception("Value is not a boolean");
    }

    public bool IsBoolean()
    {
        return Type == JsonValueType.False || Type == JsonValueType.True;
    }

    public int AsInt()
    {
        if (Type == JsonValueType.Number)
        {
            return Convert.ToInt32(Value);
        }

        throw new Exception("Value is not an integer");
    }

    public bool IsInt()
    {
        return IsDouble();
    }

    public double AsDouble()
    {
        if (Type == JsonValueType.Number)
        {
            return (double)Value;
        }

        throw new Exception("Value is not a double");
    }

    public bool IsDouble()
    {
        return Type == JsonValueType.Number;
    }

    public bool IsNull()
    {
        return Type is JsonValueType.Null or JsonValueType.Undefined;
    }

    public string AsString()
    {
        if (Type == JsonValueType.String)
        {
            return (string)Value;
        }

        throw new Exception();
    }

    public bool IsString()
    {
        return Type == JsonValueType.String;
    }

    public JsonObject AsObject()
    {
        if (Type == JsonValueType.Object)
        {
            return (JsonObject)Value;
        }

        throw new Exception();
    }

    public bool IsObject()
    {
        return Type == JsonValueType.Object;
    }

    public JsonArray AsArray()
    {
        if (Type == JsonValueType.Array)
        {
            return (JsonArray)Value;
        }

        throw new Exception();
    }

    public bool IsArray()
    {
        return Type == JsonValueType.Array;
    }
}