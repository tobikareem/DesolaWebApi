//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class FlightsFlightInspirationApi
//    { 
//        [FunctionName("FlightsFlightInspirationApi_V1ReferenceDataRecommendedLocationsGet")]
//        public async Task<IActionResult> _V1ReferenceDataRecommendedLocationsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/recommended-locations")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataRecommendedLocationsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightInspirationApi_V1ShoppingAvailabilityFlightAvailabilitiesPost")]
//        public async Task<IActionResult> _V1ShoppingAvailabilityFlightAvailabilitiesPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/shopping/availability/flight-availabilities")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingAvailabilityFlightAvailabilitiesPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightInspirationApi_V1ShoppingFlightDatesGet")]
//        public async Task<ActionResult<Object>> _V1ShoppingFlightDatesGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/shopping/flight-dates")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingFlightDatesGet");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightInspirationApi_V1ShoppingFlightDestinationsGet")]
//        public async Task<ActionResult<Object>> _V1ShoppingFlightDestinationsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/shopping/flight-destinations")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingFlightDestinationsGet");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
