using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class MediumTypeFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/mediumtype";

        public MediumTypeFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext)
        {
            _logger = loggerFactory.CreateLogger<MediumTypeFunction>();
        }

        [Function($"{nameof(MediumTypeFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.MediumType? mediumType = JsonConvert.DeserializeObject<DtoModel.MediumType>(requestBody);

                if (mediumType != null)
                {
                    EntityModel.MediumType newMediumType = mediumType.CreateFrom();

                    EntityEntry<EntityModel.MediumType> createdMediumType =
                        _blogContext.MediaTypes.Add(newMediumType);

                    await _blogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdMediumType.Entity.MediumTypeId);


                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, medium type cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(MediumTypeFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.MediumType? existingMediumType =
                    await _blogContext.MediaTypes.
                        SingleOrDefaultAsync(mediumType => mediumType.MediumTypeId == Guid.Parse(id));

                if (existingMediumType == null)
                {
                    _logger.LogWarning($"MediumType with Id {id} not found");
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                _blogContext.MediaTypes.Remove(existingMediumType);

                await _blogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(MediumTypeFunction)}_{nameof(Get)}")]
        public override async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequestData req, string blogId, string? id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.MediumType? extitingMediumType =
                            await _blogContext.MediaTypes.
                                   SingleOrDefaultAsync(mediumType => mediumType.MediumTypeId == Guid.Parse(id));

                    if (extitingMediumType == null)
                    {
                        _logger.LogWarning($"MediumType with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOKResponseDataWithJsonAsync(extitingMediumType.ToDto());
                }
                else
                {
                    _logger.LogInformation("Trying to get MediaTypes...");

                    var queryParams = req.GetQueryParameterDictionary();

                    List<EntityModel.MediumType> entityResultSet = new List<EntityModel.MediumType>();

                    (int count, int skip) = req.GetPagingProperties();

                    entityResultSet = await _blogContext.MediaTypes.
                                            Skip(skip).
                                            Take(count).
                                            ToListAsync();

                    List<DtoModel.MediumType> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return await req.CreateOKResponseDataWithJsonAsync(resultSet);
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [Function($"{nameof(MediumTypeFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.MediumType? mediumType = JsonConvert.DeserializeObject<DtoModel.MediumType>(requestBody);

                if (mediumType != null)
                {
                    EntityModel.MediumType? existingMediumType =
                            await _blogContext.MediaTypes.
                                   SingleOrDefaultAsync(mediumtype => mediumtype.MediumTypeId == Guid.Parse(id));

                    if (existingMediumType == null)
                    {
                        _logger.LogWarning($"MediumType with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingMediumType.UpdateWith(mediumType);

                    await _blogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);
                }
                else
                {

                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, MediumType cannot be modified.");
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

