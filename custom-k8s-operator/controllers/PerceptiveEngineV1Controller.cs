//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using custom_k8s_operator.finalizers.V1;
using custom_k8s_operator.models;
using custom_k8s_operator.models.V1;
using DotnetKubernetesClient;
using k8s.Models;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Finalizer;
using KubeOps.Operator.Rbac;
using Microsoft.Rest;

namespace custom_k8s_operator.controllers
{
    [EntityRbac(typeof(PerceptiveEngineV1), Verbs = RbacVerb.All)]
    public class PerceptiveEngineV1Controller : IResourceController<PerceptiveEngineV1>
    {
        private readonly IFinalizerManager<PerceptiveEngineV1> finalizerManager;
        private readonly ILogger<PerceptiveEngineV1Controller> logger;
        private readonly IKubernetesClient kubernetesClient;

        public PerceptiveEngineV1Controller(
            ILogger<PerceptiveEngineV1Controller> logger, 
            IKubernetesClient kubernetesClient, 
            IFinalizerManager<PerceptiveEngineV1> finalizerManager)
        {
            this.logger = logger;
            this.kubernetesClient = kubernetesClient;
            this.finalizerManager = finalizerManager;
        }

        /// <summary>
        /// This method is called for 'KubeOps.Operator.Kubernetes.ResourceEventType.Reconcile' events on a given entity.
        /// https://buehler.github.io/dotnet-operator-sdk/kube-ops/KubeOps.Operator.Kubernetes.ResourceEventType.html
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<ResourceControllerResult> ReconcileAsync(PerceptiveEngineV1 resource)
        {
            var resourceName = resource.Name();
            var resourceNamespace = resource.Namespace();

            logger.LogInformation($"Reconciling '{resourceName}' of type '{resource.Kind}' in namespace '{resourceNamespace}'");

            try
            {
                if(await this.kubernetesClient.Get<PerceptiveEngineV1>(resourceName, resourceNamespace) == null)
                {
                    logger.LogInformation($"{resourceName} is already deleted. No reconciliation required. Skipping reconciliation");
                    return null;
                }

                //do something with the 'resource', e.g., get and update or instantiate an AI pipeline based on the details in the object.

                //kubernetesClient.Get<V1DaemonSet>("", )

                //update state
                resource.Status.State = ResourceStates.Running;
                await this.kubernetesClient.UpdateStatus(resource);

                this.logger.LogInformation($"Updated resource status to '{resource.Status.State}'");
                await finalizerManager.RegisterFinalizerAsync<PerceptiveEngineV1Finalizer>(resource);
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

        public Task StatusModifiedAsync(PerceptiveEngineV1 resource)
        {
            logger.LogInformation($"{resource.Name()} called {nameof(StatusModifiedAsync)}.");

            return Task.CompletedTask;
        }

        public async Task DeletedAsync(PerceptiveEngineV1 resource)
        {
            logger.LogInformation($"{resource.Name()} called {nameof(DeletedAsync)}.");
        }

    }
}