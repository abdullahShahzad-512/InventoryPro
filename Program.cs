using IMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IMS.Models.Interfaces;
using IMS.Models.Repositories;
using IMS.Models;
using System;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace IMS
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthorization(option => {

                option.AddPolicy("AdminAccess", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, "Admin");
                });
                option.AddPolicy("StaffAccess", policy =>
                {
                     policy.RequireClaim(ClaimTypes.Role, "Staff");
                });
                option.AddPolicy("AdminOrStaffAccess", policy =>
                {
                        policy.RequireClaim(ClaimTypes.Role,"Admin", "Staff");
                });
            });

            builder.Services.AddScoped<IProductRepository, ProductRepositoryDB>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepositoryDB>();
            builder.Services.AddScoped<IPurchaseRepository, PurchaseRepositoryDB>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepositoryDB>();
            builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepositoryDB>();
            builder.Services.AddScoped<ISaleRepository, SaleRepositoryDB>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepositoryDB>();

            var app = builder.Build();

          

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}");
            app.MapRazorPages();

            try
            {
                var seedTask = SeedData.InitializeAsync(app.Services);
                var finished = await Task.WhenAny(seedTask, Task.Delay(10000));
                if (finished != seedTask)
                {
                    Console.WriteLine("SeedData initialization timed out after 10s; continuing startup.");
                }
                else
                {
                    await seedTask;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SeedData initialization failed: {ex.Message}");
            }

            app.Run();

        }
    }
}
