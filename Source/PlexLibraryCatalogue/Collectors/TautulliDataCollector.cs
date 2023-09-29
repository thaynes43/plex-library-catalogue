namespace PlexLibraryCatalogue.Collectors
{
    using PlexLibraryCatalogue.Configuration;
    using PlexLibraryCatalogue.DataTransferObjects;
    using Serilog;
    using System.Text;
    using System.Text.Json;
    using System.Linq;

    internal class TautulliDataCollector : IDataCollector
    {
        private readonly TautulliOptions tautulliOptions;
        private HttpClient httpClient = new HttpClient();

        public TautulliDataCollector(TautulliOptions tautulliOptions)
        {
            this.tautulliOptions = tautulliOptions;
        }
        
        public async Task<List<CatalogueFiles>> CollectData(CancellationToken cancellationToken)
        {
            var ret = new List<CatalogueFiles>();

            // Capture this now so it's constant for all libraries
            string timeStamp = DateTime.Now.ToString("yyyyMMddTHHmmss");

            var libraries = await this.GetPlexLibraries(cancellationToken);

            if (libraries.Response.Result != "success")
            {
                throw new Exception($"Failed to fetch Plex libraries. Response={libraries}.");
            }

            foreach (var library in libraries.Response.Data)
            {
                // Trigger the server to begin exporting data for this library
                var exportResponse = await this.StartExportOfLibraryData(library.SectionId, library.SectionType, cancellationToken);

                if (exportResponse.Response.Result != "success") 
                {
                    throw new Exception($"Failed to export library data. Response={exportResponse}.");
                }

                // Wait until the export for this library completes 
                await this.WaitForExportToComplete(library.SectionId, exportResponse.Response.Data.ExportId, cancellationToken);

                // Cache the exported data for this library. We will upload it later.
                var catalogue = await this.GetLibraryCatalogue(exportResponse.Response.Data.ExportId, cancellationToken);

                if (string.IsNullOrEmpty(catalogue))
                {
                    throw new Exception($"Catalogue for library {library} export {exportResponse.Response.Data.ExportId} could not be fetched.");
                }

                ret.Add(new CatalogueFiles { FileName = $"{library.SectionName.Replace(" ", string.Empty)}_{timeStamp}.csv", FileBody = catalogue });
            }

            return ret;
        }

        public async Task<GetLibrariesResponse> GetPlexLibraries(CancellationToken cancellationToken)
        {
            StringBuilder getLibrariesString = new StringBuilder("http://");
            getLibrariesString.Append($"{this.tautulliOptions.TautulliServer}:{this.tautulliOptions.TautulliPort}/api/v2?");
            getLibrariesString.Append($"apikey={this.tautulliOptions.ApiKey}");
            getLibrariesString.Append($"&cmd=get_libraries");

            var libraries = await this.httpClient.GetStringAsync(getLibrariesString.ToString(), cancellationToken);
            GetLibrariesResponse librariesResponse = JsonSerializer.Deserialize<GetLibrariesResponse>(libraries);
            Log.Debug(librariesResponse.ToString());
            return librariesResponse;
        }

        public async Task<ExportMetadataResponse> StartExportOfLibraryData(string sectionId, string sectionType, CancellationToken cancellationToken)
        {
            StringBuilder exportString = new StringBuilder("http://");
            exportString.Append($"{this.tautulliOptions.TautulliServer}:{this.tautulliOptions.TautulliPort}/api/v2?");
            exportString.Append($"apikey={this.tautulliOptions.ApiKey}");

            SectionTypeOptions sectionConfig;
            if (!this.tautulliOptions.SectionTypeConfigs.TryGetValue(sectionType, out sectionConfig))
            {
                throw new Exception($"Could not find configuration for section type={sectionType}.");
            }

            exportString.Append($"&cmd=export_metadata&section_id={sectionId}&file_format=csv&metadata_level={sectionConfig.MetaDataLevel}&media_info_level={sectionConfig.MediaInfoLevel}");

            if (sectionConfig.CustomFields != null && sectionConfig.CustomFields.Count > 0)
            {
                exportString.Append($"&custom_fields={string.Join(",", sectionConfig.CustomFields)}");
            }

            var exportData = await this.httpClient.GetStringAsync(exportString.ToString(), cancellationToken);
            ExportMetadataResponse response = JsonSerializer.Deserialize<ExportMetadataResponse>(exportData);
            Log.Debug(response.ToString());
            return response;
        }

        public async Task WaitForExportToComplete(string sectionId, int exportId, CancellationToken cancellationToken)
        {
            bool exportInProgress = true;
            DateTime startWait = DateTime.Now;

            while (exportInProgress)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GetExportsTableResponse tableStatus = await this.GetExportsTableStatus(sectionId, cancellationToken);

                Log.Debug($"Pulled status of {tableStatus.Response.Data.Data.Count} exports. Waiting for exportId={exportId}, elapsed={(DateTime.Now-startWait).TotalSeconds} seconds...");
                var exportToWaitFor = tableStatus.Response.Data.Data.Where(d => d.ExportId == exportId).OrderBy(d => d.Timestamp).First();
                Log.Debug(exportToWaitFor.ToString());

                if (exportToWaitFor.Complete == 1)
                {
                    Log.Debug($"Export {exportId} is complete. Will retrieve csv and cache for future upload.");
                    exportInProgress = false;
                }
                else
                {
                    Task.Delay(10000).Wait();
                }
            }
        }

        public async Task<string> GetLibraryCatalogue(int exportId, CancellationToken cancellationToken)
        {
            StringBuilder downloadString = new StringBuilder("http://");
            downloadString.Append($"{this.tautulliOptions.TautulliServer}:{this.tautulliOptions.TautulliPort}/api/v2?");
            downloadString.Append($"apikey={this.tautulliOptions.ApiKey}");
            downloadString.Append($"&cmd=download_export&export_id={exportId}");

            string downloadExportCSV = string.Empty;
            DateTime startTime = DateTime.Now;
            while (string.IsNullOrEmpty(downloadExportCSV))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var timeCheck = (DateTime.Now - startTime);
                if (timeCheck.TotalMinutes > 3)
                {
                    Log.Error($"Timed out waiting for library catalogue after {timeCheck.TotalSeconds} seconds.");
                    break;
                }

                try
                {
                    downloadExportCSV = await this.httpClient.GetStringAsync(downloadString.ToString(), cancellationToken);
                    Log.Debug($"Retrieved Plex Cataloge for ExportId={exportId} and it is {downloadExportCSV.Length * sizeof(char)} bytes." );
                }
                catch (Exception ex)
                {
                    // TODO timeout after a bit maybe use poly
                    Log.Error(ex, $"Could not retrieve export due to exception. Will try again...");
                    Task.Delay(10000).Wait();
                }
            }

            return downloadExportCSV;
        }

        private async Task<GetExportsTableResponse> GetExportsTableStatus(string sectionId, CancellationToken cancellationToken)
        {
            StringBuilder request = new StringBuilder("http://");
            request.Append($"{this.tautulliOptions.TautulliServer}:{this.tautulliOptions.TautulliPort}/api/v2?");
            request.Append($"apikey={this.tautulliOptions.ApiKey}");
            request.Append($"&cmd=get_exports_table&section_id={sectionId}");

            var data = await this.httpClient.GetStringAsync(request.ToString(), cancellationToken);
            GetExportsTableResponse response = JsonSerializer.Deserialize<GetExportsTableResponse>(data);
            return response;
        }
    }
}
