using LaneReady.RulesEngine.Evaluation;
using Microsoft.Extensions.DependencyInjection;

namespace LaneReady.RulesEngine;

public static class DependencyInjection
{
    public static IServiceCollection AddRulesEngine(this IServiceCollection services)
    {
        services.AddSingleton<IRulesEvaluator, RulesEvaluator>();
        return services;
    }
}
