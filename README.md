# C# Operator Sample for Kubernetes (using KubeOps)

This project has skeleton C# code to build and deploy a toy operator on your choice of kubernetes cluster.   

The code sample uses the [KubeOps](https://buehler.github.io/dotnet-operator-sdk/) Library, along with its dotnet CLI extensions.    

Check out the getting started guide [here](https://buehler.github.io/dotnet-operator-sdk/docs/getting_started.html).     

*Note: The instructions below describe how to build and run this project using a local minikube cluster. However, if your `kubectl` is already configured to manage an existing cluster (AKS/Kind/other), feel free to use that as a target. As long as `kubectl` has appropriate access to a K8S cluster, the build and deploy instructions should work*.    

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

