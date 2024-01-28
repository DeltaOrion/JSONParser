# C# JSON Parser

This is a simple JSON Parser implementation written in c# in accordance with [RFC8259](https://datatracker.ietf.org/doc/html/rfc8259) specification. This is a personal project and should not be used in production applications. Instead use the inbuilt [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-8.0) library.

## Usage

You can use JSON Converter class to parse the file to a strongly typed object.

```c#
using var stream = new FileStream("parse.json", FileMode.Open, FileAccess.Read);
using var reader = new StreamReader(stream);

var converter = new JsonConverter();
var obj = converter.Convert<ImageRef>(reader);
Console.WriteLine(JsonSerializer.Serialize(obj));
```
The above example will convert this JSON object 

```json
{
  "Image": {
    "Width": 800,
    "Height": 600,
    "Title": "View from 15th Floor",
    "Thumbnail": {
      "Url": "http://www.example.com/image/481989943",
      "Height": 125,
      "Width": 100
    },
    "Animated": false,
    "IDs": [116, 943, 234, 38793]
  }
}
```

to the following strongly typed object

```c#
public class Image
{
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public string Title { get; set; }
    
    public bool Animated { get; set; }
    
    public Thumbnail Thumbnail { get; set; }
    
    public IEnumerable<int> IDs { get; set; }
}
```

## How does it work?

The JSON parser works similarly to most parsers through the following steps

- **Lexical Analysis**: The JSON file is broken down into a stream of semantically meaningful tokens. This first uses a scanner to look over all the characters and then an evaluator to assign semantic meaning to the tokens. See `JsonLexer.cs`
- **Parsing**: The lexical tokens are turned into an abstract syntax tree. This process uses a stack to recursively parse the children of a given node. See `JsonParser.cs`. The structure of the parsed result is a `JsonNode.cs`.
- **Converting**: This stage converts the abtract syntax tree to a strongly typed object using reflection. This process essentially uses DFS to recurse over the objects typs and find equivalents in the JSON node. 



