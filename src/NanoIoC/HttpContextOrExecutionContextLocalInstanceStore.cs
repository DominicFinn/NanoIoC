﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace NanoIoC
{
	/// <summary>
	/// Stores instances in the current ExecutionContext
	/// </summary>
	sealed class HttpContextOrExecutionContextLocalInstanceStore : InstanceStore
	{
		readonly AsyncLocal<IDictionary<Type, IList<Tuple<Registration, object>>>> registrationStore;
		readonly AsyncLocal<IDictionary<Type, IList<Registration>>> injectedRegistrations;
		readonly AsyncLocal<object> mutex;
		readonly Guid id = new Guid();
		protected override Lifecycle Lifecycle => Lifecycle.HttpContextOrExecutionContextLocal;

		public override object Mutex => HttpContext.Current == null
			? this.mutex
			: this.GetCurrentContextInstanceStore();

		public HttpContextOrExecutionContextLocalInstanceStore()
		{
			this.registrationStore = new AsyncLocal<IDictionary<Type, IList<Tuple<Registration, object>>>>
			{
				Value = new Dictionary<Type, IList<Tuple<Registration, object>>>()
			};
			this.injectedRegistrations = new AsyncLocal<IDictionary<Type, IList<Registration>>>
			{
				Value = new Dictionary<Type, IList<Registration>>()
			};
			this.mutex = new AsyncLocal<object>
			{
				Value = new object()
			};
		}

		protected override IDictionary<Type, IList<Tuple<Registration, object>>> Store
		{
			get
			{
				if (HttpContext.Current != null)
					return this.GetCurrentContextInstanceStore() as IDictionary<Type, IList<Tuple<Registration, object>>>;

				if (this.registrationStore.Value == null)
					this.registrationStore.Value = new Dictionary<Type, IList<Tuple<Registration, object>>>();

				return this.registrationStore.Value;
			}
		}

		private object GetCurrentContextInstanceStore()
		{
			return HttpContext.Current.Items["__NanoIoC_InstanceStore_" + this.id] ??
				   (HttpContext.Current.Items["__NanoIoC_InstanceStore_" + this.id] = new Dictionary<Type, IList<Tuple<Registration, object>>>());
		}

		protected override IDictionary<Type, IList<Registration>> InjectedRegistrations
		{
			get
			{
				if (HttpContext.Current != null)
				{
					if (HttpContext.Current.Items["__NanoIoC_InjectedRegistrations_" + this.id] == null)
						HttpContext.Current.Items["__NanoIoC_InjectedRegistrations_" + this.id] = new Dictionary<Type, IList<Registration>>();

					return HttpContext.Current.Items["__NanoIoC_InjectedRegistrations_" + this.id] as IDictionary<Type, IList<Registration>>;
				}

				if (this.injectedRegistrations.Value == null)
					this.injectedRegistrations.Value = new Dictionary<Type, IList<Registration>>();

				return this.injectedRegistrations.Value;
			}
		}

		public override IInstanceStore Clone()
		{
			var instanceStore = new HttpContextOrExecutionContextLocalInstanceStore();

			if (HttpContext.Current != null)
			{
				// todo: replace ILists with new lists, and registrations with new registrations
				HttpContext.Current.Items["__NanoIoC_InstanceStore_" + instanceStore.id] = new Dictionary<Type, IList<Tuple<Registration, object>>>(this.Store);
				HttpContext.Current.Items["__NanoIoC_InjectedRegistrations_" + instanceStore.id] = new Dictionary<Type, IList<Registration>>(this.InjectedRegistrations);
			}
			else
			{
				// todo: replace ILists with new lists, and registrations with new registrations
				instanceStore.registrationStore.Value = new Dictionary<Type, IList<Tuple<Registration, object>>>(this.Store);
				instanceStore.injectedRegistrations.Value = new Dictionary<Type, IList<Registration>>(this.InjectedRegistrations);
			}

			instanceStore.Registrations = new Dictionary<Type, IList<Registration>>(this.Registrations);

			return instanceStore;
		}
	}
}