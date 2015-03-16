Magellan is all about navigation, an important part of which is showing views. In Magellan, a view can be a `Window`, `Dialog` or `Page`. With the [Composite WPF](Prism.md) extension, a view can also be any user control that can be added to a `Region`. The system is also extensible - see the page on [Windows Forms](WinForms.md) support for an example on writing your own view engine.

# View Results #

As described previously, controller actions return an Action Results. These contain the logic for prosecuting the request. Derived from `ActionResult` are three view types:

![http://magellan-framework.googlecode.com/hg/assets/docs/ViewResults.png](http://magellan-framework.googlecode.com/hg/assets/docs/ViewResults.png)

Each of the derived classes defer to the ViewResult base class to handle the execution. The internal execution code looks like this:

```
protected override void ExecuteInternal(ControllerContext controllerContext)
{
    ViewEngineResult = _viewEngines.FindView(controllerContext, Options, viewName);

    if (ViewEngineResult.Success)
    {
        ViewEngineResult.Render();
    }
    else
    {
        throw new ViewNotFoundException(...);
    }
}
```

`_viewEngines` is a collection of objects that implement the `IViewEngine` interface. The `ViewParameters` property provides additional information to the view engines, such as limitations on the type returned. Each of the derived types populate these - `PageResult` specifies that the view engines should only locate pages, `WindowResult` asks only for `Window`s, and so on.

# View Engines #

As we've seen above, View Results defer to View Engines to locate the view. As you can guess, View Engines are quite simple, though their job is a little complicated:

```
public interface IViewEngine
{
    ViewEngineResult FindView(
        ControllerContext controllerContext, 
        ViewResultOptions options, 
        string view);
}
```

When a View Engine figures out which view to show, it returns a custom object derived from ViewEngineResult, which encapsulates how to render the view. This way, the View Engine is only concerned with locating the view - the ViewEngineResult contains the actual logic for showing it.

Magellan comes with two View Engines that share a common base class:

![http://magellan-framework.googlecode.com/hg/assets/docs/ViewEngines.png](http://magellan-framework.googlecode.com/hg/assets/docs/ViewEngines.png)

Although there are three view results (`PageResult`, `WindowResult` and `DialogResult`), we only have two View Engines. This is because the logic of finding the view and the constraints are the same - the only difference is the call to `Show()` vs. `ShowDialog()`, which is rendering logic, not finding logic.

# View Location #

As shown in the diagram above, Magellan's View Engines derive from a `ReflectionBasedViewEngine`.

When your controller specifies something like this:

```
public ActionResult Home()
{
    return Page("Index");
}
```

The `ReflectionBasedViewEngine` will find all types in the assembly (you can use different assemblies by passing them in the view engine's constructor). It then passes them to the derived types to be filtered - for example, `WindowViewEngine` will restrict the list to only include types derived from the WPF Window class, and `PageViewEngine` will filter by the Page base class.

Once the `ReflectionBasedViewEngine` has a set of candidate types, it uses some rules to find the best view. The rules are based firstly on whether the names match, and secondly by namespace proximity, taking some conventions into account. The conventions are encapsulated in an `IViewNamespaceProvider` interface, which is also passed into the View Engine's constructor.

In the code above, we specified the "Index" view. Assuming our controllers are in a namespace called `MyCompany.MyProduct.Controllers`, and the controller is named `HomeController`, Magellan will look for these combinations of view names, via the `DefaultViewNamespaceProvider`, working through the list in order:

```
 - MyProject.Controllers.Views.Home.Index
 - MyProject.Controllers.Views.Home.IndexView
 - MyProject.Controllers.Views.Home.IndexPage
 - MyProject.Controllers.Views.Home.IndexViewPage
 - MyProject.Controllers.Views.Index.Index
 - MyProject.Controllers.Views.Index.IndexView
 - MyProject.Controllers.Views.Index.IndexPage
 - MyProject.Controllers.Views.Index.IndexViewPage
 - MyProject.Controllers.Views.Home.Index.Index
 - MyProject.Controllers.Views.Home.Index.IndexView
 - MyProject.Controllers.Views.Home.Index.IndexPage
 - MyProject.Controllers.Views.Home.Index.IndexViewPage
 - MyProject.Controllers.Views.Index
 - MyProject.Controllers.Views.IndexView
 - MyProject.Controllers.Views.IndexPage
 - MyProject.Controllers.Views.IndexViewPage
 - MyProject.Controllers.Home.Index
 - MyProject.Controllers.Home.IndexView
 - MyProject.Controllers.Home.IndexPage
 - MyProject.Controllers.Home.IndexViewPage
 - MyProject.Controllers.Index
 - MyProject.Controllers.IndexView
 - MyProject.Controllers.IndexPage
 - MyProject.Controllers.IndexViewPage
 - MyProject.Views.Home.Index
 - MyProject.Views.Home.IndexView
 - MyProject.Views.Home.IndexPage
 - MyProject.Views.Home.IndexViewPage
 - MyProject.Views.Index.Index
 - MyProject.Views.Index.IndexView
 - MyProject.Views.Index.IndexPage
 - MyProject.Views.Index.IndexViewPage
 - MyProject.Views.Home.Index.Index
 - MyProject.Views.Home.Index.IndexView
 - MyProject.Views.Home.Index.IndexPage
 - MyProject.Views.Home.Index.IndexViewPage
 - MyProject.Views.Index
 - MyProject.Views.IndexView
 - MyProject.Views.IndexPage
 - MyProject.Views.IndexViewPage
 - MyProject.Home.Index
 - MyProject.Home.IndexView
 - MyProject.Home.IndexPage
 - MyProject.Home.IndexViewPage
 - MyProject.Index
 - MyProject.IndexView
 - MyProject.IndexPage
 - MyProject.IndexViewPage
 - Index
 - IndexView
 - IndexPage
 - IndexViewPage
```

Hopefully the list above makes sense. Effectively, we're looking for the first view we can find as close to the controller as possible. Some conventions like a sub folder called "Views" are also taken into account.

# Project structure #

In practical terms, this means you can structure your project like this:

```
/Home
    HomeController.cs
    /Index
        IndexView.xaml
```

Or this:

```
/Home
    HomeController.cs
    /Views     
        /Index
            IndexView.xaml
```

Or this:

```
/Controllers
    HomeController.cs
/Views     
    /Home
        IndexView.xaml
```

Or this:

```
/Controllers
    HomeController.cs
/Views     
    /Home
        /Index
            IndexView.xaml
```

# Overriding #

If a view cannot be found, Magellan will provide a friendly exception. The screenshot below show what would happen if my controller mis-spelled the name of a view:

![http://magellan-framework.googlecode.com/hg/assets/docs/ViewNotFound.png](http://magellan-framework.googlecode.com/hg/assets/docs/ViewNotFound.png)

To change the conventions, simply create a class implementing `IViewNamespaceProvider`, and pass it to the view engine:

```
var provider = new MyCustomNamespaceProvider(); // Implements IViewNamespaceConvention and IViewNamingConvention
controllerRouteCatalog.ViewEngines.Clear();
controllerRouteCatalog.ViewEngines.Add(new PageViewEngine(new DefaultViewActivator()) { NamespaceConvention = provider, NamingConvention = provider });
controllerRouteCatalog.ViewEngines.Add(new WindowViewEngine(new DefaultViewActivator()) { NamespaceConvention = provider, NamingConvention = provider });
```