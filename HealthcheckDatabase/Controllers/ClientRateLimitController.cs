using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthcheckDatabase.Controllers
{
    public class ClientRateLimitController : Controller
    {
		private readonly ClientRateLimitOptions _options;
		private readonly IClientPolicyStore _clientPolicyStore;

		public ClientRateLimitController(IOptions<ClientRateLimitOptions> optionsAccessor, IClientPolicyStore clientPolicyStore)
		{
			_options = optionsAccessor.Value;
			_clientPolicyStore = clientPolicyStore;
		}

		[HttpGet]
		public async Task<ClientRateLimitPolicy> Get()
		{
			return await _clientPolicyStore.GetAsync($"{_options.ClientPolicyPrefix}_cl-key-1");
		}

		[HttpPost]
		public async Task Post()
		{
			var id = $"{_options.ClientPolicyPrefix}_cl-key-1";
			var clPolicy = await _clientPolicyStore.GetAsync(id);
			clPolicy.Rules.Add(new RateLimitRule
			{
				Endpoint = "*/api/testpolicyupdate",
				Period = "1h",
				Limit = 100
			});
			await _clientPolicyStore.SetAsync(id, clPolicy);
		}
	}
}
