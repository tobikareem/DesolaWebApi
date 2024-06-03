//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class FlightsAirlinesApi
//    { 
//        [FunctionName("FlightsAirlinesApi_V1AirlineDestinationsGet")]
//        public async Task<IActionResult> _V1AirlineDestinationsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/airline/destinations")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1AirlineDestinationsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsAirlinesApi_V1ReferenceDataAirlinesGet")]
//        public async Task<IActionResult> _V1ReferenceDataAirlinesGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/airlines")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataAirlinesGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsAirlinesApi_V2ReferenceDataUrlsCheckinLinksGet")]
//        public async Task<IActionResult> _V2ReferenceDataUrlsCheckinLinksGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v2/reference-data/urls/checkin-links")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V2ReferenceDataUrlsCheckinLinksGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
