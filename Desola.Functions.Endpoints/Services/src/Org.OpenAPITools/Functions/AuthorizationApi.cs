using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Desola.Functions.Endpoints.Services.src.Org.OpenAPITools.Functions;

public partial class AuthorizationApi
{
    [FunctionName("AuthorizationApi_V1SecurityOauth2TokenAccessTokenGet")]
    public async Task<IActionResult> _V1SecurityOauth2TokenAccessTokenGet([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v1/security/oauth2/token/{access_token}")] HttpRequest req, ExecutionContext context, string accessToken)
    {
        var method = this.GetType().GetMethod("V1SecurityOauth2TokenAccessTokenGet");

        if (method != null)
        {
            var response = method.Invoke(this, new object[] { req, context, accessToken });

            return await ((Task<IActionResult>)response).ConfigureAwait(false);


        }
        return new StatusCodeResult((int)HttpStatusCode.NotImplemented);
    }
}

