using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FashionShop.Repository.Validation;

namespace FashionShop.Models
{
	public class SanPhamModel
	{
		[Key]
		public int Id { get; set; }

		[Required, MinLength(4, ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
		public string Name { get; set; }

		public string Slug { get; set; }

		[Required, MinLength(4, ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
		[Range(0.01, double.MaxValue)]
		[Column(TypeName = "decimal(8,2)")]
		public decimal Price { get; set; }

		[Required, Range(1, int.MaxValue, ErrorMessage = "Chọn 1 danh mục.")]
		public int DanhMucId { get; set; }

		public DanhMucModel DanhMuc { get; set; }

		[Required, Range(1, int.MaxValue, ErrorMessage = "Chọn 1 thương hiệu.")]
		public int ThuongHieuId { get; set; }

		public ThuongHieuModel ThuongHieu { get; set; }

		public string Image { get; set; }

        [NotMapped]
		[FileExtension]
        public IFormFile ? ImageUpload { get; set; }
	}
}
