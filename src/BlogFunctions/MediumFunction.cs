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
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.EntityFrameworkCore;
using DtoModel;
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class MediumFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/media";

        public MediumFunction(BlogContext blogContext) : base(blogContext)
        {
        }

        [FunctionName($"{nameof(MediumFunction)}_{nameof(Create)}")]
        public override async Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequest req, ILogger log, string blogId)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Medium medium = JsonConvert.DeserializeObject<DtoModel.Medium>(requestBody);

                if (medium != null)
                {
                    EntityModel.Medium newMedium = medium.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Medium> createdMedium =
                        _blogContext.Media.Add(newMedium);

                    await _blogContext.SaveChangesAsync();

                    return new CreatedResult($"{req.GetEncodedUrl()}/{createdMedium.Entity.MediumId}", "OK");


                }
                else {
                    return new BadRequestObjectResult("Submitted data is invalid, media cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(MediumFunction)}_{nameof(Delete)}")]
        public override async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            try
            {
                EntityModel.Medium existinMedium =
                    await _blogContext.Media.
                        SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                       medium.MediumId == Guid.Parse(id));

                if (existinMedium == null)
                {
                    log.LogWarning($"Medium with Id {id} not found");
                    return new NotFoundResult();
                }

                _blogContext.Media.Remove(existinMedium);

                await _blogContext.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(MediumFunction)}_{nameof(Get)}")]
        public override async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequest req, ILogger log, string blogId, string id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Medium extitingMedium =
                            await _blogContext.Media.
                                   Include(medium => medium.MediumType).
                                   SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                  medium.MediumId == Guid.Parse(id));

                    if (extitingMedium == null)
                    {
                        log.LogWarning($"Medium with Id {id} not found");
                        return new NotFoundResult();
                    }

                    return new OkObjectResult(JsonConvert.SerializeObject(extitingMedium.ToDto(), _jsonSerializerSettings));
                }
                else
                {
                    log.LogInformation("Trying to get media...");

                    var queryParams = req.GetQueryParameterDictionary();

                    if (blogId == default)
                        return new BadRequestObjectResult("Required parameter 'blogid' (GUID) is not specified or cannot be parsed.");

                    List<EntityModel.Medium> entityResultSet = new List<EntityModel.Medium>();

                    (int count, int skip) = req.GetPagingProperties();

                    entityResultSet = await _blogContext.Media.
                                            Include(medium => medium.MediumType).
                                            Where(media => media.BlogId == Guid.Parse(blogId)).
                                            Skip(skip).
                                            Take(count).
                                            ToListAsync();

                    List<DtoModel.Medium> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return new OkObjectResult(JsonConvert.SerializeObject(resultSet, _jsonSerializerSettings));
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }

        }

        [FunctionName($"{nameof(MediumFunction)}_{nameof(Update)}")]
        public override async Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DtoModel.Medium medium = JsonConvert.DeserializeObject<DtoModel.Medium>(requestBody);

                if (medium != null)
                {
                    EntityModel.Medium existingMedium =
                            await _blogContext.Media.
                                   Include(medium => medium.MediumType).
                                   SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                  medium.MediumId == Guid.Parse(id));

                    if (existingMedium == null)
                    {
                        log.LogWarning($"Mediumt with Id {id} not found");
                        return new NotFoundResult();
                    }

                    existingMedium.UpdateWith(medium);

                    await _blogContext.SaveChangesAsync();

                    return new AcceptedResult();

                }
                else
                {

                    return new BadRequestObjectResult("Submitted data is invalid, medium cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }


    }
}

