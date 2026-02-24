using System.ComponentModel.DataAnnotations;

namespace ApplianceRepair.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "请输入用户名")]
    [Display(Name = "用户名")]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入密码")]
    [Display(Name = "密码")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "请确认密码")]
    [Display(Name = "确认密码")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "两次密码不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "姓名")]
    [StringLength(50)]
    public string? RealName { get; set; }

    [Display(Name = "手机号")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Display(Name = "注册为管理员")]
    public bool IsAdmin { get; set; }
}
