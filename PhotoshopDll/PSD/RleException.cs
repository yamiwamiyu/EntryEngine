using System;
namespace PhotoshopFile
{
	[Serializable]
	public class RleException : Exception
	{
		public RleException()
		{
		}
		public RleException(string message) : base(message)
		{
		}
	}
}
