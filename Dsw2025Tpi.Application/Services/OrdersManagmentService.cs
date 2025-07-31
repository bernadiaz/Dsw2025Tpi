using Dsw2025Ej15.Application.Dtos;
using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dsw2025Tpi.Application.Dtos.OrderItemModel;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagmentService : IOrdersManagmentService 
    {
        private readonly IRepository _repository;
        public OrdersManagmentService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest _request)
        {
            var _exist = await _repository.GetById<Customer>(_request.CustomerId);
            if (_exist == null)
                throw new EntityNotFoundException($"El cliente con Id {_request.CustomerId} no existe");

            if (string.IsNullOrWhiteSpace(_request.ShippingAddress) || string.IsNullOrWhiteSpace(_request.BillingAddress))
                throw new ArgumentException("Valores para el pedido no válidos");

            var _orderItemsResponses = new List<OrderItemResponse>();
            var _orderItems = new List<OrderItem>();

            foreach (var _item in _request.OrderItems)
            {
                var _product = await _repository.GetById<Product>(_item.ProductId);
                if (_product == null)
                    throw new EntityNotFoundException($"No se encontró el producto con ID {_item.ProductId}");

                if ((_item.Quantity < 0) ||
                    string.IsNullOrWhiteSpace(_item.Name) ||
                    string.IsNullOrWhiteSpace(_item.Description) ||
                    _item.UnitPrice <= 0)
                {
                    throw new ArgumentException("Valores para el pedido no válidos");
                }

                _product.ReduceStock(_item.Quantity);
                await _repository.Update(_product);

                var _orderItem = new OrderItem(_item.Quantity, _item.UnitPrice, _item.ProductId);
                _orderItems.Add(_orderItem);

                _orderItemsResponses.Add(new OrderItemResponse(
                    _item.ProductId,
                    _item.Quantity,
                    _product.Name!,
                    _product.Description!,
                    _product.CurrentUnitPrice
                ));
            }

            var _order = new Order(_request.CustomerId, _request.ShippingAddress, _request.BillingAddress, _orderItems);

            await _repository.Add(_order);

            return new OrderModel.OrderResponse(
                _order.Id,
                _order.CustomerId,
                _order.ShippingAddress,
                _order.BillingAddress,
                _orderItemsResponses,
                _order.TotalAmount
            );
        }

    }
}
