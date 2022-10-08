using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore
{
    public static class Extensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {

            #region tag
            Tag miscTag = new Tag()
            {
                Name = "Miscallenous",
                TagId = Guid.Parse("ce0e1260-58a0-466e-93ed-e5226ce4e02d"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e")
            };

            Tag newsTag = new Tag()
            {
                Name = "News",
                TagId = Guid.Parse("98def0b7-36b0-4efb-a68d-bd694bde5a3c"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e")
            };

            Tag announcementsTag = new Tag()
            {
                Name = "Announcement",
                TagId = Guid.Parse("9167fac7-ec9c-409a-a1b3-00d22f16b265"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e")
            };

            Tag devTag = new Tag()
            {
                Name = "Development",
                TagId = Guid.Parse("4a24679d-5a01-4117-8ff7-827cfc575d06"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e")
            };

            Tag casban6Tag = new Tag()
            {
                Name = "CASBAN6",
                TagId = Guid.Parse("c8e49031-5492-483e-8179-30054d9f4445"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e")
            };

            miscTag.Slug = miscTag.Name.ToSlugString();
            newsTag.Slug = newsTag.Name.ToSlugString();
            announcementsTag.Slug = announcementsTag.Name.ToSlugString();
            devTag.Slug = devTag.Name.ToSlugString();
            casban6Tag.Slug = casban6Tag.Name.ToSlugString();

            modelBuilder.Entity<Tag>().
                HasData(new[] { miscTag, newsTag, announcementsTag, devTag, casban6Tag });
            #endregion

            #region author
            Author adminAuthor = new Author()
            {
                AuthorId = Guid.Parse("41f8a099-67c6-438f-b44a-a05ce04ec25e"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                DisplayName = "Administrator",
                UserName = "admin"
            };

            Author newsAuthor = new Author()
            {
                AuthorId = Guid.Parse("b5fc1e1d-36e8-4563-89c9-2f74ca9c5ba3"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                DisplayName = "News",
                UserName = "news"
            };

            Author devAuthor = new Author()
            {
                AuthorId = Guid.Parse("b6339527-39c4-4260-9381-5024ff4cba83"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                DisplayName = "Development",
                UserName = "dev"
            };

            modelBuilder.Entity<Author>().
                HasData(new[] { adminAuthor, newsAuthor, devAuthor });
            #endregion

            #region media type
            MediumType imgJpeg = new MediumType
            {
                Name = "JpegImage",
                MimeType = "image/jpeg",
                Encoding = null,
                MediumTypeId = Guid.Parse("029d456e-1019-4b6d-8b17-7c5d46479aa9")
            };

            MediumType imgPng = new MediumType
            {
                Name = "PngImage",
                MimeType = "image/png",
                Encoding = null,
                MediumTypeId = Guid.Parse("f6113ec3-5dd8-4451-a485-ec908cd13f5f")
            };

            MediumType videoMP4 = new MediumType
            {
                Name = "VideoMP4",
                MimeType = "video/mp4",
                Encoding = null,
                MediumTypeId = Guid.Parse("7fd73ef9-79bb-445c-8a46-9d46d89ad27e")
            };

            MediumType videoH264 = new MediumType
            {
                Name = "VideoH264",
                MimeType = "video/h264",
                Encoding = null,
                MediumTypeId = Guid.Parse("2048bf73-8830-4cf3-a820-69c2ac11f0b6")
            };

            modelBuilder.Entity<MediumType>().
                HasData(new[] { imgJpeg, imgPng, videoMP4, videoH264 });

            #endregion

            #region media
            Medium postImage1 = new Medium()
            {
                MediumId = Guid.Parse("3b9831ad-e97e-4df9-a2b8-11b9193cb1d1"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                MediumUrl = new Uri("https://cdn.pixabay.com/photo/2018/03/10/12/00/teamwork-3213924_1280.jpg"),
                AlternativeText = "PostImage1",
                Description = "Image by <a href=\"https://pixabay.com/users/mohamed_hassan-5229782/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=image&amp;utm_content=3213924\">mohamed Hassan</a> from <a href=\"https://pixabay.com//?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=image&amp;utm_content=3213924\">Pixabay</a>",
                MediumTypeId = Guid.Parse("029d456e-1019-4b6d-8b17-7c5d46479aa9"),
            };

            Medium postImage2 = new Medium()
            {
                MediumId = Guid.Parse("863502b4-4054-40e0-9fb8-834007e2f38d"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                MediumUrl = new Uri("https://cdn.pixabay.com/photo/2018/09/09/08/36/network-3664108_1280.jpg"),
                AlternativeText = "PostImage2",
                Description = "Image by <a href=\"https://pixabay.com/users/geralt-9301/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=image&amp;utm_content=3664108\">Gerd Altmann</a> from <a href=\"https://pixabay.com//?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=image&amp;utm_content=3664108\">Pixabay</a>",
                MediumTypeId = Guid.Parse("029d456e-1019-4b6d-8b17-7c5d46479aa9")
            };

            Medium postImage3 = new Medium()
            {
                MediumId = Guid.Parse("a818ba58-a714-492b-b18a-8de87dbec04a"),
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                MediumUrl = new Uri("https://cdn.pixabay.com/photo/2021/08/01/19/00/cloud-6515064_1280.jpg"),
                AlternativeText = "PostImage3",
                Description = "Image by <a href=\"https://pixabay.com/users/akitada31-172067/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=image&amp;utm_content=6515064\">Roman</a> from <a href=\"https://pixabay.com//?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=image&amp;utm_content=6515064\">Pixabay</a>",
                MediumTypeId = Guid.Parse("029d456e-1019-4b6d-8b17-7c5d46479aa9")
            };

            modelBuilder.Entity<Medium>().
                HasData(new[] { postImage1, postImage2, postImage3 });

            #endregion

            #region blog
            modelBuilder.Entity<Blog>().
                HasData(new Blog()
                {
                    BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                    Name = "MSiccDev's Serverless Blog",
                    Slogan = "Open Source Serverless .NET 6 Blog Solution on Azure",
                    LogoUrl = new Uri("https://msiccdev.net/images/logo-neu-header-ohne-fv.png")
                });
            #endregion

            #region post
            Post initPost1 = new Post()
            {
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                PostId = Guid.Parse("a0421134-3462-4223-ace6-37d5438d0434"),
                Title = "The third post on this blog",
                Content = @"<h2>The first post on this blog</h2>" +
                          @"<code>Console.WriteLine(""Hello serverless blog!"")'</code>" +
                          @"<p>This is the third blog post on this Blog. It is a seed placeholder to test and demonstrate the entitiy model.</p>" +
                          @"<h3>Moving on...</h3>" +
                          @"<p>You should remove this test post once you are sure that you have everything up and running. Alternatively, you can also use this post to test the complementary Azure Function.</p>" +
                          @"<p>I absolutely recommend following my #CASBAN6 blog series where I explain a lot of the stuff I am doing during implementing this serverless blog engine.</p>",
                Published = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.Now,
                AuthorId = adminAuthor.AuthorId
            };

            initPost1.Slug = initPost1.Title.ToSlugString();

            Post initPost2 = new Post()
            {
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                PostId = Guid.Parse("cec5073b-a016-43d8-bbcd-43bbf4d2d57f"),
                Title = "The second post on this blog",
                Content = @"<h2>The second post on this blog</h2>" +
                          @"<code>Console.WriteLine(""Hello serverless blog!"")'</code>" +
                          @"<p>This is the second blog post on this Blog. It is a seed placeholder to test and demonstrate the entitiy model.</p>" +
                          @"<h3>Moving on...</h3>" +
                          @"<p>You should remove this test post once you are sure that you have everything up and running. Alternatively, you can also use this post to test the complementary Azure Function.</p>" +
                          @"<p>I absolutely recommend following my #CASBAN6 blog series where I explain a lot of the stuff I am doing during implementing this serverless blog engine.</p>",
                Published = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.Now.AddDays(-10),
                AuthorId = newsAuthor.AuthorId
            };

            initPost2.Slug = initPost2.Title.ToSlugString();

            Post initPost3 = new Post()
            {
                BlogId = Guid.Parse("32f58fde-7258-4ffe-ac34-5ab88ea29f7e"),
                PostId = Guid.Parse("99317c63-ab4e-403f-9575-1bffc9fae28c"),
                Title = "The first post on this blog",
                Content = @"<h2>The first post on this blog</h2>" +
                          @"<code>Console.WriteLine(""Hello serverless blog!"")'</code>" +
                          @"<p>This is the first blog post on this Blog. It is a seed placeholder to test and demonstrate the entitiy model.</p>" +
                          @"<h3>Moving on...</h3>" +
                          @"<p>You should remove this test post once you are sure that you have everything up and running. Alternatively, you can also use this post to test the complementary Azure Function.</p>" +
                          @"<p>I absolutely recommend following my #CASBAN6 blog series where I explain a lot of the stuff I am doing during implementing this serverless blog engine.</p>",
                Published = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.Now.AddDays(-20),
                AuthorId = devAuthor.AuthorId,
            };

            initPost3.Slug = initPost3.Title.ToSlugString();

            modelBuilder.Entity<PostTagMapping>().
                HasData(new[]
                {
                    new PostTagMapping()
                    {
                        PostId = Guid.Parse("a0421134-3462-4223-ace6-37d5438d0434"),
                        TagId = Guid.Parse("4a24679d-5a01-4117-8ff7-827cfc575d06")
                    },
                    new PostTagMapping()
                    {
                        PostId = Guid.Parse("a0421134-3462-4223-ace6-37d5438d0434"),
                        TagId = Guid.Parse("c8e49031-5492-483e-8179-30054d9f4445")
                    },

                    new PostTagMapping()
                    {
                        PostId = Guid.Parse("cec5073b-a016-43d8-bbcd-43bbf4d2d57f"),
                        TagId = Guid.Parse("4a24679d-5a01-4117-8ff7-827cfc575d06")
                    },
                    new PostTagMapping()
                    {
                        PostId = Guid.Parse("cec5073b-a016-43d8-bbcd-43bbf4d2d57f"),
                        TagId = Guid.Parse("c8e49031-5492-483e-8179-30054d9f4445")
                    },

                    new PostTagMapping()
                    {
                        PostId = Guid.Parse("99317c63-ab4e-403f-9575-1bffc9fae28c"),
                        TagId = Guid.Parse("4a24679d-5a01-4117-8ff7-827cfc575d06")
                    },
                    new PostTagMapping()
                    {
                        PostId = Guid.Parse("99317c63-ab4e-403f-9575-1bffc9fae28c"),
                        TagId = Guid.Parse("c8e49031-5492-483e-8179-30054d9f4445")
                    }
                });

            modelBuilder.Entity<PostMediumMapping>().
                HasData(new[]
                {
                    new PostMediumMapping()
                    {
                        PostId = Guid.Parse("a0421134-3462-4223-ace6-37d5438d0434"),
                        MediumId = Guid.Parse("a818ba58-a714-492b-b18a-8de87dbec04a"),
                        AsFeatuerdOnPost = true
                    },
                    new PostMediumMapping()
                    {
                        PostId = Guid.Parse("cec5073b-a016-43d8-bbcd-43bbf4d2d57f"),
                        MediumId = Guid.Parse("863502b4-4054-40e0-9fb8-834007e2f38d"),
                        AsFeatuerdOnPost = true
                    },
                    new PostMediumMapping()
                    {
                        PostId = Guid.Parse("99317c63-ab4e-403f-9575-1bffc9fae28c"),
                        MediumId = Guid.Parse("3b9831ad-e97e-4df9-a2b8-11b9193cb1d1"),
                        AsFeatuerdOnPost = true
                    }
                });

            modelBuilder.Entity<Post>().
                HasData(new[]
                {
                    initPost1,
                    initPost2,
                    initPost3
                });
            #endregion
        }


    }
}

