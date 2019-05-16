using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Basket.Abstractions;
using Basket.Abstractions.Baskets;
using Basket.Abstractions.Orders;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;

namespace OrderActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class OrderActor : Actor, IOrderActor
    {
        private readonly ActorId actorId;
        private static readonly string StateName = "Order";

        /// <summary>
        /// Initializes a new instance of OrderActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public OrderActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            this.actorId = actorId;
        }

        public async Task<Order> CreateOrderAsync(int basketId, CancellationToken cancellationToken)
        {
            var state = await StateManager.TryGetStateAsync<Order>(StateName, cancellationToken);
            if (state.HasValue)
            {
                throw new InvalidOperationException("Order was already created.");
            }

            var basketActorId = new ActorId($"{basketId:N}");
            var actor = ActorProxy.Create<IBasketActor>(basketActorId, new Uri("fabric:/Basket/BasketActorService"));
            var basket = await actor.GetProductsInBasket(cancellationToken);

            var order = new Order
            {
                OrderState = OrderState.New,
                Products = basket.ToList()
            };

            state = new ConditionalValue<Order>(true, order);

            await StateManager.SetStateAsync(StateName, state.Value, cancellationToken);
            await StateManager.SaveStateAsync(cancellationToken);

            return order;
        }

        public async Task<Order> GetOrderAsync(CancellationToken cancellationToken)
        {
            var state = await StateManager.TryGetStateAsync<Order>(StateName, cancellationToken);
            if (state.HasValue == false)
            {
                return null;
            }
            return state.Value;
        }

        public async Task SetOrderState(OrderState orderState, CancellationToken cancellationToken)
        {
            var state = await StateManager.TryGetStateAsync<Order>(StateName, cancellationToken);
            if (state.HasValue == false)
            {
                throw new InvalidOperationException("Order not found.");
            }

            state.Value.OrderState = orderState;
            await StateManager.SetStateAsync(StateName, state.Value, cancellationToken);
            await StateManager.SaveStateAsync(cancellationToken);
        }
    }
}
