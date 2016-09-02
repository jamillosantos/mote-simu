
namespace Mote.Simu.Reflection
{
	public abstract class ObjectHandler
	{
		public static void SetProperty(object instance, string property, object value)
		{
			instance.GetType().GetProperty(property).SetValue(instance, value, null);
		}
	}
}
