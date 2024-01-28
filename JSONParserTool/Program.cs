// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using JSONParserTool.Converter;
using JSONParserTool.Example;

using var stream = new FileStream("parse.json", FileMode.Open, FileAccess.Read);
using var reader = new StreamReader(stream);

var converter = new JsonConverter();
var obj = converter.Convert<ImageRef>(reader);
Console.WriteLine(JsonSerializer.Serialize(obj));