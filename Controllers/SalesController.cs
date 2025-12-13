using Microsoft.AspNetCore.Mvc;
using RetailStore.Models;
using System.Linq;

namespace RetailStore.Controllers
{
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            var mnv = HttpContext.Session.GetInt32("nvid");
            ViewBag.nvid = mnv;
            var products = from p in _context.Products
                           join i in _context.Inventories
                               on p.ProductId equals i.ProductId into pi
                           from inv in pi.DefaultIfEmpty()
                           select new ProductInventory
                           {
                               ProductId = p.ProductId,
                               ProductName = p.ProductName,
                               Price = p.Price,
                               CategoryId = (int)p.CategoryId,
                               image = p.ImageUrl,
                               //image = p.ImageUrl ?? "/images/products/null-image.png",
                               Quantity = inv.Quantity ?? 0,
                               unit = p.Unit
                           };

            var result = products.ToList();
            return View(result);
        }


        //public IActionResult Index(int page = 1)
        //{

        //    var products = _context.Products.ToList();
        //    var product = _context.Products.FirstOrDefault();
        //    Console.WriteLine($"ProductId: {product.ProductId}");

        //    return View(products); // KHÔNG skip/take
        //}

        [HttpGet]
        public IActionResult searchmagiam(string keyword)
        {
            var magiamgia = (from p in _context.Promotions
                             where p.PromoCode.Contains(keyword)
                             select new
                             {
                                 promoId = p.PromoId,
                                 p.StartDate,
                                 p.EndDate,
                                 p.Status,
                                 p.UsageLimit,
                                 p.UsedCount,
                                 p.MinOrderAmount,
                                 p.DiscountType,
                                 p.DiscountValue
                             })
                             .FirstOrDefault();

            if (magiamgia == null)
            {
                return NotFound("Không tìm thấy mã giảm giá.");
            }

            return Ok(magiamgia);
        }



        [HttpGet]
        public IActionResult Search(string keyword)
        {
            var products = (from p in _context.Products
                            join i in _context.Inventories
                                on p.ProductId equals i.ProductId into pi
                            from inv in pi.DefaultIfEmpty()
                            where p.ProductName.Contains(keyword)
                            select new
                            {
                                p.ProductId,
                                p.ProductName,
                                p.Price,
                                ImageUrl = p.ImageUrl ?? "/images/products/null-image.png",
                                p.Unit,
                                Quantity = inv != null ? inv.Quantity ?? 0 : 0
                            })
                            .Take(10)
                            .ToList();

            return Ok(products);
        }


        [HttpPost]
        public IActionResult UpdatePromoUsage(string keyword)
        {
            var promo = _context.Promotions.FirstOrDefault(p => p.PromoCode == keyword);

            if (promo == null)
                return NotFound("Không tìm thấy mã giảm giá.");

            promo.UsedCount++;

            if (promo.UsedCount >= promo.UsageLimit)
                promo.Status = "inactive";

            _context.SaveChanges();

            return Ok(new
            {
                message = "Đã cập nhật lượt sử dụng mã.",
                promo.UsedCount,
                promo.Status
            });
        }


        [HttpGet]
        public IActionResult CheckCustomerPhone(string phone)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.Phone == phone);

            if (customer == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "Không tìm thấy khách hàng."
                });
            }

            return Ok(new
            {
                success = true,
                customerId = customer.CustomerId,
                name = customer.Name
            });
        }



        [HttpPost]
        public IActionResult Checkout([FromBody] CheckoutRequest req)
        {
            if (req == null || req.Items == null || !req.Items.Any())
                return BadRequest("Dữ liệu không hợp lệ");

            // 1️⃣ Tạo Orders
            var order = new Order
            {
                UserId = req.UserId,
                CustomerId = req.CustomerId,
                PromoId = req.PromoId,
                TotalAmount = req.TotalAmount,
                DiscountAmount = req.DiscountAmount,
                Status = "paid",
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // 2️⃣ Lưu Order Items
            foreach (var item in req.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Subtotal = item.Price * item.Quantity
                };

                _context.OrderItems.Add(orderItem);
            }

            _context.SaveChanges();

            // 3️⃣ Lưu Payment
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = req.TotalAmount,
                PaymentMethod = "cash",
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);

            // 4️⃣ Cập nhật loyalty point
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == req.CustomerId);

            if (customer != null)
            {
                int earnedPoints = (int)(req.TotalAmount / 10000);
                // ví dụ: 10k = 1 điểm → bạn muốn công thức nào nói tôi sửa

                customer.LoyaltyPoints += earnedPoints;

                _context.Customers.Update(customer);
            }

            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                orderId = order.OrderId,
                newPoints = customer?.LoyaltyPoints
            });
        }


    }
}
