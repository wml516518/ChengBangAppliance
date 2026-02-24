using SqlSugar;

namespace ApplianceRepair.Models;

/// <summary>管理员自建工单</summary>
[SugarTable("ManualOrders")]
public class ManualOrder
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 32)]
    public string OrderNo { get; set; } = string.Empty;

    public int ServiceTypeId { get; set; }

    public int ServiceItemId { get; set; }

    /// <summary>保修类型：0包内（保内） 1保外</summary>
    public int WarrantyType { get; set; }

    /// <summary>金额</summary>
    public decimal Amount { get; set; }

    /// <summary>预约开始时间</summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? AppointmentStart { get; set; }

    /// <summary>预约结束时间</summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? AppointmentEnd { get; set; }

    [SugarColumn(Length = 50)]
    public string ContactName { get; set; } = string.Empty;

    [SugarColumn(Length = 20)]
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>所在区域</summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? Area { get; set; }

    /// <summary>详细地址</summary>
    [SugarColumn(Length = 300, IsNullable = true)]
    public string? Address { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    /// <summary>指派的师傅 UserId</summary>
    [SugarColumn(IsNullable = true)]
    public int? AssignedUserId { get; set; }

    /// <summary>0待指派 1已指派 2进行中 3已完成 4已取消</summary>
    public int Status { get; set; }

    /// <summary>创建该工单的管理员 UserId</summary>
    public int CreatedByUserId { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>照片路径，JSON数组格式如 ["/uploads/xxx.jpg",...]</summary>
    [SugarColumn(Length = 2000, IsNullable = true)]
    public string? Photos { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? AssignTime { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? CompleteTime { get; set; }
}
