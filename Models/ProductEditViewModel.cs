using System.ComponentModel.DataAnnotations;

namespace ApplianceRepair.Models;

public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "请输入商品名称")]
    [Display(Name = "商品名称")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "描述")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "是否上架")]
    public bool IsOnSale { get; set; } = true;

    [Required(ErrorMessage = "请输入价格")]
    [Display(Name = "价格")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Display(Name = "分类")]
    [StringLength(50)]
    public string? Category { get; set; }

    /// <summary>当前已有图片路径（编辑时显示）</summary>
    public string? ImagePath { get; set; }
}
