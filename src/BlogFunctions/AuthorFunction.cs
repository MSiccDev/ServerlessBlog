using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class AuthorFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/author";

        public AuthorFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext)
        {
            _logger = loggerFactory.CreateLogger<AuthorFunction>();
        }

        [Function($"{nameof(AuthorFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Author? author = JsonConvert.DeserializeObject<DtoModel.Author>(requestBody);

                if (author != null)
                {

                    Author? newAuthorEntity = author.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<Author> createdAuthor =
                        _blogContext.Authors.Add(newAuthorEntity);

                    await _blogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdAuthor.Entity.AuthorId);
                }
                else
                {
                    return await req.CreateResponseDataAsync(System.Net.HttpStatusCode.BadRequest, "Submitted data is invalid, author cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(System.Net.HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(AuthorFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.Author? existingAuthor = await _blogContext.Authors.
                                                        Include(author => author.UserImage).
                                                        SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                                     author.AuthorId == Guid.Parse(id));

                if (existingAuthor == null)
                {
                    _logger.LogWarning($"Author with Id {id} not found");
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                _blogContext.Authors.Remove(existingAuthor);

                await _blogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(AuthorFunction)}_{nameof(Get)}")]
        public override async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequestData req, string blogId, string? id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Author? existingAuthor =
                            await _blogContext.Authors.
                                   Include(author => author.UserImage).
                                   ThenInclude(media => media.MediumType).
                                   SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                  author.AuthorId == Guid.Parse(id));

                    if (existingAuthor == null)
                    {
                        _logger.LogWarning($"Author with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOKResponseDataWithJsonAsync(existingAuthor.ToDto());
                }
                else
                {
                    _logger.LogInformation("Trying to get authors...");

                    var queryParams = req.GetQueryParameterDictionary();

                    if ((string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default))
                        return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                    (int count, int skip) = req.GetPagingProperties();

                    List<EntityModel.Author> entityResultSet = await _blogContext.Authors.
                                                                                  Where(author => author.BlogId == Guid.Parse(blogId)).
                                                                                  Skip(skip).
                                                                                  Take(count).
                                                                                  ToListAsync();

                    List<DtoModel.Author> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return await req.CreateOKResponseDataWithJsonAsync(resultSet);
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(AuthorFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequestData req,  string blogId, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Author? author = JsonConvert.DeserializeObject<DtoModel.Author>(requestBody);

                if (author != null)
                {
                    EntityModel.Author? existingAuthor =
                            await _blogContext.Authors.
                                   Include(author => author.UserImage).
                                   ThenInclude(media => media.MediumType).
                                   SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                  author.AuthorId == Guid.Parse(id));

                    if (existingAuthor == null)
                    {
                        _logger.LogWarning($"Author with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingAuthor.UpdateWith(author);

                    await _blogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, author cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


    }
}

