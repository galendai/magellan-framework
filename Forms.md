# Introduction #

Data entry forms are common in line of business WPF applications, and they can become repetitive to write. Magellan includes a new set of controls that you can use to rapidly throw data entry forms together.

The goals of Magellan Forms are:

  * Minimal XAML
  * Abstract presentation concerns
  * Use conventions to infer as much information as possible
  * Flexible and extensible

# Getting Started #

To illustrate, take a form declared like this:

```
<Form>
    <Field For="{Binding Path=Server.Server}" />
    <Field For="{Binding Path=Server.CachedExchangeMode}" />
    <Field For="{Binding Path=Server.Username}" />
    <Field For="{Binding Path=Server.SecurityMode}" />
</Form>
```

The object model that this form is bound to looks like this:

```
public class ExchangeServerSettings : ServerSettings
{
    public string Server { get; set; }

    public string Username { get; set; }

    [DisplayName("Use cached Exchange mode")]
    public bool CachedExchangeMode { get; set; }

    [DisplayName("Security mode")]
    public ExchangeSecurityMode SecurityMode { get; set; }
}

public enum ExchangeSecurityMode
{
    [EnumDisplayName("Negotiate")]
    Negotiate,
    [EnumDisplayName("NTLM")]
    Ntlm,
    [EnumDisplayName("Kerberos")]
    Kerberos
}
```

This markup and code is all Magellan Forms needs to figure out how to render the form:

![http://magellan-framework.googlecode.com/hg/assets/docs/Forms.png](http://magellan-framework.googlecode.com/hg/assets/docs/Forms.png)

Magellan was able to infer:

  * The caption for each field, using either the `DisplayName` attribute or property name.
  * The control to use for each field, based on the control type

# Overriding #

While Magellan Forms is able to infer settings based on the bindings, the feature is optional. You can instead manually set all of the field values:

```
<Field Header="First name:">
    <TextBox Text="{Binding Path=FirstName}" Width="200" />
</Field>
```

Fields are just `ContentControls`, so you can use anything you like inside a field. You can also choose to infer most of a field while overriding a specific setting, for example:

```
<Field Header="Surname:" For="{Binding Path=LastName}" />
```

# Styling #

A `Form` is simply an `ItemsControl`, which by default uses a `StackPanel` to lay out the children. A `Field` is a `ContentControl`. This means you can use styles and templates to customize how they are rendered.

For example, suppose our design team decided that all labels should now be right-aligned:

![http://magellan-framework.googlecode.com/hg/assets/docs/FormsRight.png](http://magellan-framework.googlecode.com/hg/assets/docs/FormsRight.png)

They just have to override the Field template in App.xaml:

```
<Style TargetType="Field">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Field">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="120" />
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>

                    <Label Content="{TemplateBinding Header}" HorizontalContentAlignment="Right" Margin="2" />
                    <ContentPresenter Grid.Column="1" />
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

# Extending #

The conventions used to infer control types and display settings can be customized through two extension points:

  1. `IFieldConvention`, used to figure out the heading and other settings
  1. `IEditorStrategy`, a list of which is used to create the appropriate editor for a type

You can implement `IFieldConvention` and then register it via the attached inherited property on a field:

```
<Form Form.FieldConvention="{x:Static MyFieldConvention.Instance}"> ...
```

When the default field convention is choosing an editor, it consults a list of registered `IEditorStrategy` implementations. There are three out of the box:

```
TextBoxEditorStrategy
ComboBoxEditorStrategy
CheckBoxEditorStrategy
```

Writing your own is easy. For example, suppose you want to show a masked editor for numeric types. The code below assumes you use something like the Xceed masked editor:

```
public class MaskedTextBoxEditorStrategy : IEditorStrategy
{
    public object CreateEditor(FieldContext context)
    {
        var isInteger = context.PropertyDescriptor.PropertyType == typeof(int);
        var isDecimal = context.PropertyDescriptor.PropertyType == typeof(decimal);

        if (!isInteger && !isDecimal) 
        {   
            // This editor is only concerned with numeric types 
            return null;
        }

        var maskedTextBox = new MaskedTextBox();
        maskedTextBox.Mask = isInteger ? "999,999" : "999,999.99";
        BindingOperations.SetBinding(maskedTextBox, TextBox.TextProperty, context.FieldBinding);
        return maskedTextBox;
    }
}
```

The editor then just needs to be registered (usually somewhere in App.xaml.cs):

```
EditorStrategies.Strategies.Insert(0, new MaskedTextBoxEditorStrategy());
```

Note that when a field is inferred, all editors are consulted in order, and the first non-null result is used. That's why you typically want to Insert an editor to the top of the list rather than adding it to the end. I'm interested in feedback on this design.

# Validation #

Editor strategies can also make use of validation attributes to provide richer information and UI cues. For example, suppose you had a standard range of TextBox sizes:

```
<Style x:Key="TextBox.Small" TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}">
    <Setter Property="Width" Value="100" />
</Style>

<Style x:Key="TextBox.Normal" TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}">
    <Setter Property="Width" Value="200" />
</Style>

<Style x:Key="TextBox.Big" TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}">
    <Setter Property="Width" Value="300" />
</Style>
```

The view model could use make use of Data Annotations to specify the maximum text length and whether a field is mandatory:

```
[Required]
[StringLength(30)]
public string Username { get; set; }
```

A custom editor strategy could detect these and set the TextBox settings as follows:

```
public class CustomTextBoxEditorStrategy : IEditorStrategy
{
    public object CreateEditor(FieldContext context)
    {
        if (context.PropertyDescriptor.PropertyType != typeof(string))
        {
            // We only deal with strings
            return null;
        }

        var required = context.PropertyDescriptor.Attributes.OfType<RequiredAttribute>().FirstOrDefault();
        var length = context.PropertyDescriptor.Attributes.OfType<StringLengthAttribute>().FirstOrDefault();

        if (required != null)
        {
            context.Field.IsRequired = true;
        }

        var textBox = new TextBox();
        BindingOperations.SetBinding(textBox, TextBox.TextProperty, context.Binding);
        if (length != null)
        {
            var max = length.MaximumLength;
            if (max < 20) textBox.Style = (Style)context.Field.FindResource("TextBox.Small");
            else if (max < 50) textBox.Style = (Style)context.Field.FindResource("TextBox.Normal");
            else textBox.Style = (Style)context.Field.FindResource("TextBox.Big");

            textBox.MaxLength = max;
        }
        return textBox;
    }
}
```

The editor strategy can also set values on the bindings, such as adding new `ValidationRules`.

# Summary #
Magellan Forms provides a foundation that allows you to leverage a little infrastructure code (editor strategies, styles, annotations) to gain a lot of reuse. I like to think of this as 'semantic XAML', that is, the functional XAML just describes that I want a form with some fields, rather than all the specifics about layout. The layout decisions are differed to styles that designers can manage, giving us consistency and a single point of change. I hope you find the feature useful.

# Note for partial trust applications #
Inference relies on invoking a private member on `BindingExpression` to work. This means it won't work in XBAP's and other partial trust scenarios. The rest of the forms library should work - just don't use the For property.