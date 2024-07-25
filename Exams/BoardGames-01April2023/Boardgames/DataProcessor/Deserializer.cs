namespace Boardgames.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Boardgames.Data;
    using Boardgames.Data.Models;
    using Boardgames.Data.Models.Enums;
    using Boardgames.DataProcessor.ImportDto;
    using Newtonsoft.Json;

    public class Deserializer
    {
       

        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCreator
            = "Successfully imported creator – {0} {1} with {2} boardgames.";

        private const string SuccessfullyImportedSeller
            = "Successfully imported seller - {0} with {1} boardgames.";

        public static string ImportCreators(BoardgamesContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CreatorDto[]), new XmlRootAttribute("Creators"));

            CreatorDto[] creatorsDto;

            StringBuilder sb = new StringBuilder();

            using (var reader = new StringReader(xmlString))
            {
                creatorsDto = (CreatorDto[])xmlSerializer.Deserialize(reader);
            };

            List<Creator> creators = new();

            foreach (var c in creatorsDto)
            {
                if (!IsValid(c))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Creator creator = new Creator()
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName
                };

                foreach (var gameDto in c.Boardgames)
                {
                    if (!IsValid(gameDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    creator.Boardgames.Add(new Boardgame()
                    {
                        Name = gameDto.Name,
                        Rating = gameDto.Rating,
                        YearPublished = gameDto.YearPublished,
                        CategoryType = (CategoryType)gameDto.CategoryType,
                        Mechanics = gameDto.Mechanics
                    });
                }
                
                creators.Add(creator);
                sb.AppendLine(string.Format(SuccessfullyImportedCreator, creator.FirstName, creator.LastName, creator.Boardgames.Count()));
            }

            context.Creators.AddRange(creators);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
            }

        public static string ImportSellers(BoardgamesContext context, string jsonString)
        {
            var sellersDto = JsonConvert.DeserializeObject<SellerDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Seller> sellers = new List<Seller>();

            var boardgameIds = context.Boardgames.Select(b => b.Id).ToArray();

            foreach (var sel in sellersDto)
            {
                if (!IsValid(sel))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Seller seller = new Seller()
                {
                    Name = sel.Name,
                    Address = sel.Address,
                    Country = sel.Country,
                    Website = sel.Website
                };


                foreach (var id in sel.BoardgamesId.Distinct())
                {
                    if (!boardgameIds.Contains(id))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    BoardgameSeller boardgameSeller = new BoardgameSeller()
                    {
                        BoardgameId = id,
                        Seller = seller
                    };

                    seller.BoardgamesSellers.Add(boardgameSeller);
                }
                    sellers.Add(seller);

                    sb.AppendLine(string.Format(SuccessfullyImportedSeller, seller.Name, seller.BoardgamesSellers.Count()));
            }
            context.Sellers.AddRange(sellers);
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

