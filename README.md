# Shipwreck.BlazorFramework

MVVM helper libraries for Blazor WebAssembly.

## Assemblies

### Shipwreck.BlazorFramework.Core [nuget](https://www.nuget.org/packages/Shipwreck.BlazorFramework.Core)

#### Types

##### `BindableComponentBase<T>`
##### `BindableLayoutComponentBase<T>`

Supports firing `StateHasChanged` on handling `INotifyPropertyChanged.PropertyChanged`.

##### `ListComponentBase<T>`

Supports firing `StateHasChanged` on handling `INotifyCollectionChanged.CollectionChanged`.

### Shipwreck.BlazorFramework.ItemsControls [nuget](https://www.nuget.org/packages/Shipwreck.BlazorFramework.ItemsControls)

#### Static Web Assets

```html
<script src="Shipwreck.BlazorFramework.ItemsControls.min.js"></script>
```

#### Types

##### `ItemsControl<T> : ListComponentBase<T>`

Provides list virtualization.

###### Members

- `RenderFragment<ItemTemplateContext<T>> ItemTemplate` **(Required)**

Specifies item rendering method.

```xml
<StackPanel T="list element type" Source="some list">
  <ItemTemplate Context="context">
    <!-- A ItemTemplateContext<T> has Index and Item properties. -->
    <div data-itemindex="@context.Index"> <!-- you must specify data-itemindex in the root element. -->
       <!-- custom item presentation -->
       @context.Item.Description
    </div>
  </ItemTemplate>
</StackPanel>
```

- `IDictionary<string, object> AdditionalAttributes`


##### `StackPanel<T> : ItemsControl<T>`

for `flex-flow: column nowrap`

###### Members

- `DefaultItemHeight` minimum estimatation of the item size in pixels.

##### `WrapPanel<T> : ItemsControl<T>`

for `flex-flow: row wrap`

*NOTE: The WrapPanel provides no css styles to the rendered element by default.*

###### Members

- `DefaultItemWidth`
- `DefaultItemHeight` minimum estimatation of the item size in pixels.

