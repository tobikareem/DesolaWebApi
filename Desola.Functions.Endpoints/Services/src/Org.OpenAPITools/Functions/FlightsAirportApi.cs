//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class FlightsAirportApi
//    { 
//        [FunctionName("FlightsAirportApi_V1AirportDirectDestinationsGet")]
//        public async Task<IActionResult> _V1AirportDirectDestinationsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/airport/direct-destinations")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1AirportDirectDestinationsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsAirportApi_V1ReferenceDataLocationsAirportsGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsAirportsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/airports")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsAirportsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsAirportApi_V1ReferenceDataLocationsCMUCGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsCMUCGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/CMUC")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsCMUCGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsAirportApi_V1ReferenceDataLocationsGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
