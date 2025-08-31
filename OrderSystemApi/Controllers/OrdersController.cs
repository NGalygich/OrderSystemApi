using Microsoft.AspNetCore.Mvc;
using OrderSystemApi.Interfaces;
using OrderSystemApi.Models;
using System.Threading.Tasks;

namespace OrderSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrdersController(IOrderRepository orderRepository, IProductRepository productRepository, ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var customerExists = await _customerRepository.ExistsAsync(request.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer not found");
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = "Created",
                TotalAmount = 0
            };

            foreach (var itemRequest in request.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with id {itemRequest.ProductId} not found");
                }

                if (product.StockQuantity < itemRequest.Quantity)
                {
                    return BadRequest($"Insufficient stock for product {product.Name}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price
                };

                order.OrderItems.Add(orderItem);
                order.TotalAmount += itemRequest.Quantity * product.Price;

                product.StockQuantity -= itemRequest.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            var createdOrder = await _orderRepository.CreateAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Order>> UpdateOrder(int id, [FromBody] Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            var existingOrder = await _orderRepository.ExistsAsync(id);
            if (!existingOrder)
            {
                return NotFound();
            }

            await _orderRepository.UpdateAsync(order);
            var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(id);
            return Ok(updatedOrder);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity += orderItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            await _orderRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<Order>>> GetOrdersByCustomer(int customerId)
        {
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }
    }
}