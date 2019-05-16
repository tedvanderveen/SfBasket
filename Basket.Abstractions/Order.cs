using System;
using System.Collections.Generic;

namespace Basket.Abstractions
{
    [Serializable]
    public class Order
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public OrderState OrderState { get; set; }
    }
}