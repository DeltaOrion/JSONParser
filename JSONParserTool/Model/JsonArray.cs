namespace JSONParserTool.Model;

public class JsonArray
{
    private readonly List<JsonNode> _arr;

    public JsonArray(List<JsonNode> arr)
    {
        _arr = arr;
    }

    public JsonNode this[int index]
    {
        get => _arr[index];
        set => _arr[index] = value;
    }

    public void Push(JsonNode value)
    {
        _arr.Add(value);
    }

    public void Unshift(JsonNode value)
    {
        _arr.Insert(0, value);
    }

    public void RemoveAt(int index)
    {
        _arr.RemoveAt(index);
    }

    public int Size()
    {
        return _arr.Count;
    }
}