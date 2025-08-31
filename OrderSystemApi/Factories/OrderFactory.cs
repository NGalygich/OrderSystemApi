using OrderSystemApi.Models;

namespace OrderSystemApi.Factories
{
    public class OrderFactory
    {
        public static Order CreateOrder(int customerId, string status = "Created")
        {
            return new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                Status = status,
                TotalAmount = 0 
            };
        }
    }
}
