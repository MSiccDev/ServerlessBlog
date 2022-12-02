using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DtoModel;

namespace MSiccDev.ServerlessBlog.ModelHelper
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
                UserImage = entity.UserImage != null ? entity.UserImage.ToDto() : null,
            };
        }

        public static DtoModel.Blog ToDto(this EntityModel.Blog entity, bool includeEmpty = true)
        {
            return new DtoModel.Blog()
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
                MediumTypeId = entity.MediumTypeId,
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
                Tags = entity.Tags != null ? entity.Tags.Select(entity => entity.ToDto()).ToList() : new List<Tag>(),
                Media = entity.Media != null ? entity.Media.Select(entity => entity.ToDto()).ToList() : new List<Medium>()
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

