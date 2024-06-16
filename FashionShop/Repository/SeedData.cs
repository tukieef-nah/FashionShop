using FashionShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Respository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			if(!_context.SanPhams.Any())
			{
				ThuongHieuModel LouisVuitton = new ThuongHieuModel { Name = "Louis Vuitton", Slug = "Louis Vuitton", Description = "Louis Vuitton là thương hiệu thời trang xa xỉ của Pháp, nổi tiếng với dòng sản phẩm đồ da được thành lập vào năm 1854. Thương hiệu Louis Vuitton được đặt theo tên của nhà thiết kế đồng thời cũng là nhà sáng lập công ty.", Status = "1" };
				ThuongHieuModel Zara = new ThuongHieuModel { Name = "Zara", Slug = "zara", Description = "Nằm trong bảng xếp hạng thương hiệu thời trang thế giới, Zara là thương hiệu quần áo và phụ kiện nổi tiếng trực thuộc Inditex – một trong những nhà bán lẻ thời trang lớn nhất thế giới. Được thành lập vào năm 1974 tại Arteixo, Galicia, Tây Ban Nha, đến nay thương hiệu thời trang nổi tiếng Zara đã sở hữu hơn 2.000 cửa hàng trên toàn thế giới.", Status = "1" };

				DanhMucModel Ao = new DanhMucModel { Name = "Áo", Slug = "Áo", Description = "Áo hot trend", Status = 1 };
				DanhMucModel Quan = new DanhMucModel { Name = "Quần", Slug = "Quần", Description = "Quần hot trend", Status = 1 };
				DanhMucModel Tui = new DanhMucModel { Name = "Túi", Slug = "Túi", Description = "Túi hot trend", Status = 1 };

				_context.SanPhams.AddRange(

					new SanPhamModel { Name = "Áo", Slug = "áo", Description = "Mẫu áo thun thịnh hành nhất 2024", Price = 100, DanhMuc = Ao, ThuongHieu = LouisVuitton, Image = "1.jpg" },
					new SanPhamModel { Name = "quần", Slug = "quần", Description = "Mẫu quần thun thịnh hành nhất 2024", Price = 500, DanhMuc = Quan, ThuongHieu = Zara, Image = "2.jpg" },
					new SanPhamModel { Name = "túi", Slug = "túi", Description = "Mẫu túi thịnh hành nhất 2024", Price = 500, DanhMuc = Tui, ThuongHieu = Zara, Image = "3.jpg" }

				);
				_context.SaveChanges();
			}	
		}
	}
}
