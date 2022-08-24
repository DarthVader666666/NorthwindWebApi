using Northwind.Services;
using Northwind.Services.Employees;
using Northwind.Services.Blogging;
using Northwind.Services.EntityFrameworkCore;
using Northwind.Services.EntityFrameworkCore.Context;
using Northwind.Services.EntityFrameworkCore.Blogging;
using Northwind.Services.EntityFrameworkCore.Blogging.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace NorthwindWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BloggingContext>(options => 
            options.UseSqlServer(this.Configuration.GetConnectionString("NorthwindBlogging")));

            services.AddScoped<IBloggingService, BloggingService>(serviceProvider =>
            new BloggingService(serviceProvider.GetService<BloggingContext>()));

            switch (this.Configuration["DataStorage"].ToUpper())
            {
                case "EF_SQL":
                    {
                        services.AddDbContext<NorthwindContext>(options =>
                        options.UseSqlServer(this.Configuration.GetConnectionString("Northwind")));

                        services.AddScoped<IProductManagementService, ProductManagementService>(
                            serviceProvider => new ProductManagementService(
                                serviceProvider.GetService<NorthwindContext>()));

                        services.AddScoped<IProductCategoriesManagementService, ProductCategoriesManagementService>(
                            serviceProvider => new ProductCategoriesManagementService(
                                serviceProvider.GetService<NorthwindContext>()));

                        services.AddScoped<IProductCategoryPicturesManagementService, ProductCategoryPicturesManagementService>(
                            serviceProvider => new ProductCategoryPicturesManagementService(
                                serviceProvider.GetService<NorthwindContext>()));

                        services.AddScoped<IEmployeeManagementService, EmployeeManagementService>(
                            serviceProvider => new EmployeeManagementService(
                                 serviceProvider.GetService<NorthwindContext>()));
                    };break;

            }

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
