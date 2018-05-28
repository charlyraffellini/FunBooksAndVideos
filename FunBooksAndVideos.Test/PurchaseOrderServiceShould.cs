using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Xunit;

namespace FunBooksAndVideos.Test
{
    public class PurchaseOrderServiceShould
    {
        private const string OrderId = "Order Id";
        private const string CustomerId = "Customer Id";
        private const string TheGirlOnTheTrainTitle = "The Girl on the train";
        private Mock<ICustomerService> _customerServiceMock;
        private Mock<IShippingService> _shippingSlipServiceMock;
        private ActivateMembership _activateMembership;
        private ShippingSlip _shippingSlip;
        private Order _orderWithMembership;
        private Order _orderWithDigitalProducts;
        private Order _orderWithPhysicalProducts;

        public PurchaseOrderServiceShould()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _activateMembership = new ActivateMembership(_customerServiceMock.Object);
            _shippingSlipServiceMock = new Mock<IShippingService>();
            _shippingSlip = new ShippingSlip(_shippingSlipServiceMock.Object);
            _orderWithMembership = Order.CreateOrderWithMembership(OrderId, CustomerId);

            var physicalAndDigitalproducts = new List<Product>{Product.CreatePhysicalBook(TheGirlOnTheTrainTitle), Product.CreateDigitalBook("Alice's Adventures in Wonderland") };
            _orderWithPhysicalProducts = Order.CreateOrderWithProducts(OrderId, CustomerId, physicalAndDigitalproducts);

            var digitalproducts = new List<Product> { Product.CreateDigitalBook("Alice's Adventures in Wonderland") };
            _orderWithDigitalProducts = Order.CreateOrderWithProducts(OrderId, CustomerId, digitalproducts);
        }

        [Fact]
        public void ActivateMembershipWhenProcessMembershipOrder()
        {
            var businessRules = new List<IBusinessRule>{_activateMembership};
            var purchaseOrderService = new PurchaseOrderService(businessRules);

            purchaseOrderService.Process(_orderWithMembership);

            _customerServiceMock.Verify(c => c.ActivateMembership(CustomerId, It.IsAny<Membership>()));
        }

        [Fact]
        public void NotActivateMembershipWhenProcessProductsOrder()
        {
            var businessRules = new List<IBusinessRule> { _activateMembership };
            var purchaseOrderService = new PurchaseOrderService(businessRules);

            purchaseOrderService.Process(_orderWithPhysicalProducts);

            _customerServiceMock.Verify(c => c.ActivateMembership(CustomerId, It.IsAny<Membership>()), Times.Never);
        }

        [Fact]
        public void GenerateShippingSlipForPhysicalProducts()
        {
            var businessRules = new List<IBusinessRule> { _shippingSlip };
            var purchaseOrderService = new PurchaseOrderService(businessRules);

            purchaseOrderService.Process(_orderWithPhysicalProducts);

            Expression<Func<IEnumerable<Product>, bool>> collectionContainsPhysicalProductPredicate =
                ps => ps.First().Title == TheGirlOnTheTrainTitle &&
                      ps.Count() == 1;
            _shippingSlipServiceMock.Verify(s => s.GenerateShippingSlip(OrderId, CustomerId, It.Is(collectionContainsPhysicalProductPredicate)), Times.Once);
        }

        [Fact]
        public void NotGenerateShippingSlipWhenOrderHasNotPhysicalProducts()
        {
            var businessRules = new List<IBusinessRule> { _shippingSlip };
            var purchaseOrderService = new PurchaseOrderService(businessRules);

            purchaseOrderService.Process(_orderWithDigitalProducts);

            _shippingSlipServiceMock.Verify(s => s.GenerateShippingSlip(OrderId, CustomerId, It.IsAny<IEnumerable<Product>>()), Times.Never);
        }
    }

    public interface IShippingService
    {
        void GenerateShippingSlip(string orderId, string customerId, IEnumerable<Product> products);
    }

    public class ShippingSlip : IBusinessRule
    {
        private readonly IShippingService _shippingService;

        public ShippingSlip(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        public void Apply(Order order)
        {
            if (order.ContainsPhysicalProducts)
            {
                var physicalProducts = order.PhysicalProducts;
                _shippingService.GenerateShippingSlip(order.OrderId,order.CustomerId,physicalProducts);
            }
        }
    }

    public class Product
    {
        public string Title { get; }
        public bool IsPhysical { get; }
        private readonly string _type;

        private Product(string type, string title, bool isPhysical)
        {
            Title = title;
            IsPhysical = isPhysical;
            _type = type;
        }

        public static Product CreatePhysicalBook(string title)
        {
            return new Product("Book", title, true);
        }

        public static Product CreateDigitalBook(string title)
        {
            return new Product("Book", title, false);
        }
    }

    public interface ICustomerService
    {
        void ActivateMembership(string customerId, Membership membership);
    }

    public class ActivateMembership : IBusinessRule
    {
        private readonly ICustomerService _customerService;

        public ActivateMembership(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public void Apply(Order order)
        {
            if (order.ContainsMembership())
            {
                _customerService.ActivateMembership(order.CustomerId, order.Membership);
            }
        }
    }

    public interface IBusinessRule
    {
        void Apply(Order order);
    }

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

    public class Membership

    {
    }

    public class PurchaseOrderService
    {
        private readonly List<IBusinessRule> _businessRules;

        public PurchaseOrderService(List<IBusinessRule> businessRules)
        {
            _businessRules = businessRules;
        }

        public void Process(Order order)
        {
            _businessRules.ForEach(b => b.Apply(order));
        }
    }
}
