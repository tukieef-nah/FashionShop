using FashionShop.Models;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
    [Authorize(Roles = AppRole.Admin)]
    public class DanhMucController : Controller
	{
		private readonly DataContext _dataContext;

		public DanhMucController(DataContext context)
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

            var danhmucs = from s in _dataContext.DanhMucs
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                danhmucs = danhmucs.Where(s => s.Name!.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    danhmucs = danhmucs.OrderByDescending(s => s.Name);
                    break;
                case "status_desc":
                    danhmucs = danhmucs.OrderByDescending(s => s.Status);
                    break;
                default:
                    danhmucs = danhmucs.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<DanhMucModel>.CreateAsync(danhmucs.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMucModel danhMuc)
        {
            if (ModelState.IsValid)
            {
                //thêm dữ liệu
                danhMuc.Slug = danhMuc.Name.Replace(" ", "_");
                var slug = await _dataContext.DanhMucs.FirstOrDefaultAsync(p => p.Slug == danhMuc.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã tồn tại.");
                    return View(danhMuc);
                }

                _dataContext.Add(danhMuc);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Đã thêm danh mục thành công!";
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
            DanhMucModel danhMuc = await _dataContext.DanhMucs.FindAsync(Id);
            return View(danhMuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, DanhMucModel danhMuc)
        {
            var existed_danhMuc = await _dataContext.DanhMucs.FirstOrDefaultAsync(p => p.Slug == danhMuc.Slug);
            if (ModelState.IsValid)
            {
                //thêm dữ liệu
                danhMuc.Slug = danhMuc.Name.Replace(" ", "_");

                _dataContext.Update(danhMuc);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công!";
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
            DanhMucModel danhMuc = await _dataContext.DanhMucs.FindAsync(Id);      

            _dataContext.DanhMucs.Remove(danhMuc);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Danh mục đã xóa thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _dataContext.DanhMucs.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (danhmuc == null)
            {
                return NotFound();
            }

            return View(danhmuc);
        }
    }
}
