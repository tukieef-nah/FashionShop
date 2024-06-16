using FashionShop.Models;
using FashionShop.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FashionShop.Repository;
using Microsoft.AspNetCore.Authorization;
using FashionShop.Respository;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Controllers
{
	public class AccountController : Controller
	{
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private RoleManager<IdentityRole> _roleManager;
		private readonly DataContext _dataContext;

		public AccountController(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, RoleManager<IdentityRole> roleManager, DataContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dataContext = context;
        }

        [HttpGet]
        [Route("Account/Index")]
        [Authorize]
		[Authorize(Roles = AppRole.User)]
		public async Task<IActionResult> Info()
        {
            var Name = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(Name))
            {
                return NotFound();
            }
            var user = await _userManager.FindByNameAsync(Name);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        //show dữ liệu hiện có
        [HttpGet]
        [Route("Account/EditInfo/{Name?}")]
		[Authorize]
		[Authorize(Roles = AppRole.User)]
		public async Task<IActionResult> EditInfo()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }
            var user = await _userManager.FindByNameAsync(userName);
            return View(user);
        }

        //cập nhật thông tin mới
        [HttpPost]
        [Route("Account/EditInfo")]
        [Authorize]
		[Authorize(Roles = AppRole.User)]
		public async Task<IActionResult> EditInfo(AppUserModel model)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(userName);

				if (user == null)
				{
                    return NotFound();
				}

				if (model.NameAccount == null || model.Email == null || model.PhoneNumber == null || model.Address == null || model.UserName == null)
				{
					TempData["error"] = "Vui lòng nhập đầy đủ thông tin!";
				}
				else
				{
					var result1 = await _userManager.SetEmailAsync(user, model.Email);
					if (result1.Succeeded)
					{
						var result2 = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
						if (result2.Succeeded)
						{
							var result3 = await _userManager.SetUserNameAsync(user, model.UserName);
							if (result3.Succeeded)
							{
								user.NameAccount = model.NameAccount;
								user.Address = model.Address;
								await _userManager.UpdateAsync(user);
								TempData["success"] = "Cập nhật thông tin thành công!";
								return RedirectToAction("Info", "Account");
							}
							else
							{
								TempData["error"] = "Lỗi cập nhật tên đăng nhập!";
							}
						}

						else
						{
							TempData["error"] = "Lỗi cập nhật số điện thoại!";
						}
					}
					else
					{
						TempData["error"] = "Lỗi cập nhật email!";
					}
				}
			}
			return View(model ?? new IdentityUser { UserName = userName });
        }

        [HttpGet]
        [Route("Account/EditPassword")]
        [Authorize]
		[Authorize(Roles = AppRole.User)]
		public async Task<IActionResult> EditPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("Account/EditPassword")]
        [Authorize]
		[Authorize(Roles = AppRole.User)]
		public async Task<IActionResult> EditPassword(EditPassword pw)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            if (pw.ConfirmPassword == pw.NewPassword)
            {
                var result = await _userManager.ChangePasswordAsync(user, pw.OldPassword, pw.NewPassword);

                if (result.Succeeded)
                {
					//Đổi thành công
					TempData["success"] = "Đổi mật khẩu thành công.";
					return RedirectToAction("Info");
                }
                else
                {
                    //Thất bại
                    TempData["error"] = "Đổi mật khẩu thất bại.";
                }
            }
            else
            {
                TempData["error"] = "Mật khẩu nhập lại không khớp.";
            }
            return View(pw);
        }

        //Xóa tài khoản từ yêu cầu người dùng
        [HttpGet]
		[Route("Account/Delete")]
		[Authorize(Roles = AppRole.User)]
		public async Task<IActionResult> Delete(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}

			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var orders = _dataContext.Orders.Where(od => od.UserName == user.UserName);
			OrderModel order = _dataContext.Orders.Where(od => od.UserName == user.UserName).FirstOrDefault();
			var orderdetails = _dataContext.OrderDetails.Where(od => od.OrderCode == order.OrderCode);
			_dataContext.OrderDetails.RemoveRange(orderdetails);
			_dataContext.Orders.RemoveRange(orders);
			await _dataContext.SaveChangesAsync();

			IdentityResult result = await _userManager.DeleteAsync(user);
			TempData["success"] = "Đã xóa tài khoản thành công!";
			return RedirectToAction("Logout");
		}

        [HttpGet]
        [Route("Account/Order")]
        [Authorize(Roles = AppRole.User)]
        public async Task<IActionResult> Order( string sortOrder, int? pageNumber)
        {
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";

			var userName = User.FindFirstValue(ClaimTypes.Name);
			var orders = from s in _dataContext.Orders where s.UserName == userName
                         select s;

			switch (sortOrder)
			{
				case "date_desc":
					orders = orders.OrderByDescending(s => s.CreatedDate);
					break;
				case "status_desc":
					orders = orders.OrderBy(s => s.Status);
					break;
				default:
					orders = orders.OrderBy(s => s.CreatedDate);
					break;
			}

			int pageSize = 10;
			return View(await PaginatedList<OrderModel>.CreateAsync(orders.AsNoTracking(), pageNumber ?? 1, pageSize));
		}

        [HttpGet]
        [Route("Account/ViewOrder")]
        [Authorize(Roles = AppRole.User)]
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
                TempData["success"] = "Đã hủy đơn hàng thành công!";
                return RedirectToAction("Order");
            }
            TempData["error"] = "Đã xảy ra lỗi trong quá trình hủy đơn hàng.";
            return RedirectToAction("Order");
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginVM.UserName); 
                //Đăng nhập đồng bộ với username và password
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);

                if (result.Succeeded)
                {
                    //Lấy role kiểm tra
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if(userRoles.Contains(AppRole.Admin))
                    {
						return Redirect("/Admin");
					}
                    else
                    {
                        return Redirect(loginVM.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Thông tin đăng nhập không đúng.");
            }
            return View(loginVM);
        }

        [HttpGet]
		public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                if (user.Password == user.RePassword)
                {
                    AppUserModel newUser = new AppUserModel { UserName = user.UserName, Email = user.Email, PhoneNumber = user.PhoneNumber, Address = user.Address, NameAccount = user.NameAccount };
                    IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
                    if (result.Succeeded)
                    {
                        //Kiểm tra cấp role
                        if(!await _roleManager.RoleExistsAsync(AppRole.User))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(AppRole.User));
                        }
                        await _userManager.AddToRoleAsync(newUser, AppRole.User);
                        TempData["success"] = "Đăng ký tài khoản thành công!";
                        return Redirect("/account/login");
                    }
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    TempData["error"] = "Mật khẩu nhập lại không khớp!";
                }
            }
            return View(user);
        }

		[Route("Logout")]
		public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
