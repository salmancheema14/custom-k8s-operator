# C# Operator Sample for Kubernetes (using KubeOps)

This project has skeleton C# code to build and deploy a toy operator on your choice of kubernetes cluster.   

The code sample uses the [KubeOps](https://buehler.github.io/dotnet-operator-sdk/) Library, along with its dotnet CLI extensions.    

Check out the getting started guide [here](https://buehler.github.io/dotnet-operator-sdk/docs/getting_started.html).     

*Note: The instructions below describe how to build and run this project using a local minikube cluster. However, if your `kubectl` is already configured to manage an existing cluster (AKS/Kind/other), feel free to use that as a target. As long as `kubectl` has appropriate access to a K8S cluster, the build and deploy instructions should work*.    

---

## What are Kubernetes Operators and why do we need them?

Kubernetes Operators ([docs](https://kubernetes.io/docs/concepts/extend-kubernetes/operator/)) allow us to extend the capabilities of the Kubernetes control plane. We can use them to enable orchestration, automation and management of customized workflows. 

* Take a look at [OperatorHub](https://operatorhub.io/) to get an idea of the range of current operators. Each entry in operator hub also lists a maturity level. These are useful for understanding the range of tasks that can be done with the operator. 
* To create an operator, two things are required:     
    * `CustomResourceDefinition` (CRD): This is an api-level object in the kubernetes control plane, and can be created by applying a yaml file via `kubectl`. A CRD is essentially the contract for custom resources that will be consumed by a custom operator. Try executing `kubectl explain CustomResourceDefinition` to see details of the contract. [docs](https://kubernetes.io/docs/reference/kubernetes-api/extend-resources/custom-resource-definition-v1/)     
    * `Controller`: A controller allows custom reconciliation inside the Kubernetes control loop. Kubernetes resources implement a state-seeking pattern by default, and the `api-server` takes care of CRUD operations for all resources. Controllers can have domain-specific code that defines how to bring the current state of cluster resources in sync with the desired state. [Docs](https://kubernetes.io/docs/concepts/architecture/controller/)     
* A `CustomResource` is a single instantiation of a `CustomResourceDefinition` and can be created by using `kubectl` to apply a yaml file. [Docs](https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/)
* Here is a good [discussion](https://kubernetes.io/docs/concepts/architecture/controller/) of when operators might be needed.
    

### Operator Maturity Levels
There appear to be 5 accepted levels of maturity for Kubernetes Operators ([Redhat Docs](https://operatorframework.io/operator-capabilities/)). 
1. Basic Install : Provision Required Resources
2. Upgrades : minor and patch version upgrades
3. Full Lifecycle Support : storage, app lifecycles, backups, recovery
4. Insights : deep metrics, analytics and logging
5. Auto-Pilot : Automatic Scaling (vert/horizontal), Automatic config tuning

### Further Reading / Resources

1. CNCF Operator White Paper [link](https://github.com/cncf/tag-app-delivery/blob/eece8f7307f2970f46f100f51932db106db46968/operator-wg/whitepaper/Operator-WhitePaper_v1-0.md)
2. KubePlus Guidelines [link](https://github.com/cloud-ark/kubeplus/blob/master/Guidelines.md)
3. Best Practices [link](https://cloud.google.com/blog/products/containers-kubernetes/best-practices-for-building-kubernetes-operators-and-stateful-apps)
4. Example Operators (with source code)    
   a. Prometheus [link](https://github.com/coreos/prometheus-operator)   
   b. etcd [link](https://github.com/coreos/etcd-operator)

---

## Pre-Requisites

1. Dotnet 6.0
2. Visual Studio. This is optional, but highly useful (The project has been tested using Visual Studio 2022) 
3. Install `kubectl`on your machine, if not already installed.    
    a. If you have Azure CLI installed, you can use `az aks install-cli` command in Windows Powershell to install kubectl.    
    b. Remember to update `$PATH` variable on windows machines
4. Install a Container or Virtual Machine Manager on your machine, e.g., Docker, HyperV etc.     
     a. If you have docker already, you're probably good. (in my test setup, I had docker inside a linux VM, and installed hyper-V on my windows machine).     
     b. To enable Hyper-V, open a Windows Powershell window in Administrator mode, and run 
     ```
     Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All
     ```
6. Install Minikube on your machine ([instructions](https://minikube.sigs.k8s.io/docs/start/)).
7. Open a command prompt in Administrator mode, and start a local minikube cluster, using `minikube start`.
8. Starting your local cluster for the first time does two things:      
    a. A context entry is configured in the local kubeconfig file for the minikube cluster.
    b. The current context in `kubectl` is set to point to your local minikube cluster.
    
**Verification**    

Try running the following `kubectl` commands to confirm its connection to your local minikube cluster
```
kubectl cluster-info
kubectl get nodes
kubectl api-resources
```

**Troubleshooting**    

If your `kubectl` context is not set properly, try the following:    
1. Use `kubectl config current-context` to see the name of the target cluster.    
2. Use `kubectl config get-contexts` to see a list of all available contexts. This list should include an entry for your minikube cluster.    
3. Use `kubectl config use-context minikube` to set the current context explicitly to minikube, if not already set.   

**Minikube-specific Things**     

You can always safely destroy your local minikube cluster and re-initialize it by 
```
minikube stop
minikube delete
minikube start
```

Minikube also comes with a mildly useful dashboard. You can start it by executing `minikube dashboard`, which will trigger a browser window. 


---

## Build and Deploy Toy Operator to your Kubernetes Cluster

1. Clone this repository. 
2. Open a command prompt and navigate to the root folder of this repo (which contains the custom-k8s-operator.sln file).
3. Run the following commands to clean and build the project (Alternatively,  you can open the solution in Visual Studio, and build it)
```
dotnet clean
dotnet build
``` 
4. The build process will generate a `config` folder in your project, with CRD and Kustomization files that can be used to install your operator to a K8S cluster.
5. Open the project folder (with the .csproj file) and use `dotnet run install` command to install your CRD onto your cluster.
6. If installation is successful, you will see a new resource type when you run `kubectl api-resources`.
7.  Open the helm chart located in `<repository>\helm\`, using your favorite text editor, and customize the namespace and name parameters.
8.  Use kubectl to apply the yml file to your cluster `kubectl apply -f Skill.yml`.
9.  If you see an error about missing namespace, you can use kubectl to create the namespace, e.g., `kubectl create namespace "salman-ns"`.
10. Open Visual Studio and do the following:    
     a. Open the `SkillV1Controller.cs` file and put a breakpoint in the `ReconcileAsync` method.     
     b. Start Debugging the project. This will launch your operator and make it ready to accept calls from `kubectl`.     
     c. It will also launch a browser window. Leave it open and ignore it.     
     d. If everything works correctly, your output/debug window should look something like this.     
          
     ![image](https://user-images.githubusercontent.com/105018698/198364388-1d4e2f19-83fb-451d-b795-aacfcd09d74a.png)

11.  The breakpoint should be hit pretty quickly. Resource reconciliation is quite frequent. 
12.  Trace through the code and examine all the K8S resources created as part of deploying the custom skill.     
     a. You can use kubectl to view them, using `kubectl get skills --all-namespaces`, `kubectl get configmaps --all-namespaces`, and so on.     
     b. You can view them in the minikube dashboard. Start the dashboard by `minikube dashboard` and switch to the appropriate namespace. 

