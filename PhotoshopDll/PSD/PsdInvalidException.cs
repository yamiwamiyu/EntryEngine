using System;
namespace PhotoshopFile
{
	[Serializable]
	public class PsdInvalidException : Exception
	{
		public PsdInvalidException()
		{
		}
		public PsdInvalidException(string message) : base(message)
		{
		}
	}
}
