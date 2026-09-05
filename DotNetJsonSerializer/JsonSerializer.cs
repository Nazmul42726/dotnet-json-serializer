using System.Collections;
using System.Globalization;
using System.Text;

namespace DotnetJsonSerializer;

public static class JsonSerializer
{
    public static string Serialize(object? obj)
    {
        if (obj == null) return "null";

        return obj switch
        {
            string s    => $"\"{EscapeString(s)}\"",
            bool b      => b ? "true" : "false",
            int i       => i.ToString(),
            long l      => l.ToString(),
            float f     => f.ToString(CultureInfo.InvariantCulture),
            double d    => d.ToString(CultureInfo.InvariantCulture),
            decimal d   => d.ToString(CultureInfo.InvariantCulture),
            _           => obj is IEnumerable collection ?
                            SerializeCollection(collection) : 
                            SerializeObject(obj)
        };
    }

    private static string EscapeString(string s)
    {
        var builder = new StringBuilder();

        foreach (var ch in s)
        {
            switch (ch)
            {
                case '"':
                    builder.Append("\\\"");
                    break;

                case '\\':
                    builder.Append("\\\\");
                    break;

                case '\n':
                    builder.Append("\\n");
                    break;

                case '\r':
                    builder.Append("\\r");
                    break;

                case '\t':
                    builder.Append("\\t");
                    break;

                case '\b':
                    builder.Append("\\b");
                    break;

                case '\f':
                    builder.Append("\\f");
                    break;

                default:
                    if (ch < ' ')
                    {
                        builder.Append($"\\u{(int)ch:X4}");
                    }
                    else
                    {
                        builder.Append(ch);
                    }

                    break;
            }
        }
        return builder.ToString();
    }

    private static string SerializeObject(object obj)
    {
        var result = new StringBuilder();
        result.Append("{");
        
        var properties = obj.GetType().GetProperties();

        for(var i=0; i<properties.Length; i++)
        {   
            var property = properties[i];
            var propertyName = property.Name;
            var propertyValue = property.GetValue(obj);

            var jsonValue = $"{Serialize(propertyName)}: {Serialize(propertyValue)}";

            if(i > 0) result.Append(",");
            result.Append(jsonValue);
        }
        result.Append("}");

        return result.ToString();
    }

    private static string SerializeCollection(IEnumerable collection)
    {
        var result = new StringBuilder();
        result.Append("[");

        var first = true;

        foreach(var item in collection)
        {
            if(!first) result.Append(", ");
            result.Append(Serialize(item));
            first = false;
        }

        result.Append("]");
        return result.ToString();
    }
}