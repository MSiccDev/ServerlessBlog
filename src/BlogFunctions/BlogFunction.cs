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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using DtoModel;
using System.Net;
using MSiccDev.ServerlessBlog.ModelHelper;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class BlogFunction
    {
        private const string Route = "blog";
        internal readonly BlogContext _blogContext;

        internal readonly JsonSerializerSettings _jsonSerializerSettings;

        public BlogFunction(BlogContext blogContext)
        {
            _blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));

            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        //TODO: IMPLEMENT
        [FunctionName($"{nameof(BlogFunction)}_{nameof(Create)}")]
        public async Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Admin, new[] { "post" }, Route = Route)] HttpRequest req, ILogger log)
        {
            return new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
        }

        [FunctionName($"{nameof(BlogFunction)}_{nameof(Get)}")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequest req, ILogger log, string id = null)
        {
            try
            {
                var queryParams = req.GetQueryParameterDictionary();

                bool includeDetails = false;
                if (queryParams.Any(p => p.Key == nameof(includeDetails).ToLowerInvariant()))
                    bool.TryParse(queryParams[nameof(includeDetails).ToLowerInvariant()], out includeDetails);


                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Blog existingBlog = null;

                    if (includeDetails)
                    {
                        existingBlog = await _blogContext.Blogs.
                                            Include(blog => blog.Authors).
                                            ThenInclude(author => author.UserImage).
                                            ThenInclude(medium => medium.MediumType).
                                            Include(blog => blog.Media).
                                            ThenInclude(medium => medium.MediumType).
                                            Include(blog => blog.Tags).
                                            Include(blog => blog.Posts).
                                            ThenInclude(post => post.Author).
                                            ThenInclude(author => author.UserImage).
                                            ThenInclude(medium => medium.MediumType).
                                            Include(blog => blog.Posts).
                                            ThenInclude(post => post.Media).
                                            ThenInclude(media => media.MediumType).
                                            Include(blog => blog.Posts).
                                            ThenInclude(post => post.Tags).
                                            Include(blog => blog.Posts).
                                            ThenInclude(post => post.PostTagMappings).
                                            Include(blog => blog.Posts).
                                            ThenInclude(post => post.PostMediumMappings).
                                            SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));
                    }
                    else
                    {
                        existingBlog = await _blogContext.Blogs.SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));
                    }

                    if (existingBlog == null)
                    {
                        log.LogWarning($"Blog with Id {id} not found");
                        return new NotFoundResult();
                    }

                    return new OkObjectResult(JsonConvert.SerializeObject(existingBlog.ToDto(includeDetails), _jsonSerializerSettings));

                }
                else
                {
                    log.LogInformation("Trying to get posts", queryParams.ToArray());

                    List<EntityModel.Blog> entityResultSet = new List<EntityModel.Blog>();

                    (int count, int skip) = req.GetPagingProperties();


                    entityResultSet = await _blogContext.Blogs.
                                          Skip(skip).
                                          Take(count).
                                          ToListAsync();


                    //always just return the plain blog list, details should be loaded per blog as they can be MASSIVE
                    List<DtoModel.Blog> resultSet = entityResultSet.Select(entity => entity.ToDto(false)).ToList();

                    return new OkObjectResult(JsonConvert.SerializeObject(resultSet, _jsonSerializerSettings));
                }
            }

            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }

        }

        [FunctionName($"{nameof(BlogFunction)}_{nameof(Update)}")]
        public async Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Blog updatedBlog = JsonConvert.DeserializeObject<DtoModel.Blog>(requestBody, _jsonSerializerSettings);

                if (updatedBlog != null)
                {
                    try
                    {
                        EntityModel.Blog exisistingBlog =
                            await _blogContext.Blogs.SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));

                        if (exisistingBlog != null)
                        {
                            exisistingBlog.UpdateWith(updatedBlog);

                            await _blogContext.SaveChangesAsync();
                            return new AcceptedResult();
                        }
                        else
                        {
                            log.LogWarning($"Blog with Id '{id}' not found.");
                            return new NotFoundResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: better handling of these cases...
                        return new BadRequestObjectResult(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }

            return new BadRequestObjectResult("Submitted data is invalid, blog cannot be modified.");

        }

        //TODO: IMPLEMENT
        [FunctionName($"{nameof(BlogFunctions)}_{nameof(Delete)}")]
        public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Admin, new[] { "delete" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            return new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
        }

    }
}

