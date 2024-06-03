//using System.Net;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;

//namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions
//{ 
//    public partial class CarAndTransfersApi
//    { 
//        [FunctionName("CarAndTransfersApi_V1OrderingTransferOrdersPost")]
//        public async Task<IActionResult> _V1OrderingTransferOrdersPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/ordering/transfer-orders")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1OrderingTransferOrdersPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("CarAndTransfersApi_V1OrderingTransferOrdersTransferOrderIdTransfersCancellationPost")]
//        public async Task<IActionResult> _V1OrderingTransferOrdersTransferOrderIdTransfersCancellationPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/ordering/transfer-orders/{transferOrderId}/transfers/cancellation")]HttpRequest req, ExecutionContext context, string transferOrderId)
//        {
//            var method = this.GetType().GetMethod("V1OrderingTransferOrdersTransferOrderIdTransfersCancellationPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context, transferOrderId })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }

//        [FunctionName("CarAndTransfersApi_V1ShoppingTransferOffersPost")]
//        public async Task<IActionResult> _V1ShoppingTransferOffersPost([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v1/shopping/transfer-offers")]HttpRequest req, ExecutionContext context)
//        {
//            var method = this.GetType().GetMethod("V1ShoppingTransferOffersPost");
//            return method != null
//                ? (await ((Task<>)method.Invoke(this, new object[] { req, context })).ConfigureAwait(false))
//                : new StatusCodeResult((int)HttpStatusCode.NotImplemented);
//        }
//    }
//}
