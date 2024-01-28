using JSONParserTool.Model;

namespace JSONParserTool.Converter;

public class JsonConverter
{
    public T? Convert<T>(TextReader reader)
    {
        JsonParser parser = new JsonParser();
        var node = parser.Parse(reader);
        Type type = typeof(T);
        return (T) Convert(node, type);
    }

    private object Convert(JsonNode node, Type type)
    {
        var stack = new Stack<ConversionFrame>();
        ConversionFrame initialFrame = new ConversionFrame()
        {
            Node = node,
            Type = type
        };

        stack.Push(initialFrame);

        while (stack.Any())
        {
            var frame = stack.Pop();
            type = frame.Type;
            node = frame.Node;

            //check if the type is a literal
            if (type == typeof(bool))
            {
                frame.Result = node.AsBoolean();
                continue;
            }

            if (type == typeof(string))
            {
                frame.Result = node.AsString();
                continue;
            }

            if (type == typeof(int))
            {
                frame.Result = node.AsInt();
                continue;
            }

            if (type == typeof(double))
            {
                frame.Result = node.AsDouble();
                continue;
            }

            if (type == typeof(string))
            {
                frame.Result = node.AsString();
                continue;
            }

            bool isEnumerable = type == typeof(List<>) ||
                                (type.IsGenericType &&
                                 type.GetGenericTypeDefinition() == typeof(IEnumerable<>));


            if (isEnumerable)
            {
                //create a new list type
                var list = frame.Result;
                var listGeneric = type.GetGenericArguments()[0];
                var listTypeExact = typeof(List<>);
                var listType = listTypeExact.MakeGenericType(new Type[] { listGeneric });
                var addMethod = listType.GetMethod("Add");

                if (frame.Result == null)
                {
                    frame.Result = Activator.CreateInstance(listType);
                    list = frame.Result;
                }

                for (; frame.Index < node.AsArray().Size(); frame.Index++)
                {
                    if (frame.Child != null)
                    {
                        addMethod.Invoke(list, new object?[] { frame.Child.Result });
                        frame.Child = null;
                    }
                    else
                    {
                        frame.Child = new ConversionFrame()
                        {
                            Type = listGeneric,
                            Node = node.AsArray()[frame.Index]
                        };
                        
                        stack.Push(frame);
                        stack.Push(frame.Child);
                        break;
                    }
                }

                continue;
            }

            //instantiate the type
            var obj = frame.Result;
            if (frame.Result == null)
            {
                frame.Result = Activator.CreateInstance(type);
                obj = frame.Result;
            }

            for (; frame.Index < type.GetProperties().Length; frame.Index++)
            {
                var property = type.GetProperties()[frame.Index];
                var pName = property.Name;
                var pType = property.PropertyType;

                if (frame.Child != null)
                {
                    property.SetValue(obj, frame.Child.Result);
                    frame.Child = null;
                }
                else
                {
                    frame.Child = new ConversionFrame()
                    {
                        Type = pType,
                        Node = node.AsObject()[pName],
                    };
                    
                    stack.Push(frame);
                    stack.Push(frame.Child);
                    break;
                }
            }
        }

        return initialFrame.Result;
    }

    private class ConversionFrame
    {
        public JsonNode Node { get; set; }

        public Type Type { get; set; }

        public object? Result { get; set; }

        public int Index { get; set; } = 0;

        public ConversionFrame? Child { get; set; } = null;
    }
}