# Introduction #

Magellan is a navigation framework, but it also supports some higher-level presentation patterns. Model-view-controller is one of those, and works amazingly well as a navigation co-coordinator. This section looks at MVC support in Magellan.

# Controllers #

Using the MVC framework, the most obvious object you'll need to create is controllers. At their simplest, controllers are implemented as classes, and actions are implemented as methods on the class. Actions on a controller must be public methods, and must return `ActionResult` objects. Here is an example:

```
public class CustomerController : Controller
{
    public ActionResult Index()
    {
        Model = Customers.GetAll();
        return Page();
    }

    public ActionResult Show(int customerId) 
    {
        Model = Customers.Get(customerId);
        return Page();
    }
}
```

# Controller Factories #

Controllers are resolved from an object implementing `IControllerFactory`. Magellan ships with a lambda-based controller factory out of the box, but it's expected you'll probably replace it [with one that uses an IOC container](IOC.md).

Controller factories are quite simple:

```
public interface IControllerFactory
{
    ControllerFactoryResult CreateController(ResolvedNavigationRequest request, string controllerName);
}
```

The out of the box controller factory has a Register method which you can use to set up controllers:

```
var controllers = new ControllerFactory();
controllers.Register("Home", () => new HomeController());
controllers.Register("Tax", () => new TaxController(settings));
controllers.Register("Search", () => new SearchController(searcher));
```

You tell Magellan which controller factory to use by passing it to the `ControllerRouteCatalog`.

# Controller routing #

As [discussed in the routing section](Routing.md), a `NavigatorFactory` is configured with a set of routes. When using Magellan's MVC framework, this means using the `ControllerRouteCatalog`.

```
var routes = new ControllerRouteCatalog(controllers);
routes.MapRoute("{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional);
```

Using the route above, the following URL examples would map to the following method calls:

| `myapp://` | `HomeController.Index()` |
|:-----------|:-------------------------|
| `myapp://Home` | `HomeController.Index()` |
| `myapp://Customers` | `CustomersController.Index()` |
| `myapp://Customers/List` | `CustomersController.List()` |
| `myapp://Customers/Show/123` | `CustomersController.Show(id = 123)` |
| `myapp://Customers/Show/123?revision=3` | `CustomersController.Show(id = 123, revision = 3)` |

# More Details #

The above is just the basics of working with the Magellan MVC framework. The sections below provide more details on advanced usage and the extension points:

  * [Action Filters and Result Filters](Filters.md)
  * [Model Binders](ModelBinders.md)
  * [Action Invoker](ActionInvoker.md)
  * [Action Results](ActionResults.md)
  * [View Engines](ViewEngines.md)