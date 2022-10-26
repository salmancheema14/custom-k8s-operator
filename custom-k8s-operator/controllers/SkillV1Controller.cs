//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using custom_k8s_operator.finalizers.v1;
using custom_k8s_operator.entities;
using custom_k8s_operator.entities.v1;
using DotnetKubernetesClient;
using k8s.Models;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Finalizer;
using KubeOps.Operator.Rbac;
using Microsoft.Rest;

namespace custom_k8s_operator.controllers
{
    [EntityRbac(typeof(SkillV1), Verbs = RbacVerb.All)]
    public class SkillV1Controller : IResourceController<SkillV1>
    {
        private readonly IFinalizerManager<SkillV1> finalizerManager;
        private readonly ILogger<SkillV1Controller> logger;
        private readonly IKubernetesClient kubernetesClient;

        public SkillV1Controller(
            ILogger<SkillV1Controller> logger, 
            IKubernetesClient kubernetesClient, 
            IFinalizerManager<SkillV1> finalizerManager)
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
        public async Task<ResourceControllerResult> ReconcileAsync(SkillV1 skill)
        {
            var skillName = skill.Name();
            var skillNamespace = skill.Namespace();

            logger.LogInformation($"Reconciling '{skillName}', Kind='{skill.Kind}' in namespace '{skillNamespace}'");

            try
            {
                var currentSkill = await this.kubernetesClient.Get<SkillV1>(skillName, skillNamespace);
                if (currentSkill == null)
                {
                    logger.LogInformation($"{skillName} is already deleted. No reconciliation required. Skipping reconciliation");
                    return null;
                }

                //1. Run a 'Job' to hook up a sensor
                //2. Create a ConfigMap to store Skill configuration
                var cfgName = await CreateOrUpdateConfigMapAsync(skillName, skillNamespace, skill);
                //3. Create a Deployment with 1 pod running the skill image
                //4. Create a Service hooked up to the deployment 

                //write updated status to storage
                skill.Status.ConfigMapName = cfgName;
                skill.Status.CurrentState = SkillState.Created;
                await this.kubernetesClient.UpdateStatus(skill);

                await finalizerManager.RegisterFinalizerAsync<SkillV1Finalizer>(skill);
                return null; // returning 'null' will stop retries for reconciliation
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

        private async Task<string> CreateOrUpdateConfigMapAsync(string skillName, string skillNamespace, SkillV1 skill)
        {
            var config = await this.kubernetesClient.Get<V1ConfigMap>(skillName, skillNamespace);
            config ??= new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = skillName,
                    NamespaceProperty = skillNamespace
                }
            };

            config.Data["verboseLogging"] = skill.Spec.EnableVerboseLogging.ToString();
            config.Data["enableSilentRecognitionMode"] = skill.Spec.EnableVerboseLogging.ToString();
            config.Data["samplingIntervalInHours"] = skill.Spec.EnableVerboseLogging.ToString();

            //Save is equivalent to CreateOrUpdate. Alternatively, we can use Create and/or Update methods from the k8s client
            var updatedConfig = await this.kubernetesClient.Save<V1ConfigMap>(config);
            return updatedConfig.Name();
        }

        public Task StatusModifiedAsync(SkillV1 entity)
        {
            logger.LogInformation($"{entity.Name()} called {nameof(StatusModifiedAsync)}.");

            //maybe emit a metric for tracking operation frequency?

            return Task.CompletedTask;
        }

        public async Task DeletedAsync(SkillV1 skill)
        {
            logger.LogInformation($"{skill.Name()} called {nameof(DeletedAsync)}.");

            await this.kubernetesClient.Delete<V1ConfigMap>(skill.Name(), skill.Namespace());
        }
    }
}