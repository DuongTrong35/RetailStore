namespace RetailStore.Models
{
    public class ProductInventory
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string image { get; set; }
        public int Quantity { get; set; }
        public string unit { get; set; }

    }
}
