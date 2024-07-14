using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Castle.Core.Resource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.IO;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();

            //09
            //string suppliers = File.ReadAllText("../../../Datasets/suppliers.json");
            //Console.WriteLine(ImportSuppliers(context, suppliers));

            //10
            //string parts = File.ReadAllText("../../../Datasets/parts.json");
            //Console.WriteLine(ImportParts(context, parts));

            //11
            //string cars = File.ReadAllText("../../../Datasets/cars.json");
            //Console.WriteLine(ImportCars(context, cars));

            //12
            //string customers = File.ReadAllText("../../../Datasets/customers.json");
            //Console.WriteLine(ImportCustomers(context, customers));

            //13
            //string sales = File.ReadAllText("../../../Datasets/sales.json");
            //Console.WriteLine(ImportSales(context, sales));

            //14
            //Console.WriteLine(GetOrderedCustomers(context));

            //15
            //Console.WriteLine(GetCarsFromMakeToyota(context));

            //16
            //Console.WriteLine(GetLocalSuppliers(context));

            //17
            //Console.WriteLine(GetCarsWithTheirListOfParts(context));

            //18
            //Console.WriteLine(GetTotalSalesByCustomer(context));

            //19
            Console.WriteLine(GetSalesWithAppliedDiscount(context));

        }

        //09
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var suppliers = JsonConvert.DeserializeObject<List<Supplier>>(inputJson);
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}.";
        }

        //10
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var parts = JsonConvert.DeserializeObject<List<Part>>(inputJson);

            var validId = context.Suppliers.Select(x => x.Id).ToArray();

            var partsWithValidId = parts.Where(p => validId.Contains(p.SupplierId)).ToArray();

            context.Parts.AddRange(partsWithValidId);
            context.SaveChanges();

            return $"Successfully imported {partsWithValidId.Length}.";
        }

        //11
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var carDtos = JsonConvert.DeserializeObject<ImportCarDto[]>(inputJson);
            var partsCar = new HashSet<PartCar>();
            var cars = new HashSet<Car>();

            foreach (var dto in carDtos)
            {
                Car car = new Car()
                {
                    Make = dto.Make,
                    Model = dto.Model,
                    TraveledDistance = dto.TravelledDistance
                };

                cars.Add(car);

                foreach (var partId in dto.PartsId.Distinct()) //used distinct because of duplication part ids
                {
                    partsCar.Add(new PartCar()
                    {
                        Car = car,
                        PartId = partId
                    });
                }

            }

            context.Cars.AddRange(cars);
            context.PartsCars.AddRange(partsCar);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}.";
        }

        //12
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var customers = JsonConvert.DeserializeObject<List<Customer>>(inputJson);
            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }

        //13
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var sales = JsonConvert.DeserializeObject<List<Sale>>(inputJson);
            context.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}.";
        }

        //14
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customer = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new
                {
                    c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    c.IsYoungDriver
                });

            return JsonConvert.SerializeObject(customer, JsonSettings());            
        }

        //15
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var toyota = context.Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .Select(c => new
                {
                    c.Id,
                    c.Make,
                    c.Model,
                    c.TraveledDistance
                });

            return JsonConvert.SerializeObject(toyota, JsonSettings());
        }

        //16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => !s.IsImporter)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    PartsCount = s.Parts.ToArray().Length
                });

            return JsonConvert.SerializeObject(suppliers, JsonSettings());
        }

        //17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars2 = context.Cars
                .Select(s => new
                {
                    car = new
                    {
                        s.Make,
                        s.Model,
                        s.TraveledDistance
                    },

                    parts = s.PartsCars.Select(pc => new
                    {
                        pc.Part.Name,
                        Price = pc.Part.Price.ToString("F2")
                    })
                });

            return JsonConvert.SerializeObject(cars2, JsonSettings());

        }

        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var totalSales = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.ToArray().Length,
                    spentMoney = c.Sales.SelectMany(s => s.Car.PartsCars)
                        .Sum(pc => pc.Part.Price)
                })
                .OrderByDescending(r => r.spentMoney)
                .ThenByDescending(t => t.boughtCars);

            return JsonConvert.SerializeObject(totalSales, JsonSettings());
        }

        //19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Take(10)
                .Select(s => new
                {
                    car = new
                    {
                        s.Car.Make,
                        s.Car.Model,
                        s.Car.TraveledDistance
                    },

                    customerName = s.Customer.Name,
                    discount = s.Discount.ToString("F2"),
                    price = s.Car.PartsCars.Sum(pc => pc.Part.Price).ToString("F2"),
                    priceWithDiscount = (s.Car.PartsCars.Sum(pc => pc.Part.Price) * (1 - s.Discount/100)).ToString("F2")

                });

            return JsonConvert.SerializeObject(sales, JsonSettings());
        }


        public static JsonSerializerSettings JsonSettings()
        {
            return new JsonSerializerSettings()
            {
                //NullValueHandling = NullValueHandling.Ignore,
                //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
        }
    }
}
