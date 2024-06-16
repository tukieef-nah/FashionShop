using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models
{
	public class UserModel
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập UserName.")]
		public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string NameAccount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress]
		public string Email { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại."), Phone]
        public string PhoneNumber { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
		public string Address { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập password.")]
		//Mã hóa password
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập RePassword.")]
		public string RePassword { get; set; }
	}
}