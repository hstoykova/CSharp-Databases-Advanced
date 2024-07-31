using Cadastre.Data;
using Cadastre.Data.Enumerations;
using Cadastre.DataProcessor.ExportDtos;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Cadastre.DataProcessor
{
    public class Serializer
    {
        public static string ExportPropertiesWithOwners(CadastreContext dbContext)
        {
            var earlierDate = dbContext.Properties.Select(d => d.DateOfAcquisition.ToString("dd/MM/yyyy"));

            var properties = dbContext.Properties
                 .Where(p => p.DateOfAcquisition >= DateTime.ParseExact("01/01/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture))
                 //.AsEnumerable()
                 .OrderByDescending(p => p.DateOfAcquisition)
                 .ThenBy(p => p.PropertyIdentifier)
                 .Select(p => new
                 {
                     p.PropertyIdentifier,
                     p.Area,
                     p.Address,
                     DateOfAcquisition = p.DateOfAcquisition.ToString("dd/MM/yyyy"),
                         Owners = p.PropertiesCitizens.Select(pc => new
                         {
                             pc.Citizen.LastName,
                             MaritalStatus =  pc.Citizen.MaritalStatus.ToString()
                         })
                         .OrderBy(pc => pc.LastName)
                         .ToArray()
                 })
                 .ToArray();

            return JsonConvert.SerializeObject(properties, Formatting.Indented);
        }

        public static string ExportFilteredPropertiesWithDistrict(CadastreContext dbContext)
        {
            PropertiesLargerThan100Dto[] properties = dbContext.Properties
                .Where(p => p.Area >= 100)
                .OrderByDescending(p => p.Area)
                .ThenBy(p => p.DateOfAcquisition)
                .Select(p => new PropertiesLargerThan100Dto()
                {
                    PostalCode = p.District.PostalCode,
                    PropertyIdentifier = p.PropertyIdentifier,
                    Area = p.Area,
                    DateOfAcquisition = p.DateOfAcquisition.ToString("dd/MM/yyyy")
                })
                .ToArray();

            return XmlHelper.XmlSerializationHelper.Serialize(properties, "Properties");
        }
    }
}
