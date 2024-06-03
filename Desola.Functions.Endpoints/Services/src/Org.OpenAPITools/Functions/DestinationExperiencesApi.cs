//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class DestinationExperiencesApi
//    { 
//        [FunctionName("DestinationExperiencesApi_V1ReferenceDataLocationsCitiesGet")]
//        public async Task<IActionResult> V1ReferenceDataLocationsCitiesGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/cities")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod(nameof(V1ReferenceDataLocationsCitiesGet));
//            return method != null
//                ? await ((Task<IActionResult>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false)
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("DestinationExperiencesApi_V1ReferenceDataLocationsPois9CB40CB5D0Get")]
//        public async Task<IActionResult> V1ReferenceDataLocationsPois9CB40CB5D0Get([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/pois/9CB40CB5D0")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod(nameof(V1ReferenceDataLocationsPois9CB40CB5D0Get));
//            return method != null
//                ? await ((Task<IActionResult>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false)
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("DestinationExperiencesApi_V1ReferenceDataLocationsPoisBySquareGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsPoisBySquareGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/pois/by-square")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsPoisBySquareGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("DestinationExperiencesApi_V1ReferenceDataLocationsPoisGet")]
//        public async Task<IActionResult> _V1ReferenceDataLocationsPoisGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/reference-data/locations/pois")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ReferenceDataLocationsPoisGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("DestinationExperiencesApi_V1ShoppingActivities4615Get")]
//        public async Task<IActionResult> _V1ShoppingActivities4615Get([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/shopping/activities/4615")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingActivities4615Get");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("DestinationExperiencesApi_V1ShoppingActivitiesBySquareGet")]
//        public async Task<IActionResult> _V1ShoppingActivitiesBySquareGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/shopping/activities/by-square")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingActivitiesBySquareGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("DestinationExperiencesApi_V1ShoppingActivitiesGet")]
//        public async Task<IActionResult> _V1ShoppingActivitiesGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/shopping/activities")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingActivitiesGet");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
