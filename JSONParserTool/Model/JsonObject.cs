namespace JSONParserTool.Model;

public class JsonObject
{
    private readonly Dictionary<string, JsonNode> _children;

    public JsonObject()
    {
        _children = new Dictionary<string, JsonNode>();
    }

    public JsonNode this[string key]
    {
        get => _children[key];
        set => _children[key] = value;
    }

    public void Remove(string key)
    {
        _children.Remove(key);
    }

    public bool ContainsKey(string key)
    {
        return _children.ContainsKey(key);
    }

    public bool IsEmpty()
    {
        return _children.Any();
    }
}