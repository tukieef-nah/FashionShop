using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.ViewModels
{
    public class EditPassword
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu của bạn.")]
        public string ConfirmPassword { get; set; }
    }
}
