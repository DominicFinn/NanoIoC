# NanoIoC

A tiny IoC container, does exactly what you want, and only that.

## Getting Started

`Container.Global` is a static instance of `IContainer`. You can use this as your entry point.

### Manually Registering Dependencies

Use either of these methods:

<pre>
void IContainer.Register&lt;TAbstract, TConcrete&gt;(Lifecycle lifecycle = Lifecycle.Singleton);
void IContainer.Register(Type abstract, Type concrete, Lifecycle lifecycle = Lifecycle.Singleton);
</pre>

You will typically want to put your registrations inside an `IContainerRegistry`.

NanoIoC will find all `IContainerRegistrys` in all assemblies in the application's base directory (excluding those that start with the `System` namespace)

To run all the registries, use:

<pre>
void IContainer.RunAllRegistries();
</pre>

### Auto Registering Dependencies

You can create `TypeProcessors` that scan all types allowing you to auto-wire them up. 

NanoIoC will find all `TypeProcesors` in all assemblies in the application's base directory (excluding those that start with the `System` namespace)

For example:

<pre>
void IContainer.RunAllTypeProcessors();
</pre>

Where one of your `TypeProcessor`s might look like:
<pre>
public class ExampleTypeProcessor : ITypeProcessor
{
	public void Process(Type type, IContainer container)
	{
		if(typeof(MyInterface).IsAssignableFrom(type) && type != typeof(MyInterface))
			container.Register(typeof(MyInterface), type, Lifecycle.Singleton);
	}
}
</pre>


### Resolving Dependencies:

Use either of these methods:

<pre>
T IContainer.Resolve&lt;T&gt;();
object IContainer.Resolve(Type type);
</pre>

You can resolve concrete types that aren't registered, as long as all their dependencies are registered or directly constructable.

You can get all registered types:
<pre>
IEnumerable<T> IContainer.ResolveAll<T>()
IEnumerable IContainer.ResolveAll(Type type);
</pre>

### Injecting instances:

You can inject existing instances:

<pre>
void IContainer.Inject<T>(T instance, Lifecycle lifeCycle = Lifecycle.Singleton);
void IContainer.Inject(object instance, Type type, Lifecycle lifecycle);
</pre>