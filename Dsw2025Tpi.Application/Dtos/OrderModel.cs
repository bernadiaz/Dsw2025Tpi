using Dsw2025Tpi.Domain.Entities;
using static Dsw2025Tpi.Application.Dtos.OrderItemModel;

namespace Dsw2025Tpi.Application.Dtos;

public static class OrderModel
{
    public record OrderRequest(
        Guid CustomerId,
        string? ShippingAddress,
        string? BillingAddress,
        List<OrderItemRequest> OrderItems
    );

    public record OrderItemRequest(
        Guid ProductId,
        int Quantity,
        string Name,
        string Description,
        decimal UnitPrice
    );

    public record OrderItemResponse(
        Guid ProductId,
        int Quantity,
        string Name,
        string Description,
        decimal UnitPrice
    );

    public record OrderResponse(
        Guid OrderId,
        Guid CustomerId,
        string? ShippingAddress,
        string? BillingAddress,
        List<OrderItemResponse> OrderItems,
        decimal TotalAmount
    );

    public record StatusUpdateRequest(
        string NewStatus
    );
}

