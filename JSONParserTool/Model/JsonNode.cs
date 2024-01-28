using System.Text;

namespace JSONParserTool.Model;

public class JsonNode
{
    public string? Key { get; set; }

    public JsonValue? Value { get; set; }

    public JsonNode? Parent { get; set; }

    public JsonNode(string? key, JsonNode? parent, JsonValue? value)
    {
        Parent = parent;
        Value = value;
        Key = key;
    }

    public bool AsBoolean()
    {
        return Value.AsBoolean();
    }

    public bool IsBoolean()
    {
        return Value.IsBoolean();
    }

    public int AsInt()
    {
        return Value.AsInt();
    }

    public bool IsInt()
    {
        return Value.IsInt();
    }

    public double AsDouble()
    {
        return Value.AsDouble();
    }

    public bool IsDouble()
    {
        return Value.IsDouble();
    }

    public bool IsNull()
    {
        return Value.IsNull();
    }

    public JsonObject AsObject()
    {
        return Value.AsObject();
    }

    public bool IsObject()
    {
        return Value.IsObject();
    }

    public JsonArray AsArray()
    {
        return Value.AsArray();
    }
    

    public string AsString()
    {
        return Value.AsString();
    }

    public bool IsString()
    {
        return Value.IsString();
    }

    public string GetPath()
    {
        var builder = new StringBuilder();
        var root = this;
        while (root != null)
        {
            builder.Insert(0, root.Key);
            if (root.Parent != null)
            {
                builder.Insert(0, ".");
            }

            root = root.Parent;
        }

        return builder.ToString();
    }

    public bool IsArray()
    {
        return Value.IsArray();
    }
}