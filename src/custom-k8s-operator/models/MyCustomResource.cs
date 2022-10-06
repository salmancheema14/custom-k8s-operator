using k8s.Models;
using KubeOps.Operator.Entities;

namespace custom_k8s_operator.models
{
    [KubernetesEntity(ApiVersion = "v1apha1", Group = "percept.azure.windows.net", Kind = "MyCustomResource", PluralName = "MyCustomResources")]
    public class MyCustomResource : CustomKubernetesEntity<MyCustomResourceSpec, MyCustomResourceState>
    {
    }
}