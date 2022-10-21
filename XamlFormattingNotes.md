### XAML Conventions

#### NewLines & Indents

* up to 3 attributes in first line
* if newline is needed then each attribute should be in separate line
* if attributes are splitted in self closing control then empty should be inserted after control

#### Order of properties in _Control_

1. Attached properties
2. "Name" property
3. "Classes" property
4. Properties with fixed values
5. Bindings
6. Events

```xaml
 <RichTextBlock
    Grid.Column="0"
    Name="RichTextBlock"
    Margin="4 1"
    IsTabStop="False"
    VerticalAlignment="Center"
    Inlines="{TemplateBinding Inlines}"
    Text="{TemplateBinding Text}"
    PionterPressed="RichTextBlock_PointerPressed"/>
```
