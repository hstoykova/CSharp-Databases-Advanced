using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using TravelAgency.Data;
using TravelAgency.Data.Models;
using TravelAgency.DataProcessor.ImportDtos;

namespace TravelAgency.DataProcessor
{
    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data format!";
        private const string DuplicationDataMessage = "Error! Data duplicated.";
        private const string SuccessfullyImportedCustomer = "Successfully imported customer - {0}";
        private const string SuccessfullyImportedBooking = "Successfully imported booking. TourPackage: {0}, Date: {1}";

        public static string ImportCustomers(TravelAgencyContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomersDto[]), new XmlRootAttribute("Customers"));

            CustomersDto[] customersDto;

            StringBuilder sb = new StringBuilder();

            using (var reader = new StringReader(xmlString))
            {
                customersDto = (CustomersDto[])xmlSerializer.Deserialize(reader);
            };

            List<Customer> customers = new();

            foreach (var cusDto in customersDto)
            {
                if (!IsValid(cusDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (customers.Any(c => c.FullName == cusDto.FullName))
                {
                    sb.AppendLine(DuplicationDataMessage);
                    continue;
                }

                if (customers.Any(c => c.Email == cusDto.Email))
                {
                    sb.AppendLine(DuplicationDataMessage);
                    continue;
                }

                if (customers.Any(c => c.PhoneNumber == cusDto.PhoneNumber))
                {
                    sb.AppendLine(DuplicationDataMessage);
                    continue;
                }

                Customer customer = new Customer()
                {
                    FullName = cusDto.FullName,
                    Email = cusDto.Email,
                    PhoneNumber = cusDto.PhoneNumber
                };

                customers.Add(customer);
                sb.AppendLine(string.Format(SuccessfullyImportedCustomer, customer.FullName));
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportBookings(TravelAgencyContext context, string jsonString)
        {
            var bookingsDto = JsonConvert.DeserializeObject<BookingsDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Booking> bookings = new List<Booking>();

            foreach (var bookDto in bookingsDto)
            {
                if (!IsValid(bookDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime parsedDate;
                var bookDate = DateTime.TryParseExact(bookDto.BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);

                if (!bookDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Booking booking = new Booking()
                {
                    BookingDate = parsedDate,
                    Customer = context.Customers.FirstOrDefault(c => c.FullName == bookDto.CustomerName),
                    TourPackage = context.TourPackages.FirstOrDefault(t => t.PackageName == bookDto.TourPackageName)
                };

                bookings.Add(booking);
                sb.AppendLine(string.Format(SuccessfullyImportedBooking, bookDto.TourPackageName, bookDto.BookingDate));
            }

            context.Bookings.AddRange(bookings);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static bool IsValid(object dto)
        {
            var validateContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, validateContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                string currValidationMessage = validationResult.ErrorMessage;
            }

            return isValid;
        }
    }
}
