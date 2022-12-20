using System.Collections.Generic;
using System.Linq;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.ModelHelper
{
    public static class EntityToDtoMapExtensions
    {

        public static Author ToDto(this EntityModel.Author entity)
        {
            return new Author
            {
                AuthorId = entity.AuthorId,
                DisplayName = entity.DisplayName,
                UserName = entity.UserName,
                UserImage = entity.UserImage != null ? entity.UserImage.ToDto() : null,
            };
        }

        public static Blog ToDto(this EntityModel.Blog entity, bool includeEmpty = true)
        {
            return new Blog
            {
                BlogId = entity.BlogId,
                Authors = entity.Authors != null ? entity.Authors.Select(entity => entity.ToDto()).ToList() : (includeEmpty ? new List<Author>() : null),
                Name = entity.Name,
                Slogan = entity.Slogan,
                Posts = entity.Posts != null ? entity.Posts.Select(entity => entity.ToDto()).ToList() : (includeEmpty ? new List<Post>() : null),
                Media = entity.Media != null ? entity.Media.Select(entity => entity.ToDto()).ToList() : (includeEmpty ? new List<Medium>() : null),
                LogoUrl = entity.LogoUrl,
                Tags = entity.Tags != null ? entity.Tags.Select(entity => entity.ToDto()).ToList() : (includeEmpty ? new List<Tag>() : null)
            };
        }

        public static Medium ToDto(this EntityModel.Medium entity)
        {
            return new Medium
            {
                MediumId = entity.MediumId,
                MediumType = entity.MediumType.ToDto(),
                MediumUrl = entity.MediumUrl,
                AlternativeText = entity.AlternativeText,
                Description = entity.Description
            };
        }

        public static MediumType ToDto(this EntityModel.MediumType entity)
        {
            return new MediumType
            {
                MediumTypeId = entity.MediumTypeId,
                MimeType = entity.MimeType,
                Name = entity.Name,
                Encoding = entity.Encoding
            };
        }

        public static Tag ToDto(this EntityModel.Tag entity)
        {
            return new Tag
            {
                TagId = entity.TagId,
                Name = entity.Name,
                Slug = entity.Slug
            };
        }

        public static Post ToDto(this EntityModel.Post entity)
        {
            Post result = new Post
            {
                PostId = entity.PostId,
                BlogId = entity.BlogId,
                Author = entity.Author?.ToDto(),
                Title = entity.Title,
                Content = entity.Content,
                LastModified = entity.LastModified,
                Published = entity.Published,
                Slug = entity.Slug,
                Tags = entity.Tags?.Select(entity => entity.ToDto()).ToList(),
                Media = entity.Media?.Select(entity => entity.ToDto()).ToList()
            };

            if (result.Media == null)
                return result;

            foreach (Medium medium in result.Media)
            {
                medium.IsPostImage = entity.PostMediumMappings?.
                                            SingleOrDefault(mapping =>
                                                mapping.MediumId == medium.MediumId &&
                                                mapping.PostId == result.PostId
                                            )?.AsFeatuerdOnPost ?? false;
            }

            return result;
        }
    }
}

