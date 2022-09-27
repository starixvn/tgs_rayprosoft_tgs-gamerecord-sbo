using Tgs.GameInterface.SBOService.Client;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameRecord.SBOManualService;
using Tgs.GameRecord.SBOService.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordService(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<GameInterfaceSBOClientOptions, IServiceProvider> gameInterfaceSBOClientOptions,
        Action<GameProvidersQueryServerClientOptions, IServiceProvider> gameProvidersQueryServerClientOptions)
        => services
            .AddOptions<RedisOptions>()
            .Configure(redisDBconfigure)
            .Services
            .AddMemoryCache()
            .AddSingleton<RedisConnectionManager>()
            .AddGameRecordCore(
                gameRecordInfoOptions,
                gameInterfaceSBOClientOptions,
                gameProvidersQueryServerClientOptions,
                (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
