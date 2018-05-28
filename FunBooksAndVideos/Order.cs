using System.Collections.Generic;
using System.Linq;

namespace FunBooksAndVideos.Test
{
    public class Order
    {
        private readonly IEnumerable<Product> _products;
        public string OrderId { get; }
        public string CustomerId { get; }
        public Membership Membership { get; }
        public bool ContainsPhysicalProducts => _products.Any(p => p.IsPhysical);
        public IEnumerable<Product> PhysicalProducts => _products.Where(p => p.IsPhysical);

        public bool ContainsMembership() => Membership != null;


        public static Order CreateOrderWithMembership(string orderId, string customerId)
        {
            return new Order(orderId, customerId, new Membership());
        }

        public static Order CreateOrderWithProducts(string orderId, string customerId, IEnumerable<Product> products)
        {
            return new Order(orderId, customerId, products);
        }

        private Order(string orderId, string customerId, Membership membership)
        {
            OrderId = orderId;
            CustomerId = customerId;
            this.Membership = membership;
        }

        private Order(string orderId, string customerId, IEnumerable<Product> products)
        {
            _products = products;
            OrderId = orderId;
            CustomerId = customerId;
        }
    }
}