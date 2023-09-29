namespace PlexLibraryCatalogue.DataTransferObjects
{
    using System.Text.Json.Serialization;

    internal class ExportMetadataResponse
    {
        [JsonPropertyName("response")]
        public ExportMetaData Response { get; set; }

        public override string ToString()
        {
            return $"{this.Response}";
        }
    }

    internal class ExportMetaData
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public ExportData Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(ExportMetadataResponse)}: Result={this.Result}, Message={this.Message}, Data=[{this.Data}]";
        }
    }

    internal class ExportData
    {
        [JsonPropertyName("export_id")]
        public int ExportId { get; set; }

        public override string ToString()
        {
            return $"ExportId={this.ExportId}";
        }
    }
}
