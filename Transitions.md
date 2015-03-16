# Introduction #

When navigating between pages, transitional animation can provide a powerful means of communicating context with the user. The [Transitionals](http://www.codeplex.com/transitionals) library from Microsoft is a popular way to set up transitions when content changes, and comes with a number of out of the box transitions such as 3D cube, blinds, dissolve and star wipe.

When it comes to page navigation, we often want transitions to relate to the navigation we are performing. For example, if we click a "Next" button, we might expect a transition that 'slides' the next page into view. If we then clicked "Back", we would expect a slide in the opposite direction.

Magellan has out of the box support for the Microsoft Transitionals library. Here I will assume that you have a Magellan project ready (see the [Getting Started](GettingStarted.md) section).

You will need to add a reference to the following DLL's:

  * Magellan.Transitionals.dll
  * Transitionals.dll

You will then need to update the entry point of your application (usually App.xaml.cs) to set up the transition mappings:

```
NavigationTransitions.Table.Add("Back", "Forward", () => new SlideTransition(SlideDirection.Back));
NavigationTransitions.Table.Add("Forward", "Back", () => new SlideTransition(SlideDirection.Forward));
NavigationTransitions.Table.Add("ZoomIn", "ZoomOut", () => new ZoomInTransition());
NavigationTransitions.Table.Add("ZoomOut", "ZoomIn", () => new ZoomOutTransition());
```

These entries define a transition, as well as the 'reverse' transition that will be used when navigating "Back".

To enable the transitions, you can edit the control template of any Frame elements to use the transitional navigation element. Below is an example:

```
<Frame Name="mainFrame">
    <Frame.Template>
        <ControlTemplate TargetType="{x:Type Frame}">
            <DockPanel ClipToBounds="True">
                <NavigationTransitionPresenter Content="{TemplateBinding Content}" />
            </DockPanel>
        </ControlTemplate>
    </Frame.Template>
</Frame>
```

Finally, you will need to change any code which performs a navigation request to specify the transition to use. Procedurally, code like this:

```
navigator.Navigate<HomeController>(x => x.Index());
```

Should become:

```
navigator.NavigateWithTransition("Home", "Index", "ZoomIn");
```

And likewise, Behaviors like this:

```
<Button Content="Try Again">
    <i:Interaction.Behaviors>
        <NavigateBehavior Controller="Home" Action="Index" />
    </i:Interaction.Behaviors>
</Button>
```

Should become:

```
<Button Content="Try Again">
    <i:Interaction.Behaviors>
        <NavigateWithTransitionBehavior Transition="Forward" Controller="Home" Action="Index" />
    </i:Interaction.Behaviors>
</Button>
```

And that is it! Navigating between pages should now make use of transitions.

When defining your transitions using the `TransitionTable`, you can provide any transition from the WPF Transitionals library, or one of the four out of the box Magellan transitions designed specifically for navigation applications (slide forward, slide backward, zoom in and zoom out). For example, below is a roll transition in action:

TODO: add image

```
NavigationTransitions.Table.Add("Forward", "Back", () => new Transitionals.Transitions.RollTransition());
```

Writing your own transitions is also relatively easy - see the [Transitionals](http://www.codeplex.com/transitionals) source code for examples.
