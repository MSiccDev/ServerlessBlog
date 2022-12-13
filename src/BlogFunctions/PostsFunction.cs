using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class PostsFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/post";

        public PostsFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext)
        {
            _logger = loggerFactory.CreateLogger<PostsFunction>();
        }

        [Function($"{nameof(PostsFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Post? post = JsonConvert.DeserializeObject<DtoModel.Post>(requestBody);

                if (post != null)
                {
                    if (post.BlogId != Guid.Parse(blogId))
                        return await req.CreateResponseDataAsync(System.Net.HttpStatusCode.BadRequest,"Cannot create a post on a different blog.");

                    EntityModel.Post newPostEntity = post.CreateFrom();

                    EntityEntry<EntityModel.Post> createdPost =
                         _blogContext.Posts.Add(newPostEntity);

                    await _blogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdPost.Entity.PostId);

                }
                else
                {

                    return await req.CreateResponseDataAsync(System.Net.HttpStatusCode.BadRequest, "Submitted data is invalid, post cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(System.Net.HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [Function($"{nameof(PostsFunction)}_{nameof(Get)}")]
        public override async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route + "/{id?}")] HttpRequestData req, string blogId, string? id = null)
        {
            try
            {
                if ((string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default))
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                var queryParams = req.GetQueryParameterDictionary();

                bool includeDetails = false;
                if (queryParams.Any(p => p.Key == nameof(includeDetails).ToLowerInvariant()))
                    bool.TryParse(queryParams[nameof(includeDetails).ToLowerInvariant()], out includeDetails);


                if (!string.IsNullOrWhiteSpace(id))
                {

                    EntityModel.Post? existingPost = null;

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
                        _logger.LogWarning($"Post with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }



                    return await req.CreateOKResponseDataWithJsonAsync(existingPost.ToDto());

                }
                else
                {
                    _logger.LogInformation("Trying to get posts", queryParams.ToArray());

                    List<EntityModel.Post> entityResultSet;

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

                    return await req.CreateOKResponseDataWithJsonAsync(resultSet);
                }
            }

            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest ,ex.ToString());
            }
        }


        //TODO: SEARCH (by Title, by Author, by Tag) => search function?


        [Function($"{nameof(PostsFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Post? post = JsonConvert.DeserializeObject<DtoModel.Post>(requestBody);

                if (post != null)
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
                        _logger.LogWarning($"Post with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    DateTimeOffset lastUpdated = existingPost.LastModified;

                    existingPost.UpdateWith(post);

                    if (existingPost.LastModified > lastUpdated)
                        await _blogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted); 
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, post cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [Function($"{nameof(PostsFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.Post? existingPost = await _blogContext.Posts.
                                                        Include(post => post.Tags).
                                                        Include(post => post.Media).
                                                        SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                                     post.PostId == Guid.Parse(id));

                if (existingPost == null)
                {
                    _logger.LogWarning($"Post with Id {id} not found");
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                _blogContext.Posts.Remove(existingPost);
                await _blogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


    }
}

