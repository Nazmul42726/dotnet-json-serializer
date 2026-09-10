using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace DotnetJsonSerializer.Tests;

public class JsonSerializerTests
{
    [Fact]
    public void SerializeNull()
    {
        var result = JsonSerializer.Serialize(null);

        Assert.Equal("null", result);
    }

    [Fact]
    public void SerializeString()
    {
        var result = JsonSerializer.Serialize("hello");

        Assert.Equal("\"hello\"", result);
    }

    [Fact]
    public void SerializeInt()
    {
        var result = JsonSerializer.Serialize(30);

        Assert.Equal("30", result);
    }

    [Fact]
    public void SerializeBoolean()
    {
        var result = JsonSerializer.Serialize(true);

        Assert.Equal("true", result);
    }

    [Fact]
    public void SerializeLong()
    {
        var result = JsonSerializer.Serialize(123456789L);

        Assert.Equal("123456789", result);
    }

    [Fact]
    public void SerializeFloat()
    {
        var result = JsonSerializer.Serialize(3.14f);

        Assert.Equal("3.14", result);
    }

    [Fact]
    public void SerializeDouble()
    {
        var result = JsonSerializer.Serialize(3.14159);

        Assert.Equal("3.14159", result);
    }

    [Fact]
    public void SerializeDecimal()
    {
        var result = JsonSerializer.Serialize(99.99m);

        Assert.Equal("99.99", result);
    }

    [Fact]
    public void SerializeStringWithQuotes()
    {
        var result = JsonSerializer.Serialize("hello \"world\"");

        Assert.Equal("\"hello \\\"world\\\"\"", result);
    }

    [Fact]
    public void SerializeStringWithControlCharacter()
    {
        var result = JsonSerializer.Serialize("hello\0world");

        Assert.Equal("\"hello\\u0000world\"", result);
    }

    [Fact]
    public void SerializeObject()
    {
        var user = new User
        {
            Id = 1,
            Name = "Nazmul",
            IsActive = true,
            Address = new Address
            {
                City = "Chittagong",
                Zip = 4000
            }
        };

        var result = JsonSerializer.Serialize(user);

        Assert.Equal("{\"Id\": 1,\"Name\": \"Nazmul\",\"IsActive\": true,\"Address\": {\"City\": \"Chittagong\",\"Zip\": 4000}}", result);
    }

    [Fact]
    public void SerializeArray()
    {
        var numbers = new[] { 1, 2, 3 };
        var result = JsonSerializer.Serialize(numbers);

        Assert.Equal("[1, 2, 3]", result);
    }

    [Fact]
    public void SerializeList()
    {
        var numbers = new List<int> { 1, 2, 3 };
        var result = JsonSerializer.Serialize(numbers);

        Assert.Equal("[1, 2, 3]", result);
    }

    [Fact]
    public void SerializeObjectCollection()
    {
        var users = new[]
        {
            new User
            {
                Id = 1,
                Name = "Nazmul",
                IsActive = false,
                Address = new Address
                {
                    City = "Chittagong",
                    Zip  = 4000
                }
            }
        };

        var result = JsonSerializer.Serialize(users);

        Assert.Equal("[{\"Id\": 1,\"Name\": \"Nazmul\",\"IsActive\": false,\"Address\": {\"City\": \"Chittagong\",\"Zip\": 4000}}]", result);
    }

    [Fact]
    public void SerializeNestedCollection()
    {
        var numbers = new List<List<int>>
        {
            new() {1, 2},
            new () {3, 4}
        };

        var result = JsonSerializer.Serialize(numbers);

        Assert.Equal("[[1, 2], [3, 4]]", result);
    }

    [Fact]
    public void SerializeDictionary()
    {
        var user = new Dictionary<string, object>
        {
            ["name"] = "Shafayet Bro",
            ["age"] = 27 //i guess
        };

        var result = JsonSerializer.Serialize(user);
        var expected = "{\"name\": \"Shafayet Bro\",\"age\": 27}";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeNestedDictionary()
    {
        var data = new Dictionary<string, object>
        {
            ["user1"] = new Dictionary<string, object>
            {
                ["name"] = "Nazmul",
                ["age"] = 23
            },
            ["user2"] = new Dictionary<string, object>
            {
                ["name"] = "Don't know",
                ["age"] = 12345
            }
        };

        var result = JsonSerializer.Serialize(data);
        var expected = "{\"user1\": {\"name\": \"Nazmul\",\"age\": 23},\"user2\": {\"name\": \"Don't know\",\"age\": 12345}}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeDictionaryWithCollection()
    {
        var data = new Dictionary<string, object>
        {
            ["name"] = "Nazmul",
            ["score"] = new List<int> { 80, 85, 90 }
        };

        var result = JsonSerializer.Serialize(data);
        var expected = "{\"name\": \"Nazmul\",\"score\": [80, 85, 90]}";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeDictionaryWithObject()
    {
        var data = new Dictionary<string, object>
        {
            ["user1"] = new User
            {
                Id = 1,
                Name = "Nazmul",
                IsActive = true,
                Address = new Address
                {
                    City = "Chittagong",
                    Zip = 4000
                }
            }
        };

        var result = JsonSerializer.Serialize(data);
        var expected =
            "{\"user1\": {\"Id\": 1,\"Name\": \"Nazmul\",\"IsActive\": true,\"Address\": {\"City\": \"Chittagong\",\"Zip\": 4000}}}";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeDictionaryWithNullValue()
    {
        var data = new Dictionary<string, object?>
        {
            ["name"] = "Nazmul",
            ["address"] = null
        };

        var result = JsonSerializer.Serialize(data);
        var expected =
            "{\"name\": \"Nazmul\",\"address\": null}";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeDateTime()
    {
        var date = new DateTime(2026, 9, 7, 11, 30, 0, DateTimeKind.Utc);

        var result = JsonSerializer.Serialize(date);
        var expected = "\"2026-09-07T11:30:00.0000000Z\"";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeGuid()
    {
        var id = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        var result = JsonSerializer.Serialize(id);
        var expected = "\"12345678-1234-1234-1234-123456789abc\"";

        Assert.Equal(expected, result);
    }

    enum Status
    {
        Active = 1,
        InActive = 2
    }

    [Fact]
    public void SerializeEnum()
    {
        var status = Status.InActive;

        var result = JsonSerializer.Serialize(status);
        var expected = "2";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SerializeCircularReference()
    {
        var node = new Node();
        node.Next = node;

        var exception = Assert.Throws<InvalidOperationException>(
            () => JsonSerializer.Serialize(node));

        Assert.Equal("Circular reference!", exception.Message);
    }

    [Fact]
    public void SerializeRepeatedReference()
    {
        var address = new Address
        {
            City = "Chittagong",
            Zip = 4000
        };

        var users = new[]
        {
        new User { Id = 1, Name = "Nazmul", IsActive = true, Address = address },
        new User { Id = 2, Name = "Test", IsActive = false, Address = address }
    };

        var result = JsonSerializer.Serialize(users);

        Assert.Contains("\"City\": \"Chittagong\"", result);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; } = string.Empty;
    public int Zip { get; set; }
}

public class Node()
{
    public Node? Next { get; set; }
}