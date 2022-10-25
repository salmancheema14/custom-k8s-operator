using custom_k8s_operator.models.V1;
using k8s.Models;
using KubeOps.Operator.Finalizer;

namespace custom_k8s_operator.finalizers.V1
{
    public class PerceptiveEngineV1Finalizer : IResourceFinalizer<PerceptiveEngineV1>
    {
        private readonly ILogger<PerceptiveEngineV1Finalizer> logger;

        public PerceptiveEngineV1Finalizer(ILogger<PerceptiveEngineV1Finalizer> logger)
        {
            this.logger = logger;
        }

        public Task FinalizeAsync(PerceptiveEngineV1 entity)
        {
            logger.LogInformation($"Finalizing resource '{entity.Name}' in namespace '{entity.Namespace}'");

            //Do appropriate cleanup on the resource being deleted

            return Task.CompletedTask;
        }
    }
}
