
namespace PlexLibraryCatalogue.MediaOrganizers
{
    using PlexLibraryCatalogue.Configuration;
    using Serilog;

    internal class MediaOrganizer
    {
        private readonly ApplicationSettingsOptions applicationSettings;

        public MediaOrganizer(ApplicationSettingsOptions applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        public async Task Organize()
        {
            // TODO this is so the other thread can log first IDK
            System.Threading.Thread.Sleep(2000);

            Dictionary<string, MediaFolderData> mediaFolders = new Dictionary<string, MediaFolderData>();

            this.ParseMediaFile(mediaFolders);

            Log.Debug($"Found {mediaFolders.Count} unique movie folders.");

            foreach (var mediaFolder in mediaFolders.Values)
            {
                Log.Debug($"Processing {mediaFolder}");

                if (mediaFolder.MediaFiles.Any(f => f.AlreadyHasBeenProcessed))
                {
                    Log.Error($"Skipping {mediaFolder.FolderName} because it has already been processed. This should have been previously filtered out!");
                    continue;
                }

                if (!mediaFolder.MediaFolderExists) 
                {
                    string newMediaFolder = string.Empty;

                    if (mediaFolder.MediaIsOnSameDrive)
                    {
                        // Just make a folder and copy the media to it
                        newMediaFolder = Path.Combine(Path.GetDirectoryName(mediaFolder.MediaFiles.FirstOrDefault().OriginalFilePath), mediaFolder.FolderName);
                        Log.Debug($"No folder, same drive. Creating {newMediaFolder}");
                    }
                    else
                    {
                        // Make a folder on the overflow dir and copy everything to it
                        newMediaFolder = Path.Combine(this.applicationSettings.OverflowDir, mediaFolder.FolderName);
                        Log.Debug($"No folder, different drives. Creating {newMediaFolder}");
                    }

                    if (!this.applicationSettings.DryRun)
                    {
                        Directory.CreateDirectory(newMediaFolder);
                    }

                    this.MoveMediaFiles(newMediaFolder, mediaFolder.MediaFiles);
                }
                else
                {
                    // see if any of the media folders are on a drive we can copy to
                    // TODO overflow drive can really be a list of spots we can copy files to
                    string selectedMediaFolder = string.Empty; 
                    HashSet<string> foldersToCleanup = new HashSet<string>();

                    foreach (var file in mediaFolder.MediaFiles) 
                    { 
                        if (mediaFolder.MediaIsOnSameDrive || (file.IsInMediaFolder &&
                            Directory.GetDirectoryRoot(applicationSettings.OverflowDir) == Directory.GetDirectoryRoot(file.OriginalFilePath)))
                        {
                            selectedMediaFolder = Path.GetDirectoryName(file.OriginalFilePath);
                        }
                        else if (file.IsInMediaFolder)
                        {
                            foldersToCleanup.Add(Path.GetDirectoryName(file.OriginalFilePath));
                        }
                    }

                    if (selectedMediaFolder != string.Empty)
                    {
                        Log.Debug($"Existing folder, same or acceptable drive. Using {selectedMediaFolder}");
                    }
                    else
                    {
                        // We can't use the media folder because there is no media on a drive we can copy to
                        selectedMediaFolder = Path.Combine(this.applicationSettings.OverflowDir, mediaFolder.FolderName);
                        Log.Debug($"Existing folder, different drives, none are acceptable. Creating {selectedMediaFolder}");

                        if (!this.applicationSettings.DryRun)
                        {
                            Directory.CreateDirectory(selectedMediaFolder);
                        }
                    }

                    this.MoveMediaFiles(selectedMediaFolder, mediaFolder.MediaFiles);

                    foreach (var cleanupFolder in foldersToCleanup)
                    {
                        if (!this.applicationSettings.DryRun && Directory.EnumerateFileSystemEntries(cleanupFolder).Any())
                        {
                            throw new Exception($"Found files in {cleanupFolder}! Abort cleanup!");
                        }

                        Log.Debug($"Deleting leftover empty folder {cleanupFolder}");
                        if (!this.applicationSettings.DryRun)
                        {
                            Directory.Delete(cleanupFolder);
                        }
                    }
                }
            }
        }

        private void MoveMediaFiles(string newDir, List<MediaFileData> mediaFiles)
        {
            foreach (var fileToCopy in mediaFiles)
            {
                string newFilePath = Path.Combine(newDir, fileToCopy.GetFileNameIncludingMediaInfo);

                if (fileToCopy.OriginalFilePath == newFilePath)
                {
                    Log.Debug($"No need to move, path is the same: {fileToCopy.OriginalFilePath} = {newFilePath}");
                    return;
                }

                Log.Debug($"Moving {fileToCopy.OriginalFilePath} => {newFilePath}");

                if (File.Exists(newFilePath))
                {
                    // Hopefully this will de dupe as many dupes as they can throw at us...
                    double rand = new Random().NextDouble();
                    newFilePath = $"{Path.Combine(Path.GetDirectoryName(newFilePath), Path.GetFileNameWithoutExtension(newFilePath))}DUPE{rand}{Path.GetExtension(newFilePath)}";
                    Log.Warning($"{newFilePath} has been de-duped.");
                }

                if (File.Exists(newFilePath))
                {
                    Log.Warning($"De-duping failed... Will just leave {fileToCopy.OriginalFilePath} alone for someone to clean up manually...");
                }
                else if (!this.applicationSettings.DryRun)
                {
                    File.Move(fileToCopy.OriginalFilePath, newFilePath, false);
                }
            }
        }

        private void ParseMediaFile(Dictionary<string, MediaFolderData> mediaFolders)
        {
            using (var reader = new StreamReader(applicationSettings.MediaInfoFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line == null)
                    {
                        throw new Exception($"Cound not extract data from {applicationSettings.MediaInfoFile}");
                    }

                    if (!line.Contains("locations"))
                    {
                        this.ProcessLine(mediaFolders, line);
                    }
                    else
                    {
                        Log.Debug($"Skipping csv header line: {line}");
                    }
                }
            }
        }

