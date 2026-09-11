using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace DotnetJsonSerializer;

public static class JsonSerializer
{
    private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new();
    public static string Serialize(object? obj)
    {
        return Serialize(obj, new HashSet<object>(ReferenceEqualityComparer.Instance));
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
        if (targetType == typeof(string))
        {
            if (value is string stringValue) return stringValue;
            throw new FormatException("Cannot deserialize value to string.");
        }
        if (targetType == typeof(bool))
        {
            if (value is bool boolValue) return boolValue;
            throw new FormatException("Cannot deserialize value to bool.");
        }
        if (targetType == typeof(int))
        {
            if (value is int intValue) return intValue;
            throw new FormatException("Cannot deserialize value to int.");
        }
        if (targetType == typeof(long))
        {
            if (value is int intValue) return (long)intValue;
            if (value is long longValue) return longValue;
            throw new FormatException("Cannot deserialize value to long.");
        }
        if (targetType == typeof(float))
        {
            if (value is int intValue) return (float)intValue;
            if (value is long longValue) return (float)longValue;
            if (value is double doubleValue) return (float)doubleValue;
            throw new FormatException("Cannot deserialize value to float.");
        }
        if (targetType == typeof(double))
        {
            if (value is int intValue) return (double)intValue;
            if (value is long longValue) return (double)longValue;
            if (value is double doubleValue) return doubleValue;
            throw new FormatException("Cannot deserialize value to double.");
        }
        if (targetType == typeof(decimal))
        {
            if (value is int intValue) return (decimal)intValue;
            if (value is long longValue) return (decimal)longValue;
            if (value is double doubleValue) return (decimal)doubleValue;
            throw new FormatException("Cannot deserialize value to decimal.");
        }

        if (targetType == typeof(Guid)) return Guid.Parse((string)value!);
        if (targetType.IsEnum) return Enum.ToObject(targetType, value!);
        if (targetType == typeof(DateTime))
            return DateTime.Parse((string)value!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var items = (List<object?>)value;
            var array = Array.CreateInstance(elementType, items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                array.SetValue(DeserializeValue(items[i], elementType), i);
            }
            return array;
        }
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = targetType.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(targetType)!;

            foreach (var item in (List<object?>)value)
            {
                list.Add(DeserializeValue(item, elementType));
            }
            return list;
        }
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var elementType = targetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in (List<object?>)value)
            {
                list.Add(DeserializeValue(item, elementType));
            }
            return list;
        }
        if (targetType == typeof(Dictionary<string, object>)) return value;
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

        var type = obj.GetType();

        if (!PropertyCache.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties();
            PropertyCache[type] = properties;
        }

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            if (!property.CanRead || property.GetIndexParameters().Length > 0) continue;

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