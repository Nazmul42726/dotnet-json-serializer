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
                Zip  = 4000
            }
        };

        var result = JsonSerializer.Serialize(user);

        Assert.Equal("{\"Id\": 1,\"Name\": \"Nazmul\",\"IsActive\": true,\"Address\": {\"City\": \"Chittagong\",\"Zip\": 4000}}", result);
    }

    [Fact]
    public void SerializeArray()
    {
        var numbers = new[] {1, 2, 3};
        var result = JsonSerializer.Serialize(numbers);

        Assert.Equal("[1, 2, 3]", result);
    }

    [Fact]
    public void SerializeList()
    {
        var numbers = new List<int> {1, 2, 3};
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
            ["name"]  = "Shafayet Bro",
            ["age"] = 27 //i guess
        };

        var result = JsonSerializer.Serialize(user);
        var expected = "{\"name\": \"Shafayet Bro\",\"age\": 27}";
        
        Assert.Equal(expected, result);
    }
}

public class User
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public bool IsActive {get; set;}
    public Address Address {get; set;}
}

public class Address
{
    public string City {get; set;} = string.Empty;
    public int Zip {get; set;}
}