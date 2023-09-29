namespace PlexLibraryCatalogue
{
    using PlexLibraryCatalogue.Collectors;
    using PlexLibraryCatalogue.Configuration;
    using PlexLibraryCatalogue.MediaOrganizers;
    using PlexLibraryCatalogue.Uploaders;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    internal class CatalogueHostedService : IHostedService
    {
        private readonly ApplicationSettingsOptions applicationSettings;
        private readonly IDataCollector dataCollector;
        private readonly IUploader uploader;
        private readonly MediaOrganizer mediaOrganizer;

        public CatalogueHostedService(
            ApplicationSettingsOptions applicationSettings,
            IDataCollector dataCollector, 
            IUploader uploader,
            MediaOrganizer mediaOrganizer)
        {
            this.applicationSettings = applicationSettings;
            this.dataCollector = dataCollector;
            this.uploader = uploader;
            this.mediaOrganizer = mediaOrganizer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            switch (applicationSettings.Mode)
            {
                case ApplicationMode.UploadCatalogue:
                {
                    Task.Run(() => this.uploader.Upload(this.dataCollector.CollectData(cancellationToken).Result, cancellationToken)).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Log.Fatal(t.Exception, $"Fatal exception thrown during UploadCatalogue execution.");
                            throw t.Exception;
                        }
                    });

                    break;
                }
                case ApplicationMode.OrganizeMedia: 
                {
                    Task.Run(this.mediaOrganizer.Organize).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Log.Fatal(t.Exception, $"Fatal exception thrown during OrganizeMedia execution.");
                            throw t.Exception;
                        }
                    });

                    break;
                }
                default:
                    break;
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.CloseAndFlush();
            return Task.CompletedTask;
        }
    }
}
