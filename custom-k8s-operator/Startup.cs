//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using DotnetKubernetesClient;
using k8s;
using KubeOps.Operator;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace custom_k8s_operator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        /* snip... */
        public void ConfigureServices(IServiceCollection services)
        {
            //services
            //    .AddKubernetesOperator(); // config / settings here

            // your own dependencies
            //services.AddTransient<IManager, TestManager.TestManager>();


            var operatorBuilder = services.AddKubernetesOperator(configure => {
                configure.Name = "platform-operator";
                configure.WatcherHttpTimeout = 60 * 60; // TODO: temporarily workaround for next issue: https://github.com/buehler/dotnet-operator-sdk/issues/477. Remove after the fix
            });

            if (this.Environment.IsDevelopment())
            {
                operatorBuilder.AddWebhookLocaltunnel();
            }

            // Register Kubernetes clients
            if (!this.Environment.IsDevelopment())
            {
                services.TryAddSingleton<IKubernetes>(sp => new Kubernetes(KubernetesClientConfiguration.InClusterConfig()));
                services.TryAddSingleton<IKubernetesClient>(sp => new KubernetesClient(KubernetesClientConfiguration.InClusterConfig()));
            }
            else
            {
                services.TryAddSingleton<IKubernetes>(sp => new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile()));
                services.TryAddSingleton<IKubernetesClient>(sp => new KubernetesClient(KubernetesClientConfiguration.BuildConfigFromConfigFile()));
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            // fire up the mappings for the operator
            // this is technically not needed, but if you don't call this
            // function, the healthchecks and mappings are not
            // mapped to endpoints (therefore not callable)
            app.UseKubernetesOperator();
        }
    }
}
