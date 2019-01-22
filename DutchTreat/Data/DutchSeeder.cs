using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DutchTreat.Data
{
    public class DutchSeeder
    {
        private readonly DutchContext _context;
        private readonly IHostingEnvironment _hosting;
        private readonly UserManager<StoreUser> userManager;

        public DutchSeeder(DutchContext context, IHostingEnvironment hosting, UserManager<StoreUser> userManager)
        {
            this._context = context;
            this._hosting = hosting;
            this.userManager = userManager;
        }

        public async Task SeedAsync()
        {
            _context.Database.EnsureCreated();

            StoreUser user = await userManager.FindByEmailAsync("test@dutchtreat.com");
            if (user == null)
            {
                user = new StoreUser()
                {
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "test@dutchtreat.com",
                    UserName = "test@dutchtreat.com"
                };
                var result = await userManager.CreateAsync(user, "P@ssw0rd!");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create new user in seeder.");
                }
            }

            if (!_context.Products.Any())
            {
                //Need to create sample data
                var filepath = Path.Combine(_hosting.ContentRootPath, "Data/art.json");
                var json = File.ReadAllText(filepath);
                var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(json);
                _context.Products.AddRange(products);

                _context.Orders.Add(new Order
                {
                    OrderNumber = "12345",
                    OrderDate = DateTime.UtcNow,
                    Id = 1
                });

                _context.SaveChanges();

                var order = _context.Orders.Where(o => o.Id == 1).FirstOrDefault();
                if (order != null)
                {
                    order.User = user;

                    order.Items = new List<OrderItem>()
                    {
                        new OrderItem()
                        {
                            Product = products.First(),
                            Quantity = 5,
                            UnitPrice = products.First().Price
                        }
                    };
                }

                _context.SaveChanges();

            }

        }
    }
}
