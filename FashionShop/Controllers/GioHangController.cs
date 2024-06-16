using FashionShop.Models;
using FashionShop.Models.ViewModels;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace FashionShop.Controllers
{
	[Authorize]
	public class GioHangController : Controller
	{
		private readonly DataContext _dataContext;
		private UserManager<AppUserModel> _userManager;

		public GioHangController(DataContext _context, UserManager<AppUserModel> userManager)
		{
			_dataContext = _context;
			_userManager = userManager;
		}

		[Route("GioHangCuaToi")]
		public async Task<IActionResult> Index()
		{
			List<GioHangItemModel> gioHangItem = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang") ?? new List<GioHangItemModel>();
			GioHangItemViewModel gioHangFS = new()
			{
				GioHangItems = gioHangItem,
				GrandTotal = gioHangItem.Sum(x => x.Quantity * x.Price)
			};
			return View(gioHangFS);
		}

		[Route("DatHang")]
		[Authorize]
		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			var userName = User.FindFirstValue(ClaimTypes.Name);
			var user = await _userManager.FindByNameAsync(userName);

			if (userEmail == null)
			{
				TempData["error"] = "Bạn chưa đăng nhập, vui lòng đăng nhập để có thể đặt hàng!";
				return RedirectToAction("Login", "Account");
			}
			else
			{
				List<GioHangItemModel> gioHangItem = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang") ?? new List<GioHangItemModel>();
				InfoCheckout donHangFS = new()
				{
					NameAccount = user.NameAccount,
					PhoneNumber = user.PhoneNumber,
					Address = user.Address,
					Paymenttype = 1,
					PaymentStatus = 0,
					GioHangItems = gioHangItem,
					GrandTotal = gioHangItem.Sum(x => x.Quantity * x.Price)
				};
				return View(donHangFS);
			}
		}

		public async Task<IActionResult> Add(int Id)
		{
			SanPhamModel sanpham = await _dataContext.SanPhams.FindAsync(Id);
			List<GioHangItemModel> gioHang = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang") ?? new List<GioHangItemModel>();
			GioHangItemModel gioHangItems = gioHang.Where(c => c.Id == Id).FirstOrDefault();

			if (gioHangItems == null)
			{
				gioHang.Add(new GioHangItemModel(sanpham));
			}
			else
			{
				gioHangItems.Quantity += 1;
			}
			HttpContext.Session.SetJson("GioHang", gioHang);

			TempData["success"] = "Thêm sản phẩm vào giỏ hàng thành công!";
			return Redirect(Request.Headers["Referer"].ToString());
		}

		public async Task<IActionResult> Increase(int Id)
		{
			List<GioHangItemModel> gioHang = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang");

            GioHangItemModel gioHangItem = gioHang.Where(c => c.Id == Id).FirstOrDefault();

			//Sản phẩm đã có trong giỏ hàng
            if (gioHangItem.Quantity > 0)
            {
                ++gioHangItem.Quantity;
            }
			//Cập nhật số lượng sản phẩm trong giỏ hàng
            HttpContext.Session.SetJson("GioHang", gioHang);
            
            return RedirectToAction("Index");
		}

		public async Task<IActionResult> Decrease(int Id)
		{
			List<GioHangItemModel> gioHang = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang");

			GioHangItemModel gioHangItem = gioHang.Where(c => c.Id == Id).FirstOrDefault();

			if (gioHangItem.Quantity > 1)
			{
				--gioHangItem.Quantity;
			}
			else
			{
				gioHang.RemoveAll(p => p.Id == Id);
			}

			if (gioHangItem.Quantity == 0)
			{
				//Giảm bằng 0 thì xóa sản phẩm khỏi giỏ hàng
				HttpContext.Session.Remove("GioHang");
			}
			else
			{
				//Vẫn còn tồn tại sản phẩm thì cập nhật lại số lượng
				HttpContext.Session.SetJson("GioHang", gioHang);
			}

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Remove(int Id)
		{
			List<GioHangItemModel> gioHang = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang");

			gioHang.RemoveAll(p => p.Id == Id);

			//Sản phẩm cuối cùng được xóa
			if (gioHang.Count == 0)
			{
				HttpContext.Session.Remove("GioHang");
			}
			else
			{
				HttpContext.Session.SetJson("GioHang", gioHang);
			}

			TempData["success"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Clear(int Id)
		{
			HttpContext.Session.Remove("GioHang");
			TempData["success"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng!";
			return RedirectToAction("Index");
		}
	}
}
