//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class FlightsFlightBookingApi
//    { 
//        [FunctionName("FlightsFlightBookingApi_V1AnalyticsItineraryPriceMetricsGet")]
//        public async Task<IActionResult> _V1AnalyticsItineraryPriceMetricsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/analytics/itinerary-price-metrics")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1AnalyticsItineraryPriceMetricsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1BookingFlightOrdersFlightOrderIdDelete")]
//        public async Task<IActionResult> _V1BookingFlightOrdersFlightOrderIdDelete([HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "v1/booking/flight-orders/{flightOrderId}")]HttpRequest req, ExecutionContext context, string flightOrderId)
//        {
//            var method = this.GetType().GetMethod("V1BookingFlightOrdersFlightOrderIdDelete");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context, flightOrderId })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1BookingFlightOrdersFlightOrderIdGet")]
//        public async Task<ActionResult<Object>> _V1BookingFlightOrdersFlightOrderIdGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/booking/flight-orders/{flightOrderId}")]HttpRequest req, ExecutionContext context, string flightOrderId)
//        {
//            var method = this.GetType().GetMethod("V1BookingFlightOrdersFlightOrderIdGet");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context, flightOrderId })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1BookingFlightOrdersPost")]
//        public async Task<ActionResult<Object>> _V1BookingFlightOrdersPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/booking/flight-orders")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1BookingFlightOrdersPost");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1ShoppingFlightOffersPricingPost")]
//        public async Task<ActionResult<Object>> _V1ShoppingFlightOffersPricingPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/shopping/flight-offers/pricing")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingFlightOffersPricingPost");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1ShoppingFlightOffersUpsellingPost")]
//        public async Task<IActionResult> _V1ShoppingFlightOffersUpsellingPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/shopping/flight-offers/upselling")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingFlightOffersUpsellingPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1ShoppingSeatmapsGet")]
//        public async Task<IActionResult> _V1ShoppingSeatmapsGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/shopping/seatmaps")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingSeatmapsGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V1ShoppingSeatmapsPost")]
//        public async Task<IActionResult> _V1ShoppingSeatmapsPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/shopping/seatmaps")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingSeatmapsPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V2ShoppingFlightOffersGet")]
//        public async Task<ActionResult<Object>> _V2ShoppingFlightOffersGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v2/shopping/flight-offers")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V2ShoppingFlightOffersGet");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V2ShoppingFlightOffersPost")]
//        public async Task<ActionResult<Object>> _V2ShoppingFlightOffersPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v2/shopping/flight-offers")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V2ShoppingFlightOffersPost");
//            return method != null
//                ? (await ((Task<Object>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("FlightsFlightBookingApi_V2ShoppingFlightOffersPredictionPost")]
//        public async Task<IActionResult> _V2ShoppingFlightOffersPredictionPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v2/shopping/flight-offers/prediction")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V2ShoppingFlightOffersPredictionPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
