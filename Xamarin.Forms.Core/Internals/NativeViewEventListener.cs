using System;
using System.Reflection;

namespace Xamarin.Forms.Internals
{
	public class NativeViewEventListener : IDisposable
	{
		public EventHandler<NativeViewEventFiredEventArgs> NativeViewEventFired;

		public NativeViewEventListener(object target, string eventName, string propertyName)
		{
			this.eventName = eventName;
			this.propertyName = propertyName;
			this.target = target;
			Subscribe();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					UnSubscribe();
				}

				disposedValue = true;
			}
		}

		string eventName;
		string propertyName;
		Delegate handlerDelegate;
		EventInfo eventInfo;
		object target;
		bool disposedValue;

		void Subscribe()
		{
			eventInfo = target.GetType().GetRuntimeEvent(eventName);

			if (eventInfo == null)
			{
				throw new ArgumentNullException($"Event not found with the name {eventName}");
			}

			Action<object, object> handler = NativeEventFired;
			var methodInfo = handler.GetMethodInfo();
			handlerDelegate = methodInfo.CreateDelegate(eventInfo.EventHandlerType, this);
			//handlerDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, handler.Target, handler.Method);
			eventInfo.AddEventHandler(target, handlerDelegate);
		}

		void UnSubscribe()
		{
			eventInfo?.RemoveEventHandler(target, handlerDelegate);
		}

		void NativeEventFired(object sender, object e)
		{
			if (NativeViewEventFired != null)
				NativeViewEventFired(this, new NativeViewEventFiredEventArgs { NativeEventArgs = e, PropertyName = propertyName, EventName = eventName });
		}
	}
}

