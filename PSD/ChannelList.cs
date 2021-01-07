using System;
using System.Collections.Generic;
using System.Linq;
namespace PhotoshopFile
{
	public class ChannelList : List<Channel>
	{
		public Channel[] ToIdArray()
		{
			short num = this.Max((Channel x) => x.ID);
			Channel[] array = new Channel[(int)(num + 1)];
			foreach (Channel current in this)
			{
				if (current.ID >= 0)
				{
					array[(int)current.ID] = current;
				}
			}
			return array;
		}
		public Channel GetId(int id)
		{
			return this.Single((Channel x) => (int)x.ID == id);
		}
		public bool ContainsId(int id)
		{
			return base.Exists((Channel x) => (int)x.ID == id);
		}
	}
}
