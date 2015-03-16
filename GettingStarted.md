Let's create a very simple calculator, using Magellan's MVC framework.

  1. [Download the Magellan.dll binary](Download.md). Unzip it to a new location.
  1. Create a new **WPF Application** project using Visual Studio 2010.
  1. Add references to **Magellan.dll**
  1. Create a folder structure similar to below:

![http://magellan-framework.googlecode.com/hg/assets/docs/EmptyProject.png](http://magellan-framework.googlecode.com/hg/assets/docs/EmptyProject.png)

## Create a Controller ##

Add a new class to the **Controllers** folder named **CalculatorController**. It should have the following content, and should look very similar to an ASP.NET MVC controller:

```
public class CalculatorController : Controller
{
    public ActionResult Input()
    {
        return Page("Input", new InputViewModel());
    }

    public ActionResult Result(int a, int b)
    {
        var model = new ResultViewModel();
        model.Result = a + b;
        return Page("Result", model);
    }
}
```

## Create the Input view model ##

Under the **Views/Input** folder, add a class named **InputViewModel**. It should read something like:

```
public class InputViewModel : ViewModel
{
    public InputViewModel()
    {
        Calculate = new RelayCommand(CalculateExecuted);
    }

    public ICommand Calculate { get; private set; }

    public int A { get; set; }
    public int B { get; set; }

    private void CalculateExecuted()
    {
        Navigator.Navigate<CalculatorController>(x => x.Result(A, B));
    }
}
```

Notice how it uses a custom `ICommand` to navigate to the `Result` action on the controller. The `Navigator` property is made available from the `ViewModel` base class, and is wired up by Magellan.

## Create the Result view model ##

Under **Views/Result**, create a class named **ResultViewModel**. This one is much simpler:

```
public class ResultViewModel : ViewModel
{
    public int Result { get; set; }
}
```

## Create the Input page ##

Now that we have our models, we need to add the views. Under **Views/Input**, create a new **WPF Page** named **InputView**:

```
<Page 
    x:Class="MagellanHelloWorld.Views.Input.InputView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
    mc:Ignorable="d" 
    d:DesignHeight="300" d:DesignWidth="300"
    Title="Input">
    <Form Margin="7">
        <Field Margin="7" For="{Binding Path=A}" />
        <Field Margin="7" For="{Binding Path=B}" />
        <Field Margin="7">
            <Button Content="Calculate" Command="{Binding Path=Calculate}" />
        </Field>
    </Form>
</Page>
```

This page uses Magellan's Forms feature to create some input fields based on conventions.

## Create the Result page ##

Under **Views/Result**, create another **WPF Page** named **ResultView**:

```
<Page 
    x:Class="MagellanHelloWorld.Views.Result.ResultView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
    mc:Ignorable="d" 
    d:DesignHeight="300" d:DesignWidth="300"
    Title="Result"
    >
    <Form Margin="7">
        <Field Margin="7" Header="Result">
            <TextBlock Text="{Binding Path=Result}" />
        </Field>
    </Form>
</Page>
```

This page also uses Magellan's Forms feature, but instead of letting it infer the field, it adds a custom TextBlock to display the result in a read-only way.

## Set up the Main Window ##

**MainWindow.xaml** will host our **frame of navigation**. Open it up and add a **Frame** to it named **MainFrame**:

```
<Window 
    x:Class="MagellanHelloWorld.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="MainWindow" Height="350" Width="525">
    <Grid>
        <Frame x:Name="MainFrame" />    
    </Grid>
</Window>
```

## Bootstrap the application ##

We're going to override the way our application starts, so **delete the StartupUri attribute from App.xaml**.

When that is done, give the App.xaml.cs the following configuration code:

```
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Dispatcher.UnhandledException += (x, y) => MessageBox.Show(y.Exception.Message);

        var controllers = new ControllerFactory();
        controllers.Register("Calculator", () => new CalculatorController());

        var routes = new ControllerRouteCatalog(controllers);
        routes.MapRoute("{controller}/{action}/{id}", new { id = UrlParameter.Optional});

        var navigation = new NavigatorFactory(routes);
        var mainWindow = new MainWindow();
        var navigator = navigation.CreateNavigator(mainWindow.MainFrame);
        mainWindow.Show();

        navigator.Navigate("magellan://Calculator/Input");
        navigator.Navigate<CalculatorController>(x => x.Input());
    }
}
```

To wire up Magellan, we have to create a Navigator. These are created by a `NavigatorFactory`, which is similar to an NHibernate `SessionManager`.

The `NavigatorFactory` requires one or more route catalogs, which tell it how to handle navigation requests. Since we're using the Model-View-Controller pattern, we use a `ControllerRouteCatalog`. The `ControllerRouteCatalog` maps routes to controllers, so we also need to give that a `ControllerFactory`. As you can imagine, this kind of dependency chain is [perfect for IOC](ioc.md).

## Run ##

At this point, the application should run, and you should be able to use the calculator:

http://magellan-framework.googlecode.com/hg/assets/docs/Input.PNG

![http://magellan-framework.googlecode.com/hg/assets/docs/Result.png](http://magellan-framework.googlecode.com/hg/assets/docs/Result.png)

## Testing ##

Magellan's MVC support makes it very easy to unit test. Here's a pair of unit tests for our controller actions:

```
[TestFixture]
public class CalculatorControllerTests
{
    [Test]
    public void InputShowsInputPageWithEmptyModel()
    {
        var controller = new CalculatorController();
        var result = (PageResult)controller.Input();

        Assert.AreEqual("Input", result.ViewName);
        Assert.IsInstanceOfType(result.Model, typeof(InputViewModel));
    }

    [Test]
    public void ResultCalculatesAndShowsResultPageWithResults()
    {
        var controller = new CalculatorController();
        var result = (PageResult)controller.Result(1, 2);

        Assert.AreEqual("Result", result.ViewName);
        Assert.IsInstanceOfType(result.Model, typeof(ResultViewModel));
        Assert.AreEqual(3, ((ResultViewModel)result.Model).Result);
    }
}
```