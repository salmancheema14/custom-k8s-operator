//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using custom_k8s_operator.models;
using DotnetKubernetesClient;
using IdentityModel;
using k8s.Models;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Finalizer;
using KubeOps.Operator.Rbac;
using Microsoft.Rest;

namespace custom_k8s_operator.controllers
{
    [EntityRbac(typeof(MyCustomResource), Verbs = RbacVerb.All)]
    public class MyResourceController : IResourceController<MyCustomResource>
    {
        private readonly ILogger<MyResourceController> logger;
        private readonly IKubernetesClient kubernetesClient;

        public MyResourceController(ILogger<MyResourceController> logger, IKubernetesClient kubernetesClient)
        {
            this.logger = logger;
            this.kubernetesClient = kubernetesClient;
        }

        public async Task<ResourceControllerResult> ReconcileAsync(MyCustomResource resource)
        {
            var resourceName = resource.Name();
            var resourceNamespace = resource.Namespace();

            logger.LogInformation($"Reconciling {resource.Kind}:{resourceName}");

            try
            {
                if(await this.kubernetesClient.Get<MyCustomResource>(resourceName, resourceNamespace) == null)
                {
                    logger.LogInformation($"{resourceName} is already deleted. No reconciliation required. Aborting operation");
                    return null;
                }

                //do something with the 'resource', e.g., get and update or instantiate an AI pipeline based on the details in the object.

                //update state
                resource.Status.State = ResourceStates.Running;
                await this.kubernetesClient.UpdateStatus(resource);

                this.logger.LogInformation($"Updated resource status to '{resource.Status.State}'");
                await finalizerManager.RegisterFinalizerAsync<MonitorFinalizer>(entity);
                return null; // This won't trigger a requeue.
            }
            catch(Exception ex)
            {
                string error = "Reconciliation  failed";

                if (ex is HttpOperationException httpException)
                {
                    error += ", Request:" + httpException.Request.Content.ToString();
                    error += ", Response:" + httpException.Response.Content.ToString();
                }
                this.logger.LogError(ex, error);
                throw;
            }
        }

        public Task StatusModifiedAsync(MyCustomResource resource)
        {
            logger.LogInformation($"{resource.Name()} called {nameof(StatusModifiedAsync)}.");

            return Task.CompletedTask;
        }

        public async Task DeletedAsync(MyCustomResource resource)
        {
            logger.LogInformation($"{resource.Name()} called {nameof(DeletedAsync)}.");
        }

    }
}