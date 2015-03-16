# Introduction #

At the core, Magellan is a routing engine. The process works something like:

  1. A `NavigationRequest` is received. This might be in the form of:
    * a user-entered URI, e.g., `myapp://customers/edit/123`
    * a route dictionary, e.g., `new { controller = "Customers", action = "Edit", id = 123 }`
  1. A `Navigator` consults a collection of `RouteCatalogs`, trying to find a registered route matching the request
  1. If the route is matched, it produces a `RouteHandler` to execute the request

If you'd used ASP.NET Routing, you'll find it fairly similar.

# Why have URI's at all? #

URI's are commonly associated with the web. Why would you need them in a rich client application?

  1. They provide a loosely coupled navigation mechanism. Page A can link to page B via a URI, without any direct code dependency
  1. They can be shown to users, and even typed in by them

WPF applications can make use of [URL Protocol Handlers in Windows](http://msdn.microsoft.com/en-us/library/aa767914(VS.85).aspx) to map a URI scheme to an executable. This allows you to register `yourapp://` to your exe, and have any URL using that scheme launch your application and execute the request.

# Route catalogs #

When you create a `NavigatorFactory`, you provide it with one or more `RouteCatalogs`. `RouteCatalog` is abstract, so you need to find a concrete type.

Out of the box, Magellan provides two:

  1. `ControllerRouteCatalog`, which you can learn more about in the [MVC](MVC.md) section
  1. `ViewModelRouteCatalog`, which you can learn more about in the [MVVM](MVVM.md) section

# Creating your own route handlers #

OK, suppose we want to create a custom route handler that will show a message box, instead of using MVC or MVVM.

We might want our registration system to look like:

```
var routes = new MessageBoxRouteCatalog();
routes.MapRoute("message/notallowed", () => "That action is not allowed");
routes.MapRoute("message/error", () => "Ow, that hurt!");

var navigation = new NavigatorFactory("myapp", routes);
```

To build it, we'll need to create the `MessageBoxRouteCatalog` class:

```
public class MessageBoxRouteCatalog : RouteCatalog
{
    public void MapRoute(string spec, Func<string> messageBuilder)
    {
        var route = new Route(
            spec, 
            () => new MessageBoxRouteHandler(messageBuilder), 
            new RouteValueDictionary(), 
            new RouteValueDictionary(), 
            new RouteValidator());

        Add(route);
    }
}
```

We also need to create the `RouteHandler`, which is invoked when the route is matched and executed:

```
public class MessageBoxRouteHandler : IRouteHandler
{
    private readonly Func<string> _messageBuilder;

    public MessageBoxRouteHandler(Func<string> messageBuilder)
    {
        _messageBuilder = messageBuilder;
    }

    public void ProcessRequest(ResolvedNavigationRequest request)
    {
        MessageBox.Show(_messageBuilder());
    }
}
```

That's all there is to it! If someone navigates to your message handler:

```
navigator.Navigate("myapp://message/error");
```

Your `MessageBoxRouteHandler` will be invoked, and the error message will appear on screen.

# Route parameter segments #

Suppose instead of hard-coding the messages, you decide to turn them into parameters. Your registration syntax might be:

```
var routes = new MessageBoxRouteCatalog();
routes.MapRoute("message/error/{message}", () => "Error: {0}");
routes.MapRoute("message/warning/{message}", () => "Warning: {0}");
```

Notice the `{message}` part? That's a `parameter segment`. Magellan's routing engine will extract whatever is passed in to that value and make it available to your route handler.

That just leaves a minor change to the route handler:

```
public class MessageBoxRouteHandler : IRouteHandler
{
    private readonly Func<string> _messageFormatBuilder;

    public MessageBoxRouteHandler(Func<string> messageFormatBuilder)
    {
        _messageFormatBuilder = messageFormatBuilder;
    }

    public void ProcessRequest(ResolvedNavigationRequest request)
    {
        var format = _messageFormatBuilder();
        var message = request.RouteValues["message"];

        MessageBox.Show(string.Format(format, message));
    }
}
```

# Route validation #

Routes are automatically validated when they are registered. In our case, the `message` parameter is required. To stop someone executing an invalid route, we'll make sure they supply it. We do this by creating a custom `RouteValidator`:

```
public class MessageBoxRouteValidator : RouteValidator
{
    public MessageBoxRouteValidator()
    {
        Rules.Add(MustIncludeMessageSegment);
    }

    private static RouteValidationResult MustIncludeMessageSegment(Segment[] segments, RouteValueDictionary defaults, RouteValueDictionary constraints)
    {
        var hasMessage = segments.OfType<ParameterSegment>()
            .Any(x => x.ParameterName == "message");

        var hasDefaultMessage = defaults.ContainsKey("message");

        if (!hasMessage && !hasDefaultMessage)
        {
            return RouteValidationResult.Failure("You forgot to include a '{message}' segment, dummy!");
        }
        return RouteValidationResult.Successful();
    }
}
```

You then need to pass it to the route:

```
public class MessageBoxRouteCatalog : RouteCatalog
{
    public void MapRoute(string spec, Func<string> messageFormatBuilder)
    {
        var route = new Route(
            spec, 
            () => new MessageBoxRouteHandler(messageFormatBuilder), 
            new RouteValueDictionary(), 
            new RouteValueDictionary(), 
            new MessageBoxRouteValidator());

        Add(route);
    }
}
```

Now, if someone forgot to specify the `{message}` portion of a route:

```
routes.MapRoute("message/warning", () => "Warning: {0}");
```

They'd receive:

```
InvalidRouteException:
The route with specification 'message/warning' is invalid: 
 - You forgot to include a '{message}' segment, dummy!.
```

# Defaults #

We've forced developers to include a `{message}`, but it's harder to force users to always enter a message if entering the URL manually. For example, the following navigation request:

```
routes.MapRoute("message/error/{message}", () => "Error: {0}");
...
mainWindow.MainNavigator.Navigate("tax://message/error");
```

Would throw:

```
The request URI 'tax://message/warning' could not be routed. 
- Route with specification 'message/warning/{message}' did not match: The path does not contain a segment for parameter 'message'.
```

We can fix it by allowing defaults to be registered for a route:

```
routes.MapRoute("message/error/{message}", new { message = "Unspecified error" }, () => "Error: {0}");
```

In this case, our custom `RouteHandler` will receive the message, if supplied, or 'Unspecified error' if no message is included in the route.

To enable this, we just need to pass the defaults through to the `Route`, probably by a `MapRoute` overload:

```
public void MapRoute(string spec, object defaults, Func<string> messageFormatBuilder)
{
    var route = new Route(
        spec,
        () => new MessageBoxRouteHandler(messageFormatBuilder),
        new RouteValueDictionary(defaults),
        new RouteValueDictionary(),
        new MessageBoxRouteValidator());

    Add(route);
}
```

As with ASP.NET routing, you can also use `UrlParameter.Optional` instead of using `null`, if you want to make a distinction between null and not supplied.

# Summary #

Magellan is a navigation framework, a core function of which is routing. Magellan's higher-level presentation pattern support (MVC and MVVM) uses the routing functionality, but it's easy to extend and add your own route formats and handlers.