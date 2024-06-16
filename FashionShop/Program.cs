using FashionShop.Models;
using FashionShop.Respository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


//Connection db
builder.Services.AddDbContext<DataContext>(options =>
{
	options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});


// Add services to the container.
builder.Services.AddControllersWithViews();

//Đăng ký dịch vụ lưu cache trong bộ nhớ
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);//thời gian tồn tại
	options.Cookie.IsEssential = true;
});


//Identity
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
	.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings.
	options.Password.RequireDigit = true;//kiểu số
	options.Password.RequireLowercase = true;//chữ thường
	options.Password.RequireNonAlphanumeric = false;//kí tự đặc biệt
	options.Password.RequireUppercase = false;//chữ in hoa
	options.Password.RequiredLength = 6;//chiều dài password tối thiểu

	options.User.RequireUniqueEmail = true;
});


var app = builder.Build();

//Trang error
app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();//đường dẫn

app.UseHttpsRedirection();
app.UseAuthentication();//xác thực
app.UseAuthorization();//phân quyền

//Map của backend
app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=SanPham}/{action=Index}/{id?}");

//Thiết kế lại map cho danh mục
app.MapControllerRoute(
	name: "danhmuc",
	pattern: "/danhmuc/{Slug?}",
	defaults: new { controller = "DanhMuc", action = "Index"});

//Thiết kế lại map cho danh mục
app.MapControllerRoute(
    name: "thuonghieu",
    pattern: "/thuonghieu/{Slug?}",
    defaults: new { controller = "ThuongHieu", action = "Index" });

//Map của frontend
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=GioHang}/{action=Index}/{id?}");

//Seeding Data
var context = app.Services.CreateAsyncScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);

app.Run();
