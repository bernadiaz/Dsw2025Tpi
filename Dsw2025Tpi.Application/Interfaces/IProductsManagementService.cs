using Dsw2025Ej15.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IProductsManagementService
{
    public Task<ProductModel.ProductResponse?> GetProductById(Guid _id);
    public Task<IEnumerable<ProductModel.ProductResponse>?> GetProducts();
    public Task<ProductModel.ProductResponse> AddProduct(ProductModel.ProductRequest _request);
    public Task<ProductModel.ProductResponse> DeleteProduct(Guid _id);
    public Task<ProductModel.ProductResponse> UpdateProduct(Guid _id, ProductModel.ProductRequest _request);
}
