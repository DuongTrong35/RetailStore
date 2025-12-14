namespace RetailStore.Models
{
    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int? PromoId { get; set; }
        public string PromoCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal MoneyPay { get; set; }
        public decimal MoneyReturn { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }
}
