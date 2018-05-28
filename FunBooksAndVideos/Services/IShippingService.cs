using System.Collections.Generic;

namespace FunBooksAndVideos.Test
{
    public interface IShippingService
    {
        void GenerateShippingSlip(string orderId, string customerId, IEnumerable<Product> products);
    }
}