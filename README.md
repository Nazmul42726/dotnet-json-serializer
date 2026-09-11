# dotnet-json-serializer

A JSON serializer and deserializer built from scratch in C# using reflection and custom JSON parsing.

## Features

* Serialize primitive values:

  * `string`
  * `int`
  * `long`
  * `float`
  * `double`
  * `decimal`
  * `bool`
  * `null`
* Serialize objects using reflection
* Serialize nested objects
* Serialize arrays and collections
* Serialize `Dictionary<string, object>`
* Detect circular references
* Deserialize objects using reflection
* Deserialize `List<T>` and nested collections
* Deserialize `Dictionary<string, object>`
* Support `DateTime`, `Guid`, enums, and nullable value types
* Parse JSON objects, arrays, strings, numbers, booleans, and `null`
* Detect malformed JSON and type mismatches

## Usage

### Serialization

```csharp
var user = new User
{
    Id = 1,
    Name = "Nazmul",
    IsActive = true
};

string json = JsonSerializer.Serialize(user);
```

### Deserialization

```csharp
var user = JsonSerializer.Deserialize<User>(json);
```

## Design

The serializer uses reflection to inspect object properties and recursively converts values into JSON.

The parser reads JSON directly without using `System.Text.Json`, `Newtonsoft.Json`, or another JSON library. Parsed JSON objects are represented internally as dictionaries and arrays as lists.

Deserialization recursively converts the parsed representation into the requested C# type.

## Error Handling

The implementation reports errors for:

* Invalid JSON syntax
* Unexpected tokens
* Invalid JSON numbers
* Malformed arrays and objects
* Deserialization type mismatches
* Circular object references during serialization

## Circular References

Circular references are detected using reference tracking during serialization.

For example, if object `A` refers to `B` and `B` refers back to `A`, serialization throws an `InvalidOperationException` instead of recursing indefinitely.

Repeated references that are not circular are allowed.

## Supported Types

| Type                         | Serialization | Deserialization |
| ---------------------------- | ------------- | --------------- |
| `string`                     | Yes           | Yes             |
| `bool`                       | Yes           | Yes             |
| `int`                        | Yes           | Yes             |
| `long`                       | Yes           | Yes             |
| `float`                      | Yes           | Yes             |
| `double`                     | Yes           | Yes             |
| `decimal`                    | Yes           | Yes             |
| `DateTime`                   | Yes           | Yes             |
| `Guid`                       | Yes           | Yes             |
| `enum`                       | Yes           | Yes             |
| Objects                      | Yes           | Yes             |
| Arrays                       | Yes           | Yes             |
| `List<T>`                    | Yes           | Yes             |
| `IEnumerable<T>`             | Yes           | Yes (deserializes to List<T>)         |
| `Dictionary<string, object>` | Yes           | Yes             |

### Special Types

The serializer uses the following representations for special types:

* **DateTime**: serialized as an ISO 8601 string using round-trip formatting, preserving the `DateTimeKind` when possible.
* **Guid**: serialized as the standard hyphenated GUID string.
* **Enum**: serialized using its underlying numeric value.
* **Nullable value types**: serialized as their underlying value when non-null, or `null` when null.

## Limitations

* JSON object keys are represented as strings.
* Object properties must be writable for reflection-based deserialization.
* Constructor-based and advanced custom serialization behavior are not supported.

## Performance

Reflection introduces overhead during object serialization, particularly when discovering properties repeatedly.

The main optimization target was:

```csharp
obj.GetType().GetProperties()
```

The serializer now caches the reflected `PropertyInfo[]` for each object type. This avoids repeating property discovery when the same type is serialized many times.

A simple benchmark serialized the same `User` object 100,000 times.

| Version        | Average time (ms) |
| -------------- | ----------------: |
| Before caching |               304 |
| After caching  |               252 |

The results show an approximately **17% improvement** in this benchmark.

The benchmark uses `Stopwatch` and is affected by normal system and runtime variation, so the measurements are approximate. The averages are based on four runs for each version.

The benchmark is intentionally simple and focuses on demonstrating the effect of caching reflection metadata rather than providing a production-grade performance measurement.


## Testing

The project includes xUnit tests covering:

* Primitive values
* Strings and escaping
* Objects and nested objects
* Arrays and collections
* Dictionaries
* Special types
* JSON parsing
* Deserialization
* Invalid JSON
* Type mismatches
* Circular references

Run all tests with:

```bash
dotnet test
```

## License

See [LICENSE](LICENSE).
