using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Northwind.Services;
using Northwind.Services.Products;

namespace NorthwindWebApi.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private const string OleHeader = "15-1C-2F-00-02-00-00-00-0D-00-0E-00-14-00-21-00-FF-FF-FF-FF-42-69-74-6D-61-70-20-49-6D-61-67-65-00-50-61-69-6E-74-2E-50-69-63-74-75-72-65-00-01-05-00-00-02-00-00-00-07-00-00-00-50-42-72-75-73-68-00-00-00-00-00-00-00-00-00-A0-29-00-00";
        private readonly IProductCategoriesManagementService categoryService;
        private readonly IProductCategoryPicturesManagementService pictureService;

        public ProductCategoriesController(IProductCategoriesManagementService categoryService,
            IProductCategoryPicturesManagementService pictureService)
        {
            this.categoryService = categoryService;
            this.pictureService = pictureService;
        }

        [HttpGet("total")]
        public async Task<ActionResult<int>> ReadCategoriesAmount()
        {
            return await this.categoryService.GetCategoriesAmountAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductCategory>> ReadCategory(int id)
        {
            var task = await this.categoryService.TryGetCategoryAsync(id);

            return task.result ? this.Ok(task.category) : this.NotFound();
        }

        [HttpGet]
        public ActionResult<IAsyncEnumerable<ProductCategory>> ReadCategories(
            [FromQuery(Name = "offset")] int offset, [FromQuery(Name = "limit")] int limit)
        {
            var categories = this.categoryService.GetCategoriesAsync(offset, limit);
            return categories is not null ? this.Ok(categories) : this.BadRequest();
        }

        [HttpGet("by_name")]
        public ActionResult<IAsyncEnumerable<ProductCategory>> ReadCategories(List<string> names)
        {
            if (names is null || !names.Any())
            {
                return this.BadRequest();
            }

            var result = this.categoryService.LookupCategoriesByNameAsync(names);

            if (result is null || !result.AnyAsync().Result)
            {
                return this.NotFound("No categories with such names found.");
            }

            return this.Ok(result);
        }

        [HttpGet("{categoryId:int}/picture")]
        public async Task<IActionResult> ReadCategoryImage(int categoryId)
        {
            var (result, bytes) = await this.pictureService.TryGetPictureAsync(categoryId);

            if (!result)
            {
                return this.NotFound();
            }

            byte[] header = new byte[78];
            Array.Copy(bytes, header, header.Length);

            if (BitConverter.ToString(header) == OleHeader)
            {
                var newBytes = new byte[bytes.Length - header.Length];
                Array.Copy(bytes, 78, newBytes, 0, newBytes.Length);
                bytes = newBytes;
            }

            return this.Ok(new MemoryStream(bytes));
        }

        [HttpPost]
        public async Task<ActionResult<ProductCategory>> CreateCategory(ProductCategory category)
        {
            if (category is null)
            {
                return this.BadRequest();
            }

            var id = await this.categoryService.CreateCategoryAsync(category);
            return category is not null ? this.Ok($"New category created Id = {id}") : this.BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, ProductCategory category)
        {
            var result = await this.categoryService.UpdateCategoriesAsync(id, category);
            return result ? this.NoContent() : this.NotFound($"Category with id = {id} not found.");
        }

        [HttpPut("{categoryId:int}/picture")]
        public async Task<IActionResult> UpdateCategoryImage(int categoryId)
        {
            var stream = this.HttpContext.Request.Form.Files[0].OpenReadStream();
            var result = await this.pictureService.UpdatePictureAsync(categoryId, stream);
            stream.Close();

            if (result)
            {
                return this.NoContent();
            }
            else
            {
                return this.NotFound();
            }
        }

        [HttpDelete("{categoryId:int}/picture")]
        public async Task<IActionResult> DeleteCategoryImage(int categoryId)
        {
            var result = await this.pictureService.DestroyPictureAsync(categoryId);

            if (result)
            {
                return this.NoContent();
            }
            else
            {
                return this.NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (id <= 0)
            {
                return this.NotFound($"Category Id = {id} NotFound.");
            }

            var result = await this.categoryService.DestroyCategoryAsync(id);

            return result ? this.NoContent() : this.StatusCode(500, $"Couldn't delete category id={id}");
        }
    }
}
