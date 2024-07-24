using Microsoft.EntityFrameworkCore;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml;
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
            //string categoryProductXml = File.ReadAllText("../../../Datasets/categories-products.xml");
            //Console.WriteLine(ImportCategoryProducts(context, categoryProductXml));

            // 05
            // Console.WriteLine(GetProductsInRange(dbContext));

            // 06
            // Console.WriteLine(GetSoldProducts(dbContext));

            // 07
            // Console.WriteLine(GetCategoriesByProductsCount(dbContext));

            // 08
            //Console.WriteLine(GetUsersWithProducts(dbContext));
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

        //05
        public static string GetProductsInRange(ProductShopContext context)
        {
            var productsInRange = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Take(10)
                .Select(p => new ExportProductsInRangeDto()
                {
                    Name = p.Name,
                    Price = p.Price,
                    BuyerFullName = p.BuyerId.HasValue
                        ? $"{p.Buyer.FirstName} {p.Buyer.LastName}"
                        : null
                })
                .ToArray();

            return SerializeToXml(productsInRange, "Products");
        }

        // 06. Export Sold Products
        public static string GetSoldProducts(ProductShopContext context)
        {
            ExportUserDto[] exportUsers = context.Users
                .Where(u => u.ProductsSold.Any())
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .Select(u => new ExportUserDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProductSold = u.ProductsSold.Select(p => new ExportProductsDto()
                    {
                        Name = p.Name,
                        Price = p.Price
                    }).ToArray()
                }).ToArray();

            return SerializeToXml(exportUsers, "Users");
        }

        // 07. Export Categories By Products Count
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categoriesByProductCount = context.Categories
                .Select(c => new ExportCategoriesByProductsCountDto()
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Count > 0
                        ? c.CategoryProducts.Average(c => c.Product.Price)
                        : 0,
                    TotalRevenue = c.CategoryProducts.Sum(p => p.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            return SerializeToXml(categoriesByProductCount, "Categories");
        }

        // 08. Export Users and Products
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersProducts = context.Users
                .Where(u => u.ProductsSold.Any())
                .OrderByDescending(u => u.ProductsSold.Count)
                .Take(10)
                .AsNoTracking()
                .Select(u => new ExportUsersWithProducts()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new ExportSoldProductsDto()
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold
                            .Select(p => new ExportProductsDto()
                            {
                                Name = p.Name,
                                Price = p.Price
                            })
                            .OrderByDescending(p => p.Price)
                            .ToArray()
                    }
                })
                .ToArray();

            ExportUsersProductsDto result = new ExportUsersProductsDto()
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                UsersWithProducts = usersProducts
            };

            return SerializeToXml(result, "Users");
        }

        public static string SerializeToXml<T>(T obj, string rootName, bool omitXmlDeclaration = false)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Object to serialize cannot be null.");

            if (string.IsNullOrEmpty(rootName))
                throw new ArgumentNullException(nameof(rootName), "Root name cannot be null or empty.");

            try
            {
                XmlRootAttribute xmlRoot = new(rootName);
                XmlSerializer xmlSerializer = new(typeof(T), xmlRoot);

                XmlSerializerNamespaces namespaces = new();
                namespaces.Add(string.Empty, string.Empty);

                XmlWriterSettings settings = new()
                {
                    OmitXmlDeclaration = omitXmlDeclaration,
                    Indent = true
                };

                StringBuilder sb = new();
                using var stringWriter = new StringWriter(sb);
                using var xmlWriter = XmlWriter.Create(stringWriter, settings);

                xmlSerializer.Serialize(xmlWriter, obj, namespaces);
                return sb.ToString().TrimEnd();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Serialization error: {ex.Message}");
                throw new InvalidOperationException($"Serializing {typeof(T)} failed.", ex);
            }

        }
    }
}