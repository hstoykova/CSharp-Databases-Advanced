namespace Boardgames.DataProcessor
{
    using Boardgames.Data;
    using Boardgames.Data.Models;
    using Boardgames.DataProcessor.ExportDto;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.IdentityModel.Tokens.Jwt;

    public class Serializer
    {
        public static string ExportCreatorsWithTheirBoardgames(BoardgamesContext context)
        {
            CreatorsWithTheirBoardgames[] creators = context.Creators
                .Where(c => c.Boardgames.Any())
                .Select(c => new CreatorsWithTheirBoardgames()
                {
                    BoardgamesCount = c.Boardgames.Count(),
                    CreatorName = c.FirstName + " " + c.LastName,
                    Boardgames = c.Boardgames.Select(bg => new BoardgameDto
                    {
                        BoardgameName = bg.Name,
                        BoardgameYearPublished = bg.YearPublished
                    })
                    .OrderBy(bg => bg.BoardgameName)
                    .ToArray()
                })
                .OrderByDescending(c => c.BoardgamesCount)
                .ThenBy(c => c.CreatorName)
                .ToArray();

            return XmlHelper.XmlSerializationHelper.Serialize(creators, "Creators");
        }

        public static string ExportSellersWithMostBoardgames(BoardgamesContext context, int year, double rating)
        {
            var sellers = context.Sellers
                
                .Where(s => s.BoardgamesSellers.Any(b => b.Boardgame.YearPublished >= year &&
                    b.Boardgame.Rating <= rating)
                ) 
                .Select(s => new
                {
                    s.Name,
                    s.Website,
                    Boardgames = s.BoardgamesSellers
                    .Where(bg => bg.Boardgame.YearPublished >= year && bg.Boardgame.Rating <= rating)
                        .Select(bg => new
                        {
                            bg.Boardgame.Name,
                            bg.Boardgame.Rating,
                            bg.Boardgame.Mechanics,
                            Category = bg.Boardgame.CategoryType.ToString()
                        })                        
                        .OrderByDescending(bg => bg.Rating)
                        .ThenBy(bg => bg.Name)
                        .ToArray()
                })
                .OrderByDescending(s => s.Boardgames.Count())
                .ThenBy(s => s.Name)
                .Take(5)
                .ToArray();

            return JsonConvert.SerializeObject(sellers, Formatting.Indented);
            //return JsonConvert.SerializeObject(sellers, JsonSettings());
        }

        private static JsonSerializerSettings JsonSettings()
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
        }

    }
}