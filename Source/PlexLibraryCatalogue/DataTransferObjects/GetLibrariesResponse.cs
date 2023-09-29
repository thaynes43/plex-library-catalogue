namespace PlexLibraryCatalogue.DataTransferObjects
{
    using System.Text.Json.Serialization;

    internal class GetLibrariesResponse
    {
        [JsonPropertyName("response")]
        public GetLibrariesResponseBody Response { get; set; }

        public override string ToString()
        {
            return $"{nameof(GetLibrariesResponse)}: {this.Response}";
        }
    }

    internal class GetLibrariesResponseBody
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public List<Libraries> Data { get; set; }

        public override string ToString()
        {
            return $"Result={this.Result}, Message={this.Message}, Data=[{string.Join(", ", this.Data)}]";
        }
    }

    internal class Libraries
    {
        [JsonPropertyName("art")]
        public string Art { get; set; }

        [JsonPropertyName("child_count")]
        public string ChildCount { get;set; }

        [JsonPropertyName("count")]
        public string Count { get; set; }

        [JsonPropertyName("is_active")]
        public int IsActive { get; set; }

        [JsonPropertyName("parent_count")]
        public string ParentCount { get; set; }

        [JsonPropertyName("section_id")]
        public string SectionId { get; set; }

        [JsonPropertyName("section_name")]
        public string SectionName { get; set; }

        [JsonPropertyName("section_type")]
        public string SectionType { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        public override string ToString()
        {
            return $"[Art={this.Art}, ChildCount={this.ChildCount}, Count={this.Count}, IsActive={this.IsActive}, ParentCount={this.ParentCount}, SectionId={this.SectionId}, SectionName={this.SectionName}, SectionType={this.SectionType}, Thumb={this.Thumb}]";
        }
    }
}
