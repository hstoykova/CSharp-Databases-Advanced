using Microsoft.Data.SqlClient.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();
            //01
            //string userText = File.ReadAllText("../../../Datasets/users.json");
            //Console.WriteLine(ImportUsers(context, userText));

            //02
            //string productText = File.ReadAllText("../../../Datasets/products.json");
            //Console.WriteLine(ImportProducts(context, productText));

            //03
            //string categoriesText = File.ReadAllText("../../../Datasets/categories.json");
            //Console.WriteLine(ImportCategories(context, categoriesText));

            //04
            //string categoriesProductText = File.ReadAllText("../../../Datasets/categories-products.json");
            //Console.WriteLine(ImportCategoryProducts(context, categoriesProductText));

            //05
            //Console.WriteLine(GetProductsInRange(context));

            //06
            //Console.WriteLine(GetSoldProducts(context));

            //07
            //Console.WriteLine(GetCategoriesByProductsCount(context));

            //08
            Console.WriteLine(GetUsersWithProducts(context));

        }

        //01
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<List<User>>(inputJson);
            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }

        //02
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<List<Product>>(inputJson);
            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        //03
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<List<Category>>(inputJson);
            categories.RemoveAll(c => c.Name == null);
            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count}";
        }

        //04
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var catProd = JsonConvert.DeserializeObject<List<CategoryProduct>>(inputJson);
            context.CategoriesProducts.AddRange(catProd);
            context.SaveChanges();

            return $"Successfully imported {catProd.Count}";
        }

        //05
        public static string GetProductsInRange(ProductShopContext context)
        {
            var product = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Select(p => new
                {
                    p.Name,
                    p.Price,
                    Seller = p.Seller.FirstName + " " + p.Seller.LastName
                }).ToArray();
            JsonSerializerSettings settings = JsonSettings();

            var json = JsonConvert.SerializeObject(product, settings);
            return json;
        }

        //06
        public static string GetSoldProducts(ProductShopContext context)
        {
            var soldProducts = context.Users
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(b => new
                {
                    b.FirstName,
                    b.LastName,

                    soldProducts = b.ProductsSold.Select(p => new
                    {
                        p.Name,
                        p.Price,
                        buyerFirstName = p.Buyer.FirstName,
                        buyerLastName = p.Buyer.LastName
                    })

                });

            var json = JsonConvert.SerializeObject(soldProducts, JsonSettings());
            return json;
        }

        //07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(c => c.CategoriesProducts.Count())
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoriesProducts.Count(),
                    averagePrice = c.CategoriesProducts.Select(p => p.Product.Price).Average().ToString("F2"),
                    totalRevenue = c.CategoriesProducts.Select(p => p.Product.Price).Sum().ToString("F2")
                });

            var json = JsonConvert.SerializeObject(categories, JsonSettings());
            return json;
        }

        //08
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            

            var users = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Age,

                    SoldProducts = new
                    {
                        count = u.ProductsSold
                        .Where(p => p.BuyerId != null)
                        .ToArray().Length,

                        products = u.ProductsSold
                        .Where(p => p.BuyerId != null)
                        .Select(p => new
                        {
                            p.Name,
                            p.Price
                        })
                        
                        .ToArray()
                    }
                    
                })
                .OrderByDescending(sp => sp.SoldProducts.products.Length)
                .ToArray();

            var usersCount = users.Length;

            var json = JsonConvert.SerializeObject(new { usersCount , users}, JsonSettings());
            return json;
        }

        private static JsonSerializerSettings JsonSettings()
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
        }
    }
}