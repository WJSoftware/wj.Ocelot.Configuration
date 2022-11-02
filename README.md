# wj.Ocelot.Configuration
> Configuration package that builds Ocelot route configurations for microservices in a way that allows easy 
> per-environment overrides.

# Your Feedback Is Important!

This package is in its early stages.  We would like to hear about the most common configured and overridden properties 
of the Ocelot configuration.  Please take just 2 minutes of your time to visit this [issue in Github](https://github.com/WJSoftware/wj.Ocelot.Configuration/issues/1) 
to tell us about your Ocelot configuration needs.

# Quickstart

1. Install the nuget package.
2. Create a class that inherits from `GatewayRoutes`.
3. Add one property per microservice of type `OcelotRouteGroup<OcelotRoute>`.
4. Open your `appsettings.json` file and add a new section for your Ocelot configuration.
5. Inside this new section, follow the new arrangement to configure Ocelot (see example below).
6. Add the Ocelot configuration to the configuration builder.

## Quickstart Details

Install the package using your preferred method.  For example, using the `dotnet` CLI:

```powershell
dotnet add package wj.Ocelot.Configuration
```

Now create a new class that derives from `GatewayRoutes`:

```csharp
using RouteGroup =  OcelotRouteGroup<OcelotRoute>;
public class OcelotRoutes : GatewayRoutes<RouteGroup, OcelotRoute>
{
    #region Microservices
    public RouteGroup MicroSvcA { get; set; }
    public RouteGroup MicroSvcB { get; set; }
    // Etc. One property per microservice.
    #endregion
}
```

Now to the `appsettings.json` file.  Something like this.  This is where the usefulness of this package becomes 
evident.  You only specify the microservice's host name, port, scheme and root path once.  This means that any needed 
per-environment configuration override is easily done and is only done once.  For a detailed explanation see the *[Why 
This Package Is Needed](#why-this-package-is-needed)* section.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Ocelot": { // <-----  This is the section of interest.
    "RootPath": "/api", // <--- Usually we assign a root path like this to the gateway microservice in K8s.
    "MicroSvcA": { // <--- The A microservice.
      "Host": "microsvc-a-svc", // <--- Usually the K8s service name.
      "Port": 80, // <--- This is the default because in K8s we usually just do HTTP internally.  Added just for clarity.  This can go.
      "DownstreamScheme": "http", // <--- HTTP is the default, so no need to specify.  Added just for clarity.  This can go.
      "RootPath": "/msvcA", // <--- Root path addition to identify the A microservice routes.  Optional.
      "Routes": [ // <--- Now you only specify per-route stuff.  Host et. al. are inherited from the parent.
        {
          "DownstreamPathTemplate": "/resourceX",
          "UpstreamPathTemplate": "/resourceX", // <--- If equal to DownstreamPathTemplate, don't specify.
          "UpstreamHttpMethod": [
            "Get", "Post"
          ]
        },
        {
          "DownstreamPathTemplate": "/resourceX/{id}",
          // "UpstreamPathTemplate": "/resourceX/{id}", <--- Same as DownstreamPathTemplate, so not specified.
          "UpstreamHttpMethod": [
            "Get", "Put", "Patch", "Delete"
          ]
        }
      ]
    },
    "MicroSvcB": { // <--- This one is the same as MicroSvcA, but taking advantage of the libary features.
      "Host": "microsvc-b-svc",
      "RootPath": "/msvcB",
      "Routes": [
        {
          "DownstreamPathTemplate": "/resourceY",
          "UpstreamHttpMethod": [
            "Get", "Post"
          ]
        },
        {
          "DownstreamPathTemplate": "/resourceY/{id}",
          "UpstreamHttpMethod": [
            "Get", "Put", "Patch", "Delete"
          ]
        }
      ]
    }
  }
}
```

At this point one would go to `appsettings.Development.json` and any other number of appsettings files to do 
per-environment overrides.  If you want to see an example, read the next section.

Finally, the above configuration needs to be translated to the rigid Ocelot configuration format.  Thanks to this 
package, though, this is a breeze and done in 2 lines of code.

For **.Net6** with the simplified `program.cs` file:

```csharp
// Get the configuration we wrote in the appsettings.json files.
var ocelotConfig = builder.Configuration.GetSection("Ocelot").Get<OcelotRoutes>();
// Pass it along to the extension method.
builder.Configuration.AddOcelotConfiguration(ocelotConfig);
```

For **.Net6** projects that don't use the simplified `program.cs` file (such as projects that were migrated to .Net6 
from, say, .Net5):

```csharp
var builder = hostBuilder
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.ConfigureAppConfiguration(configBuilder =>
        {
            var configuration = configBuilder.Build();
            var ocelotConfig = configuration.GetSection("Ocelot").Get<OcelotRoutes>();
            configBuilder.AddOcelotConfiguration(ocelotConfig);
        });
    });
