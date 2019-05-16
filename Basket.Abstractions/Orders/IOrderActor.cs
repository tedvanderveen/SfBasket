using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Basket.Abstractions.Orders
{
    public interface IOrderActor : IActor
    {
        Task<Order> CreateOrderAsync(int basketId, CancellationToken cancellationToken);
        Task<Order> GetOrderAsync(CancellationToken cancellationToken);
        Task SetOrderState(OrderState orderState, CancellationToken cancellationToken);
    }
}
