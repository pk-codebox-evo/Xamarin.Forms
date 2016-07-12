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

		//this works better but maybe is slower
		public static void SetBinding(this UIView self, Expression<Func<object>> memberLamda, Binding binding)
		{
			var memberSelectorExpression = memberLamda.Body as MemberExpression;
			if (memberSelectorExpression != null)
			{
				var property = memberSelectorExpression.Member as PropertyInfo;
				var proxy = new BindableProxy(self, property);

				FindConverter(binding, proxy);

				if (NativeBindingPool.ContainsKey(self))
				{
					NativeBindingPool[self].Add(proxy, binding);
				}
				else
				{
					NativeBindingPool.Add(self, new Dictionary<BindableProxy, Binding> { { proxy, binding } });
				}
			}
		}

		public static void SetBinding(this UIView self, string propertyName, Binding binding, Action<object, object> callback = null, Func<object> getter = null)
		{
			var proxy = new BindableProxy(self, propertyName, callback, getter);

			FindConverter(binding, proxy);

			if (NativeBindingPool.ContainsKey(self))
			{
				NativeBindingPool[self].Add(proxy, binding);
			}
			else
			{
				NativeBindingPool.Add(self, new Dictionary<BindableProxy, Binding> { { proxy, binding } });
			}
		}

		static void FindConverter(Binding binding, BindableProxy proxy)
		{
			if (binding.Converter != null)
				return;

			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var converter = assembly.CreateInstance($"{assembly.GetName().Name}.{proxy.TargetPropertyType.Name}Converter") as IValueConverter;
			if (converter != null)
				binding.Converter = converter;
		}


	}
}

