Action Filters are typically attributes that you can apply to a Magellan controller or controller action in order to intercept the call and provide an alternative way of handling the request. They provide a poor man's Aspect Oriented Programming mechanism for controllers.

# Action Filters #

The sample below shows how an Action Filter might be used. The Log attribute can be applied to either methods or classes:

```
public class MyController : Controller
{
    [Log]
    public ActionResult Show(int customerId) 
    {
        ...
    }
}
```

The Log action filter attribute might be implemented as follows:

```
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        Console.WriteLine("Entering: {0}({1})",
            context.Request.Action,
            string.Join(", ", context.Request.ActionParameters.Select(x => x.Key + "=" + x.Value).ToArray())
            );
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        Console.WriteLine("Exiting: {0}({1})",
            context.Request.Action,
            string.Join(", ", context.Request.ActionParameters.Select(x => x.Key + "=" + x.Value).ToArray())
            );
    }
}
```

As you can see above, action filters have two hooks - before the action is invoked, and after the action is invoked. Before the action, you can:

  * See information about the request
  * Rewrite the parameters in the request
  * Short-circuit or 'cancel' the action by setting the Result property

After the action, you can:

  * Get access to the result of the action
  * Change the result of the action
  * See the exception that was thrown
  * Replace or suppress the exception that was thrown

# Result Filters #

Magellan also supports Result Filters via the `IResultFilter` interface. These are invoked after the action has been executed, and before the result is handled. These give you the ability to handle exceptions caused when rendering the view (rather than just when processing the action).

The logging example above could be extended to support Result Filters:

```
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogAttribute : Attribute, IActionFilter, IResultFilter
{
    // Other code here

    public void OnResultExecuting(ResultExecutingContext context)
    {
        Console.WriteLine("Rendering: {0}({1})",
            context.Request.Action,
            string.Join(", ", context.Request.ActionParameters.Select(x => x.Key + "=" + x.Value).ToArray())
            );
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        Console.WriteLine("Rendered: {0}({1})",
            context.Request.Action,
            string.Join(", ", context.Request.ActionParameters.Select(x => x.Key + "=" + x.Value).ToArray())
            );
    }
}
```

# Examples #

Here are some useful ways to use Action and View Filters:

  * Logging of all controller requests
  * Enforcing exception handling policies
  * Caching of request results (bypassing the action the second time around)
  * Performing permission checks and redirecting to another view if the user doesn't have permission