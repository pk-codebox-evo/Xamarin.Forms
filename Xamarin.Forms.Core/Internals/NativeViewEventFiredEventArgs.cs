using System;
namespace Xamarin.Forms.Internals
{
	public class NativeViewEventFiredEventArgs : EventArgs
	{
		public object NativeEventArgs { get; set; }
		public string PropertyName { get; set; }
		public string EventName { get; set; }
	}
}

