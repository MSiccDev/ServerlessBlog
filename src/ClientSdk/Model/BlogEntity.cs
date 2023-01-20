using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
namespace MSiccDev.ServerlessBlog.ClientSdk
{
    public class BlogEntity<TBlogEntity> where TBlogEntity : class
    {
        private readonly JsonSerializerSettings? _jsonSerializerSettings;
        private readonly JsonSerializer? _jsonSerializer;

        private BlogEntity(bool throwExceptions)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None
            };

            _jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);

            if (!throwExceptions)
            {
                _jsonSerializerSettings.Error = delegate(object sender, ErrorEventArgs args)
                {
                    this.Error = new RequestError
                    {
                        StatusCode = null,
                        Message = args.ErrorContext.Error.Message
                    };

                    args.ErrorContext.Handled = true;
                };
            }
        }

        public BlogEntity(HttpContent httpContent, HttpStatusCode httpStatusCode, bool throwExceptions = true) : this(throwExceptions)
        {

            if ((int)httpStatusCode < 400)
            {
                using Stream? stream = httpContent.ReadAsStreamAsync().GetAwaiter().GetResult();
                using StreamReader reader = new StreamReader(stream);
                using JsonTextReader json = new JsonTextReader(reader);

                this.Value = _jsonSerializer?.Deserialize<TBlogEntity>(json);
            }
            else
            {
                this.Error = new RequestError(httpStatusCode, httpContent.ReadAsStringAsync().GetAwaiter().GetResult());
            }
        }

        public BlogEntity(Exception exception) =>
            this.Error = new RequestError(null, exception.ToString());


        public string ToRaw(TBlogEntity? entity) =>
            JsonConvert.SerializeObject(entity, _jsonSerializerSettings);

        public string ToRawError(RequestError? error) =>
            JsonConvert.SerializeObject(error, _jsonSerializerSettings);

        public RequestError? Error { get; set; }

        public TBlogEntity? Value { get; }
    }
}
