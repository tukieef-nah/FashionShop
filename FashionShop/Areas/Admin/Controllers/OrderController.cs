using FashionShop.Models;
using FashionShop.Models.ViewModels;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FashionShop.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
    [Authorize(Roles = AppRole.Admin)]
    public class OrderController : Controller
	{
        private readonly DataContext _dataContext;
        private UserManager<AppUserModel> _userManager;

        public OrderController(DataContext context, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

		[HttpGet]
		public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
		{
			ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
			ViewData["NameAccountSortParm"] = sortOrder == "NameAccount" ? "nameaccount_desc" : "NameAccount";
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

			List<OrderViewModel> Order = new List<OrderViewModel>();
			var orders = _dataContext.Orders.ToList(); // Materialize the query into a list

			foreach (var od in orders)
			{
				var user = await _userManager.FindByNameAsync(od.UserName);
				OrderViewModel order = new OrderViewModel()
				{
					NameAccount = user.NameAccount,
					Phone = user.PhoneNumber,
					Address = user.Address,
					Orders = od,
				};
				Order.Add(order);
			}

			if (!String.IsNullOrEmpty(searchString))
			{
				Order = Order.Where(s => s.Orders.OrderCode!.Contains(searchString) || s.NameAccount!.Contains(searchString) || s.Phone!.Contains(searchString)).ToList();
			}

			switch (sortOrder)
			{
				case "nameaccount_desc":
					Order = Order.OrderByDescending(s => s.NameAccount).ToList();
					break;
				case "date_desc":
					Order = Order.OrderByDescending(s => s.Orders.CreatedDate).ToList();
					break;
				case "status_desc":
					Order = Order.OrderByDescending(s => s.Orders.Status).ToList();
					break;
				default:
					Order = Order.OrderBy(s => s.Orders.CreatedDate).ToList();
					break;
			}

			return View(Order);
		}

		[HttpGet]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            OrderModel orders = _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstOrDefault();
            var user = await _userManager.FindByNameAsync(orders.UserName);
            //Lấy danh sách sản phẩm trong đơn hàng
            var details = await _dataContext.OrderDetails.Where(o => o.OrderCode == ordercode).Include(o => o.SanPham).ToListAsync();

            //khởi tạo list trong DetailsOrderViewModel
            List<OrderDetails> OrderDetail = new List<OrderDetails>();

            //lấy thông tin
            foreach (var od in details)
            {
                OrderDetail.Add(od);
            }

            DetailsOrderViewModel dovm = new DetailsOrderViewModel()
            {
                NameAccount = user.NameAccount,
                Phone = user.PhoneNumber,
                Address = user.Address,
                Orders = orders,
                OrderDetails = OrderDetail,
            };

            return View(dovm);
        }

        public async Task<IActionResult> Status(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(p => p.OrderCode == ordercode);
            if (ModelState.IsValid)
            {
                //thêm dữ liệu
                order.Status = status;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật trạng thái thành công!";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Đã xảy ra lỗi trong quá trình cập nhật trạng thái!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string ordercode)
        {
            OrderModel orders = await _dataContext.Orders.FindAsync(ordercode);

            //Xóa các dữ liệu liên quan khỏi bảng OrderDetails
            var orderDetails = _dataContext.OrderDetails.Where(od => od.OrderCode == ordercode);
            _dataContext.OrderDetails.RemoveRange(orderDetails);

            //Xóa dữ liệu trong Orders
            _dataContext.Orders.Remove(orders);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Đã xóa đơn hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}
