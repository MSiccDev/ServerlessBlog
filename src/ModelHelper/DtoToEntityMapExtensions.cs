﻿using System;
using System.Collections.Generic;
using System.Linq;
using DtoModel;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.ModelHelper
{
    public static class DtoToEntityMapExtensions
    {
        public static EntityModel.Author ToEntity(this DtoModel.Author dto, Guid blogId)
        {
            return new EntityModel.Author()
            {
                BlogId = blogId,
                AuthorId = dto.AuthorId,
                DisplayName = dto.DisplayName,
                UserName = dto.UserName,
            };
        }

        public static EntityModel.Blog ToEntity(this DtoModel.Blog dto)
        {
            return new EntityModel.Blog()
            {
                BlogId = dto.BlogId,
                Authors = dto.Authors.Select(author => author.ToEntity(dto.BlogId)).ToList(),
                Name = dto.Name,
                Slogan = dto.Slogan,
                Posts = dto.Posts.Select(post => post.ToEntity()).ToList(),
                Media = dto.Media.Select(media => media.ToEntity(dto.BlogId)).ToList(),
                LogoUrl = dto.LogoUrl,
                Tags = dto.Tags.Select(tag => tag.ToEntity(dto.BlogId)).ToList()
            };
        }

        public static EntityModel.Medium ToEntity(this DtoModel.Medium dto, Guid blogId)
        {
            return new EntityModel.Medium()
            {
                BlogId = blogId,
                MediumId = dto.MediumId,
                MediumTypeId = dto.MediumType?.MediaTypeId ?? default,
                MediumUrl = dto.MediumUrl,
                AlternativeText = dto.AlternativeText,
                Description = dto.Description,
            };
        }

        public static EntityModel.MediumType ToEntity(this DtoModel.MediumType dto)
        {
            return new EntityModel.MediumType()
            {
                MediumTypeId = dto.MediaTypeId,
                MimeType = dto.MimeType,
                Name = dto.Name,
                Encoding = dto.Encoding
            };
        }

        public static EntityModel.Tag ToEntity(this DtoModel.Tag dto, Guid blogId)
        {
            return new EntityModel.Tag()
            {
                TagId = dto.TagId,
                Name = dto.Name,
                Slug = dto.Slug,
                BlogId = blogId
            };
        }

        public static EntityModel.Post ToEntity(this DtoModel.Post dto)
        {
            return new EntityModel.Post()
            {
                PostId = dto.PostId,
                BlogId = dto.BlogId,
                AuthorId = dto.Author?.AuthorId ?? default,
                Title = dto.Title,
                Content = dto.Content,
                LastModified = dto.LastModified,
                Published = dto.Published,
                Slug = dto.Slug,
                Tags = dto.Tags != null ? dto.Tags.Select(tagDto => tagDto.ToEntity(dto.BlogId)).ToList() : null
            };
        }





        public static EntityModel.Post CreateFrom(this DtoModel.Post newPost)
        {
            List<PostTagMapping> postTagMappings = new List<PostTagMapping>();

            foreach (DtoModel.Tag tag in newPost.Tags)
            {
                postTagMappings.Add(new PostTagMapping() { TagId = tag.TagId, PostId = newPost.PostId });
            }

            List<PostMediumMapping> postMediumMappings = new List<PostMediumMapping>();

            foreach (DtoModel.Medium medium in newPost.Media)
            {
                postMediumMappings.Add(new PostMediumMapping()
                {
                    MediumId = medium.MediumId,
                    PostId = newPost.PostId,
                    AsFeatuerdOnPost = medium?.IsPostImage ?? false
                });
            }

            EntityModel.Post result = new EntityModel.Post()
            {
                PostId = newPost.PostId,
                BlogId = newPost.BlogId,
                AuthorId = newPost.Author.AuthorId,
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



        public static EntityModel.Blog UpdateWith(this EntityModel.Blog existingBlog, DtoModel.Blog updatedblog)
        {
            if (existingBlog.BlogId != updatedblog.BlogId)
                throw new ArgumentException("BlogId must be equal in UPDATE operation.");

            if (existingBlog.Name != updatedblog.Name)
                existingBlog.Name = updatedblog.Name;

            if (existingBlog.Slogan != updatedblog.Slogan)
                existingBlog.Slogan = existingBlog.Slogan;

            if (existingBlog.LogoUrl != updatedblog.LogoUrl)
                existingBlog.LogoUrl = existingBlog.LogoUrl;

            return existingBlog;
        }

        public static EntityModel.Post UpdateWith(this EntityModel.Post existingPost, DtoModel.Post updatedPost)
        {
            if (existingPost.PostId != updatedPost.PostId)
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

            if (existingPost.AuthorId != (updatedPost.Author?.AuthorId ?? default))
            {
                existingPost.AuthorId = updatedPost.Author?.AuthorId ?? default;
                hasUpdates = true;
            }
            if (existingPost.Slug != updatedPost.Slug)
            {
                existingPost.Slug = updatedPost.Slug;
                hasUpdates = true;
            }

            List<Guid> noLongerDeliveredTagIds = existingPost.Tags.Select(tag => tag.TagId).
                                                                 Except(updatedPost.Tags.Select(tag => tag.TagId)).
                                                                 ToList();
            List<Guid> newDeliveredTagIds = updatedPost.Tags.Select(tag => tag.TagId).
                                                           Except(existingPost.Tags.Select(tag => tag.TagId)).ToList();

            if (noLongerDeliveredTagIds.Any())
            {
                foreach (Guid id in noLongerDeliveredTagIds)
                {
                    EntityModel.Tag tagToRemove = existingPost.Tags.SingleOrDefault(tag => tag.TagId == id);
                    existingPost.Tags.Remove(tagToRemove);
                }

                hasUpdates = true;
            }

            if (newDeliveredTagIds.Any())
            {
                foreach (Guid id in newDeliveredTagIds)
                {
                    EntityModel.Tag tagToAdd = updatedPost.Tags.SingleOrDefault(tag => tag.TagId == id).ToEntity(existingPost.BlogId);
                    existingPost.Tags.Add(tagToAdd);
                }

                hasUpdates = true;
            }

            if (hasUpdates)
                existingPost.LastModified = DateTimeOffset.Now;

            return existingPost;
        }

        public static EntityModel.Author UpdateWith(this EntityModel.Author existingAuthor, DtoModel.Author updatedAuthor)
        {
            if (existingAuthor.AuthorId != updatedAuthor.AuthorId)
                throw new ArgumentException("AuthorId must be equal in UPDATE operation.");

            if (existingAuthor.DisplayName != updatedAuthor.DisplayName)
                existingAuthor.DisplayName = updatedAuthor.DisplayName;

            if (existingAuthor.UserName != updatedAuthor.UserName)
                existingAuthor.UserName = updatedAuthor.UserName;

            if (existingAuthor.UserImageId != updatedAuthor.UserImage.MediumId)
                existingAuthor.UserImageId = updatedAuthor.UserImage.MediumId;

            return existingAuthor;
        }

        public static EntityModel.Tag UpdateWith(this EntityModel.Tag existingTag, DtoModel.Tag updatedTag)
        {
            if (existingTag.TagId != updatedTag.TagId)
                throw new ArgumentException("AuthorId must be equal in UPDATE operation.");

            if (existingTag.Name != updatedTag.Name)
                existingTag.Name = updatedTag.Name;

            if (existingTag.Slug != updatedTag.Slug)
                existingTag.Slug = updatedTag.Slug;

            return existingTag;
        }

        public static EntityModel.Medium UpdateWith(this EntityModel.Medium existingMedium, DtoModel.Medium updatedMedium)
        {
            if (existingMedium.MediumId != updatedMedium.MediumId)
                throw new ArgumentException("MediumId must be equal in UPDATE operation.");

            if (existingMedium.AlternativeText != updatedMedium.AlternativeText)
                existingMedium.AlternativeText = updatedMedium.AlternativeText;

            if (existingMedium.Description != updatedMedium.Description)
                existingMedium.Description = updatedMedium.Description;

            if (existingMedium.MediumTypeId != updatedMedium.MediumType.MediaTypeId)
                existingMedium.MediumTypeId = updatedMedium.MediumType.MediaTypeId;

            if (existingMedium.MediumUrl != updatedMedium.MediumUrl)
                existingMedium.MediumUrl = updatedMedium.MediumUrl;

            return existingMedium;
        }
    }
}

