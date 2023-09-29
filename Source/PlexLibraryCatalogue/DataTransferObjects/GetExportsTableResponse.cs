
using System.Text.Json.Serialization;

namespace PlexLibraryCatalogue.DataTransferObjects
{
    internal class GetExportsTableResponse
    {
        [JsonPropertyName("response")]
        public GetExportsTable Response { get; set; }

        public override string ToString()
        {
            return $"{this.Response}";
        }
    }

    internal class GetExportsTable
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public ExportsTableData Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(ExportMetadataResponse)}: Result={this.Result}, Message={this.Message}, Data=[{this.Data}]";
        }
    }

    internal class ExportsTableData
    {
        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        [JsonPropertyName("recordsTotal")]
        public int RecordsTotal { get; set; }

        [JsonPropertyName("recordsFiltered")]
        public int RecordsFiltered { get; set; }

        [JsonPropertyName("data")]
        public List<ExportsTable> Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(ExportsTableData)}: Result={this.Draw}, Message={this.RecordsTotal}, Data=[{string.Join(',', this.Data)}\n]";
        }
    }

    internal class ExportsTable
    {
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }

        [JsonPropertyName("art_level")]
        public int ArtLevel { get; set; }

        [JsonPropertyName("complete")]
        public int Complete { get; set; }

        [JsonPropertyName("custom_fields")]
        public string CustomFields { get; set; }

        [JsonPropertyName("exists")]
        public bool Exists { get; set; }

        [JsonPropertyName("export_id")]
        public int ExportId { get; set; }

        [JsonPropertyName("exported_items")]
        public int ExportedItems { get; set; }

        [JsonPropertyName("file_format")]
        public string FileFormat { get; set; }

        [JsonPropertyName("file_size")]
        public int FileSize { get; set; }

        [JsonPropertyName("filename")]
        public string FileName { get; set; }

        [JsonPropertyName("individual_files")]
        public int IndividualFiles { get; set; }

        [JsonPropertyName("media_info_level")]
        public int MediaInfoLevel { get; set; }

        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }

        [JsonPropertyName("media_type_title")]
        public string MediaTypeTitle { get; set; }

        [JsonPropertyName("metadata_level")]
        public int MetadataLevel { get; set; }

        [JsonPropertyName("rating_key")]
        public int? RatingKey { get; set; }

        [JsonPropertyName("section_id")]
        public int SectionId { get; set; }

        [JsonPropertyName("thumb_level")]
        public int ThumbLevel { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        public override string ToString()
        {
            return $"\n    [Timestamp={this.Timestamp}, ArtLevel={this.ArtLevel}, Complete={this.Complete}, CustomFields={this.CustomFields}, " +
                   $"Exists={this.Exists}, ExportId={this.ExportId}, ExportedItems={this.ExportedItems}, FileFormat={this.FileFormat}, FileSize={this.FileSize}, " +
                   $"FileName={this.FileName}, IndividualFiles={this.IndividualFiles}, MediaInfoLevel={this.MediaInfoLevel}, MediaType={this.MediaType}, " +
                   $"MediaTypeTitle={this.MediaTypeTitle}], MetadataLevel={this.MetadataLevel}, RatingKey={this.RatingKey}, SectionId={this.SectionId}, " + 
                   $"ThumbLevel={this.ThumbLevel}, Title={this.Title}, TotalItems={this.TotalItems}, UserId={this.UserId}]";
        }
    }
}
