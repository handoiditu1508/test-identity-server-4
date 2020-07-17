using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Client
{
	class Program
	{
		private static async Task Main()
		{
			HttpClientHandler clientHandler = new HttpClientHandler();
			clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

			#region discover endpoints from metadata
			var client = new HttpClient(clientHandler);

			var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
			if (disco.IsError)
			{
				Console.WriteLine(disco.Error);
				return;
			}
			Console.WriteLine("Step 1 Completed");
			#endregion

			#region request token
			var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = disco.TokenEndpoint,
				ClientId = "client",
				ClientSecret = "secret",

				Scope = "api1"
			});
			
			if (tokenResponse.IsError)
			{
				Console.WriteLine(tokenResponse.Error);
				return;
			}

			Console.WriteLine(tokenResponse.Json);
			Console.WriteLine("\n\n");
			Console.WriteLine("Step 2 Completed");
			#endregion

			#region call api
			var apiClient = new HttpClient(clientHandler);
			apiClient.SetBearerToken(tokenResponse.AccessToken);

			var response = await apiClient.GetAsync("https://localhost:6001/identity");
			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine(response.StatusCode);
			}
			else
			{
				var content = await response.Content.ReadAsStringAsync();
				Console.WriteLine(JArray.Parse(content));
			}
			Console.WriteLine("Step 3 Completed");
			#endregion
		}
	}
}
