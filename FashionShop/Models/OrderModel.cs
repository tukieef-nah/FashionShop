using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionShop.Models
{
    public class OrderModel
    {
        [Key]
        public string OrderCode { get; set; }

        public string UserName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }

        public int Paymenttype { get; set; }

        public int PaymentStatus { get; set; }
    }
}
