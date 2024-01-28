namespace JSONParserTool.Example;

public class Image
{
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public string Title { get; set; }
    
    public bool Animated { get; set; }
    
    public Thumbnail Thumbnail { get; set; }
    
    public IEnumerable<int> IDs { get; set; }
}