using NerdAmigo.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdAmigo.Common.IO
{
	public class LocalFileStorageItemInfo<TStorableObject> : IFileStorageItemInfo<TStorableObject> where TStorableObject : class, IFileStorableObject<TStorableObject>
	{
		private const string BASE_PATH = "~/App_Data";

		private static FileSystemWatcher _FileWatcher;
		private static HashSet<LocalFileStorageItemInfo<TStorableObject>> _ItemInfoInstances;

		private FileInfo mFileInfo;
		private string mBasePath;
		private string mFileName;
		private HashSet<Stream> mStreams;
		private HashSet<Action> mUpdateActions;
		private HashSet<Action> mDeleteActions;

		static LocalFileStorageItemInfo()
		{
			_ItemInfoInstances = new HashSet<LocalFileStorageItemInfo<TStorableObject>>();
		}

		public LocalFileStorageItemInfo(IPathMapper aPathMapper, TStorableObject aStorageItem)
		{
			this.mBasePath = aPathMapper.MapPath(BASE_PATH);
			this.mFileName = aStorageItem.FileName;
			string tFullPath = Path.Combine(this.mBasePath, this.mFileName);
			this.mFileInfo = new FileInfo(tFullPath);
			this.mStreams = new HashSet<Stream>();
			this.mUpdateActions = new HashSet<Action>();
			this.mDeleteActions = new HashSet<Action>();

			_ItemInfoInstances.Add(this);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				HashSet<Exception> disposeExceptions = new HashSet<Exception>();
				foreach(Stream s in this.mStreams)
				{
					try
					{
						s.Dispose();
					}
					catch (Exception ex)
					{
						disposeExceptions.Add(ex);
					}
				}

				_ItemInfoInstances.Remove(this);

				if(disposeExceptions.Count > 0)
				{
					throw new AggregateException("One or more exceptions encountered while disposing", disposeExceptions);
				}
			}
		}

		public void OnCreate(Action createAction)
		{
			throw new NotImplementedException();
		}

		public void OnUpdate(Action updateAction)
		{
			InitializeFileSystemWatcher(this.mBasePath);
			if (!mUpdateActions.Contains(updateAction))
			{
				this.mUpdateActions.Add(updateAction);
			}
		}

		public void OnDelete(Action deleteAction)
		{
			InitializeFileSystemWatcher(this.mBasePath);
			if (!mDeleteActions.Contains(deleteAction))
			{
				this.mDeleteActions.Add(deleteAction);
			}
		}

		private static void InitializeFileSystemWatcher(string aBasePath)
		{
			if (_FileWatcher == null)
			{
				_FileWatcher = new FileSystemWatcher(aBasePath);
				_FileWatcher.Changed += mFileWatcher_Changed;
				_FileWatcher.Deleted += mFileWatcher_Deleted;
				_FileWatcher.IncludeSubdirectories = true;
				_FileWatcher.EnableRaisingEvents = true;
			}
		}

		private static void mFileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			foreach (LocalFileStorageItemInfo<TStorableObject> instance in _ItemInfoInstances)
			{
				if (e.FullPath == instance.mFileInfo.FullName)
				{
					foreach (Action deleteAction in instance.mDeleteActions)
					{
						deleteAction();
					}
				}
			}
		}

		private static void mFileWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			foreach (LocalFileStorageItemInfo<TStorableObject> instance in _ItemInfoInstances)
			{
				if (e.FullPath == instance.mFileInfo.FullName)
				{
					foreach (Action updateAction in instance.mUpdateActions)
					{
						updateAction();
					}
				}
			}
		}

		public Stream Open()
		{
			//open the file, store a reference to the stream used
			if (!this.mFileInfo.Exists)
			{
				throw new Exception(String.Format("Cannot open a storage item that does not exist '{0}'", this.mFileName));
			}

			Stream fileStream = this.mFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			this.mStreams.Add(fileStream);

			return fileStream;
		}

		public void Save(System.IO.Stream data)
		{
			throw new NotImplementedException();
		}

		public void Delete()
		{
			throw new NotImplementedException();
		}

		public bool Exists()
		{
			return this.mFileInfo.Exists;
		}
	}
}
