using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Abstractions;
using Basket.Abstractions.Baskets;
using Basket.Abstractions.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace BasketApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(long id, CancellationToken cancellationToken)
        {
            try
            {
                var actorId = new ActorId($"{id:N}");
                var actor = ActorProxy.Create<IOrderActor>(actorId, new Uri("fabric:/Basket/OrderActorService"));
                var order = await actor.GetOrderAsync(cancellationToken);
                if (order == null)
                {
                    return NotFound();
                }

                return Ok(order);
            }
            catch (Exception e)
            {
                var response = Json(new
                {
                    e.Message
                });
                response.StatusCode = 500;
                return response;
            }
        }

        [HttpPut("{id}/orderState")]
        public async Task<ActionResult<OrderState>> UpdateOrderState(long id, [FromBody] OrderState orderState, CancellationToken cancellationToken)
        {
            var actorId = new ActorId($"{id:N}");
            var actor = ActorProxy.Create<IOrderActor>(actorId, new Uri("fabric:/Basket/OrderActorService"));
            try
            {
                await actor.SetOrderState(orderState, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound();
            }

            return Ok(orderState);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> Post([FromBody] int basketId)
        {
            try
            {
                var orderId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var actorId = new ActorId($"{orderId:N}");
                var actor = ActorProxy.Create<IOrderActor>(actorId, new Uri("fabric:/Basket/OrderActorService"));
                var order = await actor.CreateOrderAsync(basketId, CancellationToken.None);
                return CreatedAtAction(nameof(GetOrder), new { id = orderId }, order);
            }
            catch (Exception e)
            {
                var response = Json(new
                {
                    e.Message
                });
                response.StatusCode = 500;
                return response;
            }
        }
    }
}
