using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;

public class OrdersManagmentService : IOrdersManagmentService
{
    private readonly IRepository _repository;

    public OrdersManagmentService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest request)
    {
        var customer = await _repository.GetById<Customer>(request.CustomerId);
        if (customer == null)
            throw new EntityNotFoundException($"El cliente con ID {request.CustomerId} no existe.");

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) || string.IsNullOrWhiteSpace(request.BillingAddress))
            throw new ArgumentException("Dirección de envío o facturación inválida.");

        var orderItems = new List<OrderItem>();
        var orderItemResponses = new List<OrderModel.OrderItemResponse>();

        foreach (var item in request.OrderItems)
        {
            var product = await _repository.GetById<Product>(item.ProductId);
            if (product == null)
                throw new EntityNotFoundException($"Producto con ID {item.ProductId} no encontrado.");

            if (item.Quantity <= 0 || item.UnitPrice <= 0)
                throw new ArgumentException("Cantidad o precio inválido.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Stock insuficiente para el producto {product.Name}");

            product.ReduceStock(item.Quantity);
            await _repository.Update(product);

            var orderItem = new OrderItem(item.Quantity, item.UnitPrice, item.ProductId);
            orderItems.Add(orderItem);

            orderItemResponses.Add(new OrderModel.OrderItemResponse(
                item.ProductId,
                item.Quantity,
                product.Name!,
                product.Description!,
                product.CurrentUnitPrice
            ));
        }

        var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems);
        await _repository.Add(order);

        return new OrderModel.OrderResponse(
            order.Id,
            order.Status,
            order.CustomerId,
            order.ShippingAddress,
            order.BillingAddress,
            orderItemResponses,
            order.TotalAmount
        );
    }

    public async Task<IEnumerable<OrderModel.OrderResponse>> GetOrders(string? status, Guid? customerId, int pageNumber, int pageSize)
    {
        var allOrders = await _repository.GetAll<Order>("OrderItems.Product") ?? new List<Order>();
        var filtered = allOrders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            filtered = filtered.Where(o => o.Status == parsedStatus);
        }

        if (customerId.HasValue)
        {
            filtered = filtered.Where(o => o.CustomerId == customerId.Value);
        }

        var paged = filtered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return paged.Select(order => new OrderModel.OrderResponse(
            order.Id,
            order.Status,
            order.CustomerId,
            order.ShippingAddress!,
            order.BillingAddress!,
            order.OrderItems.Select(oi => new OrderModel.OrderItemResponse(
                oi.ProductId,
                oi.Quantity,
                oi.Product?.Name ?? "",
                oi.Product?.Description ?? "",
                oi.UnitPrice)).ToList(),
            order.TotalAmount
        ));
    }

    public async Task<OrderModel.OrderResponse?> GetOrderById(Guid id)
    {
        var order = await _repository.GetById<Order>(id, "OrderItems.Product");
        if (order == null)
            return null;

        return new OrderModel.OrderResponse(
            order.Id,
            order.Status,
            order.CustomerId,
            order.ShippingAddress!,
            order.BillingAddress!,
            order.OrderItems.Select(oi => new OrderModel.OrderItemResponse(
                oi.ProductId,
                oi.Quantity,
                oi.Product?.Name ?? "",
                oi.Product?.Description ?? "",
                oi.UnitPrice)).ToList(),
            order.TotalAmount
        );
    }

    public async Task<OrderModel.OrderResponse> UpdateOrderStatus(Guid id, string newStatus)
    {
        var order = await _repository.GetById<Order>(id, "OrderItems.Product");
        if (order == null)
            throw new EntityNotFoundException("Orden no encontrada.");

        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var statusParsed))
            throw new ArgumentException("Estado inválido.");

        order.Status = statusParsed;
        await _repository.Update(order);

        return new OrderModel.OrderResponse(
            order.Id,
            order.Status,
            order.CustomerId,
            order.ShippingAddress!,
            order.BillingAddress!,
            order.OrderItems.Select(oi => new OrderModel.OrderItemResponse(
                oi.ProductId,
                oi.Quantity,
                oi.Product?.Name ?? "",
                oi.Product?.Description ?? "",
                oi.UnitPrice)).ToList(),
            order.TotalAmount
        );
    }
}


