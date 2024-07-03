namespace MusicHub
{
    using System;
    using System.Text;
    using Data;
    using Initializer;
    using MusicHub.Data.Models;

    public class StartUp
    {
        public static void Main()
        {
            MusicHubDbContext context =
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //int dur = int.Parse(Console.ReadLine());
            //Console.WriteLine(ExportSongsAboveDuration(context, dur));
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .ToArray()
                .Select(a => new
                {
                    a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy"),
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs
                        .OrderByDescending(s => s.Name)
                        .ThenBy(s => s.Writer.Name)
                        .Select(s => new
                        {
                            s.Name,
                            s.Price,
                            WriterName = s.Writer.Name
                        }).ToArray(),
                    a.Price
                })
                .OrderByDescending(a => a.Price)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var album in albums)
            {
                sb.AppendLine($"-AlbumName: {album.Name}");
                sb.AppendLine($"-ReleaseDate: {album.ReleaseDate}");
                sb.AppendLine($"-ProducerName: {album.ProducerName}");
                sb.AppendLine("-Songs:");

                int count = 1;

                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{count}");
                    sb.AppendLine($"---SongName: {song.Name}");
                    sb.AppendLine($"---Price: {song.Price:F2}");
                    sb.AppendLine($"---Writer: {song.WriterName}");
                    count++;
                }
                sb.AppendLine($"-AlbumPrice: {album.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .ToArray()
                .Where(s => (int)s.Duration.TotalSeconds > duration)
                .Select(s => new
                {
                    s.Name,
                    SongPerformers = s.SongPerformers
                        .Select(sp => sp.Performer.FirstName + " " + sp.Performer.LastName)
                        .OrderBy(sp => sp)
                        .ToArray(),
                    WriterName = s.Writer.Name,
                    AlbumProdName = s.Album.Producer.Name,
                    Dur = s.Duration.ToString("c")
                })
                .OrderBy(s => s.Name)
                .ThenBy(s => s.WriterName)
                .ToArray();

            StringBuilder sb = new();

            int count = 1;

            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{count}");
                sb.AppendLine($"---SongName: {song.Name}");
                sb.AppendLine($"---Writer: {song.WriterName}");

                foreach (var performer in song.SongPerformers)
                {
                    sb.AppendLine($"---Performer: {performer}");

                }

                sb.AppendLine($"---AlbumProducer: {song.AlbumProdName}");
                sb.AppendLine($"---Duration: {song.Dur}");

                count++;
            }


            return sb.ToString().TrimEnd();
        }
    }
}
