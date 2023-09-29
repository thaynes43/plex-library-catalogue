namespace PlexLibraryCatalogue.Uploaders
{
    using PlexLibraryCatalogue.Configuration;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;
    using Google.Apis.Upload;
    using System.Collections.Generic;
    using Serilog;
    using PlexLibraryCatalogue.DataTransferObjects;
    using System.Text;

    internal class GoogleDriveUploader : IUploader
    {
        private readonly GoogleAPIOptions googleAPIOptions;

        public GoogleDriveUploader(GoogleAPIOptions googleAPIOptions) 
        {
            this.googleAPIOptions = googleAPIOptions;
        }

        public async Task Upload(List<CatalogueFiles> files, CancellationToken cancellationToken)
        {
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.Upload(file.FileName, file.FileBody, cancellationToken);
            }
        }

        private async Task Upload(string fileName, string fileBody, CancellationToken cancellationToken)
        {
            // Load the Service account credentials and define the scope of its access.
            var credential = GoogleCredential.FromFile(this.googleAPIOptions.ServiceAccountKeyFile)
                            .CreateScoped(DriveService.ScopeConstants.Drive);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            // Upload file Metadata
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string>() { this.googleAPIOptions.DriveDirectoryId }
            };

            await using (var fsSource = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(fileBody)))
            {
                var request = service.Files.Create(fileMetadata, fsSource, "text/csv");
                request.Fields = "*";
                var results = await request.UploadAsync(cancellationToken);

                if (results.Status == UploadStatus.Failed)
                {
                    Log.Error($"Error uploading file: {results.Exception.Message}");
                }

                // the file id of the new file we created
                Log.Debug($"Uploaded file id {request.ResponseBody?.Id}");
            }
        }
    }
}
