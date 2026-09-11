using System.Collections;
using System.Globalization;
using System.Text;

namespace DotnetJsonSerializer;

public static class JsonSerializer
{
    public static string Serialize(object? obj)
    {
        return Serialize(obj, []);
    }

    public static T? Deserialize<T>(string json)
    {
        var parser = new JsonParser(json);
        var result = parser.Parse();

        return (T?)DeserializeValue(result, typeof(T));
    }

    private static object? DeserializeValue(object? value, Type targetType)
    {
        if (Nullable.GetUnderlyingType(targetType) is Type underlyingType)
            return DeserializeValue(value, underlyingType);

        if (value is null) return null;
        if (targetType == typeof(string)) return (string)value;
        if (targetType == typeof(bool)) return (bool)value;
        if (targetType == typeof(int)) return Convert.ToInt32(value);
        if (targetType == typeof(long)) return Convert.ToInt64(value);
        if (targetType == typeof(float)) return Convert.ToSingle(value);
        if (targetType == typeof(double)) return Convert.ToDouble(value);
        if (targetType == typeof(decimal)) return Convert.ToDecimal(value);
        if (targetType == typeof(Guid)) return Guid.Parse((string)value!);
        if (targetType.IsEnum) return Enum.ToObject(targetType, value!);
        if (targetType == typeof(DateTime))
            return DateTime.Parse((string)value!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        if (value is Dictionary<string, object?> dictionary) return DeserializeObject(dictionary, targetType);

        return value;
    }

    private static object DeserializeObject(
        Dictionary<string, object?> dictionary,
        Type targetType)
    {
        var instance = Activator.CreateInstance(targetType)!;

        foreach (var entry in dictionary)
        {
            var property = targetType.GetProperty(entry.Key);

            if (property is null || !property.CanWrite) continue;

            var value = DeserializeValue(entry.Value, property.PropertyType);
            property.SetValue(instance, value);
        }
        return instance;
    }

    private static string Serialize(
        object? obj,
        HashSet<object> references)
    {
        if (obj == null) return "null";
        if (!obj.GetType().IsValueType && !references.Add(obj))
        {
            throw new InvalidOperationException("Circular reference!");
        }
        try
        {
            if (obj is string s) return $"\"{EscapeString(s)}\"";
            if (obj is bool b) return b ? "true" : "false";
            if (obj is int i) return i.ToString();
            if (obj is long l) return l.ToString();
            if (obj is float f) return f.ToString(CultureInfo.InvariantCulture);
            if (obj is double d) return d.ToString(CultureInfo.InvariantCulture);
            if (obj is decimal dc) return dc.ToString(CultureInfo.InvariantCulture);
            if (obj is DateTime dt) return $"\"{dt.ToString("O", CultureInfo.InvariantCulture)}\"";
            if (obj is Guid guid) return $"\"{guid}\"";
            if (obj is Enum e) return Convert.ToInt64(e).ToString();
            return SerializeComplexType(obj, references);
        }
        finally
        {
            references.Remove(obj);
        }
    }

    private static string SerializeComplexType(
        object obj,
        HashSet<object> references)
    {
        if (obj is IDictionary dictionary) return SerializeDictionary(dictionary, references);
        if (obj is IEnumerable collection) return SerializeCollection(collection, references);
        return SerializeObject(obj, references);
    }

    private static readonly Dictionary<char, string> EscapeSequences = new()
    {
        ['"'] = "\\\"",
        ['\\'] = "\\\\",
        ['\n'] = "\\n",
        ['\r'] = "\\r",
        ['\t'] = "\\t",
        ['\b'] = "\\b",
        ['\f'] = "\\f"
    };

    private static string EscapeString(string s)
    {
        var builder = new StringBuilder();

        foreach (var ch in s)
        {
            if (EscapeSequences.TryGetValue(ch, out var escaped)) builder.Append(escaped);
            else if (ch < ' ') builder.Append($"\\u{(int)ch:X4}");
            else builder.Append(ch);
        }
        return builder.ToString();
    }

    private static string SerializeObject(
        object obj,
        HashSet<object> references)
    {
        var result = new StringBuilder();
        result.Append("{");

        var properties = obj.GetType().GetProperties();

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var propertyName = property.Name;
            var propertyValue = property.GetValue(obj);

            var jsonValue = $"{Serialize(propertyName, references)}: {Serialize(propertyValue, references)}";

            if (i > 0) result.Append(",");
            result.Append(jsonValue);
        }
        result.Append("}");

        return result.ToString();
    }

    private static string SerializeCollection(
        IEnumerable collection,
        HashSet<object> references)
    {
        var result = new StringBuilder();
        result.Append("[");

        var first = true;

        foreach (var item in collection)
        {
            if (!first) result.Append(", ");
            result.Append(Serialize(item, references));
            first = false;
        }

        result.Append("]");
        return result.ToString();
    }

    private static string SerializeDictionary(
        IDictionary dictionary,
        HashSet<object> references)
    {
        var result = new StringBuilder();
        result.Append("{");

        var first = true;

        foreach (DictionaryEntry entry in dictionary)
        {
            if (!first) result.Append(",");

            result.Append(Serialize(entry.Key, references));
            result.Append(": ");
            result.Append(Serialize(entry.Value, references));

            first = false;
        }

        result.Append("}");

        return result.ToString();
    }

}