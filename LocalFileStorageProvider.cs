using NerdAmigo.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdAmigo.Common.IO
{
	public class LocalFileStorageProvider<TStorableObject> : IFileStorageProvider<TStorableObject> where TStorableObject : class, IFileStorableObject<TStorableObject>
    {
		private IPathMapper mPathMapper;
		public LocalFileStorageProvider(IPathMapper aPathMapper)
		{
			this.mPathMapper = aPathMapper;
		}

		public IFileStorageItemInfo<TStorableObject> GetStorageItemInfo(TStorableObject aStorageItem)
		{
			return new LocalFileStorageItemInfo<TStorableObject>(this.mPathMapper, aStorageItem);
		}
	}
}
