namespace Invoices.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;
    using Invoices.Data;
    using Invoices.Data.Models;
    using Invoices.Data.Models.Enums;
    using Invoices.DataProcessor.ImportDto;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedClients
            = "Successfully imported client {0}.";

        private const string SuccessfullyImportedInvoices
            = "Successfully imported invoice with number {0}.";

        private const string SuccessfullyImportedProducts
            = "Successfully imported product - {0} with {1} clients.";


        public static string ImportClients(InvoicesContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ClientDto[]), new XmlRootAttribute("Clients"));

            ClientDto[] clientsDto;

            StringBuilder sb = new StringBuilder();

            using (var reader = new StringReader(xmlString))
            {
                clientsDto = (ClientDto[])xmlSerializer.Deserialize(reader);
            };

            List<Client> clients = new();

            foreach (var client in clientsDto)
            {
                if (!IsValid(client))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client clientModel = new Client()
                {
                    Name = client.Name,
                    NumberVat = client.NumberVat
                };

                foreach (var address in client.Addresses)
                {
                    if (!IsValid(address))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Address addressModel = new Address()
                    {
                        StreetName = address.StreetName,
                        StreetNumber = address.StreetNumber,
                        PostCode = address.PostCode,
                        Country = address.Country,
                        City = address.City
                    };

                    clientModel.Addresses.Add(addressModel);
                }
                clients.Add(clientModel);

                sb.AppendLine(string.Format(SuccessfullyImportedClients, clientModel.Name));
            }

            context.Clients.AddRange(clients);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        public static string ImportInvoices(InvoicesContext context, string jsonString)
        {
            var invoicesDto = JsonConvert.DeserializeObject<InvoiceDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Invoice> invoices = new List<Invoice>();

            foreach (var invDto in invoicesDto)
            {
                if (!IsValid(invDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime dueDate = DateTime.ParseExact(invDto.DueDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                DateTime issueDate = DateTime.ParseExact(invDto.IssueDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

                if (dueDate < issueDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Invoice invoice = new Invoice()
                {
                    Number = invDto.Number,
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    Amount = invDto.Amount,
                    CurrencyType = (CurrencyType)invDto.CurrencyType,
                    ClientId = invDto.ClientId
                };

                invoices.Add(invoice);
                sb.AppendLine(string.Format(SuccessfullyImportedInvoices, invoice.Number));
            }

            context.Invoices.AddRange(invoices);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        
        public static string ImportProducts(InvoicesContext context, string jsonString)
        {
            var productsDto = JsonConvert.DeserializeObject<ProductDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Product> products = new List<Product>();

            var clients = context.Clients.Select(cl => cl.Id).ToList();

            foreach (var proDto in productsDto)
            {
                if (!IsValid(proDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var product = new Product()
                {
                    Name = proDto.Name,
                    Price = proDto.Price,
                    CategoryType = (CategoryType)proDto.CategoryType
                };

                var uniqueIds = proDto.Clients.Distinct().ToList();

                foreach (var clientId in uniqueIds)
                {
                    if (!clients.Contains(clientId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    ProductClient productClient = new ProductClient()
                    {
                        ClientId = clientId,
                        Product = product
                    };

                    product.ProductsClients.Add(productClient);
                }

                products.Add(product);
                sb.AppendLine(string.Format(SuccessfullyImportedProducts, product.Name, product.ProductsClients.Count));
            }

            context.Products.AddRange(products);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
