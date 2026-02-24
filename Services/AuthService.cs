using System.Security.Cryptography;
using System.Text;
using ApplianceRepair.Models;

namespace ApplianceRepair.Services;

public class AuthService
{
    private readonly SqlSugarService _db;

    public AuthService(SqlSugarService db)
    {
        _db = db;
    }

    public static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    public User? GetByName(string userName)
    {
        return _db.Db.Queryable<User>().First(u => u.UserName == userName);
    }

    public User? GetById(int id)
    {
        return _db.Db.Queryable<User>().First(u => u.Id == id);
    }

    public (bool ok, string msg) Register(string userName, string password, string? realName, string? phone, bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Length < 2)
            return (false, "用户名至少2个字符");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "密码至少6个字符");

        if (GetByName(userName) != null)
            return (false, "用户名已存在");

        var user = new User
        {
            UserName = userName.Trim(),
            PasswordHash = HashPassword(password),
            RealName = realName?.Trim(),
            Phone = phone?.Trim(),
            IsAdmin = isAdmin
        };
        _db.Db.Insertable(user).ExecuteCommand();
        return (true, "注册成功");
    }

    public User? Login(string userName, string password)
    {
        var user = GetByName(userName);
        if (user == null) return null;
        if (!VerifyPassword(password, user.PasswordHash)) return null;
        return user;
    }
}
