using System;
using System.Collections.Concurrent;
using System.Linq;

namespace MSiccDev.ServerlessBlog.MappingHelper
{
    public static class EntityToDtoMapExtensions
    {

        public static DtoModel.Author ToDto(this EntityModel.Author entity)
        {
            return new DtoModel.Author()
            {
                AuthorId = entity.AuthorId,
                DisplayName = entity.DisplayName,
                UserName = entity.UserName,
                UserImage = entity.UserImage != null ? entity.UserImage.ToDto() : null
            };
        }

        public static DtoModel.Blog ToDto(this EntityModel.Blog entity)
        {
            return new DtoModel.Blog()
            {
                BlogId = entity.BlogId,
                Authors = entity.Authors.Select(entity => entity.ToDto()).ToList(),
                Name = entity.Name,
                Slogan = entity.Slogan,
                Posts = entity.Posts.Select(entity => entity.ToDto()).ToList(),
                Media = entity.Media.Select(entity => entity.ToDto()).ToList(),
                LogoUrl = entity.LogoUrl,
                Tags = entity.Tags.Select(entity => entity.ToDto()).ToList()
            };
        }

        public static DtoModel.Media ToDto(this EntityModel.Media entity)
        {
            return new DtoModel.Media()
            {
                MediaId = entity.MediaId,
                MediaType = entity.MediaType.ToDto(),
                MediaUrl = entity.MediaUrl,
                AlternativeText = entity.AlternativeText,
                Description = entity.Description
            };
        }

        public static DtoModel.MediaType ToDto(this EntityModel.MediaType entity)
        {
            return new DtoModel.MediaType()
            {
                MediaTypeId = entity.MediaTypeId,
                MimeType = entity.MimeType,
                Name = entity.Name,
                Encoding = entity.Encoding
            };
        }

        public static DtoModel.Tag ToDto(this EntityModel.Tag entity)
        {
            return new DtoModel.Tag()
            {
                TagId = entity.TagId,
                Name = entity.Name,
                Slug = entity.Slug
            };
        }

        public static DtoModel.Post ToDto(this EntityModel.Post entity)
        {
            return new DtoModel.Post()
            {
                PostId = entity.PostId,
                BlogId = entity.BlogId,
                Author = entity.Author.ToDto(),
                Title = entity.Title,
                Content = entity.Content,
                LastModified = entity.LastModified,
                Published = entity.Published,
                PostImage = entity.PostImage != null ? entity.PostImage.ToDto() : null,
                Slug = entity.Slug,
                Tags = entity.Tags.Select(entity => entity.ToDto()).ToList()
            };
        }
    }
}

