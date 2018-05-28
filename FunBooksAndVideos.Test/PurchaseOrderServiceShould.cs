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
}
