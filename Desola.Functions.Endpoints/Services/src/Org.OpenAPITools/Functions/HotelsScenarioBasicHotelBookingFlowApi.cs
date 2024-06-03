//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class HotelsScenarioBasicHotelBookingFlowApi
//    { 
//        [FunctionName("HotelsScenarioBasicHotelBookingFlowApi_V1SecurityOauth2TokenPost")]
//        public async Task<IActionResult> _V1SecurityOauth2TokenPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/security/oauth2/token")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1SecurityOauth2TokenPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
