using Foodie1.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Infrastucture.Extentions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseDatabaseMigration(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var roleManeger = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                var userManeger = serviceScope.ServiceProvider.GetService<UserManager<User>>();
                Task.Run(async () =>
                {
                    var restaurantRole = GlobalConstants.Restaurant;
                    var userRole = GlobalConstants.User;
                    var administratorRole = GlobalConstants.Administrator;
                    var waiterRole = "Waiter";
                    var chefRole = "Chef";

                    var restaurantRoleExist = await roleManeger.RoleExistsAsync(restaurantRole);
                    var userRoleExist = await roleManeger.RoleExistsAsync(userRole);
                    var administratorRoleExist = await roleManeger.RoleExistsAsync(administratorRole);
                    var waiterRoleExist = await roleManeger.RoleExistsAsync(userRole);
                    var chefRoleExist = await roleManeger.RoleExistsAsync(administratorRole);


                    if (!waiterRoleExist)
                    {
                        await roleManeger.CreateAsync(new IdentityRole { Name = waiterRole });
                    }
                    if (!chefRoleExist)
                    {
                        await roleManeger.CreateAsync(new IdentityRole { Name = chefRole });
                    }
                    if (!restaurantRoleExist)
                    {
                        await roleManeger.CreateAsync(new IdentityRole { Name = restaurantRole });
                    }
                    if (!userRoleExist)
                    {
                        await roleManeger.CreateAsync(new IdentityRole { Name = userRole });
                    }
                    if (!administratorRoleExist)
                    {
                        await roleManeger.CreateAsync(new IdentityRole { Name = administratorRole });
                    }

                    var administratorExist = await userManeger.FindByNameAsync(administratorRole);

                    if (administratorExist == null)
                    {
                        var newUser = new User { Email = "administrator@abv.bg", UserName = "administrator@abv.bg" };

                        await userManeger.CreateAsync(newUser, "34523452Aa");

                        await userManeger.AddToRoleAsync(newUser, administratorRole);
                    }

                }).Wait();



            }

            return app;
        }
    }
}
