using System.Collections.Generic;

namespace FunBooksAndVideos.Test
{
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