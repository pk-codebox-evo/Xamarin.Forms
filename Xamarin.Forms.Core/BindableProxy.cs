using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace Xamarin.Forms
{
	internal class BindableProxy : BindableObject
	{
		readonly object targetObject;
		readonly string targetProperty;
		readonly PropertyInfo propInfo;

		Action<object, object> callbackSetValue;
		Func<object> callbackGetValue;


		public BindableProperty Property;

		public Type TargetPropertyType;

		public string TargetPropertyName => targetProperty;
		public BindableProxy(object target, PropertyInfo targetPropInfo)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (targetPropInfo == null)
				throw new ArgumentException("targetProperty should not be null or empty", nameof(targetPropInfo));
			targetProperty = targetPropInfo.Name;
			targetObject = target;

			propInfo = targetPropInfo;

			Init();
		}

		public BindableProxy(object target, string targetProp, Action<object, object> setValue = null, Func<object> getValue = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (string.IsNullOrEmpty(targetProp))
				throw new ArgumentException("targetProperty should not be null or empty", nameof(targetProp));
			targetProperty = targetProp;
			targetObject = target;

			callbackSetValue = setValue;
			callbackGetValue = getValue;

			propInfo = targetObject.GetType().GetProperty(targetProperty);

			Init();
		}

		void OnPropertyChanged(object oldValue, object newValue)
		{
			if (callbackSetValue != null)
				callbackSetValue(oldValue, newValue);
			else
				SetTargetValue(newValue);
		}

		void Init()
		{
			Property = BindableProperty.Create(targetProperty, typeof(object), typeof(BindableProxy), propertyChanged: (bo, o, n) => ((BindableProxy)bo).OnPropertyChanged(o, n));

			TargetPropertyType = propInfo.PropertyType;
		}

		internal void OnTargetPropertyChanged(object valueFromNative = null, IValueConverter converter = null)
		{
			//this comes converted
			var currentValue = GetValue(Property);

			var nativeValue = GetTargetValue();

			if (valueFromNative == null)
				valueFromNative = nativeValue;


			if (valueFromNative == currentValue)
				return;

			SetValueCore(Property, valueFromNative);
		}

		void SetTargetValue(object value)
		{
			if (value == null)
				return;
			var valueType = value.GetType();

			if (TargetPropertyType == valueType)
			{
				propInfo.SetValue(targetObject, value);
			}
			else
			{
				throw new InvalidCastException($"Can't bind properties of different types target property {TargetPropertyType}, and the value is {valueType}");
			}

		}

		object GetTargetValue()
		{
			if (callbackGetValue != null)
				return callbackGetValue();

			if (!propInfo.CanRead)
			{
				System.Diagnostics.Debug.WriteLine($"No GetMethod found for {TargetPropertyName}");
				return null;
			}

			var obj = propInfo.GetValue(targetObject);
			return obj;
		}
	}
}

