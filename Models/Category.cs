using SqlSugar;

namespace ApplianceRepair.Models;

[SugarTable("Categories")]
public class Category
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>排序，数字越小越靠前</summary>
    public int SortOrder { get; set; }
}
