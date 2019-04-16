# SocketLeakDetection

Stand-alone application and Akka.NET actors who can detect socket leaks and port exhaustion anywhere on the local system.

## Building this solution
To run the build script associated with this solution, execute the following:

**Windows**
```
c:\> build.cmd all
```

**Linux / OS X**
```
c:\> build.sh all
```

If you need any information on the supported commands, please execute the `build.[cmd|sh] help` command.

This build script is powered by [FAKE](https://fake.build/); please see their API documentation should you need to make any changes to the [`build.fsx`](build.fsx) file.

### Conventions
The attached build script will automatically do the following based on the conventions of the project names added to this project:

* Any project name ending with `.Tests` will automatically be treated as a [XUnit2](https://xunit.github.io/) project and will be included during the test stages of this build script;
* Any project name ending with `.Tests` will automatically be treated as a [NBench](https://github.com/petabridge/NBench) project and will be included during the test stages of this build script; and
* Any project meeting neither of these conventions will be treated as a NuGet packaging target and its `.nupkg` file will automatically be placed in the `bin\nuget` folder upon running the `build.[cmd|sh] all` command.

### DocFx for Documentation
This solution also supports [DocFx](http://dotnet.github.io/docfx/) for generating both API documentation and articles to describe the behavior, output, and usages of your project. 

All of the relevant articles you wish to write should be added to the `/docs/articles/` folder and any API documentation you might need will also appear there.

All of the documentation will be statically generated and the output will be placed in the `/docs/_site/` folder. 

#### Previewing Documentation
To preview the documentation for this project, execute the following command at the root of this folder:

```
C:\> serve-docs.cmd
```

This will use the built-in `docfx.console` binary that is installed as part of the NuGet restore process from executing any of the usual `build.cmd` or `build.sh` steps to preview the fully-rendered documentation. For best results, do this immediately after calling `build.cmd buildRelease`.

### Release Notes, Version Numbers, Etc
This project will automatically populate its release notes in all of its modules via the entries written inside [`RELEASE_NOTES.md`](RELEASE_NOTES.md) and will automatically update the versions of all assemblies and NuGet packages via the metadata included inside [`common.props`](src/common.props).

If you add any new projects to the solution created with this template, be sure to add the following line to each one of them in order to ensure that you can take advantage of `common.props` for standardization purposes:

```
<Import Project="..\common.props" />
```

### Code Signing via SignService
This project uses [SignService](https://github.com/onovotny/SignService) to code-sign NuGet packages prior to publication. The `build.cmd` and `build.sh` scripts will automatically download the `SignClient` needed to execute code signing locally on the build agent, but it's still your responsibility to set up the SignService server per the instructions at the linked repository.

Once you've gone through the ropes of setting up a code-signing server, you'll need to set a few configuration options in your project in order to use the `SignClient`:

* Add your Active Directory settings to [`appsettings.json`](appsettings.json) and
* Pass in your signature information to the `signingName`, `signingDescription`, and `signingUrl` values inside `build.fsx`.

Whenever you're ready to run code-signing on the NuGet packages published by `build.fsx`, execute the following command:

```
C:\> build.cmd nuget SignClientSecret={your secret} SignClientUser={your username}
```

This will invoke the `SignClient` and actually execute code signing against your `.nupkg` files prior to NuGet publication.

If one of these two values isn't provided, the code signing stage will skip itself and simply produce unsigned NuGet code packages.

##Project Description

This project is designed to detect a sudden increase in the TCP port count in a system, and gracefully shutdown the `ActorSystem` when this is observed. 

###Methodology 

The impulse response is done through the use of an Exponential Weighted Moving Average(EWMA). `EWMA` allows the observer to determine how much weight we want to emphasize in older readings versus newer readings. 

To calculate when a large increase in TCP ports is detected, two EWMA's will be taken. One will have a short sample size and the other will have a larger sample size. Having a shorter sample size will result in the average being more responsive to change. Where a larger sample size will smooth out quick changes and be less responsive to change. 

Using this behavior, we compare the two EMWA's and determine the difference between the two average. When there is a spike in the readings, the difference between the two averages will increase, with a fast increase resulting in a larger difference between the two averages. 

###Implementation

The project is intended to help detect and warn when an increase in TCP port count is expected for one or more network interfaces. The project will be implemented by the creation of a `TcpPortUseSupervise` actor. This actor will be created using the `ActorSystem` that we want to monitor and terminate if a large increase in TCP connections is detected. 

[A work flow of the below TCP port detection](docs/images/Message-Decision-Tree.png)

The `TcpPortUseSupervise` actor will create a `TcpPortMonitoring` actor which will scan all of the open TCP ports and group these by the local endpoints.  The count for each local endpoint will then be sent back to the `TcpPortUseSupervise` actor. 

The `TcpPortUseSupevise` actor will then take these readings and determine if the number of TCP ports exceeds our normal operating numbers(`MinPorts`). If the `MinPorts` number is exceeded, the actor will then signal the `SocketLeakdetectorActor` to begin monitoring the TCP ports for that particular endpoint. 

The `SocketLeakDetectorActor` will then continue to monitor the TCP port count for the specified endpoint through the use of the EWMA. When a fast increase in the TCP count is observed that exceeds the defined `MaxDifference`, a warning will be logged and a timer will be started. If the increase does not normalize within the set `BreachDuration` period, this actor will then signal the `TcpPortUseSupevise` actor that we need to terminate the `ActorSystem` to prevent port exhaustion. 

If the `SocketLeakDetectorActor` does not see a fast increase in the TCP port count but does see a continuous increase in the TCP port count, it will continue to monitor that local endpoint. If the number of TCP ports exceeds the `MaxPorts` allowed and does not drop below the `MinPorts` withing the `BreachDuration` period, the `SocketLeakDetectorActor` will send a termination message back to the `SocketLeakDetectorActor` Actor. 

A `BreachDuration` period is set, to allow the system to normalize in the event that the system experiences a short increase in the TCP port count but is able to lower these ports back to a normal state without outside intervention. 

[The signal shutdown logic can be seen in the below diagram](docs/images/Message-Decision-Tree.png)

###Configuration

You will only need to create the `TcpPortUseSupervise` actor to be able to monitor the TCP port growth in your system. This actor comes with default settings and does not require you to pass any settings if you do not want to edit these. You create your own `SocketLeakDetectorSettigns` to modify how responsive you want your system to be. The following are the allowed configurations: 

####SocketLeakDetectorSettings

MaxPorts: The maximum allowed TCP ports for a set endpoint. If a port count for a particular endpoint exceeds this number, we will signal for a system shutdown.

You 
 
The default number is set to 100 ports.  you want to s 

MaxDifference: Thi

