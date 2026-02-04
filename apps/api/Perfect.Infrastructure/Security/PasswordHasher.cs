using Perfect.Application.Common;

namespace Perfect.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string input) => BCrypt.Net.BCrypt.HashPassword(input);
    public bool Verify(string input, string hash) => BCrypt.Net.BCrypt.Verify(input, hash);
}
