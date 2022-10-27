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
using KubeOps.Operator.Entities.Extensions;
using IdentityModel;

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



                //1. Create a ConfigMap to store Skill configuration
                var cfgName = await CreateOrUpdateConfigMapAsync(skill);
                //2. Create a Deployment with 1 pod running the skill image
                var deploymentName = await CreateOrUpdateDeploymentAsync(skill);
                
                //3. [TODO] Create a Service hooked up to the deployment 

                //write updated status to storage
                skill.Status.CurrentState = SkillState.Created;
                await this.kubernetesClient.UpdateStatus(skill);

                //[EXPLORE] what are finalizers
                //await finalizerManager.RegisterFinalizerAsync<SkillV1Finalizer>(skill);
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
            await this.kubernetesClient.Delete<V1Deployment>(skill.Name(), skill.Namespace());
        }

        #region Code to construct Kubernetes resources for the given skill

        private async Task<string> CreateOrUpdateConfigMapAsync(SkillV1 skill)
        {
            var skillName = skill.Name();
            var skillNamespace = skill.Namespace();

            var config = await this.kubernetesClient.Get<V1ConfigMap>(skillName, skillNamespace);
            bool configExists = config != null;
            config ??= new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = skillName,
                    NamespaceProperty = skillNamespace
                },
                Data = new Dictionary<string, string>()
            };

            config.Data["verboseTelemetry"] = skill.Spec.EnableVerboseTelemetry.ToString();
            config.Data["platform"] = skill.Spec.Platform;
            config.Data["samplingIntervalInHours"] = skill.Spec.SamplingIntervalInHours.ToString();

            //[EXPLORE] What are owner references and sub-resources in kubernetes
            //if(!configExists)
            //{
            //    config.AddOwnerReference(skill.MakeOwnerReference());
            //}

            //Save is equivalent to CreateOrUpdate. Alternatively, we can use Create and/or Update methods from the k8s client
            var updatedConfig = await this.kubernetesClient.Save<V1ConfigMap>(config);
            return updatedConfig.Name();
        }

        private async Task<string> CreateOrUpdateDeploymentAsync(SkillV1 skill)
        {
            var skillName = skill.Name();
            var skillNamespace = skill.Namespace();

            var clusterResource = await this.kubernetesClient.Get<V1Deployment>(skillName, skillNamespace);

            V1Deployment updatedResource;
            if (clusterResource == null)
            {
                var targetResource = CopySkillToDeploymentResource(new V1Deployment(), skill);

                //make current entity an owner of child resource
                //targetResource.AddOwnerReference(skill.MakeOwnerReference());
                updatedResource = await kubernetesClient.Create(targetResource);
                this.logger.LogInformation($"Child resource '{targetResource.Name()}' was created.");
            }
            else
            {
                var targetResource = CopySkillToDeploymentResource(clusterResource, skill);
                updatedResource = await this.kubernetesClient.Update(targetResource);
                this.logger.LogInformation($"Child resource '{targetResource.Name()}' was updated.");
            }

            return updatedResource.Name();
        }

        private static V1Deployment CopySkillToDeploymentResource(V1Deployment resource, SkillV1 skill)
        {
            resource.ApiVersion = "apps/v1";
            resource.Kind = "Deployment";

            // Update metadata
            resource.Metadata = new V1ObjectMeta
            {
                Name = skill.Name(),
                NamespaceProperty = skill.Namespace(),
                Labels = new Dictionary<string, string>()
                {
                    { "app", skill.Name() }
                }
            };

            // Update Spec, Assumption: Percept workloads will have deployments with 1 replica only
            resource.Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector()
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        { "app", skill.Name() }
                    }
                }
            };

            // Set new Containers in the deployment
            //Additional Reading: https://kubernetes.io/docs/concepts/containers/images/
            resource.Spec.Template ??= new V1PodTemplateSpec();
            resource.Spec.Template.Metadata ??= new V1ObjectMeta
            {
                Labels = new Dictionary<string, string>
                {
                    { "app", skill.Name() }
                }
            };

            resource.Spec.Template.Spec ??= new V1PodSpec();
            resource.Spec.Template.Spec.Containers = new List<V1Container>
            {
                new V1Container()
                {
                    Name = skill.Name(),
                    Image = skill.Spec.ImageUri,
                    ImagePullPolicy = "IfNotPresent"
                }
            };

            return resource;
        }

        #endregion
    }
}