using Dsw2025Ej15.Application.Dtos;
using Dsw2025Ej15.Application.Exceptions;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Application.Interfaces;

namespace Dsw2025Ej15.Application.Services;

public class ProductsManagementService : IProductsManagementService
{
    private readonly IRepository _repository;

    public ProductsManagementService(IRepository repository)
    {
        _repository = repository;
    }
    public async Task<ProductModel.ProductResponse?> GetProductById(Guid _id)
    {
        var _product = await _repository.GetById<Product>(_id);
        return _product != null ?
            new ProductModel.ProductResponse(_product.Id, _product.Sku!, _product.InternalCode!,
            _product.Name!, _product.Description!, _product.CurrentUnitPrice,
            _product.StockQuantity, _product.IsActive) : null;
    }
    public async Task<IEnumerable<ProductModel.ProductResponse>?> GetProducts()
    {
        var _products = await _repository.GetFiltered<Product>(_p => _p.IsActive);
        return _products?.Select(_p => new ProductModel.ProductResponse(_p.Id, _p.Sku!, _p.InternalCode!,
                _p.Name!, _p.Description!, _p.CurrentUnitPrice, _p.StockQuantity, _p.IsActive));
    }

    public async Task<ProductModel.ProductResponse> AddProduct(ProductModel.ProductRequest _request)
    {
        if (string.IsNullOrWhiteSpace(_request.Sku) || string.IsNullOrWhiteSpace(_request.Name) ||
            string.IsNullOrWhiteSpace(_request.Description) || string.IsNullOrWhiteSpace(_request.InternalCode)
            || _request.CurrentUnitPrice <= 0 || _request.StockQuantity <0)
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }

        var _exist = await _repository.First<Product>(_p => _p.Sku == _request.Sku);
        if (_exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {_request.Sku}");
        var _product = new Product(_request.Sku, _request.InternalCode, _request.Name,
            _request.Description, _request.CurrentUnitPrice, _request.StockQuantity);
        await _repository.Add(_product);
        return new ProductModel.ProductResponse(_product.Id, _product.Sku!, _product.InternalCode!,
            _product.Name!, _product.Description!, _product.CurrentUnitPrice,
            _product.StockQuantity, _product.IsActive);
    }
    public async Task<ProductModel.ProductResponse> DeleteProduct(Guid _id)
    {
        var _product = await _repository.GetById<Product>(_id);
        if (_product == null) throw new EntityNotFoundException($"El producto con Id {_id} no existe");
        _product.IsActive = false;
        await _repository.Update(_product);
        return new ProductModel.ProductResponse(_product.Id, _product.Sku!, _product.InternalCode!,
            _product.Name!, _product.Description!, _product.CurrentUnitPrice,
            _product.StockQuantity, _product.IsActive);
    }
    public async Task<ProductModel.ProductResponse> UpdateProduct(Guid _id, ProductModel.ProductRequest _request)
    {
        var _product = await _repository.GetById<Product>(_id);
        if (_product == null) throw new EntityNotFoundException($"El producto con Id {_id} no existe");
        if (string.IsNullOrWhiteSpace(_request.Sku) || string.IsNullOrWhiteSpace(_request.Name) ||
            string.IsNullOrWhiteSpace(_request.Description) || string.IsNullOrWhiteSpace(_request.InternalCode)
            || _request.CurrentUnitPrice <= 0 || _request.StockQuantity < 0)
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }
        var _exist = await _repository.First<Product>(_p => _p.Sku == _request.Sku);
        if (_exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {_request.Sku}");
        _product.Sku = _request.Sku;
        _product.InternalCode = _request.InternalCode;
        _product.Name = _request.Name;
        _product.Description = _request.Description;
        _product.CurrentUnitPrice = _request.CurrentUnitPrice;
        _product.StockQuantity = _request.StockQuantity;
        await _repository.Update(_product);
        return new ProductModel.ProductResponse(_product.Id, _product.Sku!, _product.InternalCode!,
            _product.Name!, _product.Description!, _product.CurrentUnitPrice,
            _product.StockQuantity, _product.IsActive);
    }
}
