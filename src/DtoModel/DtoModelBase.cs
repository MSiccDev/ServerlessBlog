using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class DtoModelBase
    {
        public virtual Guid? BlogId { get; set; }

        public virtual Guid? ResourceId { get; set; }
    }
}
