using SqlSugar;

namespace ApplianceRepair.Models;

[SugarTable("Users")]
public class User
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 50)]
    public string UserName { get; set; } = string.Empty;

    [SugarColumn(Length = 100)]
    public string PasswordHash { get; set; } = string.Empty;

    [SugarColumn(Length = 50)]
    public string? RealName { get; set; }

    [SugarColumn(Length = 20)]
    public string? Phone { get; set; }

    /// <summary>true=管理员，false=普通用户</summary>
    public bool IsAdmin { get; set; }

    /// <summary>true=师傅（技师），可接单</summary>
    public bool IsTechnician { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
}
