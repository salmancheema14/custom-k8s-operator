//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

using k8s.Models;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace custom_k8s_operator.entities.v1
{
    [KubernetesEntity(ApiVersion = "v1", Group = "percept.demo.local", Kind = "Skill", PluralName = "skills")]
    [KubernetesEntityShortNames("sk")]
    public class SkillV1 : CustomKubernetesEntity<SkillV1Spec, SkillV1Status>
    {
    }
}