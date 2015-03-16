# Action Invokers #

The action invoker is used by the controller to handle the details of executing the action. This allows you to continue to derive from the Controller class while executing actions in a very different way. By default, the controller's `ActionInvoker` property is set to the `DefaultActionInvoker`.

The default action invoker uses reflection to map the requests Action property to a method on the controller. It then performs the following steps:

  1. Executes all pre-action filters
  1. Executes the action
  1. Executes all post-action filters
  1. Executes the action result
  1. Action filters provide a way for you to implement cross-cutting concerns on controllers, such as authorization, logging, state management, and other examples.

When the action has been executed, it will return an [Action Result](ActionResults.md). This is an important point - controllers do not show views directly, they simply return an object which is responsible for resolving and showing the view. This allows you handle the navigation request in a very different way without altering controllers.