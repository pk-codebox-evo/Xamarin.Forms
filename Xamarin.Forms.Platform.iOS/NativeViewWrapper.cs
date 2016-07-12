using System;
using System.Collections.Generic;

#if __UNIFIED__
using CoreGraphics;
using Foundation;
using UIKit;

#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
#endif

#if !__UNIFIED__
// Save ourselves a ton of ugly ifdefs below
using CGSize = System.Drawing.SizeF;
#endif

namespace Xamarin.Forms.Platform.iOS
{

	class NativeViewPropertyListener : NSObject
	{
		readonly INativeViewBindableController nativeBindableController;

		public NativeViewPropertyListener(INativeViewBindableController nativeViewBindableController)
		{
			nativeBindableController = nativeViewBindableController;
		}

		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			nativeBindableController.OnNativePropertyChange(keyPath);
		}
	}

	public class NativeViewWrapper : View, INativeViewBindableController
	{
		NativeViewPropertyListener listener;
		public NativeViewWrapper(UIView nativeView, GetDesiredSizeDelegate getDesiredSizeDelegate = null, SizeThatFitsDelegate sizeThatFitsDelegate = null, LayoutSubviewsDelegate layoutSubViews = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			SizeThatFitsDelegate = sizeThatFitsDelegate;
			LayoutSubViews = layoutSubViews;
			NativeView = nativeView;
		}

		void INativeViewBindableController.ApplyNativeBindings()
		{

			if (NativeBindingExtensions.NativeBindingPool.ContainsKey(NativeView))
			{
				BindableProxies = NativeBindingExtensions.NativeBindingPool[NativeView];

			}
			else
				BindableProxies = new Dictionary<BindableProxy, Binding>();


			foreach (var item in BindableProxies)
			{
				item.Key.SetBinding(item.Key.Property, item.Value);
				item.Key.BindingContext = this.BindingContext;

				if (item.Value.Mode == BindingMode.TwoWay)
				{
					if (listener == null)
						listener = new NativeViewPropertyListener(this);
					NativeView.AddObserver(listener, new NSString(item.Key.TargetPropertyName), 0, IntPtr.Zero);
				}

			}
		}

		void INativeViewBindableController.OnNativePropertyChange(string property, object newValue)
		{
			foreach (var item in BindableProxies)
			{
				if (item.Key.TargetPropertyName == property.ToString())
				{
					item.Key.OnTargetPropertyChanged(newValue, item.Value.Converter);
				}
			}
		}

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public LayoutSubviewsDelegate LayoutSubViews { get; set; }

		public UIView NativeView { get; }


		internal Dictionary<BindableProxy, Binding> BindableProxies
		{
			get;
			set;
		}

		public SizeThatFitsDelegate SizeThatFitsDelegate { get; set; }

		//protected override void OnBindingContextChanged()
		//{
		//	base.OnBindingContextChanged();
		//	if (BindableProxies == null)
		//		return;
		//	foreach (var item in BindableProxies)
		//		item.Key.BindingContext = BindingContext;
		//}


	}
}