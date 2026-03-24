using System.Collections.Generic;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class SayGamesServicesTag
    {
        [JsonConstructor]
        public SayGamesServicesTag(string name)
        {
            Name = name;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("commit")]
        public SGSGitLabCommit Commit { get; set; }

        [JsonProperty("release")]
        public object Release { get; set; }

        [JsonProperty("protected")]
        public bool Protected { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
    
    public class SGSGitLabCommit
    {
        [JsonConstructor]
        public SGSGitLabCommit() { }
        
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("short_id")]
        public string ShortId { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("parent_ids")]
        public List<string> ParentIds { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_email")]
        public string AuthorEmail { get; set; }

        [JsonProperty("authored_date")]
        public string AuthoredDate { get; set; }

        [JsonProperty("committer_name")]
        public string CommitterName { get; set; }

        [JsonProperty("committer_email")]
        public string CommitterEmail { get; set; }

        [JsonProperty("committed_date")]
        public string CommittedDate { get; set; }

        [JsonProperty("trailers")]
        public Dictionary<string, string> Trailers { get; set; } = new Dictionary<string, string>();

        [JsonProperty("extended_trailers")]
        public Dictionary<string, string> ExtendedTrailers { get; set; } = new Dictionary<string, string>();

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
    }
}