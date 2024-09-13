# Blazor.DynamicJsInterop

Invoking JavaScript made easy, and clean.

Instead of
```
@inject IJSRuntime JsRuntime
```
use our
```
@inject IDynamicJSRuntime JsRuntime
```
This enables use JavaScript just like C#.

## Install

Install Nuget
```
Blazor.DynamicJsInterop
```

Add the library in your Project Startup
```
services.AddDynamicJSRuntime();
```

## Methods

Lets imagine we have the following JavaScript Method:

```
function add(first, second) {
  return first + second;
}
```

This is how we call it with the normal IJSRuntime

```
var result = await JsRuntime.InvokeAsync<int>("add", 1, 2);
```

And this is how we call it with the new IDynamicJSRuntime

```
var result = await JsRuntime.Window.add(1, 2);
```

You can also call every other Window Method like that.

## Properties
You can also get Property values the same way

```
var result = await JsRuntime.Window.name;
```

And set values (as we cant await assign operations directly, we need to await a seperate Property which holds the Task to the current Assign operation)
```
JsRuntime.Window.name = "DynamicJs";
await JsRuntime.WriteOperation;
```

## Objects

If your Method or Property returns a JavaScript Object, IDynamicJSRuntime returns a IDynamicJSObjectReference. This Object holds a reference to the actual JavaScript Object, just like the build in IJSObjectReference.

This enables us to do something like this
```
var document = await JsRuntime.Window.document;
await document.clear();
```

or even easier
```
await JsRuntime.Window.document.clear();
```

## Isolated JavaScript Modules
Instead of IDynamicJSRuntime, we can use its generic alternative
```
@inject IDynamicJSRuntime<TComponent> JsRuntime
```
where TComponent is the acutal Razor Component with a isolated JavaScript file

This enables us to call Methods/Properties inside our Isolated JavaScript
```
await JsRuntime.Module.add(1, 2);
```

The Module Property references our Isolated JavaScript.

### Importing Isolated JavaScript
As you may have noticed, we havent imported our Isolated JavaScript like
```
var module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "{path}/component.js");
```
Thats because this Library Resolves and imports the JavaScript automatically the first time it is used.

How?

Currently this Library expects the Namespace to be exactly the same as the Path of the JavaScript file within the Project.

For Example, the JS File is located under /Components/Pages/Index/Index.js. So its namespace needs to {ProjectName}.Components.Pages.Index.

I plan to change this in the future.

Tip: 
<mark>Remember to always await every Operation with IDynamicJSRuntime</mark>

### This Project is MIT Licensed