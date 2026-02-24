using SqlSugar;

namespace ApplianceRepair.Models;

/// <summary>服务项目，隶属于某个 ServiceType，如"空调安装"属于"安装服务"</summary>
[SugarTable("ServiceItems")]
public class ServiceItem
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int ServiceTypeId { get; set; }

    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
