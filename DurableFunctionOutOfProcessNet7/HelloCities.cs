using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionOutOfProcessNet7
{
    public class HelloCities
    {
        private readonly ILogger _logger;

        public HelloCities(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HelloCities>();
        }

        [Function(nameof(StarterAsync))]
        public async Task<HttpResponseData> StarterAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            [DurableClient] DurableClientContext durableContext)
        {
            string instanceId = await durableContext.Client.ScheduleNewOrchestrationInstanceAsync(nameof(HelloCitiesOrchestrator));
            _logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return durableContext.CreateCheckStatusResponse(req, instanceId);
        }

        [Function(nameof(HelloCitiesOrchestrator))]
        public async Task<string> HelloCitiesOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            string result = "";
            result += await context.CallActivityAsync<string>(nameof(SayHelloActivity), "Tokyo") + " ";
            result += await context.CallActivityAsync<string>(nameof(SayHelloActivity), "London") + " ";
            result += await context.CallActivityAsync<string>(nameof(SayHelloActivity), "Seattle");
            return result;
        }

        [Function(nameof(SayHelloActivity))]
        public string SayHelloActivity([ActivityTrigger] string cityName)
        {
            _logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }
    }
}
