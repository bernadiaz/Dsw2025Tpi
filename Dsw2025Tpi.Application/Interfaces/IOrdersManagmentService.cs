using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IOrdersManagmentService
{
    Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest request);
    Task<IEnumerable<OrderModel.OrderResponse>> GetOrders(string? status, Guid? customerId, int pageNumber, int pageSize);
    Task<OrderModel.OrderResponse?> GetOrderById(Guid id);
    Task<OrderModel.OrderResponse> UpdateOrderStatus(Guid id, string newStatus);
}

