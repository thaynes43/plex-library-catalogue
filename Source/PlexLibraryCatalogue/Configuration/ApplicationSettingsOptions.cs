namespace PlexLibraryCatalogue.Configuration
{
    internal class ApplicationSettingsOptions
    {
        public const string ApplicationSettings = "ApplicationSettings";

        public ApplicationMode Mode { get; set; }

        public bool FetchMediaInfo { get; set; }  

        public string MediaInfoFile { get; set; }

        public bool DryRun { get; set; }

        public string OverflowDir { get; set; }
    }

    public enum ApplicationMode
    {
        UploadCatalogue,
        OrganizeMedia
    }
}
