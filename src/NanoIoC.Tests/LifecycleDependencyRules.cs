using NUnit.Framework;

namespace NanoIoC.Tests
{
	public class LifecycleDependencyRules
	{
		private static Lifecycle[][] InvalidCombinations()
		{
			return new[]
			{
				new[] {Lifecycle.Singleton, Lifecycle.HttpContextOrExecutionContextLocal},
				new[] {Lifecycle.Singleton, Lifecycle.Transient},
				new[] {Lifecycle.HttpContextOrExecutionContextLocal, Lifecycle.Transient}
			};
		}

		[TestCaseSource(nameof(InvalidCombinations))]
		public void CannotDependOnAnInstanceWithShorterLifecycle(Lifecycle dependant, Lifecycle dependency)
		{
			var container = new Container();
			container.Register<Dependant>(dependant);
			container.Register<Dependency>(dependency);

			Assert.That(() => container.Resolve<Dependant>(),
				Throws.InstanceOf<ContainerException>()
					.With.Message.StringMatching("It's lifecycle \\(.*\\) is shorter than the dependee's"));
		}

		private static Lifecycle[][] ValidCombinations()
		{
			return new[]
			{
				new[] {Lifecycle.Singleton, Lifecycle.Singleton},
				new[] {Lifecycle.HttpContextOrExecutionContextLocal, Lifecycle.Singleton},
				new[] {Lifecycle.HttpContextOrExecutionContextLocal, Lifecycle.HttpContextOrExecutionContextLocal},
				new[] {Lifecycle.Transient, Lifecycle.Singleton},
				new[] {Lifecycle.Transient, Lifecycle.HttpContextOrExecutionContextLocal},
				new[] {Lifecycle.Transient, Lifecycle.Transient}
			};
		}

		[TestCaseSource(nameof(ValidCombinations))]
		public void CanDependOnAnInstanceWithLongerOrSameLifecycle(Lifecycle dependant, Lifecycle dependency)
		{
			var container = new Container();
			container.Register<Dependant>(dependant);
			container.Register<Dependency>(dependency);

			Assert.DoesNotThrow(() => container.Resolve<Dependant>());
		}

		private class Dependant { public Dependant(Dependency dependency) { } }
		private class Dependency { }
	}
}