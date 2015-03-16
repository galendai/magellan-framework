# Introduction #

Magellan makes use of `TraceSources` internally for providing diagnostic information. By default nothing is traced, but you can configure the trace sources to see details.

Configuring the trace sources can be done through the application configuration file:

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sources>
      <source name="Magellan" switchValue="Verbose">
        <listeners>
          <clear />
          <add name="Console" type="System.Diagnostics.ConsoleTraceListener" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

Or through code, for example, in the `App.xaml` code behind:

```
Magellan.Diagnostics.TraceSources.MagellanSource.Switch.Level = SourceLevels.Verbose;
```

Here is an example of what is logged at the Information level:

```
Magellan Information: 0 : Controller 'MagellanHelloWorld.Controllers.CalculatorController' is executing request 'magellan://calculator/Input'.
Magellan Information: 0 : The PageViewEngine is rendering the page 'MagellanHelloWorld.Views.Input.InputView'.
Magellan Information: 0 : Request completed: 'magellan://calculator/Input'.
```

Here is what is logged at the Verbose level:

```
Magellan Verbose: 0 : Resolving controller 'Calculator' for request 'magellan://calculator/Input'
Magellan Information: 0 : Controller 'MagellanHelloWorld.Controllers.CalculatorController' is executing request 'magellan://calculator/Input'.
Magellan Verbose: 0 : DefaultActionInvoker found the action 'Input' as method 'Magellan.Framework.ActionResult Input()'
Magellan Verbose: 0 : DefaultActionInvoker found the following action filters for action 'Input': ''.
Magellan Verbose: 0 : DefaultActionInvoker found the following result filters for action 'Input': ''.
Magellan Verbose: 0 : The ViewEngineCollection is consulting the view engine 'Magellan.Framework.PageViewEngine' for the view 'Input'.
Magellan Information: 0 : The PageViewEngine is rendering the page 'MagellanHelloWorld.Views.Input.InputView'.
Magellan Verbose: 0 : The view 'InputView' does not implement the IView interface, so the model is being set as the DataContext.
Magellan Verbose: 0 : The object 'InputViewModel' implements the INavigationAware interface, so it is being provided with a navigator.
Magellan Verbose: 0 : The PageViewEngine has navigated to the page 'MagellanHelloWorld.Views.Input.InputView'.
Magellan Information: 0 : Request completed: 'magellan://calculator/Input'.
```

The Warning and Error levels are rarely used - warning is used when Magellan decides on a course of action that is potentially incorrect, and errors are usually logged before an exception is thrown.