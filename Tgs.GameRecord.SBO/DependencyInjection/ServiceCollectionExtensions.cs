using Tgs.GameCommand.Client;
using Tgs.GameInterface.SBOService.Client;
using Tgs.GameRecord.SBO.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecord(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameInterfaceSBOClientOptions, IServiceProvider> gameInterfaceSBOClientOptions)
        => services
        .AddOptions<RedisOptions>()
        .Configure(redisDBconfigure)
        .Services
        .AddOptions<GameRecordInfoOptions>()
        .Configure(gameRecordInfoOptions)
        .Services
        .AddSingleton<RedisConnectionManager>()
        .AddGameRecordCore(
            gameCommandClientOptions,
            gameInterfaceSBOClientOptions,
             redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