```

And this is it.  Now Ocelot is fully configured.

**IMPORTANT**:  The quickstart example merely shows minimal configuration.  The package actually allows to set 
timeouts and more in the microservice route group, so that the values apply to all of the microservice routes.  See 
below to learn about the ready-to-go Ocelot properties that can be applied to the microservice route group and how to 
create a new class that can provide even more Ocelot route properties at this level.

# Why This Package Is Needed

Ocelot is a practical way to route HTTP calls in a microservice architecture.  However, the configuration is not 
friendly at all when it comes to defining the routes in terms of **DRYness** (Don't Repeat Yourself) and value 
overrides.

So what is the problem with Ocelot's configuration design?  Simple:  The `Routes` property in the configuration JSON 
is an **array**.  The .Net Configuration engine cannot override properties at the *array element* level.  This means 
that even if only one tiny piece of information needs to change in a 1000-line route configuration in a different 
environment, all 1000 lines must be repeated in a new configuration file.

The second problem is that some required values must always be repeated, namely the microservice's host, port number, 
scheme, and there's no single property to set a root path.  This means that common pieces of routes will be repeated 
in the `UpstreamPathTemplate` property in every route.

So **wj.Ocelot.Configuration** was born to re-design the route configuration in a manner that allows easy, 
single-place configuration value declarations that can be as easily overridden by following the rules of the .Net 
Configuration engine.

As an example, let's create the `appsettings.Development.json` file for the [Quickstart](#quickstart-details) example.

Usually, the `Development` environment is the environment used by developers when running a project in their own local 
machines.  This means most likely that a developer that runs the gateway project will also be running some other 
projects, like the *MicroSvcA* project described in the configuration example in **Quickstart**.  This means, that in 
`Development`, the server and port must be reconfigured to `localhost` and whatever port the *MicroSvcA* project is 
being run.

Without this package, the developer would be forced to copy the entire routes configuration to just make the host and 
port changes.  Furthermore, the host and port specification is repeated in every route definition for the same 
microservice, so configuration data must be repeated, disrespecting the **DRY**.  With this package, the route 
information is not duplicated and can actually be targetted for override by the .Net Configuration engine.

This would be the `appsettings.Development.json` file for the **Quickstart** example:

```json
{
  "Ocelot": {
    "MicroSvcA": {
      "Host": "localhost",
      "Port": 7007,
    },
    "MicroSvcB": {
      "Host": "localhost",
      "Port": 7008,
    }
  }
}
```

That tiny environment-specific JSON file will not grow.  The microservices could be defining thousands of different 
routes, and this file would not change one bit.

# What Comes in the Box

**wj.Ocelot.Configuration** comes with 3 basic classes and one extension method (2 overloads).  Each class is used to 
read the new configuration hierarchy at the different levels (individual route, microservice and gateway) and all can 
be used as base classes to increase the number of supported properties.

## The OcelotRoute Class

This is the class used at the inner-most level in the configuration hierarchy:  The individual route level.

This class will, over time and following demand, acquire new properties that mirror the properties in the **Ocelot** 
route configuration.  To enter the technical realm, this class mimics the most popular properties in the `FileRoute` 
class provided by the **Ocelot** package.  This class was born because many of the `FileRoute` properties are not 
nullable, and nullable is something required when doing parent inheritance of property values.

For example, the `FileRoute` class has the `Priority` property, which is of type `int` (as opposed to `int?` or 
`Nullable<int>`).  Because, if not specified, it will have the value `0`, one cannot know if this `0` is because it 
wasn't specified in configuration or if it was specified in configuration as `0` at the individual route level and 
therefore it should not be overridden by the parent's value.

From the above paragraph, one can infer that properties added to this class in the future, or properties added to a 
derived class, should be of nullable return types.  This way, if a value is not specified, the property value will be 
`null`, and it is therefore easy to spot as an opportunity to inherit the value from the route's parent (of type 
`OCelotRouteGroup` or a derived class).

The currently defined properties of this class are:

| Property | Maps to (in `FileRoute`) | Package Version | Remarks |
| - | - | - | - |
| `DownstreamPathTemplate` | `DownstreamPathTemplate` | 0.1.0
| `UpstreamPathTemplate` | `UpstreamPathTemplate` | 0.1.0 | Not a 1:1 relation.  The upstream template inherits the values of `RootPath` from the parents.  Also, if left unspecified in the JSON file (it will be `null`), will acquire the value of `DownstreamPathTemplate`. |
| `UpstreamHttpMethod` | `UpstreamHttpMethod` | 0.1.0
| `TimeOut` | `QoSOptions.TimeoutValue` | 0.1.0 | This one is of type `TimeSpan?` (as opposed to `int` in `FileRoute`). |
| `Priority` | `Priority` | 0.1.0 | The only difference is that this class' property is nullable:  `int?`.

If you are in need of an Ocelot configuration property not found here, simply inherit from `OcelotRoute` and add the 
needed configuration properties.  Note that adding it here will only provide individual-route visiblity.  Apply this 
same reasoning with the `OcelotRouteGroup` class (explained next) if you want the properties to apply to all routes in 
a microservice.

Whenever possible, name the property the same as in Ocelot's `FileRoute` class so the standard mapping algorithm can 
automatically pick it up for mapping.  For this to work, however, the property's data type must be the same (in 
nullable version).  More about this topic [later](#the-property-value-mapping-algorithm).

## The OcelotRouteGroup Class

This is the class used at the middle level in the configuration hierarchy:  The microservice level.

> **ABOUT THE TERM "MICROSERVICE"**: As you probably have noted, the term *microservice* is used a lot here.  Ocelot, 
> however, does not necessarily only work in a microservices scenario.  While this document uses the microservices 
> example because it is easier to understand, strictly speaking a single microservice could have more than one group 
> in the configuration JSON file.  This is why this class is called a **route group**¨.  Routes are logically grouped 
> together to ease configuration.  Per-microservice grouping is not really a requirement.

Most often than not, the same configuration value is repeated endlessly across routes that belong to a specific 
microservice.  This is not **DRY**, so this package provides the means to eliminate this:  Per-microservice settings.

This class' properties, excluding the `Routes` property, also mimic Ocelot's `FileRoute` class, except of course, 
making sure nullable data types are used.  The values present here will apply to all individual routes in the `Routes` 
collection, as long as the individual routes do not specify a value of their own.

Inherit from this class and add properties to expand the number of microservice-level properties available to your 
project.

The currently defined properties of this class are:

| Property | Maps to (in `FileRoute`) | Package Version | Remarks |
| - | - | - | - |
| `Host` | `DownstreamHostAndPort[].Host` | 0.1.0 | Set in tandem with the `Port` property. |
| `Port` | `DownstreamHostAndPort[].Port` | 0.1.0 | Set in tandem with the `Host` property. |
| `DownstreamScheme` | `DownstreamScheme` | 0.1.0
| `TimeOut` | `QoSOptions.TimeoutValue` | 0.1.0 | This one is of type `TimeSpan?` (as opposed to `int` in `FileRoute`). |
| `Priority` | `Priority` | 0.1.0 | The only difference is that this class' property is nullable:  `int?`.
| `RootPath` | As a part of `UpstreamPathTemplate` | 0.1.0 | See [this section](#the-in-box-mapping-algorithm-explained) for details. |

## The GatewayRoutes Class

This is the class used at the top level in the configuration hierarchy:  The gateway level.

This is the only class of the three that is abstract, meaning it can only be used as a base class.  This is to clearly 
indicate that a derived class is required because the library cannot possibly know how many or even if there is a 
minimum number of route groups (microservices, to continue with the term).  In short, the library needs the route 
groups to be defined.

Inherit from this class as shown in the [Quickstart details](#quickstart-details), creating one property for each 
microservice (route group).  These properties will be discovered using reflection, so it is important that they follow 
the instructions regarding the property's data type.

The currently defined properties of this class are:

| Property | Maps to (in `FileRoute`) | Package Version | Remarks |
| - | - | - | - |
| `RootPath` | As a part of `UpstreamPathTemplate` | 0.1.0 | See [this section](#the-in-box-mapping-algorithm-explained) for details. |

## The Extensions Class

This is a static class that defines the `AddOcelotConfiguration()` extension method for the `IConfigurationBuilder` 
interface.

The method comes with 2 overloads.  The simpler one is for the cases where the properties already mapped by the 
library are enough for the gateway application.  In this case, only one custom type will have been made (the gateway 
routes class).  This overload does not allow setting a custom mapper function and will work solely using the in-box 
mapping algorithm.

The second overload, on the other hand, will require the specification of all the types used in configuration (for the 
3 levels gateway, microservice and individual route), and will allow the inclusion of a new mapping algorithm.  This 
is not a requirement, though, because the in-box algorithm can pick up new properties as long as they are named the 
same as in Ocelot's `FileRoute` class, and have the same return type (or its nullable version).

**IMPORTANT**:  It his highly recommended to always use nullable types.

## The Property Value Mapping Algorithm

The in-box property mapping algorithm is based on 2 qualities of the properties:  **Name** and **return type**.  If a 
property is found in `OcelotRoute` or `OcelotRouteGroup` that has the same name and return type as a property in 
Ocelot's `FileRoute` class, it will transfer the value.  As already mentioned, the return type will be considered a 
match if it is either an exact match, or the nullable version of Ocelot's return type.

Once the match has been established, the value is simply calculated by giving the individual route level priority over 
the route group (microservice) level.  In other words, if the individual route specify a value, that value will be 
set; if no individual route value is present, then the route group value is the one set.  If both values are absent, 
then the property will be left with Ocelot's default value.

### The In-Box Mapping Algorithm Explained

Intentionally missing the automatic property match can be helpful.  For example, this library intentionally misses the 
automatic match for the `TimeoutValue` property in several ways.  First of all, it is not part of a sub-object, while 
in Ocelot it is a property in a sub-object.  With this alone the automatic match is guaranteed to fail.  This, 
however, is not the only change.  The matching property is not named the same.  This library has named the property 
`TimeOut`, and its data type is `TimeSpan?`, not `int`.  This is an opinionated decision to more easily specify the 
timeout value.

Another good reason to avoid embedded properties is because the current algorithm does not recursively traverse 
property values to individually set values.  This is an unsupported scenario.  The current algorithm is only capable 
of setting the vlaue of a property with a custom object as a whole.  It will not drill down that object to 
individually set properties.

As a final example, let's talk about the `UpstreamPathTemplate` property.  It is defined with the same name as in the 
Ocelot library, and it has the same data type.  So why is it avoiding the automatic matching?   For 2 reasons:

1. It is built from pieces.  The actual value configured in Ocelot will be the chaining of the gateway's root path, 
the group level's root path, and finally its own value.
2. It will acquire the value from `DownstreamPathTemplate` if left unspecified.  Most often than not and thanks to the 
REST specification, these two will match.  Implementing this copy operation supports the **DRY** principle.

### Providing a New Mapping Function

This is something that is only needed if properties are added at the individual route or group route levels that do 
not match any of Ocelot's `FileRoute` properties as explained previously, or if the value will undergo some extra 
logic like in the examples in the previous section.

The mapping logic is provided when configuring using the `IConfiguraitonBuilder`'s extension method 
`AddOcelotConfiguration()`.  Generally speaking, it is done like this:

```csharp
builder.Configuration.AddOcelotConfiguration<MyRoutes, MyRouteGroup, MyRoute>(ocelotConfig, opt =>
{
    opt.MapperDelegate = (route, parent, rootPath) =>
    {
        FileRoute fr = opt.DefaultMapperDelegate(route, parent, rootPath);
        // Now do whatever you want with the FileRoute object.
        // The route and parent parameters are the individual route and route group level configurations.
        // The rootPath parameter is the gateway's root path.
    };
});
```

As shown in the example, it is highly recommended to always call the default mapper in order to only have to provide 
the additional logic required for the additional properties defined in `MyRoute` or `MyRouteGroup` that aren't matched 
by the in-box algorithm.
