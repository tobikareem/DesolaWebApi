//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class HotelsApi
//    { 
//        [FunctionName("HotelsApi_V1BookingHotelBookingsPost")]
//        public async Task<IActionResult> _V1BookingHotelBookingsPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/booking/hotel-bookings")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1BookingHotelBookingsPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V1ReferenceDataLocationsHotelGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsHotelGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/hotel")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsHotelGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V1ReferenceDataLocationsHotelsByCityGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsHotelsByCityGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/hotels/by-city")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsHotelsByCityGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V1ReferenceDataLocationsHotelsByGeocodeGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsHotelsByGeocodeGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/hotels/by-geocode")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsHotelsByGeocodeGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V1ReferenceDataLocationsHotelsByHotelsGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsHotelsByHotelsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/hotels/by-hotels")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsHotelsByHotelsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V2EReputationHotelSentimentsGet")]
//        public async Task<IActionResult> _V2EReputationHotelSentimentsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v2/e-reputation/hotel-sentiments")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V2EReputationHotelSentimentsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V3ShoppingHotelOffersGet")]
//        public async Task<IActionResult> _V3ShoppingHotelOffersGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v3/shopping/hotel-offers")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V3ShoppingHotelOffersGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("HotelsApi_V3ShoppingHotelOffersHotelOfferIdGet")]
//        public async Task<IActionResult> _V3ShoppingHotelOffersHotelOfferIdGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v3/shopping/hotel-offers/{hotelOfferId}")]HttpRequest req, ExecutionContext context, string hotelOfferId)
//        {
//            var method = this.GetType().GetMethod("V3ShoppingHotelOffersHotelOfferIdGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context, hotelOfferId })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
