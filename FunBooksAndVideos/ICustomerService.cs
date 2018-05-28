namespace FunBooksAndVideos.Test
{
    public interface ICustomerService
    {
        void ActivateMembership(string customerId, Membership membership);
    }
}