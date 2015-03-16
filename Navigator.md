# Introduction #

In Magellan 1.0, everything was configured similarly to ASP.NET MVC, using static calls like `ControllerBuilder.Current.SetControllerFactory()`. In Magellan 2.0, an effort has been made to eliminate these static objects, creating a cleaner model.

The design is inspired by NHibernate's configuration model. In NHibernate, you configure a `SessionManager`, which knows everything about how to map to your objects, the database driver to use, the connection string to use, and so on. You then use that session manager to produce `ISession`'s, which are used to process a set of queries.

# Creating a NavigatorFactory #

The `NavigatorFactory` is the central point of Magellan configuration - Magellan's `SessionManager`. A `NavigatorFactory` requires a collection of [route catalogs](Routing.md). You can also hand it the URI prefix to use. For example:

```
var navigatorFactory = new NavigatorFactory("myapp", routes1, routes2, routes3); 
```

# Creating Navigator #

The `NavigatorFactory` knows about your configuration (routing and URI rules), but it doesn't know how to navigate. That's the job of a `Navigator`.

A `Navigator` is bound to a **frame of navigation** - think of this as a single tab within a modern web browser. In a simple application, you may only have one `Frame`, and thus one `Navigator`. In a more complicated application, such as an application with nested frames, or multiple tabs, you will have more than one `Navigator`.

You get a `Navigator` by asking the `NavigatorFactory` for one, telling it the frame of navigation:

```
// DefaultFrame is a property that returns a WPF <Frame /> element
var navigator = navigatorFactory.CreateNavigator(myWindow.DefaultFrame); 
```

For page-based navigation with back/forward capability, we use WPF's `Frame` object. If you don't need back/forward capability, you can pass a `ContentControl` instance instead. If you have a custom object which manages back/forward, you can implement `INavigationService`.

# Navigating #

Once you have a reference to a navigator, you can navigate. Easy!

You can navigate using a URI, which Magellan will map to a route.

```
navigator.Navigate("myapp://customers/edit/123");
```

If you don't know how the URI is formatted, but you have all the important parameters, you can navigate that way instead:

```
navigator.Navigate(new { controller = "customers", action = "edit", id = 123 });
```

Magellan will calculate what the URI should have been, and execute it for you.

For MVC, you can navigate using a nice helper method:

```
navigator.Navigate<TaxController>(x => x.CalculateReturn(GrossIncome, true));
```

**Note that all of the Navigate methods are extension methods - you'll need to add a using statement for the Magellan namespace.**

# Getting the current Navigator #

If you are within a view (page, window, dialog, etc.), or a `ViewModel`, using Magellan's MVC or MVVM support, you can get the Navigator that created you by implementing `INavigationAware`. This makes the Navigator property available:

```
public class MyModel : ViewModel 
{
    public void GoSomewhere() 
    {
       Navigator.Navigate("myapp://foo/bar");
    }
}
```