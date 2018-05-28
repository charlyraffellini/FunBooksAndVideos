namespace FunBooksAndVideos.Test
{
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
}