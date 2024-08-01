namespace Medicines.DataProcessor
{
    using Medicines.Data;
    using Medicines.Data.Models;
    using Medicines.Data.Models.Enums;
    using Medicines.DataProcessor.ImportDtos;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data!";
        private const string SuccessfullyImportedPharmacy = "Successfully imported pharmacy - {0} with {1} medicines.";
        private const string SuccessfullyImportedPatient = "Successfully imported patient - {0} with {1} medicines.";

        public static string ImportPatients(MedicinesContext context, string jsonString)
        {
            var patientsDto = JsonConvert.DeserializeObject<PatientsDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Patient> patients = new List<Patient>();

            foreach (var patDto in patientsDto)
            {
                if (!IsValid(patDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Patient patient = new Patient() 
                { 
                    FullName = patDto.FullName,
                    AgeGroup = (AgeGroup)patDto.AgeGroup,
                    Gender = (Gender)patDto.AgeGroup
                };

                foreach (var medId in patDto.Medicines)
                {
                    if (patient.PatientsMedicines.Any(pm => pm.MedicineId == medId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    PatientMedicine patMed = new PatientMedicine()
                    {
                        MedicineId = medId,
                        Patient = patient
                    };

                    patient.PatientsMedicines.Add(patMed);
                }

                patients.Add(patient);
                sb.AppendLine(string.Format(SuccessfullyImportedPatient, patient.FullName, patient.PatientsMedicines.Count));
            }

            context.Patients.AddRange(patients);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportPharmacies(MedicinesContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PharmaciesDto[]), new XmlRootAttribute("Pharmacies"));

            PharmaciesDto[] pharmaciesDto;

            StringBuilder sb = new StringBuilder();

            using (var reader = new StringReader(xmlString))
            {
                pharmaciesDto = (PharmaciesDto[])xmlSerializer.Deserialize(reader);
            };

            List<Pharmacy> pharmacies = new();

            foreach (var pharDto in pharmaciesDto)
            {
                if (!IsValid(pharDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (pharDto.IsNonStop != "true" && pharDto.IsNonStop != "false")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                Pharmacy pharmacy = new Pharmacy()
                {
                    Name = pharDto.Name,
                    PhoneNumber = pharDto.PhoneNumber,
                    IsNonStop = bool.Parse(pharDto.IsNonStop)
                };

                foreach (var medDto in pharDto.Medicines)
                {
                    if (!IsValid(medDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime parsedProdDate;
                    DateTime parsedExpiryDate;
                    var productionDate = DateTime.TryParseExact(medDto.ProductionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedProdDate);
                    var expiryDate = DateTime.TryParseExact(medDto.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedExpiryDate);

                    if (!productionDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (!expiryDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //if (medDto.Producer is null)
                    //{
                    //    sb.AppendLine(ErrorMessage);
                    //    continue;
                    //}

                    if (parsedProdDate >= parsedExpiryDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (pharmacy.Medicines.Any(m => m.Name == medDto.Name && m.Producer == medDto.Producer))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Medicine med = new Medicine()
                    {
                        Category = (Category)medDto.Category,
                        Name = medDto.Name,
                        Price = medDto.Price,
                        ProductionDate = parsedProdDate,
                        ExpiryDate = parsedExpiryDate,
                        Producer = medDto.Producer
                    };

                    //pharmacies.Add(pharmacy);
                    pharmacy.Medicines.Add(med);
                }

                pharmacies.Add(pharmacy);
                sb.AppendLine(string.Format(SuccessfullyImportedPharmacy, pharmacy.Name, pharmacy.Medicines.Count));
            }

            context.Pharmacies.AddRange(pharmacies);
            context.SaveChanges();

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
