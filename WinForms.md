# Introduction #

Magellan does not natively support Windows Forms, but adding support is as easy as writing your own View Engine.

As described in the documentation on View Engines, there are two classes we need to write. The first is an object that implements `IViewEngine`, which is responsible for finding the view. The second is an object inheriting from `ViewEngineResult`, which contains logic for rendering the form (i.e., calling `Form.Show()`).

There are also a couple of design considerations:

  1. Magellan controllers like to create View Models, and it's the `ViewEngineResult`'s job to make them available for binding on the view.
  1. Windows Forms applications typically use naming conventions like `frmCustomerDetails`, `CustomerDetailsForm` or `CustomerDetailsWindow`. Our engine should support these.

# Binding Models to Forms #

To support the model, we're going to create an interface that allows the model to be given to the form. The form can then figure out how to display the model (either using a Windows Forms `BindingContext`, or manual code).

```
public interface IBindableForm
{
    void Bind(object model);
}
```

Our form implementation could look like this:

```
public partial class MainForm : Form, IBindableForm
{
    public MainForm()
    {
        InitializeComponent();
    }

    public void Bind(object model)
    {
        Text = model.ToString();
    }
}
```

# The Controller #

Here is what a `Controller` might look like:

```
public class HomeController : Controller
{
    public ActionResult Launch()
    {
        Model = "Hello world!";
        return Window("Main");
    }
}
```

Notice that there is no reference to Windows Forms here. The `Window()` return value merely indicates that we want to show a Window - it doesn't indicate what kind of Window should be shown. This should make it easier to convert to WPF in the future.

# The View Engine #

The view engine is also pretty short. Since we want to reuse the naming conventions and reflection code that the other view engines use, we can derive from `ReflectionBasedViewEngine`:

```
public class FormsViewEngine : ReflectionBasedViewEngine
{
    private readonly IViewActivator _viewActivator;

    public FormsViewEngine(IViewActivator viewActivator)
    {
        _viewActivator = viewActivator;
    }

    protected override IEnumerable<string> GetAlternativeNames(string viewName)
    {
        yield return viewName;
        yield return viewName + "Form";
        yield return viewName + "Window";
        yield return viewName + "Dialog";
        yield return "frm" + viewName;
    }

    protected override bool ShouldHandle(ControllerContext controllerContext, ParameterValueDictionary viewParameters, string viewName)
    {
        var viewType = viewParameters.GetOrDefault<string>("ViewType");
        return viewType == "Window" || viewType == "Dialog";
    }

    protected override IEnumerable<Type> FilterCandidateTypes(ControllerContext controllerContext, ParameterValueDictionary viewParameters, string viewName, IEnumerable<Type> candidates)
    {
        return candidates.Where(x => typeof (Form).IsAssignableFrom(x));
    }

    protected override ViewEngineResult CreateViewResult(ControllerContext controllerContext, ParameterValueDictionary viewParameters, Type type)
    {
        return new WindowsFormViewEngineResult(type, _viewActivator, viewParameters);
    }
}
```

Notice how we override `GetAlternativeNames` to support our conventions. We also override `FilterCandidateTypes` to restrict the view to objects derived from the Windows Forms `Form` class.

# The View Engine Result #

The View Engine above has taken charge of finding the view. When a view is found, it creates a `WindowsFormViewEngineResult`. The view engine result has the job of instantiating and rendering the view:

```
public class WindowsFormViewEngineResult : ViewEngineResult
{
    private readonly Type _formType;
    private readonly IViewActivator _viewActivator;
    private readonly ParameterValueDictionary _viewParameters;

    public WindowsFormViewEngineResult(Type formType, IViewActivator viewActivator, ParameterValueDictionary viewParameters) : base(true, new string[] {})
    {
        _formType = formType;
        _viewActivator = viewActivator;
        _viewParameters = viewParameters;
    }

    public override void Render()
    {
        var form = (Form)_viewActivator.Instantiate(_formType);
        var bindableForm = form as IBindableForm;
        if (bindableForm != null)
        {
            var model = _viewParameters.GetOrDefault<object>("Model");
            bindableForm.Bind(model);
        }

        var isDialog = _viewParameters.GetOrDefault<string>("ViewType") == "Dialog";
        if (isDialog) form.ShowDialog();
        else form.Show();
    }
}
```

We make use of two critical view parameters - the `ViewType`, which indicates whether to show it as a `Window` or a `Dialog`, and the `Model, which is populated from the `Model` property that was set on the controller.

# Wiring It Up #
To support our view engine, we just have to add it to the list of registered view engines on our `ControllerRouteCatalog`:

```
catalog.ViewEngines.Add(new FormsViewEngine(new DefaultViewActivator()));
```

The `DefaultViewActivator` implements the `IViewActivator` interface, which is used for actually constructing the form (it's passed to the view engine result). If you want, you can use an IOC container instead.

# Summary #

Hopefully this illustrates how easy it is to write a View Engine. The biggest benefit to the Magellan approach is that our controllers are consistent whether we are using WPF or Windows Forms. There's also the added benefit that if we upgrade a view to WPF, our controllers don't change. Likewise if we change our naming conventions for the forms, we don't need to change the controllers. Finally, our controllers and models remain testable, which is always important.