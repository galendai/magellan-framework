# Introduction #

Magellan has the ability to automatically execute controller actions on a background thread, and dispatch back to the UI thread for rendering results. This page describes how to enable the feature.

# Example #

The controller action below makes a WCF call to load a list of customers, which are used as the model for the view. This service call could take some time to evaluate:

```
public class CustomerController : Controller
{
    public ActionResult List()
    {
        Model = CustomerService.GetCustomers();
        return Page("CustomerList");
    }
}
```

With asynchronous controllers enabled, Magellan will invoke the List action on a background thread. Since page navigation has to take place on the UI thread, the page rendering will be automatically dispatched to the UI thread, but this happens after the controller action has been executed. The result is that the UI remains snappy and responsive.

# Enabling Asynchronous Controllers #

There are three simple options for enabling asynchronous controllers. The first option is to inherit from `AsyncController` instead of `Controller`:

```
public class CustomerController : AsyncController
```

The second option is to assign the `AsyncActionInvoker` when the controller is constructed (this is actually what both `AsyncControllerFactory` and `AsyncController` do):

```
public class CustomerController : Controller
{
    public CustomerController() 
    { 
        ActionInvoker = new AsyncActionInvoker();
    }
}
```

The preferred approach is to rely on controller factories to set the action invoker. Instead of using CoontrollerFactory, you can use AsyncControllerFactory:

```
var controllerFactory = new AsyncControllerFactory();
controllerFactory.Register("Home", () => new HomeController());
controllerFactory.Register("Customer", () => new CustomerController());
```

If you are using a custom controller factory, you just need to replace the ActionInvoker - for example:

```
public ControllerFactoryResult CreateController(NavigationRequest request, string controllerName)
{
    var controller = // Create controller
    if (controller is ControllerBase)
    {
        ((ControllerBase) controller).ActionInvoker = new AsyncActionInvoker();
    }
    return new ControllerFactoryResult(controller);
}
```

# Reporting Progress #

Now that navigation is occurring on a background thread, it's nice to show a progress indicator while navigation is happening. For this, Magellan now provides an `INavigationProgressListener` interface that you can implement.

```
public interface INavigationProgressListener
{
    void UpdateProgress(NavigationRequest request, NavigationStage navigationStage);
}
```

For example, the code behind for the Window is going to show a progress bar:

```
public partial class MainWindow : Window, INavigationProgressListener
{
    public MainWindow()
    {
        InitializeComponent();
        NavigationProgress.Listeners.Add(this);
        Loaded += (x, y) => Navigator.For(Frame).NavigateWithTransition("Home", "Home", "ZoomIn");
    }

    public void UpdateProgress(NavigationEvent navigationEvent)
    {
        Dispatcher.Invoke(
            new Action(delegate
            {
                if (navigationEvent is BeginRequestNavigationEvent)
                {
                    BusyIndicator.Visibility = Visibility.Visible;
                }

                if (navigationEvent is CompleteNavigationEvent)
                {
                    BusyIndicator.Visibility = Visibility.Collapsed;
                }
            }));
    }
}
```

The iPhone sample application uses a spinning circle to indicate navigation progress:

![http://magellan-framework.googlecode.com/hg/assets/docs/BusyIndicator.png](http://magellan-framework.googlecode.com/hg/assets/docs/BusyIndicator.png)

# Summary #

The benefit of this approach is that controller code is the same whether we are using single threads or background threads, which also allows unit tests to remain singly threaded while at runtime the application is multi-threaded. Enabling this feature is quite easy, and so long as your controllers don't depend on the UI thread it should Just Work.