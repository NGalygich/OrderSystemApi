namespace OrderSystemApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; } 
        public decimal TotalAmount { get; set; }        
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }        
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
