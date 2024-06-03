//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class MarketInsightsApi
//    { 
//        [FunctionName("MarketInsightsApi_V1LocationAnalyticsCategoryRatedAreasGet")]
//        public async Task<IActionResult> _V1LocationAnalyticsCategoryRatedAreasGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/location/analytics/category-rated-areas")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1LocationAnalyticsCategoryRatedAreasGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("MarketInsightsApi_V1TravelAnalyticsAirTrafficBookedGet")]
//        public async Task<IActionResult> _V1TravelAnalyticsAirTrafficBookedGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/travel/analytics/air-traffic/booked")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1TravelAnalyticsAirTrafficBookedGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("MarketInsightsApi_V1TravelAnalyticsAirTrafficBusiestPeriodGet")]
//        public async Task<IActionResult> _V1TravelAnalyticsAirTrafficBusiestPeriodGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/travel/analytics/air-traffic/busiest-period")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1TravelAnalyticsAirTrafficBusiestPeriodGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("MarketInsightsApi_V1TravelAnalyticsAirTrafficTraveledGet")]
//        public async Task<IActionResult> _V1TravelAnalyticsAirTrafficTraveledGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/travel/analytics/air-traffic/traveled")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1TravelAnalyticsAirTrafficTraveledGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
