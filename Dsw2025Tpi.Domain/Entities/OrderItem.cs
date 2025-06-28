using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class OrderItem : EntityBase
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public OrderItem() { }
    public OrderItem(int _quantity, decimal _unitPrice, Guid _productId)
    {
        Quantity = _quantity;
        UnitPrice = _unitPrice;
        SubTotal = Quantity * UnitPrice;
        ProductId = _productId;
    }
    public Guid ProductId { get; }
    public Guid OrderId { get; }
}
