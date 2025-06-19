
namespace DataGEMS.Gateway.App.Event
{
	public class EventBroker
	{
		#region User Deleted

		private EventHandler<OnEventArgs> _userDeleted;
		public event EventHandler<OnEventArgs> UserDeleted
		{
			add { this._userDeleted += value; }
			remove { this._userDeleted -= value; }
		}

		public void EmitUserDeleted(Guid id)
		{
			this.EmitUserDeleted(this, new List<Guid>() { id });
		}

		public void EmitUserDeleted(IEnumerable<Guid> ids)
		{
			this.EmitUserDeleted(this, ids);
		}

		public void EmitUserDeleted(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserDeleted(this, events);
		}

		public void EmitUserDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userDeleted?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserDeleted(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region User Touched

		private EventHandler<OnEventArgs> _userTouched;
		public event EventHandler<OnEventArgs> UserTouched
		{
			add { this._userTouched += value; }
			remove { this._userTouched -= value; }
		}

		public void EmitUserTouched(Guid id)
		{
			this.EmitUserTouched(this, new List<Guid>() { id });
		}

		public void EmitUserTouched(IEnumerable<Guid> ids)
		{
			this.EmitUserTouched(this, ids);
		}

		public void EmitUserTouched(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserTouched(this, events);
		}

		public void EmitUserTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userTouched?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserTouched(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserProfile Deleted

		private EventHandler<OnEventArgs> _userProfileDeleted;
		public event EventHandler<OnEventArgs> UserProfileDeleted
		{
			add { this._userProfileDeleted += value; }
			remove { this._userProfileDeleted -= value; }
		}

		public void EmitUserProfileDeleted(Guid id)
		{
			this.EmitUserProfileDeleted(this, new List<Guid>() { id });
		}

		public void EmitUserProfileDeleted(IEnumerable<Guid> ids)
		{
			this.EmitUserProfileDeleted(this, ids);
		}

		public void EmitUserProfileDeleted(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserProfileDeleted(this, events);
		}

		public void EmitUserProfileDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userProfileDeleted?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserProfileDeleted(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userProfileDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserProfile Touched

		private EventHandler<OnEventArgs> _userProfileTouched;
		public event EventHandler<OnEventArgs> UserProfileTouched
		{
			add { this._userProfileTouched += value; }
			remove { this._userProfileTouched -= value; }
		}

		public void EmitUserProfileTouched(Guid id)
		{
			this.EmitUserProfileTouched(this, new List<Guid>() { id });
		}

		public void EmitUserProfileTouched(IEnumerable<Guid> ids)
		{
			this.EmitUserProfileTouched(this, ids);
		}

		public void EmitUserProfileTouched(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserProfileTouched(this, events);
		}

		public void EmitUserProfileTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userProfileTouched?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserProfileTouched(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userProfileTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserCollection Deleted

		private EventHandler<OnEventArgs> _userCollectionDeleted;
		public event EventHandler<OnEventArgs> UserCollectionDeleted
		{
			add { this._userCollectionDeleted += value; }
			remove { this._userCollectionDeleted -= value; }
		}

		public void EmitUserCollectionDeleted(Guid id)
		{
			this.EmitUserCollectionDeleted(this, new List<Guid>() { id });
		}

		public void EmitUserCollectionDeleted(IEnumerable<Guid> ids)
		{
			this.EmitUserCollectionDeleted(this, ids);
		}

		public void EmitUserCollectionDeleted(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserCollectionDeleted(this, events);
		}

		public void EmitUserCollectionDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userCollectionDeleted?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserCollectionDeleted(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userCollectionDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserCollection Touched

		private EventHandler<OnEventArgs> _userCollectionTouched;
		public event EventHandler<OnEventArgs> UserCollectionTouched
		{
			add { this._userCollectionTouched += value; }
			remove { this._userCollectionTouched -= value; }
		}

		public void EmitUserCollectionTouched(Guid id)
		{
			this.EmitUserCollectionTouched(this, new List<Guid>() { id });
		}

		public void EmitUserCollectionTouched(IEnumerable<Guid> ids)
		{
			this.EmitUserCollectionTouched(this, ids);
		}

		public void EmitUserCollectionTouched(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserCollectionTouched(this, events);
		}

		public void EmitUserCollectionTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userCollectionTouched?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserCollectionTouched(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userCollectionTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserDatasetCollection Deleted

		private EventHandler<OnEventArgs> _userDatasetCollectionDeleted;
		public event EventHandler<OnEventArgs> UserDatasetCollectionDeleted
		{
			add { this._userDatasetCollectionDeleted += value; }
			remove { this._userDatasetCollectionDeleted -= value; }
		}

		public void EmitUserDatasetCollectionDeleted(Guid id)
		{
			this.EmitUserDatasetCollectionDeleted(this, new List<Guid>() { id });
		}

		public void EmitUserDatasetCollectionDeleted(IEnumerable<Guid> ids)
		{
			this.EmitUserDatasetCollectionDeleted(this, ids);
		}

		public void EmitUserDatasetCollectionDeleted(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserDatasetCollectionDeleted(this, events);
		}

		public void EmitUserDatasetCollectionDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userDatasetCollectionDeleted?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserDatasetCollectionDeleted(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userDatasetCollectionDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserDatasetCollection Touched

		private EventHandler<OnEventArgs> _userDatasetCollectionTouched;
		public event EventHandler<OnEventArgs> UserDatasetCollectionTouched
		{
			add { this._userDatasetCollectionTouched += value; }
			remove { this._userDatasetCollectionTouched -= value; }
		}

		public void EmitUserDatasetCollectionTouched(Guid id)
		{
			this.EmitUserDatasetCollectionTouched(this, new List<Guid>() { id });
		}

		public void EmitUserDatasetCollectionTouched(IEnumerable<Guid> ids)
		{
			this.EmitUserDatasetCollectionTouched(this, ids);
		}

		public void EmitUserDatasetCollectionTouched(IEnumerable<OnEventArgs> events)
		{
			this.EmitUserDatasetCollectionTouched(this, events);
		}

		public void EmitUserDatasetCollectionTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userDatasetCollectionTouched?.Invoke(sender, new OnEventArgs(ids));
		}

		public void EmitUserDatasetCollectionTouched(Object sender, IEnumerable<OnEventArgs> events)
		{
			if (events == null) return;
			foreach (OnEventArgs ev in events) this._userDatasetCollectionTouched?.Invoke(sender, ev);
		}

		#endregion
	}
}
