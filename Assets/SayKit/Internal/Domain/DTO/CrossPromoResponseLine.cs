using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class CrossPromoResponseLine
    {
        [JsonConstructor]
        public CrossPromoResponseLine() { }
        
        private string _localName = string.Empty;

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }
        
        [JsonProperty("app_title")]
        public string AppTitle { get; set; }
        
        [JsonProperty("app_store_id")]
        public string AppStoreId { get; set; }
        
        [JsonProperty("app_scheme")]
        public string AppScheme { get; set; }
        
        [JsonProperty("click_url")]
        public string ClickUrl { get; set; }
        
        [JsonProperty("impression_url")]
        public string ImpressionUrl { get; set; }
        
        [JsonProperty("result_url")]
        public string ResultUrl { get; set; }
        
        [JsonProperty("creative_text")]
        public string CreativeText { get; set; }
        
        [JsonProperty("creative_button")]
        public string CreativeButton { get; set; }
        
        [JsonProperty("creative_type")]
        public string CreativeType { get; set; }
        
        [JsonProperty("creative_url")]
        public string CreativeUrl { get; set; }
        
        [JsonProperty("hasCatalog")]
        public bool HasCatalog { get; set; }
        
        [JsonProperty("catalogParams")]
        public string CatalogParams { get; set; }

        public bool WasLoaded { get; set; }
        public bool WasInstalled { get; set; }
        
        public string GetLocalName()
        {
            if (_localName == "")
            {
                var extension = "";

                if (CreativeType == "video")
                {
                    extension = ".mp4";
                }

                _localName = "crosspromo_" + ExternalId + extension;
            }

            return _localName;
        }

        public bool IsReady()
        {
            return !WasInstalled && WasLoaded;
        }
    }
}