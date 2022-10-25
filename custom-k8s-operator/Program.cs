//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using KubeOps.Operator;

namespace custom_k8s_operator
{
    public class Program
    {
        public static Task<int> Main(string[] args) =>
            CreateHostBuilder(args)
            .Build()
            .RunOperatorAsync(args);

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}