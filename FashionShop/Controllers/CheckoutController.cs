using FashionShop.Models;
using FashionShop.Models.ViewModels;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace FashionShop.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
        private UserManager<AppUserModel> _userManager;

        public CheckoutController(DataContext context, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

        [Route("ThanhToan")]
		public async Task<IActionResult> ThanhToan(string OrderCode)
		{
			var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);
            var thanhToan = new ThanhToan
			{
				OrderCode = OrderCode,
                NameAccount = user.NameAccount,
                GrandTotal = _dataContext.OrderDetails.Where(o => o.OrderCode == OrderCode).Sum(x => x.SanPham.Price * x.Quantity),
				Paymenttype = _dataContext.Orders.Where(o => o.OrderCode == OrderCode).FirstOrDefault().Paymenttype,
				PaymentStatus = _dataContext.Orders.Where(o => o.OrderCode == OrderCode).FirstOrDefault().PaymentStatus
			};
			return View(thanhToan);
		}

		public IActionResult ExchangePayment (string OrderCode)
		{
			OrderModel orders = _dataContext.Orders.Where(o => o.OrderCode == OrderCode).FirstOrDefault();
			if(orders.Paymenttype == 2)
			{
				orders.Paymenttype = 1;
			}	
			else
			{
				orders.Paymenttype = 2;
			}	

			_dataContext.Update(orders);
			_dataContext.SaveChanges();
			return Redirect("/account/order");
		}

		public IActionResult Paid(string OrderCode)
		{
			OrderModel orders = _dataContext.Orders.Where(o => o.OrderCode == OrderCode).FirstOrDefault();
			orders.PaymentStatus = 1;
			_dataContext.Update(orders);
			_dataContext.SaveChanges();
			return Redirect("/account/order");
		}

		public async Task<IActionResult> Checkout(int Paymenttype)
		{
			var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userName == null)
			{
				return Redirect("/account/login");
			}
			else
			{
				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = ordercode;
				orderItem.UserName = userName;
				orderItem.Status = 0;
				orderItem.CreatedDate = DateTime.Now;
				orderItem.Paymenttype = Paymenttype;
				orderItem.PaymentStatus = 0;
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
				List<GioHangItemModel> gioHangItem = HttpContext.Session.GetJson<List<GioHangItemModel>>("GioHang") ?? new List<GioHangItemModel>();
				foreach (var gioHang in gioHangItem)
				{
					var orderdetails = new OrderDetails();
					orderdetails.OrderCode = ordercode;
                    orderdetails.SanPhamId = gioHang.Id;
					orderdetails.Quantity = gioHang.Quantity;
					_dataContext.Add(orderdetails);
					_dataContext.SaveChanges();
				}
				HttpContext.Session.Remove("GioHang");
				if (Paymenttype == 2)
				{
					TempData["success"] = "Đặt hàng thành công, vui lòng thanh toán đơn hàng để tiến hành duyệt đơn hàng!";
					return RedirectToAction("ThanhToan", new RouteValueDictionary { { "ordercode", ordercode } });
				}
				else
				{
					TempData["success"] = "Đặt hàng thành công, vui lòng chờ duyệt đơn hàng!";
					return Redirect("/account/order");
				}
			}
		}


	}
}
