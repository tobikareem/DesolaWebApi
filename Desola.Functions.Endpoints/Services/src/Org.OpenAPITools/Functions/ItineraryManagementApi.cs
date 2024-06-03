//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class ItineraryManagementApi
//    { 
//        [FunctionName("ItineraryManagementApi_V1TravelPredictionsTripPurposeGet")]
//        public async Task<IActionResult> _V1TravelPredictionsTripPurposeGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/travel/predictions/trip-purpose")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1TravelPredictionsTripPurposeGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("ItineraryManagementApi_V3TravelTripParserPost")]
//        public async Task<IActionResult> _V3TravelTripParserPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v3/travel/trip-parser")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V3TravelTripParserPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
