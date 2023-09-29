
namespace PlexLibraryCatalogue.Configuration
{
    public sealed class TautulliOptions
    {
        public const string Tautulli = "Tautulli";

        public string? TautulliServer { get; set; }

        public string? TautulliPort { get; set; }

        public string? ApiKey { get; set; }

        public Dictionary<string, SectionTypeOptions> SectionTypeConfigs { get; set; }
    }

    public class SectionTypeOptions
    {
        // See https://github.com/Tautulli/Tautulli/wiki/Exporter-Guide#metadata-and-media-info-export-levels for MetaDataLevel & MediaInfoLevel values
        public int MetaDataLevel { get; set; }

        public int MediaInfoLevel { get; set; }

        public List<string> CustomFields { get; set; }
    }
}
