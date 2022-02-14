using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HmacServer.Filters
{
    public class HmacAuthentication : ActionFilterAttribute
    {
        private readonly IConfiguration _configuration;

        public HmacAuthentication(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var hashClient = context.HttpContext.Request.Headers["X-hmac-key"].ToString();


            if (string.IsNullOrEmpty(hashClient))
                context.Result = new BadRequestResult();

            var isValid = IsValid(context, hashClient);

            if (isValid == false)
                context.Result = new UnauthorizedResult();

            base.OnActionExecuting(context);
        }

        private bool IsValid(ActionExecutingContext context, string hashClient)
        {
            var requestContentBase64 = "";
            var requestBodyContent = "";
            var req = context.HttpContext.Request;

            var secretKey = _configuration["HmacSecretKey"];

            //streamReader
            if (req.Body.Length > 0)
            {

                var bodyStream = new StreamReader(req.Body);
                bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
                requestBodyContent = bodyStream.ReadToEnd();
            }

            var hashServer = ComputeHash(requestBodyContent);

            if (hashServer != null)
            {
                requestContentBase64 = Convert.ToBase64String(hashServer);
            }

            var data = $"{requestContentBase64}";
            var signature = Encoding.UTF8.GetBytes(data);
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            using (var hmac = new HMACSHA256(secretKeyBytes))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                var signatureBase64 = Convert.ToBase64String(signatureBytes);
                var result = hashClient.Equals(signatureBase64);

                if (result.Equals(false))
                    return false;
            }

            return true;
        }

        private byte[] ComputeHash(string requestContent)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = null;
                var content = Encoding.UTF8.GetBytes(requestContent);

                if (content.Length != 0)
                    hash = sha256.ComputeHash(content);

                return hash;
            }
        }
    }
}
