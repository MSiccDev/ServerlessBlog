using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class BlogOverview : Blog
    {
        [JsonConstructor]
        public BlogOverview()
        {

        }

        public BlogOverview(Blog blog, int authorsCount, int mediaCount, int tagCount, int postCount)
        {
            this.Name = blog.Name;
            this.Slogan = blog.Slogan;
            this.LogoUrl = blog.LogoUrl;

            this.AuthorsCount = authorsCount;
            this.MediaCount = mediaCount;
            this.TagCount = tagCount;
            this.PostCount = postCount;
        }

        [JsonProperty(Required = Required.Always)]
        public int AuthorsCount { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int MediaCount { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int TagCount { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int PostCount { get; set; }
    }
}
