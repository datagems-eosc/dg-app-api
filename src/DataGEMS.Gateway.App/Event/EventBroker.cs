
namespace DataGEMS.Gateway.App.Event
{
	public class EventBroker
	{
		#region User Deleted

		private EventHandler<OnEventArgs<Guid>> _userDeleted;
		public event EventHandler<OnEventArgs<Guid>> UserDeleted
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

		public void EmitUserDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserDeleted(this, events);
		}

		public void EmitUserDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region User Touched

		private EventHandler<OnEventArgs<Guid>> _userTouched;
		public event EventHandler<OnEventArgs<Guid>> UserTouched
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

		public void EmitUserTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserTouched(this, events);
		}

		public void EmitUserTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserProfile Deleted

		private EventHandler<OnEventArgs<Guid>> _userProfileDeleted;
		public event EventHandler<OnEventArgs<Guid>> UserProfileDeleted
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

		public void EmitUserProfileDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserProfileDeleted(this, events);
		}

		public void EmitUserProfileDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userProfileDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserProfileDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userProfileDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserProfile Touched

		private EventHandler<OnEventArgs<Guid>> _userProfileTouched;
		public event EventHandler<OnEventArgs<Guid>> UserProfileTouched
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

		public void EmitUserProfileTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserProfileTouched(this, events);
		}

		public void EmitUserProfileTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userProfileTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserProfileTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userProfileTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserCollection Deleted

		private EventHandler<OnEventArgs<Guid>> _userCollectionDeleted;
		public event EventHandler<OnEventArgs<Guid>> UserCollectionDeleted
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

		public void EmitUserCollectionDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserCollectionDeleted(this, events);
		}

		public void EmitUserCollectionDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userCollectionDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserCollectionDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userCollectionDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserCollection Touched

		private EventHandler<OnEventArgs<Guid>> _userCollectionTouched;
		public event EventHandler<OnEventArgs<Guid>> UserCollectionTouched
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

		public void EmitUserCollectionTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserCollectionTouched(this, events);
		}

		public void EmitUserCollectionTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userCollectionTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserCollectionTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userCollectionTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserDatasetCollection Deleted

		private EventHandler<OnEventArgs<Guid>> _userDatasetCollectionDeleted;
		public event EventHandler<OnEventArgs<Guid>> UserDatasetCollectionDeleted
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

		public void EmitUserDatasetCollectionDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserDatasetCollectionDeleted(this, events);
		}

		public void EmitUserDatasetCollectionDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userDatasetCollectionDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserDatasetCollectionDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userDatasetCollectionDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserDatasetCollection Touched

		private EventHandler<OnEventArgs<Guid>> _userDatasetCollectionTouched;
		public event EventHandler<OnEventArgs<Guid>> UserDatasetCollectionTouched
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

		public void EmitUserDatasetCollectionTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserDatasetCollectionTouched(this, events);
		}

		public void EmitUserDatasetCollectionTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userDatasetCollectionTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserDatasetCollectionTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userDatasetCollectionTouched?.Invoke(sender, ev);
		}

		#endregion

		#region Conversation Deleted

		private EventHandler<OnEventArgs<Guid>> _conversationDeleted;
		public event EventHandler<OnEventArgs<Guid>> ConversationDeleted
		{
			add { this._conversationDeleted += value; }
			remove { this._conversationDeleted -= value; }
		}

		public void EmitConversationDeleted(Guid id)
		{
			this.EmitConversationDeleted(this, new List<Guid>() { id });
		}

		public void EmitConversationDeleted(IEnumerable<Guid> ids)
		{
			this.EmitConversationDeleted(this, ids);
		}

		public void EmitConversationDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitConversationDeleted(this, events);
		}

		public void EmitConversationDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._conversationDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitConversationDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._conversationDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region Conversation Touched

		private EventHandler<OnEventArgs<Guid>> _conversationTouched;
		public event EventHandler<OnEventArgs<Guid>> ConversationTouched
		{
			add { this._conversationTouched += value; }
			remove { this._conversationTouched -= value; }
		}

		public void EmitConversationTouched(Guid id)
		{
			this.EmitConversationTouched(this, new List<Guid>() { id });
		}

		public void EmitConversationTouched(IEnumerable<Guid> ids)
		{
			this.EmitConversationTouched(this, ids);
		}

		public void EmitConversationTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitConversationTouched(this, events);
		}

		public void EmitConversationTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._conversationTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitConversationTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._conversationTouched?.Invoke(sender, ev);
		}

		#endregion

		#region ConversationDataset Deleted

		private EventHandler<OnEventArgs<Guid>> _conversationDatasetDeleted;
		public event EventHandler<OnEventArgs<Guid>> ConversationDatasetDeleted
		{
			add { this._conversationDatasetDeleted += value; }
			remove { this._conversationDatasetDeleted -= value; }
		}

		public void EmitConversationDatasetDeleted(Guid id)
		{
			this.EmitConversationDatasetDeleted(this, new List<Guid>() { id });
		}

		public void EmitConversationDatasetDeleted(IEnumerable<Guid> ids)
		{
			this.EmitConversationDatasetDeleted(this, ids);
		}

		public void EmitConversationDatasetDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitConversationDatasetDeleted(this, events);
		}

		public void EmitConversationDatasetDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._conversationDatasetDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitConversationDatasetDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._conversationDatasetDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region ConversationDataset Touched

		private EventHandler<OnEventArgs<Guid>> _conversationDatasetTouched;
		public event EventHandler<OnEventArgs<Guid>> ConversationDatasetTouched
		{
			add { this._conversationDatasetTouched += value; }
			remove { this._conversationDatasetTouched -= value; }
		}

		public void EmitConversationDatasetTouched(Guid id)
		{
			this.EmitConversationDatasetTouched(this, new List<Guid>() { id });
		}

		public void EmitConversationDatasetTouched(IEnumerable<Guid> ids)
		{
			this.EmitConversationDatasetTouched(this, ids);
		}

		public void EmitConversationDatasetTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitConversationDatasetTouched(this, events);
		}

		public void EmitConversationDatasetTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._conversationDatasetTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitConversationDatasetTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._conversationDatasetTouched?.Invoke(sender, ev);
		}

		#endregion

		#region ConversationMessage Deleted

		private EventHandler<OnEventArgs<Guid>> _conversationMessageDeleted;
		public event EventHandler<OnEventArgs<Guid>> ConversationMessageDeleted
		{
			add { this._conversationMessageDeleted += value; }
			remove { this._conversationMessageDeleted -= value; }
		}

		public void EmitConversationMessageDeleted(Guid id)
		{
			this.EmitConversationMessageDeleted(this, new List<Guid>() { id });
		}

		public void EmitConversationMessageDeleted(IEnumerable<Guid> ids)
		{
			this.EmitConversationMessageDeleted(this, ids);
		}

		public void EmitConversationMessageDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitConversationMessageDeleted(this, events);
		}

		public void EmitConversationMessageDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._conversationMessageDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitConversationMessageDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._conversationMessageDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region ConversationMessage Touched

		private EventHandler<OnEventArgs<Guid>> _conversationMessageTouched;
		public event EventHandler<OnEventArgs<Guid>> ConversationMessageTouched
		{
			add { this._conversationMessageTouched += value; }
			remove { this._conversationMessageTouched -= value; }
		}

		public void EmitConversationMessageTouched(Guid id)
		{
			this.EmitConversationMessageTouched(this, new List<Guid>() { id });
		}

		public void EmitConversationMessageTouched(IEnumerable<Guid> ids)
		{
			this.EmitConversationMessageTouched(this, ids);
		}

		public void EmitConversationMessageTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitConversationMessageTouched(this, events);
		}

		public void EmitConversationMessageTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._conversationMessageTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitConversationMessageTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._conversationMessageTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserSettings Deleted

		private EventHandler<OnEventArgs<Guid>> _userSettingsDeleted;
		public event EventHandler<OnEventArgs<Guid>> UserSettingsDeleted
		{
			add { this._userSettingsDeleted += value; }
			remove { this._userSettingsDeleted -= value; }
		}

		public void EmitUserSettingsDeleted(Guid id)
		{
			this.EmitUserSettingsDeleted(this, new List<Guid>() { id });
		}

		public void EmitUserSettingsDeleted(IEnumerable<Guid> ids)
		{
			this.EmitUserSettingsDeleted(this, ids);
		}

		public void EmitUserSettingsDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserSettingsDeleted(this, events);
		}

		public void EmitUserSettingsDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._userSettingsDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserSettingsDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userSettingsDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserSettings Touched

		private EventHandler<OnEventArgs<Guid>> _userSettingsTouched;
		public event EventHandler<OnEventArgs<Guid>> UserSettingsTouched
		{
			add { this._userSettingsTouched += value; }
			remove { this._userSettingsTouched -= value; }
		}

		public void EmitUserSettingsTouched(Guid id)
		{
			this.EmitUserSettingsTouched(this, new List<Guid>() { id });
		}

		public void EmitUserSettingsTouched(IEnumerable<Guid> ids)
		{
			this.EmitUserSettingsTouched(this, ids);
		}

		public void EmitUserSettingsTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitUserSettingsTouched(this, events);
		}

		public void EmitUserSettingsTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._userSettingsTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitUserSettingsTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._userSettingsTouched?.Invoke(sender, ev);
		}

		#endregion

		#region Collection Deleted

		private EventHandler<OnEventArgs<Guid>> _collectionDeleted;
		public event EventHandler<OnEventArgs<Guid>> CollectionDeleted
		{
			add { this._collectionDeleted += value; }
			remove { this._collectionDeleted -= value; }
		}

		public void EmitCollectionDeleted(Guid id)
		{
			this.EmitCollectionDeleted(this, new List<Guid>() { id });
		}

		public void EmitCollectionDeleted(IEnumerable<Guid> ids)
		{
			this.EmitCollectionDeleted(this, ids);
		}

		public void EmitCollectionDeleted(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitCollectionDeleted(this, events);
		}

		public void EmitCollectionDeleted(Object sender, IEnumerable<Guid> ids)
		{
			this._collectionDeleted?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitCollectionDeleted(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._collectionDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region Collection Touched

		private EventHandler<OnEventArgs<Guid>> _collectionTouched;
		public event EventHandler<OnEventArgs<Guid>> CollectionTouched
		{
			add { this._collectionTouched += value; }
			remove { this._collectionTouched -= value; }
		}

		public void EmitCollectionTouched(Guid id)
		{
			this.EmitCollectionTouched(this, new List<Guid>() { id });
		}

		public void EmitCollectionTouched(IEnumerable<Guid> ids)
		{
			this.EmitCollectionTouched(this, ids);
		}

		public void EmitCollectionTouched(IEnumerable<OnEventArgs<Guid>> events)
		{
			this.EmitCollectionTouched(this, events);
		}

		public void EmitCollectionTouched(Object sender, IEnumerable<Guid> ids)
		{
			this._collectionTouched?.Invoke(sender, new OnEventArgs<Guid>(ids));
		}

		public void EmitCollectionTouched(Object sender, IEnumerable<OnEventArgs<Guid>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<Guid> ev in events) this._collectionTouched?.Invoke(sender, ev);
		}

		#endregion

		#region UserDatasetGrant Deleted

		private EventHandler<OnEventArgs<String>> _userDatasetGrantDeleted;
		public event EventHandler<OnEventArgs<String>> UserDatasetGrantDeleted
		{
			add { this._userDatasetGrantDeleted += value; }
			remove { this._userDatasetGrantDeleted -= value; }
		}

		public void EmitUserDatasetGrantDeleted(String id)
		{
			this.EmitUserDatasetGrantDeleted(this, new List<String>() { id });
		}

		public void EmitUserDatasetGrantDeleted(IEnumerable<String> ids)
		{
			this.EmitUserDatasetGrantDeleted(this, ids);
		}

		public void EmitUserDatasetGrantDeleted(IEnumerable<OnEventArgs<String>> events)
		{
			this.EmitUserDatasetGrantDeleted(this, events);
		}

		public void EmitUserDatasetGrantDeleted(Object sender, IEnumerable<String> ids)
		{
			this._userDatasetGrantDeleted?.Invoke(sender, new OnEventArgs<String>(ids));
		}

		public void EmitUserDatasetGrantDeleted(Object sender, IEnumerable<OnEventArgs<String>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<String> ev in events) this._userDatasetGrantDeleted?.Invoke(sender, ev);
		}

		#endregion

		#region UserDatasetGrant Touched

		private EventHandler<OnEventArgs<String>> _userDatasetGrantTouched;
		public event EventHandler<OnEventArgs<String>> UserDatasetGrantTouched
		{
			add { this._userDatasetGrantTouched += value; }
			remove { this._userDatasetGrantTouched -= value; }
		}

		public void EmitUserDatasetGrantTouched(String id)
		{
			this.EmitUserDatasetGrantTouched(this, new List<String>() { id });
		}

		public void EmitUserDatasetGrantTouched(IEnumerable<String> ids)
		{
			this.EmitUserDatasetGrantTouched(this, ids);
		}

		public void EmitUserDatasetGrantTouched(IEnumerable<OnEventArgs<String>> events)
		{
			this.EmitUserDatasetGrantTouched(this, events);
		}

		public void EmitUserDatasetGrantTouched(Object sender, IEnumerable<String> ids)
		{
			this._userDatasetGrantTouched?.Invoke(sender, new OnEventArgs<String>(ids));
		}

		public void EmitUserDatasetGrantTouched(Object sender, IEnumerable<OnEventArgs<String>> events)
		{
			if (events == null) return;
			foreach (OnEventArgs<String> ev in events) this._userDatasetGrantTouched?.Invoke(sender, ev);
		}

		#endregion
	}
}
