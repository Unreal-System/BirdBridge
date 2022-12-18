using Newtonsoft.Json;
using Tweetinvi.Models.V2;

namespace BirdBridge
{
    public class JsonObj
    {
        [JsonProperty("data")]
        public TweetV2 Tweet { get; set; }

        [JsonProperty("includes")]
        public BirdBridge.TweetIncludesV2 Includes { get; set; }

        [JsonProperty("errors")]
        public ErrorV2[] Errors { get; set; }
    }

    public class TweetIncludesV2
    {
        [JsonProperty("media")]
        public BirdBridge.MediaV2[] Media { get; set; }

        [JsonProperty("places")]
        public PlaceV2[] Places { get; set; }

        [JsonProperty("polls")]
        public PollV2[] Polls { get; set; }

        [JsonProperty("tweets")]
        public TweetV2[] Tweets { get; set; }

        [JsonProperty("users")]
        public UserV2[] Users { get; set; }
    }

    public class MediaV2
    {
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("media_key")]
        public string MediaKey { get; set; }

        [JsonProperty("preview_image_url")]
        public string PreviewImageUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("non_public_metrics")]
        public MediaNonPublicMetricsV2 NonPublicMetrics { get; set; }

        [JsonProperty("organic_metrics")]
        public MediaOrganicMetricsV2 OrganicMetrics { get; set; }

        [JsonProperty("promoted_metrics")]
        public MediaPromotedMetricsV2 PromotedMetrics { get; set; }
        
        [JsonProperty("public_metrics")]
        public MediaPublicMetricsV2 PublicMetrics { get; set; }

        [JsonProperty("variants")]
        public BirdBridge.VideoEntityVariant[] Variants { get; set; }
    }

    public class VideoEntityVariant
    {
        [JsonProperty("bit_rate")]
        public int Bitrate { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }
    }
}
