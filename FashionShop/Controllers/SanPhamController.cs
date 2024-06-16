using FashionShop.Models;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Controllers
{
	public class SanPhamController : Controller
	{
        private readonly DataContext _dataContext;

        public SanPhamController(DataContext context)
        {
            _dataContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var sanphams = from s in _dataContext.SanPhams
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                sanphams = sanphams.Where(s => s.Name!.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "price_desc":
                    sanphams = sanphams.OrderByDescending(s => s.Price);
                    break;
                default:
                    sanphams = sanphams.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<SanPhamModel>.CreateAsync(sanphams.Include(p => p.DanhMuc).Include(p => p.ThuongHieu).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Route("ChiTietSanPham/{id?}")]
        public async Task<IActionResult> Details(int Id)
		{
            if(Id == null)
                return RedirectToAction("Index");

            var SanPhamById = _dataContext.SanPhams.Where(s => s.Id == Id).FirstOrDefault();
            return View(SanPhamById);
		}
    }
}
