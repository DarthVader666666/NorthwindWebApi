using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Northwind.Services;
using Northwind.Services.Products;

namespace NorthwindWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductManagementService service;

        public ProductsController(IProductManagementService service)
        {
            this.service = service;
        }

        [HttpGet("total")]
        public async Task<ActionResult<int>> ReadAmountOfProducts()
        {
            return await this.service.GetProductsCountAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> ReadProduct(int id)
        {
            var task = await this.service.TryGetProductAsync(id);
            return task.result ? this.Ok(task.product) : this.NotFound();
        }

        [HttpGet]
        public ActionResult<IAsyncEnumerable<Product>> ReadProducts(
            [FromQuery(Name = "offset")] int offset, [FromQuery(Name = "limit")] int limit)
        {
            var categories = this.service.GetProductsAsync(offset, limit);
            return categories is not null ? this.Ok(categories) : this.BadRequest();
        }

        [HttpGet("by_categoryId/{categoryId:int}")]
        public ActionResult<IAsyncEnumerable<Product>> ReadProducts(int categoryId)
        {
            var categories = this.service.GetProductsForCategoryAsync(categoryId);
            return categories is not null ? this.Ok(categories) : this.BadRequest();
        }

        [HttpGet("by_name")]
        public ActionResult<IAsyncEnumerable<Product>> ReadProducts(List<string> names)
        {
            if (names is null || !names.Any())
            {
                return this.BadRequest();
            }

            var result = this.service.LookupProductsByNameAsync(names);

            if (result is null || !result.AnyAsync().Result)
            {
                return this.NotFound("No categories with such names found.");
            }

            return this.Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (product is null)
            {
                return this.BadRequest();
            }

            var id = await this.service.CreateProductAsync(product);
            return product is not null ? this.Ok($"New product created Id = {id}") : this.BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            var result = await this.service.UpdateProductAsync(id, product);
            return result ? this.NoContent() : this.NotFound($"product with id = {id} not found.");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return this.NotFound($"product Id = {id} NotFound.");
            }

            var result = await this.service.DestroyProductAsync(id);

            return result ? this.NoContent() : this.NotFound($"product Id = {id} NotFound.");
        }
    }
}
