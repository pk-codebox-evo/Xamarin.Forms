using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public static class NativeBindingExtensions
	{
		internal static Dictionary<UIView, Dictionary<BindableProxy, Binding>> NativeBindingPool = new Dictionary<UIView, Dictionary<BindableProxy, Binding>>();

		public static void SetBinding(this UIView self, Expression<Func<object>> memberLamda, Binding binding)
		{
			SetBinding(self, memberLamda, binding, null);
		}

		//this works better but maybe is slower
		public static void SetBinding(this UIView self, Expression<Func<object>> memberLamda, Binding binding, string eventName)
		{
			var memberSelectorExpression = memberLamda.Body as MemberExpression;
			if (memberSelectorExpression != null)
			{
				var property = memberSelectorExpression.Member as PropertyInfo;
				var proxy = new BindableProxy(self, property, eventName);
				SetBinding(self, binding, proxy);
			}
		}

		public static void SetBinding(this UIView self, string propertyName, Binding binding, Action<object, object> callback = null, Func<object> getter = null)
		{
			var proxy = new BindableProxy(self, propertyName, callback, getter);
			SetBinding(self, binding, proxy);
		}

		static void SetBinding(UIView view, Binding binding, BindableProxy bindableProxy)
		{
			FindConverter(binding, bindableProxy);

			if (NativeBindingPool.ContainsKey(view))
			{
				NativeBindingPool[view].Add(bindableProxy, binding);
			}
			else
			{
				NativeBindingPool.Add(view, new Dictionary<BindableProxy, Binding> { { bindableProxy, binding } });
			}
		}

		static void FindConverter(Binding binding, BindableProxy proxy)
		{
			if (binding.Converter != null)
				return;

			//this needs to be done upfront and cached.
			var assembly = Assembly.GetExecutingAssembly();
			var converterClassName = $"{assembly.GetName().Name}.{proxy.TargetPropertyType.Name}Converter";
			var converter = assembly.CreateInstance(converterClassName) as IValueConverter;
			if (converter != null)
				binding.Converter = converter;
		}


	}
}

