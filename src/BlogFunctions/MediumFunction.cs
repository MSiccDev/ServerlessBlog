using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.EntityFrameworkCore;
using DtoModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.VisualBasic;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class MediumFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/media";

        public MediumFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext)
        {
            _logger = loggerFactory.CreateLogger<MediumFunction>();
        }

        [Function($"{nameof(MediumFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Medium? medium = JsonConvert.DeserializeObject<DtoModel.Medium>(requestBody);

                if (medium != null)
                {
                    EntityModel.Medium newMedium = medium.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Medium> createdMedium =
                        _blogContext.Media.Add(newMedium);

                    await _blogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdMedium.Entity.MediumId);


                }
                else {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, media cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(MediumFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.Medium? existinMedium =
                    await _blogContext.Media.
                        SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                       medium.MediumId == Guid.Parse(id));

                if (existinMedium == null)
                {
                    _logger.LogWarning($"Medium with Id {id} not found");
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                _blogContext.Media.Remove(existinMedium);

                await _blogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(MediumFunction)}_{nameof(Get)}")]
        public override async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequestData req, string blogId, string? id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Medium? existingMedium =
                            await _blogContext.Media.
                                   Include(medium => medium.MediumType).
                                   SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                  medium.MediumId == Guid.Parse(id));

                    if (existingMedium == null)
                    {
                        _logger.LogWarning($"Medium with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOKResponseDataWithJsonAsync(existingMedium.ToDto());
                }
                else
                {
                    _logger.LogInformation("Trying to get media...");

                    var queryParams = req.GetQueryParameterDictionary();

                    if ((string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default))
                        return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                    (int count, int skip) = req.GetPagingProperties();

                    List<EntityModel.Medium> entityResultSet = await _blogContext.Media.
                                                                                  Include(medium => medium.MediumType).
                                                                                  Where(media => media.BlogId == Guid.Parse(blogId)).
                                                                                  Skip(skip).
                                                                                  Take(count).
                                                                                  ToListAsync();

                    List<DtoModel.Medium> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return await req.CreateOKResponseDataWithJsonAsync(resultSet);
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }

        }

        [Function($"{nameof(MediumFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Medium? medium = JsonConvert.DeserializeObject<DtoModel.Medium>(requestBody);

                if (medium != null)
                {
                    EntityModel.Medium? existingMedium =
                            await _blogContext.Media.
                                   Include(medium => medium.MediumType).
                                   SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                  medium.MediumId == Guid.Parse(id));

                    if (existingMedium == null)
                    {
                        _logger.LogWarning($"Mediumt with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingMedium.UpdateWith(medium);

                    await _blogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);

                }
                else
                {

                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, medium cannot be modified.");
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

