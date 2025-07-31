using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class Order : EntityBase
{
    public DateTime DateTime { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public Order() { }
    public Order(Guid _customerId, string? _shippingAddress, string? _billingAddress, List<OrderItem> _orderItems)
    {
        CustomerId = _customerId;
        ShippingAddress = _shippingAddress;
        BillingAddress = _billingAddress;
        OrderItems = _orderItems;
        DateTime = DateTime.UtcNow;
        TotalAmount = OrderItems.Sum(_item => _item.SubTotal);
        
    }
    public Guid CustomerId { get; }
}
