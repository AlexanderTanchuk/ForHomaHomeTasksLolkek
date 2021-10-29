using System.Collections.Generic;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces
{
    public interface IOrderItemsReserver
    {
        Task CallFunctionAsync(Order order);

        Task SendToServiceBusAsync(Order order);
    }
}
