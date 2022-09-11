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
using MSiccDev.ServerlessBlog.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class PostsFunction
    {
        private readonly BlogContext _blogContext;

        private const string Route = "post";

        public PostsFunction(BlogContext blogContext)
        {
            _blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));
        }


        [FunctionName($"{nameof(PostsFunction)}_{nameof(Create)}")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Route)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Post post = JsonConvert.DeserializeObject<Post>(requestBody);

            if (post != null)
            {
                try
                {
                    EntityEntry<Post> createdPostEntity = await _blogContext.AddAsync(post);
                    await _blogContext.SaveChangesAsync();

                    return new CreatedResult($"/{createdPostEntity.Entity.Slug}", createdPostEntity.Entity);
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
        public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route + "/{id?}")] HttpRequest req, ILogger log, string id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var existingPost = await _blogContext.Posts.FindAsync(Guid.Parse(id));

                    if (existingPost == null)
                    {
                        log.LogWarning($"Post with Id {id} not found");
                        return new NotFoundResult();
                    }

                    return new OkObjectResult(existingPost);

                }
                else
                {
                    var queryParams = req.GetQueryParameterDictionary();
                    log.LogInformation("Trying to get posts", queryParams.ToArray());

                    //simple paging of results
                    bool hasCountParam = int.TryParse(queryParams["count"], out int count);
                    bool hasSkipParam = int.TryParse(queryParams["skip"], out int skip);

                    List<Post> resultSet = await _blogContext.Posts.
                                          OrderByDescending(post => post.Published).
                                          Skip(hasSkipParam ? skip : 0).
                                          Take(hasCountParam ? count : 10).ToListAsync();

                    return new OkObjectResult(resultSet);
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
        public async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Post post = JsonConvert.DeserializeObject<Post>(requestBody);

            if (post != null)
            {
                try
                {
                    var existingPost = await _blogContext.Posts.FindAsync(Guid.Parse(id));

                    if (existingPost == null)
                    {
                        log.LogWarning($"Post with Id {id} not found");
                        return new NotFoundResult();
                    }

                    if (post != existingPost)
                    {
                        existingPost = post;
                        await _blogContext.SaveChangesAsync();
                    }

                    return new OkObjectResult(post);
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
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            try
            {
                var existingPost = await _blogContext.Posts.FindAsync(Guid.Parse(id));

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

