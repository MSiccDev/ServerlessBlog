using System;
using System.Collections.Generic;
using System.Linq;
using MSiccDev.ServerlessBlog.EntityModel;
namespace MSiccDev.ServerlessBlog.ModelHelper
{
    public static class DtoToEntityMapExtensions
    {

        public static Blog CreateFrom(this DtoModel.Blog newBlog) =>
            new Blog
            {
                BlogId = newBlog.BlogId.GetValueOrDefault() == default ? Guid.NewGuid() : newBlog.BlogId.GetValueOrDefault(),
                Name = newBlog.Name,
                Slogan = newBlog.Slogan,
                LogoUrl = newBlog.LogoUrl
            };

        public static Post CreateFrom(this DtoModel.Post newPost)
        {
            List<PostTagMapping> postTagMappings = new List<PostTagMapping>();

            foreach (DtoModel.Tag tag in newPost.Tags)
            {
                postTagMappings.Add(new PostTagMapping
                    { TagId = tag.ResourceId.GetValueOrDefault(), PostId = newPost.ResourceId.GetValueOrDefault() });
            }

            List<PostMediumMapping> postMediumMappings = new List<PostMediumMapping>();

            foreach (DtoModel.Medium medium in newPost.Media)
            {
                postMediumMappings.Add(new PostMediumMapping
                {
                    MediumId = medium.ResourceId.GetValueOrDefault(),
                    PostId = newPost.ResourceId.GetValueOrDefault(),
                    AsFeatuerdOnPost = medium.IsPostImage ?? false
                });
            }

            Post result = new Post
            {
                PostId = newPost.ResourceId.GetValueOrDefault(),
                BlogId = newPost.BlogId.GetValueOrDefault(),
                AuthorId = newPost.Author.ResourceId.GetValueOrDefault(),
                Title = newPost.Title,
                Content = newPost.Content,
                Published = newPost.Published,
                LastModified = DateTimeOffset.Now,
                Slug = newPost.Slug,
                PostTagMappings = postTagMappings,
                PostMediumMappings = postMediumMappings
            };

            return result;
        }

        public static Author CreateFrom(this DtoModel.Author dto, Guid blogId) =>
            new Author
            {
                BlogId = blogId,
                AuthorId = dto.ResourceId.GetValueOrDefault(),
                DisplayName = dto.DisplayName,
                UserName = dto.UserName,
                UserImageId = dto.UserImage?.ResourceId.GetValueOrDefault()
            };

        public static Medium CreateFrom(this DtoModel.Medium dto, Guid blogId) =>
            new Medium
            {
                BlogId = blogId,
                MediumId = dto.ResourceId.GetValueOrDefault(),
                MediumTypeId = dto.MediumType?.ResourceId.GetValueOrDefault() ?? default,
                MediumUrl = dto.MediumUrl,
                AlternativeText = dto.AlternativeText,
                Description = dto.Description,
            };

        public static MediumType CreateFrom(this DtoModel.MediumType dto) =>
            new MediumType
            {
                MediumTypeId = dto.ResourceId.GetValueOrDefault(),
                Encoding = dto.Encoding,
                MimeType = dto.MimeType,
                Name = dto.Name
            };

        public static Tag CreateFrom(this DtoModel.Tag dto, Guid blogId) =>
            new Tag
            {
                TagId = dto.ResourceId.GetValueOrDefault(),
                Name = dto.Name,
                Slug = dto.Slug,
                BlogId = blogId
            };



        public static Blog UpdateWith(this Blog existingBlog, DtoModel.Blog updatedBlog)
        {
            if (existingBlog.BlogId != updatedBlog.BlogId)
                throw new ArgumentException("BlogId must be equal in UPDATE operation.");

            if (existingBlog.Name != updatedBlog.Name)
                existingBlog.Name = updatedBlog.Name;

            if (existingBlog.Slogan != updatedBlog.Slogan)
                existingBlog.Slogan = updatedBlog.Slogan;

            if (existingBlog.LogoUrl != updatedBlog.LogoUrl)
                existingBlog.LogoUrl = existingBlog.LogoUrl;

            return existingBlog;
        }

