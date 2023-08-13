# Setup 

Lets get started with UWB. We first need to setup your Unity project for UWB.

## Prerequisites

```
Unity 2021.3.x
```

Newer Unity versions should work, but are untested!

## Registry Setup

UWB packages are hosted on our UPM registry. As such, the Voltstro UPM registry needs to be added to your project.

To setup the registry with your project, [see here](https://github.com/Voltstro/VoltstroUPM#setup). The Voltstro UPM page also lists some other info that you may be interested in.

**HOWEVER**, an additional scope needs to be added. You need to make sure `com.cysharp.unitask` is added (more details are provided in the [UniTask part](#unitask)). Once you are done configuring your projects registries, your configuration should look like:

![Registry](~/assets/images/articles/user/setup/Registry.webp)

> [!INFO]
> If you are using [UnityNuGet](https://github.com/xoofx/UnityNuGet), and you choose not to use Voltstro UPM as a UnityNuGet mirror, then don't add the `org.nuget` scope to the Voltstro UPM registry entry.

### UniTask

> [!INFO]
> If you are not already using UniTask, you can [skip this part](#installation).

The reason why we need to add the additional `com.cysharp.unitask` scope to VoltstroUPM is because UWB depends on [UniTask](https://github.com/Cysharp/UniTask). VoltstroUPM does provide a mirror copy (from OpenUPM) of UniTask,
however you may already have UniTask installed via [OpenUPM](https://openupm.com/packages/com.cysharp.unitask/), or via [Git](https://github.com/Cysharp/UniTask#install-via-git-url). If you do have it installed already,
and you don't want to use Voltstro UPM's mirror of it, then DO NOT define the additional scope as apart of VoltstroUPM.

> [!WARNING]
> If you already have UniTask installed via Git, please make sure it is the latest version!

## Installation

Once you have the Voltstro UPM registry setup, you can now install the packages via the UPM GUI.

![Packages](~/assets/images/articles/user/setup/Packages.webp)

### Packages

UWB is split into multiple packages. The packages that you need will depend on your use case.

You will need the core "Unity Web Browser" package (`dev.voltstro.unitywebbrowser`). An "engine" package (and it's "engine native" package) is also required. For more details on what engines are available and their packages, see the [engines section](Engines.md).

For more information on the different packages, see the [packages section](Packages.md).

Once you are ready, you can move onto the [usage section](Usage.md).
