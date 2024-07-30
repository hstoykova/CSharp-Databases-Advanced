namespace Cadastre.DataProcessor
{
    using Cadastre.Data;
    using Cadastre.Data.Enumerations;
    using Cadastre.Data.Models;
    using Cadastre.DataProcessor.ImportDtos;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage =
            "Invalid Data!";
        private const string SuccessfullyImportedDistrict =
            "Successfully imported district - {0} with {1} properties.";
        private const string SuccessfullyImportedCitizen =
            "Succefully imported citizen - {0} {1} with {2} properties.";

        public static string ImportDistricts(CadastreContext dbContext, string xmlDocument)
        {
            XmlRootAttribute root = new XmlRootAttribute("Districts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DistrictsDto[]), root);
            using StringReader reader = new StringReader(xmlDocument);
            var districtsDto = (DistrictsDto[])xmlSerializer.Deserialize(reader);


            StringBuilder sb = new StringBuilder();

            List<District> districts = new();

            foreach (var d in districtsDto)
            {
                if (!IsValid(d))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (dbContext.Districts.Any(n => n.Name == d.Name))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                District district = new District()
                {
                    Region = (Region)Enum.Parse(typeof(Region), d.Region),
                    Name = d.Name,
                    PostalCode = d.PostalCode
                };

                foreach (var propDto in d.Properties)
                {
                    if (!IsValid(propDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (dbContext.Properties.Any(pi => pi.PropertyIdentifier == propDto.PropertyIdentifier)
                        || district.Properties.Any(d => d.PropertyIdentifier == propDto.PropertyIdentifier))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (dbContext.Properties.Any(pa => pa.Address == propDto.Address)
                        || district.Properties.Any(s => s.Address == propDto.Address))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //DateTime dateOfAcquisition = DateTime.ParseExact(propDto.DateOfAcquisition, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    Property property = new Property()
                    {
                        PropertyIdentifier = propDto.PropertyIdentifier,
                        Area = propDto.Area,
                        Details = propDto.Details,
                        Address = propDto.Address,
                        DateOfAcquisition = DateTime.ParseExact(propDto.DateOfAcquisition, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
                    };

                    district.Properties.Add(property);
                }

                districts.Add(district);
                sb.AppendLine(string.Format(SuccessfullyImportedDistrict, district.Name, district.Properties.Count()));
            }

            dbContext.Districts.AddRange(districts);
            dbContext.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportCitizens(CadastreContext dbContext, string jsonDocument)
        {
            var citizensDto = JsonConvert.DeserializeObject<CitizensDto[]>(jsonDocument);

            StringBuilder sb = new StringBuilder();

            List<Citizen> citizens = new List<Citizen>();

            var existingPropertyIds = dbContext.Properties
                .Select(p => p.Id)
                .ToArray();

            foreach (var cit in citizensDto)
            {
                if (!IsValid(cit) || !Enum.TryParse(typeof(MaritalStatus), cit.MaritalStatus, true, out object result))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //if (cit.MaritalStatus != "Unmarried" || cit.MaritalStatus != "Married"
                //    || cit.MaritalStatus != "Divorced" || cit.MaritalStatus != "Widowed")
                //{
                //    sb.AppendLine(ErrorMessage);
                //    continue;
                //}

                Citizen citizen = new Citizen()
                {
                    FirstName = cit.FirstName,
                    LastName = cit.LastName,
                    BirthDate = DateTime.ParseExact(cit.BirthDate, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    MaritalStatus = (MaritalStatus)result,
                    //PropertiesCitizens = cit.Properties
                };

                foreach (int propertyId in cit.Properties.Distinct())
                {
                    if (existingPropertyIds.All(p => p != propertyId))
                    {
                        continue;
                    }

                    PropertyCitizen propertiesCitizens = new PropertyCitizen()
                    {
                        Citizen = citizen,
                        PropertyId = propertyId
                    };

                    citizen.PropertiesCitizens.Add(propertiesCitizens);
                }


                citizens.Add(citizen);
                sb.AppendLine(string.Format(SuccessfullyImportedCitizen, citizen.FirstName, citizen.LastName, citizen.PropertiesCitizens.Count()));
            }

            dbContext.AddRange(citizens);
            dbContext.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
