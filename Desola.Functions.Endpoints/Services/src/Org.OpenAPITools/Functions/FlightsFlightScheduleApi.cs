//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class FlightsFlightScheduleApi
//    { 
//        [FunctionName("FlightsFlightScheduleApi_V1AirportPredictionsOnTimeGet")]
//        public async Task<IActionResult> _V1AirportPredictionsOnTimeGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/airport/predictions/on-time")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1AirportPredictionsOnTimeGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightScheduleApi_V1TravelPredictionsFlightDelayGet")]
//        public async Task<IActionResult> _V1TravelPredictionsFlightDelayGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/travel/predictions/flight-delay")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1TravelPredictionsFlightDelayGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightScheduleApi_V2ScheduleFlightsGet")]
//        public async Task<IActionResult> _V2ScheduleFlightsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v2/schedule/flights")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V2ScheduleFlightsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
