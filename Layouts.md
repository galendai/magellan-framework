# Introduction #

When building WPF pages and windows, it's common to want a consistent layout across views. In ASP.NET, this is often accomplished with Master Pages. Magellan brings a similar concept to WPF, in the form of Shared Layouts.

The Magellan source code includes a new Wizard example application that demonstrates the shared layout feature. In this page I'll describe how it works.

![http://magellan-framework.googlecode.com/hg/assets/docs/Layout.png](http://magellan-framework.googlecode.com/hg/assets/docs/Layout.png)

# Creating and Using Shared Layouts #

To create a shared layout, I typically add a Shared folder under Views:

![http://magellan-framework.googlecode.com/hg/assets/docs/LayoutProject.png](http://magellan-framework.googlecode.com/hg/assets/docs/LayoutProject.png)

The shared layout, **Main.xaml**, is simply a `UserControl` with a number of `ZonePlaceHolders`. Each `ZonePlaceHolder` is given a `Name`, which we'll refer to later. The example below shows how a three-column layout might be declared. Since the layout is a `UserControl`, it can contain any XAML you wish:

```
<UserControl
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:magellan="http://xamlforge.com/magellan" 
    x:Class="Wizard.Views.Shared.Main" 
    >
    <DockPanel>
        <ZonePlaceHolder Name="Left" DockPanel.Dock="Left" Width="300" />
        <ZonePlaceHolder Name="Right" DockPanel.Dock="Right" Width="300" />
        <ZonePlaceHolder Name="Content" />
    </DockPanel>
</UserControl>
```

Now that our layout is declared, we can reference it from any other Window, Page or UserControl. Here's what a page might look like:

```
<Page
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
    xmlns:magellan="http://xamlforge.com/magellan" 
    x:Class="Wizard.Views.Wizard.AccountDetailsPage" 
    Title="Account Details"
    >
    <Layout Source="/Wizard;component/Views/Shared/Main.xaml">
        <Zone ZonePlaceHolderName="Content">
            <TextBlock Text="The center content goes here" />
        </Zone>

        <Zone ZonePlaceHolderName="Left">
            <TextBlock Text="This content will appear on the left" />
        </Zone>

        <Zone ZonePlaceHolderName="Right">
            <TextBlock Text="This content will appear on the right" />
        </Zone>
    </Layout>
</Page>
```

The page uses the `Layout` control to reference the shared layout. The `Source` property is the URI to the the XAML file that contains the shared layout.

# Composition #

The `Layout` element is a control that takes a `Source` and a set of `Zones`. When it is loaded into the scene, it loads the shared layout user control and sets it as the content. Any `ZonePlaceHolders` inside the control will then have their content injected, based on the zone names matching.

The logical tree of the final page will look like this:

![http://magellan-framework.googlecode.com/hg/assets/docs/LayoutVisualTree.png](http://magellan-framework.googlecode.com/hg/assets/docs/LayoutVisualTree.png)

Since the Layout is a child of the Page, this opens a number of possibilities:

  * The layout will inherit the `DataContext` of the page. If your DataContexts for each page have the same properties in common - such as using a shared View Model - the layout can make use of those.
  * The layout can use WPF's Routed Events to communicate with the page.
  * The layout can use `RelativeSource FindAncestor` bindings to get properties from the page, such as the page title.

# Default Zone Content #

The `ZonePlaceHolders` are content controls, and their content is overridden when the layout is merged into the page, but only if the page specifies a corresponding zone. This means you can set default content for zones, and allow individual pages to override them.

For example, wizards often have a Back button. Using WPF Pages, the ZonePlaceHolder could be written as:

```
<ZonePlaceHolder Grid.Column="0" Name="BackNavigation">
    <Button Content="Back" Command="NavigationCommands.BrowseBack" />  
</ZonePlaceHolder>
```

If a page that refers to the layout doesn't specify a Zone with the same name, the back button will appear. But a page could override the content, for example:

```
<Zone ZonePlaceHolderName="BackNavigation"> 
    <Button Content="Cancel" Command="{Binding CancelWizardCommand}" />
</Zone>
```

Or it may just choose to clear the content, removing the button from the tree:

```
<Zone ZonePlaceHolderName="BackNavigation" Content="{x:Null}" />
```

# Default Shared Layouts #

Since the `Layout` control's `Source` property is a dependency property, you can use a `Style` to set the default layout source:

```
<Style TargetType="Layout">
    <Setter Property="Source" Value="/Wizard;component/Views/Shared/Main.xaml" />
</Style>
```

When writing pages, you can now just use a Layout element without a source:

```
<Layout>
    <Zone ZonePlaceHolderName="Content">
        <TextBlock Text="The center content goes here" />
    </Zone>
```

Or a page may choose to override the Layout:

```
<Layout Source="/Wizard;component/Views/Shared/Alternative.xaml">
    <Zone ZonePlaceHolderName="Content">
        <TextBlock Text="The center content goes here" />
    </Zone>
```

Interestingly, thanks to dependency properties, you could also use a `Trigger` or data binding to selectively change the layout based on user preferences.

# Nested Layouts #

Nested layouts allow you to apply "inheritance" to layouts. For example, you can set up a common layout that provides a Title and Content zone. You can then create another layout that references the first layout, and sub-divides the content into two column, Left and Right. You might create another layout subdividing the two column layout even further - it's layouts all the way down.

![http://magellan-framework.googlecode.com/hg/assets/docs/NestedLayouts.png](http://magellan-framework.googlecode.com/hg/assets/docs/NestedLayouts.png)

Nested layouts are easy to create. First the **Main.xaml**:

```
<UserControl 
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
    xmlns:magellan="http://xamlforge.com/magellan"
    >
    <DockPanel>
        <ZonePlaceHolder Name="Title" DockPanel.Dock="Top" />
        <ZonePlaceHolder Name="Content" />
    </DockPanel>
</UserControl>
```

Now the TwoColumn.xaml, which references Main.xaml:

```
<UserControl 
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
    xmlns:magellan="http://xamlforge.com/magellan"
    >

    <Layout Source="/MyAssembly;component/Layouts/Main.xaml">        
        <Zone ZonePlaceHolderName="Title">
            <ZonePlaceHolder Name="Title" />
        </Zone>

        <Zone ZonePlaceHolderName="Content">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>

                <ZonePlaceHolder Grid.Column="0" Name="Left" />
                <ZonePlaceHolder Grid.Column="1" Name="Right" />
            </Grid>
        </Zone>
    </Layout>
</UserControl>
```

Note how this layout references the first. To re-expose the `Title` zone, we create a `Zone` with a `ZonePlaceHolder`. We also expose a `Left` and `Right` zone by splitting the `Content` zone using a `Grid`.

# Summary #

Shared Layouts allow you create a consistent look and feel for your views while minimizing XAML and code behind. They can be used not only on `Pages`, but from any XAML - you might create a Shared Layout for Dialogs with OK/Cancel buttons, or for tab pages within an options dialog.

To make use of shared layouts, you just need a reference to **Magellan.dll**. You don't have to use Magellan's MVC framework to use this feature, as they are completely independent.