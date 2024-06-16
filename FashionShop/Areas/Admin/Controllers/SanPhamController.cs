using FashionShop.Models;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Authorize(Roles = AppRole.Admin)]
    public class SanPhamController : Controller
    {
        private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public SanPhamController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["DanhMucSortParm"] = sortOrder == "DanhMucId.Name" ? "danhmuc_desc" : "DanhMucId.Name";
            ViewData["ThuongHieuSortParm"] = sortOrder == "ThuongHieu" ? "thuonghieu_desc" : "ThuongHieu";

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
                case "name_desc":
                    sanphams = sanphams.OrderByDescending(s => s.Name);
                    break;
                case "price_desc":
                    sanphams = sanphams.OrderByDescending(s => s.Price);
                    break;
                case "danhmuc_desc":
                    sanphams = sanphams.OrderByDescending(s => s.DanhMuc.Name);
                    break;
                case "thuonghieu_desc":
                    sanphams = sanphams.OrderByDescending(s => s.ThuongHieu.Name);
                    break;
                default:
                    sanphams = sanphams.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<SanPhamModel>.CreateAsync(sanphams.Include(p => p.DanhMuc).Include(p => p.ThuongHieu).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.DanhMucs = new SelectList(_dataContext.DanhMucs, "Id", "Name");
            ViewBag.ThuongHieus = new SelectList(_dataContext.ThuongHieus, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPhamModel sanPham)
        {
            ViewBag.DanhMucs = new SelectList(_dataContext.DanhMucs, "Id", "Name", sanPham.DanhMucId);
            ViewBag.ThuongHieus = new SelectList(_dataContext.ThuongHieus, "Id", "Name", sanPham.ThuongHieuId);

			if (ModelState.IsValid)
			{
				//thêm dữ liệu
				sanPham.Slug = sanPham.Name.Replace(" ", "_");
				var slug = await _dataContext.SanPhams.FirstOrDefaultAsync(p => p.Slug == sanPham.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Sản phẩm đã tồn tại.");
                    return View(sanPham);
				}

				if (sanPham.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/SanPhams");
                    //Tạo chuỗi random ngẫu nhiên + tên ảnh
					string imageName = Guid.NewGuid().ToString() + "_" + sanPham.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
					await sanPham.ImageUpload.CopyToAsync(fs);
					fs.Close();
					sanPham.Image = imageName;
				}

				_dataContext.Add(sanPham);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Đã thêm sản phẩm thành công!";
				return RedirectToAction("Index");
			}
			else
			{
                //Thông báo lỗi
				TempData["error"] = "Lỗi dữ liệu nhập vào chưa hợp lệ.";
				List<string> errors = new List<string>();
                //Thông báo lỗi cụ thể
				foreach(var value in ModelState.Values)
				{
					foreach(var error in value.Errors)
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
            SanPhamModel sanPham = await _dataContext.SanPhams.FindAsync(Id);
            ViewBag.Danhmucs = new SelectList(_dataContext.DanhMucs, "Id", "Name", sanPham.DanhMucId);
            ViewBag.ThuongHieus = new SelectList(_dataContext.ThuongHieus, "Id", "Name", sanPham.ThuongHieuId);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPhamModel sanPham)
        {
            ViewBag.DanhMucs = new SelectList(_dataContext.DanhMucs, "Id", "Name", sanPham.DanhMucId);
            ViewBag.ThuongHieus = new SelectList(_dataContext.ThuongHieus, "Id", "Name", sanPham.ThuongHieuId);

            var existed_product = _dataContext.SanPhams.Find(sanPham.Id);//Tìm sản phẩm theo Id
            if (ModelState.IsValid)
            {
                //thêm dữ liệu
                sanPham.Slug = sanPham.Name.Replace(" ", "_");
                
                if (sanPham.ImageUpload != null)
                {                  
                    //Upload ảnh mới
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/SanPhams");
                    //Tạo chuỗi random ngẫu nhiên + tên ảnh
                    string imageName = Guid.NewGuid().ToString() + "_" + sanPham.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    //Xóa hình ảnh cũ
                    string oldfilePath = Path.Combine(uploadsDir, existed_product.Image);
                    //nếu dường dẫn images/SanPhams/.. có tồn tại
                    try
                    {
                        if (System.IO.File.Exists(oldfilePath))
                        {
                            //xóa hình ảnh
                            System.IO.File.Delete(oldfilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa hình ảnh!");
                    }

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await sanPham.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_product.Image = imageName;                  
                }

                //Update thông tin sản phẩm
                existed_product.Name = sanPham.Name;
                existed_product.Description = sanPham.Description;
                existed_product.Price = sanPham.Price;
                existed_product.DanhMucId = sanPham.DanhMucId;
                existed_product.ThuongHieuId = sanPham.ThuongHieuId;

                _dataContext.Update(existed_product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin sản phẩm thành công!";
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
            SanPhamModel sanPham = await _dataContext.SanPhams.FindAsync(Id);

            if(sanPham == null)
            {
                return NotFound();
            }
            
            /*if (!string.Equals(sanPham.Image, "noImage.jpg"))
            {*/
            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/SanPhams");
            string oldfilePath = Path.Combine(uploadsDir, sanPham.Image);

            //nếu dường dẫn images/SanPhams/.. có tồn tại
            try
            {
                if (System.IO.File.Exists(oldfilePath))
                {
                    //xóa hình ảnh
                    System.IO.File.Delete(oldfilePath);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa hình ảnh!");
            }
            //}

            _dataContext.SanPhams.Remove(sanPham);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã xóa thành công!";
            return RedirectToAction("Index");
        }

		public async Task<IActionResult> Details(int id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var sanpham = await _dataContext.SanPhams.Include(s => s.DanhMuc).Include(e => e.ThuongHieu).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

			if (sanpham == null)
			{
				return NotFound();
			}

			return View(sanpham);
		}

	}
}
