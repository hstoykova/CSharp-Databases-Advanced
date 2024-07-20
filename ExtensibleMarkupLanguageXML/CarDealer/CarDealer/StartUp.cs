using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();

            //09
            //string suppliersXml = File.ReadAllText("../../../Datasets/suppliers.xml");
            //Console.WriteLine(ImportSuppliers(context, suppliersXml));

            //10
            //string partsXml = File.ReadAllText("../../../Datasets/parts.xml");
            //Console.WriteLine(ImportParts(context, partsXml));

            //11
            //string carsXml = File.ReadAllText("../../../Datasets/cars.xml");
            //Console.WriteLine(ImportCars(context, carsXml));

            //12
            //string customersXml = File.ReadAllText("../../../Datasets/customers.xml");
            //Console.WriteLine(ImportCustomers(context, customersXml));

            //13
            //string salesXml = File.ReadAllText("../../../Datasets/sales.xml");
            //Console.WriteLine(ImportSales(context, salesXml));

            //14
            //Console.WriteLine(GetCarsWithDistance(context));

            //15
            //Console.WriteLine(GetCarsFromMakeBmw(context));

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
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SuppliersDto[]), new XmlRootAttribute("Suppliers"));

            Supplier[] suppliers;

            using (var reader = new StringReader(inputXml))
            {
                suppliers = ((SuppliersDto[])xmlSerializer.Deserialize(reader)).Select(dto => new Supplier
                {
                    Name = dto.Name,
                    IsImporter = dto.IsImporter
                })
                .ToArray();

                context.Suppliers.AddRange(suppliers);
                context.SaveChanges();
            };

            return $"Successfully imported {suppliers.Length}";
        }

        //10
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PartsDto[]), new XmlRootAttribute("Parts"));

            Part[] parts;

            using (var reader = new StringReader(inputXml))
            {
                parts = ((PartsDto[])xmlSerializer.Deserialize(reader))
                    .Where(p => context.Suppliers.Where(c => c.Id == p.SupplierId).Any())
                    .Select(dto => new Part
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        Quantity = dto.Quantity,
                        SupplierId = dto.SupplierId
                    })
                .ToArray();

                context.Parts.AddRange(parts);
                context.SaveChanges();
            };

            return $"Successfully imported {parts.Length}";
        }

        //11
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CarsDto[]), new XmlRootAttribute("Cars"));

            CarsDto[] carsDto;

            using (var reader = new StringReader(inputXml))
            {
                carsDto = (CarsDto[])xmlSerializer.Deserialize(reader);
            };

            List<Car> cars = new();

            foreach (var dto in carsDto)
            {
                Car car = new Car()
                {
                    Make = dto.Make,
                    Model = dto.Model,
                    TraveledDistance = dto.TraveledDistance
                };

                int[] carPartIds = dto.partIds
                    .Select(p => p.Id)
                    .Distinct()
                    .ToArray();

                var carParts = new List<PartCar>();

                foreach (var id in carPartIds)
                {
                    carParts.Add(new PartCar()
                    {
                        Car = car,
                        PartId = id
                    });
                }

                car.PartsCars = carParts;
                cars.Add(car);
            }

            context.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        //12
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomersDto[]), new XmlRootAttribute("Customers"));

            Customer[] customers;

            using (var reader = new StringReader(inputXml))
            {
                customers = ((CustomersDto[])xmlSerializer.Deserialize(reader)).Select(dto => new Customer
                {
                    Name = dto.Name,
                    BirthDate = dto.BirthDate,
                    IsYoungDriver = dto.IsYoungDriver
                })
                .ToArray();

                context.Customers.AddRange(customers);
                context.SaveChanges();
            };

            return $"Successfully imported {customers.Length}";
        }

        //13
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SalesDto[]), new XmlRootAttribute("Sales"));

            Sale[] sales;

            using (var reader = new StringReader(inputXml))
            {
                var carIds = context.Cars.Select(c => c.Id).Distinct().ToArray();

                sales = ((SalesDto[])xmlSerializer.Deserialize(reader))
                    .Where(c => carIds.Contains(c.CarId))
                    .Select(dto => new Sale
                    {
                        CarId = dto.CarId,
                        CustomerId = dto.CustomerId,
                        Discount = dto.Discount
                    })
                .ToArray();

                context.Sales.AddRange(sales);
                context.SaveChanges();
            };

            return $"Successfully imported {sales.Length}";
        }

        //14
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            CarsWithDistanceDto[] carsWithDistanceDtos = context.Cars
                .Select(dto => new CarsWithDistanceDto()
                {
                    Make = dto.Make,
                    Model = dto.Model,
                    TraveledDistance = dto.TraveledDistance
                })
                .OrderBy(dto => dto.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .ToArray();

            return SerializeToXml(carsWithDistanceDtos, "cars");
        }

        //15
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            CarsWithMakeDto[] carsWithMake = context.Cars
                .Where(c => c.Make == "BMW")
                .Select(c => new CarsWithMakeDto()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                })
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .ToArray();

            return SerializeToXml(carsWithMake, "cars", true);
        }

        //16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var isImporter = context.Suppliers.Where(s => s.IsImporter).Select(s => s.Id).ToArray();

            LocalSuppliersDto[] localSuppliers = context.Suppliers
                .Where(s => isImporter.Contains(s.Id) == false)
                .Select(c => new LocalSuppliersDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                    PartsCount = c.Parts.Count
                })
                .ToArray();

            return SerializeToXml(localSuppliers, "suppliers");
        }

        //17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            CarsWithTheirListOfPartsDto[] carsWithParts = context.Cars
                .Select(c => new CarsWithTheirListOfPartsDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                    parts = c.PartsCars.Select(p => new ListOfPartsDto()
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToList()
                })
                .OrderByDescending(c => c.TraveledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .ToArray();

            return SerializeToXml(carsWithParts, "cars");
        }

        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var temp = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    FullName = c.Name,
                    BoughtCars = c.Sales.Count,
                    SalesInfo = c.Sales.Select(s => new
                    {
                        Prices = c.IsYoungDriver
                        ? s.Car.PartsCars.Sum(pc => Math.Round((double)pc.Part.Price * 0.95, 2))
                        : s.Car.PartsCars.Sum(pc => (double)pc.Part.Price)
                    }).ToArray()
                }).ToArray();

            var customerSalesInfo = temp
                .OrderByDescending(x =>
                    x.SalesInfo.Sum(y => y.Prices))
                .Select(a => new TotalSalesByCustomerDto()
                {
                    Name = a.FullName,
                    BoughtCars = a.BoughtCars,
                    SpentMoney = a.SalesInfo.Sum(b => b.Prices).ToString("F2")
                })
                .ToArray();

            return SerializeToXml(customerSalesInfo, "customers");
        }

        //19
        // SalesWithAppliedDiscountDto
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            SalesWithAppliedDiscountDto[] salesWithDiscount = context.Sales
                .Select(s => new SalesWithAppliedDiscountDto() {
                    Car = new CarsWithDistanceDto()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },
                    Discount = (int)s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartsCars.Sum(pc => pc.Part.Price),
                    PriceWithDiscount = Math.Round((double)(s.Car.PartsCars.Sum(pc => pc.Part.Price) * (1 - s.Discount / 100)), 4)
                })
                .ToArray();
            return SerializeToXml(salesWithDiscount, "sales");
        }

        //XML export template
        private static string SerializeToXml<T>(T dto, string xmlRootAttribute, bool omitDeclaration = false)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootAttribute));
            StringBuilder stringBuilder = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = omitDeclaration,
                Encoding = new UTF8Encoding(false),
                Indent = true
            };

            using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add(string.Empty, string.Empty);

                try
                {
                    xmlSerializer.Serialize(xmlWriter, dto, xmlSerializerNamespaces);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return stringBuilder.ToString();
        }

    }
}