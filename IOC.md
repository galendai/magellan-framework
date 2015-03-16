As applications become more complicated, modern WPF applications can benefit immensely from IOC containers. Magellan was designed to work with IOC containers, though to keep it simple and accessible, it doesn't require one out of the box.

Magellan's extensibility points make using a container easy. Magellan resolves and instantiates only two types of objects:

  * Controllers
  * Views

# Controller Factory #

To take control of resolving controllers, implement the `IControllerFactory` interface. Here is a simple example using [Autofac](http://code.google.com/p/autofac):

```
public class AutofacControllerFactory : IControllerFactory
{
    private readonly IContainer _container;

    public AutofacControllerFactory(IContainer container)
    {
        _container = container;
    }

    public IController CreateController(ResolvedNavigationRequest request, string controllerName)
    {
        var controller = _container.ResolveNamed<IController>(controllerName);
        return new ControllerFactoryResult(controller);
    }
}
```

The `ControllerFactoryResult` has a second overload that also allows you to pass a callback that is invoked when the request has been processed - this can be used for disposing the controller if required.

Once you have written your controller factory, you must supply it to the `ControllerRouteCatalog`:

```
var builder = new ContainerBuilder();
builder.RegisterType<HomeController>().Named<IController>("Home");
builder.RegisterType<SettingsController>().Named<IController>("Settings");

var container = builder.Build();

var routes = new ControllerRouteCatalog(new AutofacControllerFactory(container));
```

When a navigation request is processed, the Navigator will consult the controller factory. It will use this to ask the factory for a controller, then execute the controller. The Dispose method will be called as soon as the navigation request has completed.

Typically, each navigation will create a new controller - if you want to use the same controller, you could have your controller factory recycle the objects, or make them singletons.

# ViewModel Factory #

TODO: Need to write the same thing as above, this time for MVVM.

# Views #

The second object that Magellan creates is views. This is done through the `IViewActivator` implementation. The default implementation looks for a public, parameterless constructor.

This time, let's use Castle Windsor to control view instantiation.

```
public class WindsorViewActivator : IViewActivator
{
    private readonly IWindsorContainer _container;

    public WindsorViewActivator(IWindsorContainer container)
    {
        _container = container;
    }

    public object Instantiate(Type viewType)
    {
        return _container.Resolve(viewType);        
    }
}
```

We then need to tell the view engines that it is available. Since the view engines are automatically registered, we need to manually re-register them:

```
var activator = new WindsorViewActivator(container);
routes.ViewEngines.Clear();
routes.ViewEngines.Add(new PageViewEngine(activator));
routes.ViewEngines.Add(new WindowViewEngine(activator));
```