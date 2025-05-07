using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Pos.Api.Dtos;
using Quivi.Pos.Api.Dtos.Requests.BackgroundJobs;
using Quivi.Pos.Api.Dtos.Responses.BackgroundJobs;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [Authorize]
    [ApiController]
    public class BackgroundJobsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly JobStorage jobStorage;

        public BackgroundJobsController(IMapper mapper,
                                        JobStorage jobStorage)
        {
            this.mapper = mapper;
            this.jobStorage = jobStorage;
        }

        [HttpGet]
        public GetBackgroundJobResponse Get([FromQuery] GetBackgroundJobsRequest request)
        {
            IStorageConnection storageConnection = jobStorage.GetConnection();

            List<Dtos.BackgroundJob> jobs = new List<Dtos.BackgroundJob>();
            foreach (var id in request.Ids)
            {
                var jobData = storageConnection.GetJobData(id);
                jobs.Add(new Dtos.BackgroundJob
                {
                    Id = id,
                    State = mapper.Map<string, JobState>(jobData.State),
                });
            }

            return new GetBackgroundJobResponse
            {
                Data = jobs,
            };
        }
    }
}