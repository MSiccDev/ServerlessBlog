using System;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public abstract class DtoModelBase
    {
        public virtual Guid? BlogId { get; set; }

        public virtual Guid? ResourceId { get; set; }
    }
}
