namespace Events.Helpers;

using System.Security.Cryptography;
using System.Text;

public static class HashHelper
{
    public static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var stringBuilder = new StringBuilder();

        foreach (var b in bytes)
        {
            stringBuilder.Append(b.ToString("x2")); // Convert to hexadecimal
        }

        return stringBuilder.ToString().Substring(0, 8); // Return first 8 characters
    }
}