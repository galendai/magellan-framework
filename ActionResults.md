Action Results are a concept from ASP.NET MVC and other MVC frameworks, and they are also supported by Magellan. They provide a powerful mechanism for decoupling the controller from UI specific concerns. Before discussing how they work, we should discuss why they exist.

# Why Action Results? #

Suppose you weren't using Magellan and you had an action like this:

```
public void Login(string username, string password) 
{
    var window = IsValidLogin(username, password)
        ? new WelcomeWindow();
        : new LoginWindow();
    window.Show();
}
```

This code is nice and small, but unfortunately it would be difficult to test. Your test would have to set up a WPF environment, call the controller method, and check to see which Window was opened.

To make the code more testable, you might inject a service that supports showing Windows:

```
public void Login(string username, string password) 
{
    if (IsValidLogin(username, password))
        _windowService.Show<WelcomeWindow>();
    else _windowService.Show<LoginWindow>();
}
```

This is a little more testable - your test can use a mocking library to provide a test version of the `IWindowService`, and you can test to see if it was called without necessarily creating any Windows at test time. Unfortunately, it forces you to do interaction based testing instead of state based testing. You can't just call the controller and verify the result; you have to set up a record/replay mock to see what the controller did. Mocks are great, but if you can avoid mocks, that's even better in my books.

To make controller actions more testable, Magellan uses `ActionResults` that allow controllers to declare their intentions, rather than actually doing anything. `ActionResults` are an example of the Command Pattern. At runtime Magellan takes care of executing them, but from a testing point of view, you can just interrogate them to see what the controller specified.

Here's a Magellan version of the action above:

```
public ActionResult Login(string username, string password) 
{
    return IsValidLogin(username, password)
        ? Window("Welcome")
        : Window("Login");
}
```

The tests would be as simple as:

```
[Test]
public void InvalidLoginShouldShowLoginWindow() 
{
    var controller = new MyController();
    var result = controller.Login("fred", "invalidPassword") as WindowViewResult;
    Assert.IsNotNull(result);
    Assert.AreEqual("Login", result.ViewName);
}

[Test]
public void ValidLoginShouldShowWelcomeWindow() 
{
    var controller = new MyController();
    var result = controller.Login("fred", "validPassword") as WindowViewResult;
    Assert.IsNotNull(result);
    Assert.AreEqual("Welcome", result.ViewName);
}
```

Could they be any simpler?

# Types of Action Results #

The `ActionResult` class is actually an abstract base class, with many derived types. The class is actually super simple:

```
public abstract class ActionResult
{
    public void Execute(ControllerContext controllerContext)
    {
        ExecuteInternal(controllerContext);
    }
    protected abstract void ExecuteInternal(ControllerContext controllerContext);
}
```

Derived from the ActionResult base class are:

  * `ViewResult`, which has some derived types of its own:
    * `PageResult`, for navigating to a WPF Page
    * `WindowResult`, for showing a WPF Window in non-modal state
    * `DialogResult`, for showing a WPF Window in a dialog state
  * `BackResult`, for sending a GoBack() request to the current navigation service
  * `RedirectResult`, for invoking a different action on a potentially different controller
  * `StartProcessResult`, which launches a process via Process.Start()
  * `CancelResult`, which does nothing

# Controller Helpers #

To use these derived types, you could simply write:

```
public ActionResult MyAction() 
{
    return new PageResult("MyPage");
}
```

However, the Magellan Controller base class defines a number of helpful methods for creating these results. They make the controller actions a little shorter and more declarative, for example:

```
public ActionResult MyAction() 
{
    return Page("MyPage");
}
```

Here are some examples of the methods in action. It should be easy to guess which type of `ActionResult` is in play:

```
return Page("MyPage");                  // Navigates to the page
return Page("MyPage", true);            // Navigates to the page, and removes all 'back' entries from the journal
return Window("MyWindow");              // Creates the Window and calls Window.Show()
return Dialog("MyWindow");              // Creates the Window and calls Window.ShowDialog();
return StartProcess("calc.exe");        // Launches calc.exe
return StartProcess("calc.exe", true);  // Launches calc.exe and waits for the user to close it
return Back();                          // Navigates back in the navigation service
return Back(true);                      // Navigates back and removes the page from the journal
return Redirect("MyAction");            // Executes MyAction and returns that instead
return Cancel();                        // Does nothing
```

The `StartProcessResult` is another good example to consider. If your controller actually called `Process.Start()`, how would you test it? With the result, you can just cast it to `StartProcessResult` and verify the properties to see what process the controller wanted to start.

The `ViewResult` derived classes share some logic in common for dealing with [View Engines](ViewEngines.md), which get their own section.

# Rolling your Own #

Because the `ActionResult` base class is so simple, rolling your own is easy. Let's take a `MessageBox` for example. Here's what our controller might look like:

```
public class MyController : CustomController
{
    public ActionResult SayHello()
    {
        return MessageBox("Hello world!");
    }
}
```

A test for such a controller can look like this:

```
[Test]
public void ShouldShowMessageBox()
{
    var controller = new MyController();
    var result = controller.SayHello() as MessageBoxResult;
    Assert.IsNotNull(result);
    Assert.AreEqual("Hello world!", result.Message);
}
```

We probably want to create a base class for our controllers with the useful helpers:

```
public class CustomController : Controller
{
    public MessageBoxResult MessageBox(string message)
    {
        return new MessageBoxResult("My Application", message, MessageBoxButtons.OK);
    }

    public MessageBoxResult MessageBox(string title, string message) 
    {
        return new MessageBoxResult(title, message, MessageBoxButtons.OK);
    }
}
```

And finally, the star of the show, the `MessageBoxResult`:

```
public class MessageBoxResult : ActionResult
{
    private readonly string _title;
    private readonly string _message;
    private readonly MessageBoxButtons _buttons;

    public MessageBoxResult(string title, string message, MessageBoxButtons buttons)
    {
        _title = title;
        _message = message;
        _buttons = buttons;
    }

    public string Title
    {
        get { return _title; }
    }

    public string Message
    {
        get { return _title; }
    }    

    public MessageBoxButtons Buttons
    {
        get { return _buttons; }
    }    

    protected override void ExecuteInternal(ControllerContext controllerContext)
    {
        MessageBox.Show(_title, _message, _buttons);
    }
}
```