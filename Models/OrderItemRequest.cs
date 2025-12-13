namespace RetailStore.Models
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
