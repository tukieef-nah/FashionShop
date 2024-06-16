using FashionShop.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Respository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<ThuongHieuModel> ThuongHieus { get; set; }

        public DbSet<SanPhamModel> SanPhams { get; set; }

        public DbSet<DanhMucModel> DanhMucs { get; set; }

        public DbSet<OrderModel> Orders { get; set; }

        public DbSet<OrderDetails> OrderDetails { get; set; }
    }
}
