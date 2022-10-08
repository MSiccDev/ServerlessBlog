using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DtoModel;

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
                UserName = entity.UserName
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

        public static DtoModel.Medium ToDto(this EntityModel.Medium entity)
        {
            return new DtoModel.Medium()
            {
                MediumId = entity.MediumId,
                MediumType = entity.MediumType.ToDto(),
                MediumUrl = entity.MediumUrl,
                AlternativeText = entity.AlternativeText,
                Description = entity.Description
            };
        }

        public static DtoModel.MediumType ToDto(this EntityModel.MediumType entity)
        {
            return new DtoModel.MediumType()
            {
                MediaTypeId = entity.MediumTypeId,
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
            DtoModel.Post result = new DtoModel.Post()
            {
                PostId = entity.PostId,
                BlogId = entity.BlogId,
                Author = entity.Author != null ? entity.Author.ToDto() : null,
                Title = entity.Title,
                Content = entity.Content,
                LastModified = entity.LastModified,
                Published = entity.Published,
                Slug = entity.Slug,
                Tags = entity.Tags != null ? entity.Tags.Select(entity => entity.ToDto()).ToList() : null,
                Media = entity.Media != null ? entity.Media.Select(entity => entity.ToDto()).ToList() : null
            };

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