        private void ProcessLine(Dictionary<string, MediaFolderData> mediaFolders, string line)
        {
            var values = line.Split(',');
            string filePath = values[0];
            string resolution = values[1];
            string doviProfile = values[2];

            // Aquaman (2018) was split on two lines, IDK why, but this will skip the line with no path... 
            if (string.IsNullOrEmpty(filePath) )
            {
                Log.Warning($"No file path in line {line}!");
                return;
            }

            if (!Path.HasExtension(filePath)) 
            {
                Log.Warning($"No extension in file path {filePath} from line {line}! Please adjust manually, there must be extra commas.");
                return; 
            }

            // ...and this will allow me to fix the missing resolution from the line with a path later
            if (string.IsNullOrEmpty(resolution) )
            {
                Log.Warning($"No resolution in line {line}! Using placeholder, please manually adjust.");
                resolution = "UNKNOWN";
            }

            if (!string.IsNullOrEmpty(doviProfile))
            {
                doviProfile = $"DV{doviProfile}";
            }

            if (File.Exists(filePath) || this.applicationSettings.DryRun)
            {
                string key = Path.GetFileNameWithoutExtension(filePath);

                if (key.Contains(resolution))
                {
                    Log.Warning($"Will not add {filePath} - it has already been processed");
                    return;
                }

                if (!mediaFolders.TryGetValue(key, out MediaFolderData data))
                {
                    data = new MediaFolderData();
                    data.FolderName = key;
                }

                mediaFolders[key] = data;

                bool alreadyInADir = false;
                string dirName =  Directory.GetParent(filePath).Name;
                if (dirName == key)
                {
                    alreadyInADir = true;
                }

                data.MediaFiles.Add(
                    new MediaFileData
                    {
                        OriginalFilePath = filePath,
                        Resolution = resolution,
                        DOVIProfile = doviProfile,
                        IsInMediaFolder = alreadyInADir
                    });
            }
            else
            {
                Log.Warning($"No file found in line: {line}");
            }
        }
    }
}
