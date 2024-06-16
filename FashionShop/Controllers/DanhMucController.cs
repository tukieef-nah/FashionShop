using FashionShop.Models;
using FashionShop.Respository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Controllers
{
	public class DanhMucController : Controller
	{
		private readonly DataContext _dataContext;

		public DanhMucController(DataContext context)
		{
			_dataContext = context;
		}

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber, string Slug = "")
        {
			ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";

			//Lấy dòng danh mục chứa Slug == Slug đã chọn trong Danh mục
			DanhMucModel danhmuc = _dataContext.DanhMucs.Where(a => a.Slug == Slug).FirstOrDefault();
			//Xử lý không tìm thấy kết quả
			if (danhmuc == null)
				return RedirectToAction("Index");

			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}

			ViewData["CurrentFilter"] = searchString;

			var sanPhams = from m in _dataContext.SanPhams
                           where m.DanhMuc.Name == Slug
                           select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                sanPhams = sanPhams.Where(s => s.Name!.Contains(searchString));
            }

			switch (sortOrder)
			{
				case "price_desc":
					sanPhams = sanPhams.OrderByDescending(s => s.Price);
					break;
				default:
					sanPhams = sanPhams.OrderBy(s => s.Id);
					break;
			}

			int pageSize = 10;

			return View(await PaginatedList<SanPhamModel>.CreateAsync(sanPhams.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
	}
}
