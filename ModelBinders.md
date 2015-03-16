# Introduction #

Magellan's MVC and MVVM frameworks need to map input parameters - such as "123" - to types, like an Int32. To do this, they make use of model binders.

# Model Binders #

Magellan provides only one binder - a `DefaultModelBinder`. Generally, this should be all you need, because it tries very hard to do the conversion. It tries:

  * Casting, if supported by the source/target
  * Type converters on the source or target type
  * Implicit assignment converters
  * Hope and pray

If you wish to customize how request parameters are mapped to action method parameters, you can do so through implementing the `IModelBinder` interface. There are lots of interesting tricks you can do with this - I'll blog about them some time.

# With MVC #

The `ControllerRouteCatalog` exposes a `ModelBinders` dictionary:

```
var catalog = new ControllerRouteCatalog(controllerFactory);
catalog.ModelBinders.Add(typeof(Money), new MoneyModelBinder());
```


# With MVVM #

Similarly, the `ViewModelRouteCatalog` exposes a `ModelBinders` dictionary:

```
var catalog = new ViewModelRouteCatalog(viewModelFactory);
catalog.ModelBinders.Add(typeof(Money), new MoneyModelBinder());
```