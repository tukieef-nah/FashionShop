using FashionShop.Models;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
    [Authorize(Roles = AppRole.Admin)]
    public class ThuongHieuController : Controller
	{
		private readonly DataContext _dataContext;

		public ThuongHieuController(DataContext context)
		{
			_dataContext = context;
		}

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var thuonghieus = from s in _dataContext.ThuongHieus
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                thuonghieus = thuonghieus.Where(s => s.Name!.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    thuonghieus = thuonghieus.OrderByDescending(s => s.Name);
                    break;
                case "status_desc":
                    thuonghieus = thuonghieus.OrderByDescending(s => s.Status);
                    break;
                default:
                    thuonghieus = thuonghieus.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<ThuongHieuModel>.CreateAsync(thuonghieus.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuongHieuModel thuongHieu)
        {
            if (ModelState.IsValid)
            {
                //thêm dữ liệu
                thuongHieu.Slug = thuongHieu.Name.Replace(" ", "_");
                var slug = await _dataContext.ThuongHieus.FirstOrDefaultAsync(p => p.Slug == thuongHieu.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã tồn tại.");
                    return View(thuongHieu);
                }

                _dataContext.Add(thuongHieu);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Đã thêm thương hiệu thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                //Thông báo lỗi
                TempData["error"] = "Lỗi dữ liệu nhập vào chưa hợp lệ.";
                List<string> errors = new List<string>();
                //Thông báo lỗi cụ thể
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Edit(int Id)
        {
            //tìm kiếm sản phảm có id
            ThuongHieuModel thuongHieu = await _dataContext.ThuongHieus.FindAsync(Id);
            return View(thuongHieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ThuongHieuModel thuongHieu)
        {
            var existed_thuongHieu = await _dataContext.ThuongHieus.FirstOrDefaultAsync(p => p.Slug == thuongHieu.Slug);
            if (ModelState.IsValid)
            {
                //thêm dữ liệu
                thuongHieu.Slug = thuongHieu.Name.Replace(" ", "_");

                _dataContext.Update(thuongHieu);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thương hiệu thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                //Thông báo lỗi
                TempData["error"] = "Lỗi dữ liệu nhập vào chưa hợp lệ.";
                List<string> errors = new List<string>();
                //Thông báo lỗi cụ thể
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            ThuongHieuModel thuongHieu = await _dataContext.ThuongHieus.FindAsync(Id);

            _dataContext.ThuongHieus.Remove(thuongHieu);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thương hiệu đã xóa thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thuonghieu = await _dataContext.ThuongHieus.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (thuonghieu == null)
            {
                return NotFound();
            }

            return View(thuonghieu);
        }
    }
}
