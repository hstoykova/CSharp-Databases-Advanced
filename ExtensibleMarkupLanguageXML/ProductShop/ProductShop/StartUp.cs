using ProductShop.Data;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();

            //01
            //string usersXml = File.ReadAllText("../../../Datasets/users.xml");
            //Console.WriteLine(ImportUsers(context, usersXml));

            //02
            //string productsXml = File.ReadAllText("../../../Datasets/products.xml");
            //Console.WriteLine(ImportProducts(context, productsXml));

            //03
            //string categoriesXml = File.ReadAllText("../../../Datasets/categories.xml");
            //Console.WriteLine(ImportCategories(context, categoriesXml));

            //04  ImportCategoryProducts
            string categoryProductXml = File.ReadAllText("../../../Datasets/categories-products.xml");
            Console.WriteLine(ImportCategoryProducts(context, categoryProductXml));

        }
        //01
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UsersDto[]), new XmlRootAttribute("Users"));

            User[] users;

            using (var reader = new StringReader(inputXml))
            {
                users = ((UsersDto[])xmlSerializer.Deserialize(reader)).Select(dto => new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Age = dto.Age
                })
                .ToArray();

                context.Users.AddRange(users);
                context.SaveChanges();
            };

            return $"Successfully imported {users.Length}";
        }

        //02
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductsDto[]), new XmlRootAttribute("Products"));

            Product[] products;

            using (var reader = new StringReader(inputXml))
            {
                var userIds = context.Users.Select(u => u.Id).ToArray();

                products = ((ProductsDto[])xmlSerializer.Deserialize(reader))
                    .Where(p => userIds.Contains(p.BuyerId) && userIds.Contains(p.SellerId))
                    .Select(dto => new Product
                {
                   Name = dto.Name,
                   Price = dto.Price,
                   SellerId = dto.SellerId,
                   BuyerId = dto.BuyerId
                })
                .ToArray();

                context.Products.AddRange(products);
                context.SaveChanges();
            };

            return $"Successfully imported {products.Length}";
        }

        //03
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoriesDto[]), new XmlRootAttribute("Categories"));

            Category[] categories;
            
            using (var reader = new StringReader(inputXml))
            {
                categories = ((CategoriesDto[])xmlSerializer.Deserialize(reader))
                    .Where(c => c.Name != null)
                    .Select(dto => new Category
                    {
                        Name = dto.Name
                    })
                .ToArray();

                context.Categories.AddRange(categories);
                context.SaveChanges();
            };

            return $"Successfully imported {categories.Length}";
        }

        //04
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoriesAndProductsDto[]), new XmlRootAttribute("CategoryProducts"));

            CategoryProduct[] categoryProduct;

            using (var reader = new StringReader(inputXml))
            {
                var validCategoryIds = context.Categories
                    .Select(c => c.Id).ToArray();

                var validProductIds = context.Products
                    .Select(p => p.Id).ToArray();

                categoryProduct = ((CategoriesAndProductsDto[])xmlSerializer.Deserialize(reader))
                    .Where(cp => validCategoryIds.Contains(cp.CategoryId) && validProductIds.Contains(cp.ProductId))
                    .Select(dto => new CategoryProduct
                    {
                        CategoryId = dto.CategoryId,
                        ProductId = dto.ProductId
                    })
                .ToArray();

                context.CategoryProducts.AddRange(categoryProduct);
                context.SaveChanges();
            };

            return $"Successfully imported {categoryProduct.Length}";
        }

    }
}