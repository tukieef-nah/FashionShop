using FashionShop.Models;
using FashionShop.Models.ViewModels;
using FashionShop.Repository;
using FashionShop.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = AppRole.Admin)]
    public class UserController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        public UserController(DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = dataContext;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["NameAccountSortParm"] = sortOrder == "NameAccount" ? "nameaccount_desc" : "NameAccount";
            ViewData["UserNameSortParm"] = sortOrder == "UserName" ? "username_desc" : "UserName";
            ViewData["AddressSortParm"] = sortOrder == "Address" ? "address_desc" : "Address";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var users = _userManager.Users;

            var roles = _roleManager.Roles;

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.UserName!.Contains(searchString) || s.NameAccount!.Contains(searchString) || s.PhoneNumber!.Contains(searchString) || s.Email!.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "nameaccount_desc":
                    users = users.OrderByDescending(s => s.NameAccount);
                    break;
                case "username_desc":
                    users = users.OrderByDescending(s => s.UserName);
                    break;
                case "address_desc":
                    users = users.OrderByDescending(s => s.Address);
                    break;
                default:
                    users = users.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<AppUserModel>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        [Route("Details")]
        public async Task<IActionResult> Details(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(UserModel user)
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
                        if (!await _roleManager.RoleExistsAsync(AppRole.User))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(AppRole.User));
                        }
                        await _userManager.AddToRoleAsync(newUser, AppRole.User);
                        TempData["success"] = "Thêm User thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //Thông báo lỗi
                        TempData["error"] = "Lỗi dữ liệu nhập vào chưa hợp lệ.";
                        foreach (IdentityError error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    TempData["error"] = "Mật khẩu nhập lại không khớp!";
                }
            }
            return View(user);
            //return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            return View(user);
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
				return NotFound();
			}

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);

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
							    return RedirectToAction("Index");
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
			return View(model ?? new IdentityUser { Id = id });
		}

        [HttpGet]
        [Route("Delete")]
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
            // Xóa các bản ghi liên quan trong bảng OrderDetails
            var orders = _dataContext.Orders.Where(od => od.UserName == user.UserName);
            OrderModel order = _dataContext.Orders.Where(od => od.UserName == user.UserName).FirstOrDefault();
            var orderdetails = _dataContext.OrderDetails.Where(od => od.OrderCode == order.OrderCode);
            _dataContext.OrderDetails.RemoveRange(orderdetails);
            _dataContext.Orders.RemoveRange(orders);
            await _dataContext.SaveChangesAsync();

            IdentityResult result = await _userManager.DeleteAsync(user);

            TempData["success"] = "Đã xóa User thành công!";
            return RedirectToAction("Index");
        }
    }
}
