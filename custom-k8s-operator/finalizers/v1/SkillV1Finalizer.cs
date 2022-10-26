using custom_k8s_operator.entities.v1;
using k8s.Models;
using KubeOps.Operator.Finalizer;

namespace custom_k8s_operator.finalizers.v1
{
    public class SkillV1Finalizer : IResourceFinalizer<SkillV1>
    {
        private readonly ILogger<SkillV1Finalizer> logger;

        public SkillV1Finalizer(ILogger<SkillV1Finalizer> logger)
        {
            this.logger = logger;
        }

        public Task FinalizeAsync(SkillV1 entity)
        {
            logger.LogInformation($"Finalizing resource '{entity.Name}' in namespace '{entity.Namespace}'");

            //Do appropriate cleanup on the resource being deleted

            return Task.CompletedTask;
        }
    }
}
