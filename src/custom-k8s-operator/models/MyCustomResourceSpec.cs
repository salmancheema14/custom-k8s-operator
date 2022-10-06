namespace custom_k8s_operator.models
{
    public class MyCustomResourceSpec
    {
        public string Name { get; set; }

        public IDictionary<string,string> Properties => new Dictionary<string,string>();
    }
}