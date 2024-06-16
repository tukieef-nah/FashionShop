using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.ViewModels
{
    //Vì login chỉ có 2 trường username và password không có email, nếu dùng usermodel thì dính theo trường email [required], và như này tiện trả về lỗi cho form login
    public class LoginViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người dùng")]
        public string UserName { get; set; }

        //Mã hóa password
        [DataType(DataType.Password)]
		[Required(ErrorMessage = "Vui lòng nhập password")]
		public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
