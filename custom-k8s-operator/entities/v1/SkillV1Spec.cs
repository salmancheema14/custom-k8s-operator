//  <copyright company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>

namespace custom_k8s_operator.entities.v1
{
    public class SkillV1Spec
    {
        public string SkillName { get; set; }

        public bool EnableSilentRecognitionMode { get; set; }

        public bool EnableVerboseLogging { get; set; }

        public int SamplingIntervalInHours { get; set; }

        public string Sensor { get; set; }

        public string Platform { get; set; }

        public string SourceImageUri { get; set; }

        public string RuntimeState { get; set; }
    }
}