using System;
using System.Collections.Generic;
namespace PhotoshopFile
{
	public class ImageResources : List<ImageResource>
	{
		public ImageResource Get(ResourceID id)
		{
			return base.Find((ImageResource x) => x.ID == id);
		}
		public void Set(ImageResource resource)
		{
			Predicate<ImageResource> match = (ImageResource res) => res.ID == resource.ID;
			int num = base.FindIndex(match);
			int num2 = base.FindLastIndex(match);
			if (num == -1)
			{
				base.Add(resource);
				return;
			}
			if (num != num2)
			{
				base.RemoveAll(match);
				base.Insert(num, resource);
				return;
			}
			base[num] = resource;
		}
	}
}
