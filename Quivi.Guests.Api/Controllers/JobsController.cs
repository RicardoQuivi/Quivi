using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Quivi.Guests.Api.Dtos.Requests.Jobs;
using Quivi.Guests.Api.Dtos.Responses.Jobs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly JobStorage jobStorage;

        public JobsController(IMapper mapper,
                                        JobStorage jobStorage)
        {
            this.mapper = mapper;
            this.jobStorage = jobStorage;
        }

        [HttpGet]
        public GetJobsResponse Get([FromQuery] GetJobsRequest request)
        {
            IStorageConnection storageConnection = jobStorage.GetConnection();

            List<Dtos.Job> jobs = [];
            foreach (var id in request.Ids)
            {
                var jobData = storageConnection.GetJobData(id);
                jobs.Add(new Dtos.Job
                {
                    Id = id,
                    State = mapper.Map<string, Dtos.JobState>(jobData.State),
                });
            }

            return new GetJobsResponse
            {
                Data = jobs,
            };
        }
    }
}