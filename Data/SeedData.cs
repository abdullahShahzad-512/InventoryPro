using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IMS.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {

            var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var superAdmin = await userManager.FindByEmailAsync("superadmin@gmail.com");
            if(superAdmin == null)
            {
                superAdmin = new IdentityUser
                {
                    UserName = "superadmin@gmail.com",
                    Email= "superadmin@gmail.com",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(superAdmin, "Admin@123");
                await userManager.AddClaimAsync(superAdmin, new Claim(ClaimTypes.Role, "Admin"));

                
            }
        }
    }
}
