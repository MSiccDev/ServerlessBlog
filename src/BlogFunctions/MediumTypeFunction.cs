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
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class MediumTypeFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/mediumtype";

        public MediumTypeFunction(BlogContext blogContext) : base(blogContext)
        {
        }

        [FunctionName($"{nameof(MediumTypeFunction)}_{nameof(Create)}")]
        public async Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequest req, ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.MediumType mediumType = JsonConvert.DeserializeObject<DtoModel.MediumType>(requestBody);

                if (mediumType != null)
                {
                    EntityModel.MediumType newMediumType = mediumType.CreateFrom();

                    EntityEntry<EntityModel.MediumType> createdMediumType =
                        _blogContext.MediaTypes.Add(newMediumType);

                    await _blogContext.SaveChangesAsync();

                    return new CreatedResult($"{req.GetEncodedUrl()}/{createdMediumType.Entity.MediumTypeId}", "OK");


                }
                else
                {
                    return new BadRequestObjectResult("Submitted data is invalid, medium type cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(MediumTypeFunction)}_{nameof(Delete)}")]
        public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            try
            {
                EntityModel.MediumType existingMediumType =
                    await _blogContext.MediaTypes.
                        SingleOrDefaultAsync(mediumType => mediumType.MediumTypeId == Guid.Parse(id));

                if (existingMediumType == null)
                {
                    log.LogWarning($"MediumType with Id {id} not found");
                    return new NotFoundResult();
                }

                _blogContext.MediaTypes.Remove(existingMediumType);

                await _blogContext.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(MediumTypeFunction)}_{nameof(Get)}")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequest req, ILogger log, string id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.MediumType extitingMediumType =
                            await _blogContext.MediaTypes.
                                   SingleOrDefaultAsync(mediumType => mediumType.MediumTypeId == Guid.Parse(id));

                    if (extitingMediumType == null)
                    {
                        log.LogWarning($"MediumType with Id {id} not found");
                        return new NotFoundResult();
                    }

                    return new OkObjectResult(JsonConvert.SerializeObject(extitingMediumType.ToDto(), _jsonSerializerSettings));
                }
                else
                {
                    log.LogInformation("Trying to get MediaTypes...");

                    var queryParams = req.GetQueryParameterDictionary();

                    List<EntityModel.MediumType> entityResultSet = new List<EntityModel.MediumType>();

                    (int count, int skip) = req.GetPagingProperties();

                    entityResultSet = await _blogContext.MediaTypes.
                                            Skip(skip).
                                            Take(count).
                                            ToListAsync();

                    List<DtoModel.MediumType> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return new OkObjectResult(JsonConvert.SerializeObject(resultSet, _jsonSerializerSettings));
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }


        [FunctionName($"{nameof(MediumTypeFunction)}_{nameof(Update)}")]
        public async Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.MediumType mediumType = JsonConvert.DeserializeObject<DtoModel.MediumType>(requestBody);

                if (mediumType != null)
                {
                    EntityModel.MediumType existingMediumType =
                            await _blogContext.MediaTypes.
                                   SingleOrDefaultAsync(mediumtype => mediumtype.MediumTypeId == Guid.Parse(id));

                    if (existingMediumType == null)
                    {
                        log.LogWarning($"MediumType with Id {id} not found");
                        return new NotFoundResult();
                    }

                    existingMediumType.UpdateWith(mediumType);

                    await _blogContext.SaveChangesAsync();

                    return new AcceptedResult();

                }
                else
                {

                    return new BadRequestObjectResult("Submitted data is invalid, MediumType cannot be modified.");
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

