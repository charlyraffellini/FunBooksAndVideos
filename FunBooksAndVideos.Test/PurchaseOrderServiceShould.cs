using System.Collections.Generic;
using Moq;
using Xunit;

namespace FunBooksAndVideos.Test
{
    public class PurchaseOrderServiceShould
    {
        private const string OrderId = "Order Id";
        private const string CustomerId = "Customer Id";
        private Mock<ICustomerService> _customerServiceMock;
        private ActivateMembership _activateMembership;
        private Order _orderWithMembership;

        public PurchaseOrderServiceShould()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _activateMembership = new ActivateMembership(_customerServiceMock.Object);
            _orderWithMembership = Order.CreateOrderWithMembership(OrderId, CustomerId);
        }

        [Fact]
        public void ProcessMembershipOrder()
        {
            var businessRules = new List<IBusinessRule>{_activateMembership};
            var purchaseOrderService = new PurchaseOrderService(businessRules);

            purchaseOrderService.Process(_orderWithMembership);

            _customerServiceMock.Verify(c => c.ActivateMembership(CustomerId, It.IsAny<Membership>()));
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
        public string OrderId { get; }
        public string CustomerId { get; }
        public Membership Membership { get; }
        public bool ContainsMembership() => Membership != null;


        public static Order CreateOrderWithMembership(string orderId, string customerId)
        {
            return new Order(orderId, customerId, new Membership());
        }

        private Order(string orderId, string customerId, Membership membership)
        {
            OrderId = orderId;
            CustomerId = customerId;
            this.Membership = membership;
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
