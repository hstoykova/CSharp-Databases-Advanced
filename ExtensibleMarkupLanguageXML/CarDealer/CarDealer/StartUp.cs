using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
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
    }
}