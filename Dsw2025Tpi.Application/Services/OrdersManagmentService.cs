using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services;

public class OrdersManagmentService : IOrdersManagmentService
{
    private readonly IRepository _repository;
    private readonly ILogger<OrdersManagmentService> _logger;

    public OrdersManagmentService(IRepository repository, ILogger<OrdersManagmentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest request)
    {
        _logger.LogInformation("Creando orden para cliente de id : {id}", request.CustomerId);
        var customer = await _repository.GetById<Customer>(request.CustomerId);
        if (customer == null)
        {
            _logger.LogError("Cliente con ID {id} no encontrado", request.CustomerId);
            throw new EntityNotFoundException($"El cliente con ID {request.CustomerId} no existe.");
        }
            

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) || string.IsNullOrWhiteSpace(request.BillingAddress))
        {
            _logger.LogError("Dirección de envío o facturación inválida.");
            throw new ArgumentException("Dirección de envío o facturación inválida.");
        }
            

        if (request.OrderItems == null || !request.OrderItems.Any())
        {
            _logger.LogError("Orden sin ítems.");
            throw new InvalidOperationException("No se puede crear una orden sin ítems.");
        }
            

        var orderItems = new List<OrderItem>();
        var orderItemResponses = new List<OrderModel.OrderItemResponse>();

        foreach (var item in request.OrderItems)
        {
            var product = await _repository.GetById<Product>(item.ProductId);
            if (product == null)
            {
                _logger.LogError("Producto con ID {id} no encontrado", item.ProductId);
                throw new EntityNotFoundException($"Producto con ID {item.ProductId} no encontrado.");
            }

            if (item.Quantity <= 0)
            {
                _logger.LogError("Cantidad o precio inválido para el producto {productName}", product.Name);
                throw new ArgumentException("Cantidad o precio inválido.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                _logger.LogError("Stock insuficiente para el producto {productName}", product.Name);
                throw new InvalidOperationException($"Stock insuficiente para el producto {product.Name}");
            }

            product.ReduceStock(item.Quantity);
            await _repository.Update(product);

            var orderItem = new OrderItem(item.Quantity, product.CurrentUnitPrice, item.ProductId);
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

        _logger.LogInformation("Orden creada con ID {id}", order.Id);
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
        _logger.LogInformation("Consulta de todas las órdenes");
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
        _logger.LogInformation("Consulta de orden por ID: {id}", id);
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
        _logger.LogInformation("Actualizando estado de la orden con ID: {id} a {status}", id, newStatus);
        var order = await _repository.GetById<Order>(id, "OrderItems.Product");
        if (order == null)
        {
            _logger.LogError("Orden con ID {id} no encontrada", id);
            throw new EntityNotFoundException("Orden no encontrada.");
        }

        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var statusParsed))
        {
            _logger.LogError("Estado inválido: {status}", newStatus);
            throw new ArgumentException("Estado inválido.");
        }

        order.Status = statusParsed;
        await _repository.Update(order);
        _logger.LogInformation("Estado de la orden con ID {id} actualizado a {status}", id, statusParsed);

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


