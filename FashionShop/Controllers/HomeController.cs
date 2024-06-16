using FashionShop.Models;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FashionShop.Controllers
{
	public class HomeController : Controller
	{
		private readonly DataContext _dataContext;

		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger, DataContext context)
		{
			_logger = logger;
			_dataContext = context;
		}

		[HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
		{
			ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";

			ViewData["CurrentFilter"] = searchString;

			var sanphams = from s in _dataContext.SanPhams
						   select s;

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

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error(int statuscode)
		{
			if(statuscode == 404)
			{
				return View("NotFound");
			}
			else
			{
				return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
			}
		}
	}
}
