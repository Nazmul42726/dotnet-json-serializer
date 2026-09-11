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
| Arrays                       | Yes           | —               |
| `List<T>`                    | Yes           | Yes             |
| `IEnumerable<T>`             | Yes           | Limited         |
| `Dictionary<string, object>` | Yes           | Yes             |

## Limitations

* JSON object keys are represented as strings.
* Collection deserialization currently focuses on `List<T>`.
* Object properties must be writable for reflection-based deserialization.
* Constructor-based and advanced custom serialization behavior are not supported.
* Reflection metadata caching has not been implemented.

## Performance

Reflection is used during object serialization and deserialization, which introduces overhead compared with manually written serialization.

The main optimization opportunity is caching reflected property metadata for frequently serialized types.

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
