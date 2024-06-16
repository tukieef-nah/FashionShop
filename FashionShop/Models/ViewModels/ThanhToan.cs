namespace FashionShop.Models.ViewModels
{
	public class ThanhToan
	{
		public string OrderCode	{ get; set; }

		public string NameAccount { get; set; }

		//Tổng tiền
		public decimal GrandTotal { get; set; }

		public int Paymenttype { get; set; }

		public int PaymentStatus { get; set; }
	}
}
