using System.Diagnostics;
using DotnetJsonSerializer;

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

const int iterations = 100000;

var stopwatch = Stopwatch.StartNew();

for (var i = 0; i < iterations; i++)
{
    JsonSerializer.Serialize(user);
}

stopwatch.Stop();

Console.WriteLine($"Iterations: {iterations}");
Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms");

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Address Address { get; set; } = new();
}

public class Address
{
    public string City { get; set; } = string.Empty;
    public int Zip { get; set; }
}
