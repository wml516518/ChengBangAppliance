using SqlSugar;

namespace ApplianceRepair.Models;

/// <summary>订单/服务类型，如安装服务、维修服务等</summary>
[SugarTable("ServiceTypes")]
public class ServiceType
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 50)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
