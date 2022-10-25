//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using k8s.Models;
using KubeOps.Operator.Entities;

namespace custom_k8s_operator.models.V1
{
    [KubernetesEntity(ApiVersion = "v1", Group = "perceptive.azure.windows.net", Kind = "PerceptiveEngine", PluralName = "PerceptiveEngines")]
    public class PerceptiveEngineV1 : CustomKubernetesEntity<PerceptiveEngineV1Spec, PerceptiveEngineV1Status>
    {
    }
}