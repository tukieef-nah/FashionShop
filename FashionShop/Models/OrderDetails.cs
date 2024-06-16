using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionShop.Models
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }

        public string OrderCode { get; set; }
        [ForeignKey("OrderCode")]
        public OrderModel Orders { get; set; }

        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public SanPhamModel SanPham { get; set; }

        public int Quantity { get; set; }

    }
}