        public static Post UpdateWith(this Post existingPost, DtoModel.Post updatedPost)
        {
            if (existingPost.PostId != updatedPost.ResourceId.GetValueOrDefault())
                throw new ArgumentException("PostId must be equal in UPDATE operation.");

            bool hasUpdates = false;

            if (existingPost.Title != updatedPost.Title)
            {
                existingPost.Title = updatedPost.Title;
                hasUpdates = true;
            }
            if (existingPost.Content != updatedPost.Content)
            {
                existingPost.Content = updatedPost.Content;
                hasUpdates = true;
            }

            if (existingPost.AuthorId != (updatedPost.Author?.ResourceId.GetValueOrDefault() ?? default))
            {
                existingPost.AuthorId = updatedPost.Author?.ResourceId.GetValueOrDefault() ?? default;
                hasUpdates = true;
            }
            if (existingPost.Slug != updatedPost.Slug)
            {
                existingPost.Slug = updatedPost.Slug;
                hasUpdates = true;
            }

            List<Guid> noLongerDeliveredTagIds = existingPost.Tags.Select(tag => tag.TagId).
                                                              Except(updatedPost.Tags.Select(tag => tag.ResourceId.GetValueOrDefault())).
                                                                 ToList();
            List<Guid> newDeliveredTagIds = updatedPost.Tags.Select(tag => tag.ResourceId.GetValueOrDefault()).
                                                        Except(existingPost.Tags.Select(tag => tag.TagId)).ToList();

            if (noLongerDeliveredTagIds.Any())
            {
                foreach (Guid id in noLongerDeliveredTagIds)
                {
                    Tag tagToRemove = existingPost.Tags.SingleOrDefault(tag => tag.TagId == id);
                    existingPost.Tags.Remove(tagToRemove);
                }

                hasUpdates = true;
            }

            if (newDeliveredTagIds.Any())
            {
                foreach (Guid id in newDeliveredTagIds)
                {
                    Tag tagToAdd = updatedPost.Tags.SingleOrDefault(tag => tag.ResourceId.GetValueOrDefault() == id).CreateFrom(existingPost.BlogId);
                    existingPost.Tags.Add(tagToAdd);
                }

                hasUpdates = true;
            }

            if (hasUpdates)
                existingPost.LastModified = DateTimeOffset.Now;

            return existingPost;
        }

        public static Author UpdateWith(this Author existingAuthor, DtoModel.Author updatedAuthor)
        {
            if (existingAuthor.AuthorId != updatedAuthor.ResourceId.GetValueOrDefault())
                throw new ArgumentException("AuthorId must be equal in UPDATE operation.");

            if (existingAuthor.DisplayName != updatedAuthor.DisplayName)
                existingAuthor.DisplayName = updatedAuthor.DisplayName;

            if (existingAuthor.UserName != updatedAuthor.UserName)
                existingAuthor.UserName = updatedAuthor.UserName;

            if (existingAuthor.UserImageId != updatedAuthor.UserImage.ResourceId.GetValueOrDefault())
                existingAuthor.UserImageId = updatedAuthor.UserImage.ResourceId.GetValueOrDefault();

            return existingAuthor;
        }

        public static Tag UpdateWith(this Tag existingTag, DtoModel.Tag updatedTag)
        {
            if (existingTag.TagId != updatedTag.ResourceId.GetValueOrDefault())
                throw new ArgumentException("TagId must be equal in UPDATE operation.");

            if (existingTag.Name != updatedTag.Name)
                existingTag.Name = updatedTag.Name;

            if (existingTag.Slug != updatedTag.Slug)
                existingTag.Slug = updatedTag.Slug;

            return existingTag;
        }

        public static Medium UpdateWith(this Medium existingMedium, DtoModel.Medium updatedMedium)
        {
            if (existingMedium.MediumId != updatedMedium.ResourceId.GetValueOrDefault())
                throw new ArgumentException("MediumId must be equal in UPDATE operation.");

            if (existingMedium.AlternativeText != updatedMedium.AlternativeText)
                existingMedium.AlternativeText = updatedMedium.AlternativeText;

            if (existingMedium.Description != updatedMedium.Description)
                existingMedium.Description = updatedMedium.Description;

            if (existingMedium.MediumTypeId != updatedMedium.MediumType.ResourceId.GetValueOrDefault())
                existingMedium.MediumTypeId = updatedMedium.MediumType.ResourceId.GetValueOrDefault();

            if (existingMedium.MediumUrl != updatedMedium.MediumUrl)
                existingMedium.MediumUrl = updatedMedium.MediumUrl;

            return existingMedium;
        }

        public static MediumType UpdateWith(this MediumType existingMediumType, DtoModel.MediumType updatedMediumType)
        {
            if (existingMediumType.MediumTypeId != updatedMediumType.ResourceId.GetValueOrDefault())
                throw new ArgumentException("MediumId must be equal in UPDATE operation.");

            if (existingMediumType.Name != updatedMediumType.Name)
                existingMediumType.Name = updatedMediumType.Name;

            if (existingMediumType.MimeType != updatedMediumType.MimeType)
                existingMediumType.MimeType = updatedMediumType.MimeType;

            if (existingMediumType.Encoding != updatedMediumType.Encoding)
                existingMediumType.Encoding = updatedMediumType.Encoding;

            return existingMediumType;
        }

    }
}

