using System.Threading.Tasks;
using UnnamedStudios.Common.Model;
using Zero.Core.Model;

namespace Zero.Game.Model
{
    public class ZeroGameClient : ServiceClientBase
    {
        public ZeroGameClient(string deploymentIp, string token) : base(GetOptions($"https://{deploymentIp}", token))
        {

        }

        public Task<ServiceResponse<StartConnectionResponse>> ConnectionStartAsync(StartConnectionRequest request, ServiceRequestOptions options = null)
        {
            var route = ZeroGameScopes.Connection
                .V1();

            return Post<StartConnectionRequest, StartConnectionResponse>(route, request, GetRequestOptions(options));
        }

        public Task<ServiceResponse<StartWorldResponse>> WorldStartAsync(StartWorldRequest request, ServiceRequestOptions options = null)
        {
            var route = ZeroGameScopes.World
                .V1();

            return Post<StartWorldRequest, StartWorldResponse>(route, request, GetRequestOptions(options));
        }

        public Task<ServiceResponse> WorldStopAsync(uint worldId, ServiceRequestOptions options = null)
        {
            var route = ZeroGameScopes.World
                .WithId(worldId)
                .V1();

            return Delete(route, GetRequestOptions(options));
        }

        private static ServiceClientOptions GetOptions(string url, string token)
        {
            var options = new ServiceClientOptions
            {
                BaseServiceUrl = url,
                HeaderProvider = new TokenHeaderProvider(token),
                IgnoreSslErrors = true
            };
            return options;
        }

        private static ServiceRequestOptions GetRequestOptions(ServiceRequestOptions inputOptions, bool? logResponseBody = null, bool? logRequestBody = null)
        {
            if (inputOptions == null)
            {
                inputOptions = new ServiceRequestOptions();
            }

            if (logResponseBody.HasValue)
            {
                inputOptions.LogResponseBody = logResponseBody.Value;
            }

            if (logRequestBody.HasValue)
            {
                inputOptions.LogRequestBody = logRequestBody.Value;
            }

            return inputOptions;
        }
    }
}
