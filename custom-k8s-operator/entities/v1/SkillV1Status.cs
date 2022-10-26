//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

namespace custom_k8s_operator.entities.v1
{
    public class SkillV1Status
    {
        public string CurrentState { get; set; }

        public string DeploymentName { get; set; }

        public string ConfigMapName { get; set; }
    }
}
