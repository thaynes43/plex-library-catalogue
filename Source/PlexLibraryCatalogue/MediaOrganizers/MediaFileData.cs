
namespace PlexLibraryCatalogue.MediaOrganizers
{
    internal class MediaFileData
    {
        public string OriginalFilePath { get; set; }

        public string Resolution { get; set; }

        public string DOVIProfile { get; set; }

        public bool IsInMediaFolder { get; set; }

        public bool AlreadyHasBeenProcessed
        {
            get
            {
                return Path.GetFileNameWithoutExtension(this.OriginalFilePath).Contains(this.Resolution);
            }
        }

        public string GetFileNameIncludingMediaInfo
        {
            get
            {
                return $"{Path.GetFileNameWithoutExtension(this.OriginalFilePath)} - {this.Resolution}{this.DOVIProfile}{Path.GetExtension(this.OriginalFilePath)}";
            }
        }

        public override string ToString()
        {
            return $"OriginalFilePath={this.OriginalFilePath}, Resolution={this.Resolution}, DV={this.DOVIProfile}, InFolder={this.IsInMediaFolder}, NewName={this.GetFileNameIncludingMediaInfo}";
        }
    }
}
