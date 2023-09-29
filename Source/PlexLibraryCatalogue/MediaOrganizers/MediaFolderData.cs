namespace PlexLibraryCatalogue.MediaOrganizers
{
    using System.Text;

    internal class MediaFolderData
    {
        public string FolderName { get; set; }

        public List<MediaFileData> MediaFiles { get; set; } = new List<MediaFileData>();

        public bool MediaFolderExists
        {
            get
            {
                return this.MediaFiles.Any(f => f.IsInMediaFolder);
            }
        }

        public bool MediaIsOnSameDrive
        {
            get
            {
                HashSet<string> roots = new HashSet<string>();

                foreach (var file in this.MediaFiles)
                {
                    string root = Directory.GetDirectoryRoot(file.OriginalFilePath);
                    roots.Add(root);
                    if (roots.Count() > 1)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder($"Media for FolderName={this.FolderName}, FolderExists={this.MediaFolderExists}:");

            foreach (var file in this.MediaFiles)
            {
                builder.Append($"\n  {file.ToString()}");
            }

            return builder.ToString();
        }
    }
}
