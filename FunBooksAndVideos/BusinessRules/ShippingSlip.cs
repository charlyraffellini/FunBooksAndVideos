namespace FunBooksAndVideos.Test
{
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
}