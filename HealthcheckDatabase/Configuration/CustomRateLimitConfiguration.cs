using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthcheckDatabase.Configuration
{
    public class CustomRateLimitConfiguration : RateLimitConfiguration
    {

        public CustomRateLimitConfiguration(IHttpContextAccessor httpContextAccessor, IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) : base(httpContextAccessor, ipOptions, clientOptions)
        {
            base.RegisterResolvers();

            //ClientResolvers.Add(new ClientQueryStringResolveContributor(HttpContextAccessor, ClientRateLimitOptions.ClientIdHeader));
        }
    }

}
