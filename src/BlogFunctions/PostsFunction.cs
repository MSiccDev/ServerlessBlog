using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using MSiccDev.ServerlessBlog.MappingHelper;
using MSiccDev.ServerlessBlog.ModelHelper;
using MSiccDev.ServerlessBlog.EntityModel;
using DtoModel;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class PostsFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/post";

        public PostsFunction(BlogContext blogContext) : base(blogContext)
        {
        }

        [FunctionName($"{nameof(PostsFunction)}_{nameof(Create)}")]
        public override async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = Route)] HttpRequest req, ILogger log, string blogId)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DtoModel.Post post = JsonConvert.DeserializeObject<DtoModel.Post>(requestBody);

            if (post != null)
            {
                try
                {
                    if (post.BlogId != Guid.Parse(blogId))
                        return new BadRequestObjectResult($"Cannot create a post on a different blog.");

                    EntityModel.Post newPostEntity = post.CreateFrom();

                    EntityEntry<EntityModel.Post> createdPostEntity =
                         _blogContext.Posts.Add(newPostEntity);

                    await _blogContext.SaveChangesAsync();

                    return new CreatedResult($"/{Route}/{createdPostEntity.Entity.PostId}", "OK");
                }
                catch (Exception ex)
                {
                    //TODO: better handling of these cases...
                    return new BadRequestObjectResult(ex);
                }
            }

            return new BadRequestObjectResult("Submitted data is invalid, post cannot be created.");
        }


        [FunctionName($"{nameof(PostsFunction)}_{nameof(Get)}")]
        public override async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route + "/{id?}")] HttpRequest req, ILogger log, string blogId, string id = null)
        {
            try
            {
                var queryParams = req.GetQueryParameterDictionary();

                bool includeDetails = false;
                if (queryParams.Any(p => p.Key == nameof(includeDetails).ToLowerInvariant()))
                    bool.TryParse(queryParams[nameof(includeDetails).ToLowerInvariant()], out includeDetails);


                if (!string.IsNullOrWhiteSpace(id))
                {

                    EntityModel.Post existingPost = null;

                    if (includeDetails)
                    {
                        existingPost = await _blogContext.Posts.
                                          Include(post => post.Tags).
                                          Include(post => post.PostTagMappings).
                                          Include(post => post.Author).
                                          Include(post => post.Media).
                                          ThenInclude(media => media.MediumType).
                                          Include(post => post.PostMediumMappings).
                                          SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                       post.PostId == Guid.Parse(id));
                    }
                    else
                    {
                        existingPost = await _blogContext.Posts.SingleOrDefaultAsync(post => post.PostId == Guid.Parse(id));
                    }

                    if (existingPost == null)
                    {
                        log.LogWarning($"Post with Id {id} not found");
                        return new NotFoundResult();
                    }



                    return new OkObjectResult(JsonConvert.SerializeObject(existingPost.ToDto(), _jsonSerializerSettings));

                }
                else
                {
                    log.LogInformation("Trying to get posts", queryParams.ToArray());

                    List<EntityModel.Post> entityResultSet = new List<EntityModel.Post>();

                    (int count, int skip) = req.GetPagingProperties();

                    if (includeDetails)
                    {
                        entityResultSet = await _blogContext.Posts.
                                          Where(post => post.BlogId == Guid.Parse(blogId)).
                                          OrderByDescending(post => post.Published).
                                          Include(post => post.Tags).
                                          Include(post => post.PostTagMappings).
                                          Include(post => post.Author).
                                          Include(post => post.Media).
                                          ThenInclude(media => media.MediumType).
                                          Include(post => post.PostMediumMappings).
                                          Skip(skip).
                                          Take(count).ToListAsync();
                    }
                    else
                    {
                        entityResultSet = await _blogContext.Posts.
                                          Where(post => post.BlogId == Guid.Parse(blogId)).
                                          OrderByDescending(post => post.Published).
                                          Skip(skip).
                                          Take(count).ToListAsync();

                    }

                    List<DtoModel.Post> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return new OkObjectResult(JsonConvert.SerializeObject(resultSet, _jsonSerializerSettings));
                }
            }

            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }


        //TODO: SEARCH (by Title, by Author, by Tag)


        [FunctionName($"{nameof(PostsFunction)}_{nameof(Update)}")]
        public override async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DtoModel.Post post = JsonConvert.DeserializeObject<DtoModel.Post>(requestBody);

            if (post != null)
            {
                try
                {
                    var existingPost = await _blogContext.Posts.
                                              Include(post => post.Tags).
                                              Include(post => post.PostTagMappings).
                                              Include(post => post.Author).
                                              Include(post => post.Media).
                                              ThenInclude(media => media.MediumType).
                                              Include(post => post.PostMediumMappings).
                                              SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                           post.PostId == Guid.Parse(id));

                    if (existingPost == null)
                    {
                        log.LogWarning($"Post with Id {id} not found");
                        return new NotFoundResult();
                    }

                    DateTimeOffset lastUpdated = existingPost.LastModified;

                    existingPost.UpdateWith(post);

                    if (existingPost.LastModified > lastUpdated)
                        await _blogContext.SaveChangesAsync();

                    return new AcceptedResult();
                }
                catch (Exception ex)
                {
                    //TODO: better handling of these cases...
                    return new BadRequestObjectResult(ex);
                }
            }

            return new BadRequestObjectResult("Submitted data is invalid, post cannot be modified.");
        }


        [FunctionName($"{nameof(PostsFunction)}_{nameof(Delete)}")]
        public override async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            try
            {
                EntityModel.Post existingPost = await _blogContext.Posts.
                                                        Include(post => post.Tags).
                                                        Include(post => post.Media).
                                                        SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                                     post.PostId == Guid.Parse(id));

                if (existingPost == null)
                {
                    log.LogWarning($"Post with Id {id} not found");
                    return new NotFoundResult();
                }

                _blogContext.Posts.Remove(existingPost);
                await _blogContext.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }
    }
}

