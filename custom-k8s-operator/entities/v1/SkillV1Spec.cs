//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

namespace custom_k8s_operator.entities.v1
{
    public class SkillV1Spec
    {
        public bool EnableVerboseTelemetry { get; set; }

        public int SamplingIntervalInHours { get; set; }

        public string? Platform { get; set; }

        public string? ImageUri { get; set; }
    }
}