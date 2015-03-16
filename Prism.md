# Introduction #

Magellan was designed to work with Composite WPF from day one. Composite WPF provides support for multiple modules, loosely coupled pub/sub eventing, and regions for sub-dividing zones in the UI. However, Composite WPF does not enforce any particular UI pattern - MVVM, MVP and MVC could all work.

Magellan and Composite WPF can work well together to create a composite navigation-oriented application using the MVC pattern. Here are some examples:

  * Composite WPF modules could contain views and controllers
  * Composite WPF events could be used for navigation - i.e., a Navigate event that could be raised by different modules and services
  * Instead of pages, Magellan view results could return `UserControls` that are added to regions

# Region Support #

For region support, `Magellan.Composite.dll` contains some extensions that can be used. A controller may look like this:

```
public class ShellController : CompositeController 
{
    public ActionResult Explorer()
    {
        return CompositeView("Explorer").InRegion("LeftRegion");
    }
}
```

On application startup, the view can be navigated to via:

```
Navigator.Primary.Navigate("Shell", "Explorer");
```

Lastly, an additional Region View Engine needs to be registered. As discussed in the IOC topic, you can also use a custom view activator to control how views are instantiated, if you want to use IOC. In this case we'll use the Microsoft common `ServiceLocator`:

```
catalog.ViewEngines.Clear();
catalog.ViewEngines.Add(new PageViewEngine(new ServiceLocatorViewActivator()));
catalog.ViewEngines.Add(new WindowViewEngine(new ServiceLocatorViewActivator()));
catalog.ViewEngines.Add(new CompositeViewEngine(new ServiceLocatorViewActivator()));
```

When the `InRegion` extension method is used, and the view class derives from `UIElement`, the region view engine will use the service locator to resolve the default `RegionManager`, and then add the view to the region.

# Controller Factory #

There is also a new controller factory that can be used with the Common Service Locator. It will automatically back onto `ServiceLocator.Current` to resolve controllers, so you just have to register them in the container:

```
var catalog = new ControllerRouteCatalog(new ServiceLocatorControllerFactory());
```

# Getting the code #

Since the Prism team don't publish assemblies, it's hard to create a version that's officially linked to theirs. You'll need to download the code referenced above from the [source](Download.md) package and compile it yourself.