using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NorthwindWebApi.Models;
using Northwind.Services;
using Northwind.Services.Blogging;
using Northwind.Services.Employees;
using Northwind.Services.Products;

namespace NorthwindWebApi.Controllers
{
    [Route("api/articles")]
    [ApiController]
    public class BlogArticlesController : ControllerBase
    {
        private readonly IBloggingService blogService;
        private readonly IEmployeeManagementService employeeService;
        private readonly IProductManagementService productService;

        public BlogArticlesController(IBloggingService blogService,
            IEmployeeManagementService employeeService,
            IProductManagementService productService)
        {
            this.blogService = blogService;
            this.employeeService = employeeService;
            this.productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlogArticle(BlogArticle blogArticle)
        {
            if (blogArticle == null)
            {
                return this.BadRequest();
            }

            var employeeExists = this.employeeService.TryGetEmployeeAsync((int)blogArticle.EmployeeId).Result.result;

            if (!employeeExists)
            {
                return this.NotFound("No such employee.");
            }

            await this.blogService.CreateBlogArticleAsync(blogArticle);

            var responsePayload = new BlogArticleCreatedModel
            {
                Title = blogArticle.Title,
                Text = blogArticle.Text,
                AuthorId = blogArticle.ArticleId,
            };

            return blogArticle is not null ? this.Ok(responsePayload) : this.BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBlogArticle(int id)
        {
            if (id <= 0)
            {
                return this.BadRequest();
            }

            var blogCreated = await this.blogService.DestroyBlogArticleAsync(id);

            return blogCreated ? this.Ok($"BlogArticle id={id} deleted.") : this.NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> ReadAllBlogArticles()
        {
            var blogs = this.blogService.GetBlogArticlesAsync();

            if (blogs is null)
            {
                return this.BadRequest();
            }

            if (!await blogs.AnyAsync())
            {
                return this.NotFound("No blogs found.");
            }

            var responsePayload = 
                from blog in blogs
                let employee = this.employeeService.TryGetEmployeeAsync((int)blog.EmployeeId).Result.employee
                select
                new BlogArticleReadAllModel
                {
                    ArticleId = blog.ArticleId,
                    Title = blog.Title,
                    PostedDate = blog.PublishDate,
                    AuthorId = blog.EmployeeId,
                    AuthorName = employee.FirstName + " " +
                    employee.LastName + ", " + employee.Title,
                };

            return this.Ok(responsePayload);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ReadBlogArticle(int id)
        {
            var (result, blog) = await this.blogService.TryGetBlogArticleAsync(id);

            if (!result)
            {
                return this.NotFound();
            }

            var (empExists, employee) = await this.employeeService.TryGetEmployeeAsync((int)blog.EmployeeId);

            if (!empExists)
            {
                return this.NotFound("No such employee.");
            }

            var readBlog = new BlogArticleReadOneModel
            {
                ArticleId = blog.ArticleId,
                Title = blog.Title,
                Posted = blog.PublishDate,
                AuthorId = blog.EmployeeId,
                AuthorName = employee.FirstName + " " +
                employee.LastName + ", " + employee.Title,
                Text = blog.Text,
            };

            return this.Ok(readBlog);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBlogArticle(int id, BlogArticle blog)
        {
            var updated = await this.blogService.UpdateBlogArticleAsync(id, blog);

            if (!updated)
            {
                this.NotFound("Article not found.");
            }

            var payload = new BlogArticleUpdatedModel
            {
                Title = blog.Title,
                Text = blog.Text,
                AuthorId = blog.EmployeeId
            };

            return this.Ok(payload);
        }

        [HttpGet("{articleId:int}/products")]
        public ActionResult<IAsyncEnumerable<Product>> ReadRelatedProducts(int articleId)
        {
            var articleProducts = this.blogService.GetBlogArticleProductsAsync(articleId);

            if (!articleProducts.AnyAsync().Result)
            {
                return this.NotFound();
            }

            var products = articleProducts.SelectAwait(async x => 
            (await this.productService.TryGetProductAsync(x.ProductId)).product);

            return this.Ok(products);
        }

        [HttpPost("{articleId:int}/products/{productId:int}")]
        public async Task<IActionResult> PostBlogArticleProduct(int articleId, int productId)
        {
            var created = await this.blogService.CreateBlogArticleProductAsync(articleId, productId);

            if (!created)
            {
                return this.NotFound();
            }

            return this.Ok("New link created.");
        }

        [HttpDelete("{articleId:int}/products/{productId:int}")]
        public async Task<IActionResult> DeleteBlogArticleProduct(int articleId, int productId)
        {
            return await this.blogService.DestroyBlogArticleProductAsync(articleId, productId) ?
                this.Ok("Link was removed.") : this.NotFound();
        }

        [HttpPost("{articleId:int}/comments")]
        public async Task<IActionResult> PostBlogComment(int articleId)
        {
            using var stream = this.HttpContext.Request.Body;
            var options = new JsonSerializerOptions();

            var body = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(stream, options);

            var text = body["text"].ToString();
            var customerId = int.Parse(body["customerId"].ToString());

            var result = await this.blogService.CreateCommentAsync(articleId, customerId, text);

            if (result == -1)
            {
                return this.NotFound();
            }

            return this.Ok("Comment was created.");
        }

        [HttpGet("{articleId:int}/comments")]
        public async Task<ActionResult<IAsyncEnumerable<BlogComment>>> ReadBlogCommentsByArticleId(int articleId)
        {
            var comments = this.blogService.ReadAllComments(articleId);

            if (!await comments.AnyAsync() || comments == null)
            {
                return this.NotFound();
            }

            return this.Ok(comments);
        }

        [HttpPut("{articleId:int}/comments/{customerId:int}")]
        public async Task<IActionResult> UpdateComment(int articleId, int customerId)
        {
            using var reader = new StreamReader(this.HttpContext.Request.Body);
            var text = await reader.ReadToEndAsync();

            var result = await this.blogService.UpdateCommentAsync(articleId, customerId, text);

            return result ? this.Ok("Comment had been changed.") : this.NotFound();
        }

        [HttpDelete("{articleId:int}/comments/{customerId:int}")]
        public async Task<IActionResult> DeleteComment(int articleId, int customerId)
        {
            var result = await this.blogService.DestroyCommentAsync(articleId, customerId);

            return result ? this.Ok("Comment had been deleted.") : this.NotFound();
        }
    }
}
