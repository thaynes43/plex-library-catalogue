namespace PlexLibraryCatalogue.Configuration
{
    internal class GoogleAPIOptions
    {
        public const string GoogleAPI = "GoogleAPI";

        public string? ServiceAccountKeyFile { get; set; }

        public string? DriveDirectoryId { get; set; }
    }
}
