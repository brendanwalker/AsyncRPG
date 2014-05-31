// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from AsyncRPG on 2014-05-30 23:03:53Z.
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace AsyncRPGSharedLib.Database
{
	using System;
	using System.ComponentModel;
	using System.Data;
#if MONO_STRICT
	using System.Data.Linq;
#else   // MONO_STRICT
	using DbLinq.Data.Linq;
	using DbLinq.Vendor;
#endif  // MONO_STRICT
	using System.Data.Linq.Mapping;
	using System.Diagnostics;
	
	
	public partial class AsyncRPGDataContext : DataContext
	{
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion
		
		
		public AsyncRPGDataContext(string connectionString) : 
				base(connectionString)
		{
			this.OnCreated();
		}
		
		public AsyncRPGDataContext(string connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public AsyncRPGDataContext(IDbConnection connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Table<Accounts> Accounts
		{
			get
			{
				return this.GetTable<Accounts>();
			}
		}
		
		public Table<Characters> Characters
		{
			get
			{
				return this.GetTable<Characters>();
			}
		}
		
		public Table<Config> Configs
		{
			get
			{
				return this.GetTable<Config>();
			}
		}
		
		public Table<EnergyTanks> EnergyTanks
		{
			get
			{
				return this.GetTable<EnergyTanks>();
			}
		}
		
		public Table<GameEvents> GameEvents
		{
			get
			{
				return this.GetTable<GameEvents>();
			}
		}
		
		public Table<Games> Games
		{
			get
			{
				return this.GetTable<Games>();
			}
		}
		
		public Table<Loot> Loot
		{
			get
			{
				return this.GetTable<Loot>();
			}
		}
		
		public Table<Mobs> Mobs
		{
			get
			{
				return this.GetTable<Mobs>();
			}
		}
		
		public Table<MobSpawners> MobSpawners
		{
			get
			{
				return this.GetTable<MobSpawners>();
			}
		}
		
		public Table<MobSpawnTableEntries> MobSpawnTableEntries
		{
			get
			{
				return this.GetTable<MobSpawnTableEntries>();
			}
		}
		
		public Table<MobSpawnTables> MobSpawnTables
		{
			get
			{
				return this.GetTable<MobSpawnTables>();
			}
		}
		
		public Table<MobTypes> MobTypes
		{
			get
			{
				return this.GetTable<MobTypes>();
			}
		}
		
		public Table<Portals> Portals
		{
			get
			{
				return this.GetTable<Portals>();
			}
		}
		
		public Table<Rooms> Rooms
		{
			get
			{
				return this.GetTable<Rooms>();
			}
		}
		
		public Table<RoomTemplates> RoomTemplates
		{
			get
			{
				return this.GetTable<RoomTemplates>();
			}
		}
	}
	
	#region Start MONO_STRICT
#if MONO_STRICT

	public partial class AsyncRPGDataContext
	{
		
		public AsyncRPGDataContext(IDbConnection connection) : 
				base(connection)
		{
			this.OnCreated();
		}
	}
	#region End MONO_STRICT
	#endregion
#else     // MONO_STRICT
	
	public partial class AsyncRPGDataContext
	{
		
		public AsyncRPGDataContext(IDbConnection connection) : 
				base(connection, new DbLinq.Sqlite.SqliteVendor())
		{
			this.OnCreated();
		}
		
		public AsyncRPGDataContext(IDbConnection connection, IVendor sqlDialect) : 
				base(connection, sqlDialect)
		{
			this.OnCreated();
		}
		
		public AsyncRPGDataContext(IDbConnection connection, MappingSource mappingSource, IVendor sqlDialect) : 
				base(connection, mappingSource, sqlDialect)
		{
			this.OnCreated();
		}
	}
	#region End Not MONO_STRICT
	#endregion
#endif     // MONO_STRICT
	#endregion
	
	[Table(Name="accounts")]
	public partial class Accounts : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _accountID;
		
		private System.DateTime _creationDate;
		
		private string _emailAddress;
		
		private string _emailVerificationKey;
		
		private int _opSlEvel;
		
		private string _passwordHash;
		
		private string _salt;
		
		private string _userName;
		
		private EntitySet<Characters> _characters;
		
		private EntitySet<Games> _games;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAccountIDChanged();
		
		partial void OnAccountIDChanging(int value);
		
		partial void OnCreationDateChanged();
		
		partial void OnCreationDateChanging(System.DateTime value);
		
		partial void OnEmailAddressChanged();
		
		partial void OnEmailAddressChanging(string value);
		
		partial void OnEmailVerificationKeyChanged();
		
		partial void OnEmailVerificationKeyChanging(string value);
		
		partial void OnOpsLevelChanged();
		
		partial void OnOpsLevelChanging(int value);
		
		partial void OnPasswordHashChanged();
		
		partial void OnPasswordHashChanging(string value);
		
		partial void OnSaltChanged();
		
		partial void OnSaltChanging(string value);
		
		partial void OnUserNameChanged();
		
		partial void OnUserNameChanging(string value);
		#endregion
		
		
		public Accounts()
		{
			_characters = new EntitySet<Characters>(new Action<Characters>(this.Characters_Attach), new Action<Characters>(this.Characters_Detach));
			_games = new EntitySet<Games>(new Action<Games>(this.Games_Attach), new Action<Games>(this.Games_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_accountID", Name="account_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int AccountID
		{
			get
			{
				return this._accountID;
			}
			set
			{
				if ((_accountID != value))
				{
					this.OnAccountIDChanging(value);
					this.SendPropertyChanging();
					this._accountID = value;
					this.SendPropertyChanged("AccountID");
					this.OnAccountIDChanged();
				}
			}
		}
		
		[Column(Storage="_creationDate", Name="creation_date", DbType="datetime", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public System.DateTime CreationDate
		{
			get
			{
				return this._creationDate;
			}
			set
			{
				if ((_creationDate != value))
				{
					this.OnCreationDateChanging(value);
					this.SendPropertyChanging();
					this._creationDate = value;
					this.SendPropertyChanged("CreationDate");
					this.OnCreationDateChanged();
				}
			}
		}
		
		[Column(Storage="_emailAddress", Name="email_address", DbType="varchar(254)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EmailAddress
		{
			get
			{
				return this._emailAddress;
			}
			set
			{
				if (((_emailAddress == value) 
							== false))
				{
					this.OnEmailAddressChanging(value);
					this.SendPropertyChanging();
					this._emailAddress = value;
					this.SendPropertyChanged("EmailAddress");
					this.OnEmailAddressChanged();
				}
			}
		}
		
		[Column(Storage="_emailVerificationKey", Name="email_verification_key", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EmailVerificationKey
		{
			get
			{
				return this._emailVerificationKey;
			}
			set
			{
				if (((_emailVerificationKey == value) 
							== false))
				{
					this.OnEmailVerificationKeyChanging(value);
					this.SendPropertyChanging();
					this._emailVerificationKey = value;
					this.SendPropertyChanged("EmailVerificationKey");
					this.OnEmailVerificationKeyChanged();
				}
			}
		}
		
		[Column(Storage="_opSlEvel", Name="ops_level", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int OpsLevel
		{
			get
			{
				return this._opSlEvel;
			}
			set
			{
				if ((_opSlEvel != value))
				{
					this.OnOpsLevelChanging(value);
					this.SendPropertyChanging();
					this._opSlEvel = value;
					this.SendPropertyChanged("OpsLevel");
					this.OnOpsLevelChanged();
				}
			}
		}
		
		[Column(Storage="_passwordHash", Name="password_hash", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string PasswordHash
		{
			get
			{
				return this._passwordHash;
			}
			set
			{
				if (((_passwordHash == value) 
							== false))
				{
					this.OnPasswordHashChanging(value);
					this.SendPropertyChanging();
					this._passwordHash = value;
					this.SendPropertyChanged("PasswordHash");
					this.OnPasswordHashChanged();
				}
			}
		}
		
		[Column(Storage="_salt", Name="salt", DbType="char(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Salt
		{
			get
			{
				return this._salt;
			}
			set
			{
				if (((_salt == value) 
							== false))
				{
					this.OnSaltChanging(value);
					this.SendPropertyChanging();
					this._salt = value;
					this.SendPropertyChanged("Salt");
					this.OnSaltChanged();
				}
			}
		}
		
		[Column(Storage="_userName", Name="username", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string UserName
		{
			get
			{
				return this._userName;
			}
			set
			{
				if (((_userName == value) 
							== false))
				{
					this.OnUserNameChanging(value);
					this.SendPropertyChanging();
					this._userName = value;
					this.SendPropertyChanged("UserName");
					this.OnUserNameChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_characters", OtherKey="AccountID", ThisKey="AccountID", Name="Accounts_Characters")]
		[DebuggerNonUserCode()]
		public EntitySet<Characters> Characters
		{
			get
			{
				return this._characters;
			}
			set
			{
				this._characters = value;
			}
		}
		
		[Association(Storage="_games", OtherKey="OwnerAccountID", ThisKey="AccountID", Name="Accounts_Games")]
		[DebuggerNonUserCode()]
		public EntitySet<Games> Games
		{
			get
			{
				return this._games;
			}
			set
			{
				this._games = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void Characters_Attach(Characters entity)
		{
			this.SendPropertyChanging();
			entity.Accounts = this;
		}
		
		private void Characters_Detach(Characters entity)
		{
			this.SendPropertyChanging();
			entity.Accounts = null;
		}
		
		private void Games_Attach(Games entity)
		{
			this.SendPropertyChanging();
			entity.Accounts = this;
		}
		
		private void Games_Detach(Games entity)
		{
			this.SendPropertyChanging();
			entity.Accounts = null;
		}
		#endregion
	}
	
	[Table(Name="characters")]
	public partial class Characters : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _accountID;
		
		private float _angle;
		
		private int _archetype;
		
		private int _characterID;
		
		private int _energy;
		
		private int _gameID;
		
		private bool _gender;
		
		private int _health;
		
		private System.DateTime _lastPingTime;
		
		private int _lastSentEventID;
		
		private string _name;
		
		private bool _newEventsPosted;
		
		private int _pictureID;
		
		private int _powerLevel;
		
		private int _roomX;
		
		private int _roomY;
		
		private int _roomZ;
		
		private float _x;
		
		private float _y;
		
		private float _z;
		
		private EntityRef<Accounts> _accounts = new EntityRef<Accounts>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAccountIDChanged();
		
		partial void OnAccountIDChanging(int value);
		
		partial void OnAngleChanged();
		
		partial void OnAngleChanging(float value);
		
		partial void OnArchetypeChanged();
		
		partial void OnArchetypeChanging(int value);
		
		partial void OnCharacterIDChanged();
		
		partial void OnCharacterIDChanging(int value);
		
		partial void OnEnergyChanged();
		
		partial void OnEnergyChanging(int value);
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnGenderChanged();
		
		partial void OnGenderChanging(bool value);
		
		partial void OnHealthChanged();
		
		partial void OnHealthChanging(int value);
		
		partial void OnLastPingTimeChanged();
		
		partial void OnLastPingTimeChanging(System.DateTime value);
		
		partial void OnLastSentEventIDChanged();
		
		partial void OnLastSentEventIDChanging(int value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		
		partial void OnNewEventsPostedChanged();
		
		partial void OnNewEventsPostedChanging(bool value);
		
		partial void OnPictureIDChanged();
		
		partial void OnPictureIDChanging(int value);
		
		partial void OnPowerLevelChanged();
		
		partial void OnPowerLevelChanging(int value);
		
		partial void OnRoomXChanged();
		
		partial void OnRoomXChanging(int value);
		
		partial void OnRoomYChanged();
		
		partial void OnRoomYChanging(int value);
		
		partial void OnRoomZChanged();
		
		partial void OnRoomZChanging(int value);
		
		partial void OnXChanged();
		
		partial void OnXChanging(float value);
		
		partial void OnYChanged();
		
		partial void OnYChanging(float value);
		
		partial void OnZChanged();
		
		partial void OnZChanging(float value);
		#endregion
		
		
		public Characters()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_accountID", Name="account_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int AccountID
		{
			get
			{
				return this._accountID;
			}
			set
			{
				if ((_accountID != value))
				{
					this.OnAccountIDChanging(value);
					this.SendPropertyChanging();
					this._accountID = value;
					this.SendPropertyChanged("AccountID");
					this.OnAccountIDChanged();
				}
			}
		}
		
		[Column(Storage="_angle", Name="angle", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Angle
		{
			get
			{
				return this._angle;
			}
			set
			{
				if ((_angle != value))
				{
					this.OnAngleChanging(value);
					this.SendPropertyChanging();
					this._angle = value;
					this.SendPropertyChanged("Angle");
					this.OnAngleChanged();
				}
			}
		}
		
		[Column(Storage="_archetype", Name="archetype", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Archetype
		{
			get
			{
				return this._archetype;
			}
			set
			{
				if ((_archetype != value))
				{
					this.OnArchetypeChanging(value);
					this.SendPropertyChanging();
					this._archetype = value;
					this.SendPropertyChanged("Archetype");
					this.OnArchetypeChanged();
				}
			}
		}
		
		[Column(Storage="_characterID", Name="character_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int CharacterID
		{
			get
			{
				return this._characterID;
			}
			set
			{
				if ((_characterID != value))
				{
					this.OnCharacterIDChanging(value);
					this.SendPropertyChanging();
					this._characterID = value;
					this.SendPropertyChanged("CharacterID");
					this.OnCharacterIDChanged();
				}
			}
		}
		
		[Column(Storage="_energy", Name="energy", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Energy
		{
			get
			{
				return this._energy;
			}
			set
			{
				if ((_energy != value))
				{
					this.OnEnergyChanging(value);
					this.SendPropertyChanging();
					this._energy = value;
					this.SendPropertyChanged("Energy");
					this.OnEnergyChanged();
				}
			}
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_gender", Name="gender", DbType="bit", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool Gender
		{
			get
			{
				return this._gender;
			}
			set
			{
				if ((_gender != value))
				{
					this.OnGenderChanging(value);
					this.SendPropertyChanging();
					this._gender = value;
					this.SendPropertyChanged("Gender");
					this.OnGenderChanged();
				}
			}
		}
		
		[Column(Storage="_health", Name="health", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Health
		{
			get
			{
				return this._health;
			}
			set
			{
				if ((_health != value))
				{
					this.OnHealthChanging(value);
					this.SendPropertyChanging();
					this._health = value;
					this.SendPropertyChanged("Health");
					this.OnHealthChanged();
				}
			}
		}
		
		[Column(Storage="_lastPingTime", Name="last_ping_time", DbType="datetime", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public System.DateTime LastPingTime
		{
			get
			{
				return this._lastPingTime;
			}
			set
			{
				if ((_lastPingTime != value))
				{
					this.OnLastPingTimeChanging(value);
					this.SendPropertyChanging();
					this._lastPingTime = value;
					this.SendPropertyChanged("LastPingTime");
					this.OnLastPingTimeChanged();
				}
			}
		}
		
		[Column(Storage="_lastSentEventID", Name="last_sent_event_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int LastSentEventID
		{
			get
			{
				return this._lastSentEventID;
			}
			set
			{
				if ((_lastSentEventID != value))
				{
					this.OnLastSentEventIDChanging(value);
					this.SendPropertyChanging();
					this._lastSentEventID = value;
					this.SendPropertyChanged("LastSentEventID");
					this.OnLastSentEventIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) 
							== false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[Column(Storage="_newEventsPosted", Name="new_events_posted", DbType="bit", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool NewEventsPosted
		{
			get
			{
				return this._newEventsPosted;
			}
			set
			{
				if ((_newEventsPosted != value))
				{
					this.OnNewEventsPostedChanging(value);
					this.SendPropertyChanging();
					this._newEventsPosted = value;
					this.SendPropertyChanged("NewEventsPosted");
					this.OnNewEventsPostedChanged();
				}
			}
		}
		
		[Column(Storage="_pictureID", Name="picture_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int PictureID
		{
			get
			{
				return this._pictureID;
			}
			set
			{
				if ((_pictureID != value))
				{
					this.OnPictureIDChanging(value);
					this.SendPropertyChanging();
					this._pictureID = value;
					this.SendPropertyChanged("PictureID");
					this.OnPictureIDChanged();
				}
			}
		}
		
		[Column(Storage="_powerLevel", Name="power_level", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int PowerLevel
		{
			get
			{
				return this._powerLevel;
			}
			set
			{
				if ((_powerLevel != value))
				{
					this.OnPowerLevelChanging(value);
					this.SendPropertyChanging();
					this._powerLevel = value;
					this.SendPropertyChanged("PowerLevel");
					this.OnPowerLevelChanged();
				}
			}
		}
		
		[Column(Storage="_roomX", Name="room_x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomX
		{
			get
			{
				return this._roomX;
			}
			set
			{
				if ((_roomX != value))
				{
					this.OnRoomXChanging(value);
					this.SendPropertyChanging();
					this._roomX = value;
					this.SendPropertyChanged("RoomX");
					this.OnRoomXChanged();
				}
			}
		}
		
		[Column(Storage="_roomY", Name="room_y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomY
		{
			get
			{
				return this._roomY;
			}
			set
			{
				if ((_roomY != value))
				{
					this.OnRoomYChanging(value);
					this.SendPropertyChanging();
					this._roomY = value;
					this.SendPropertyChanged("RoomY");
					this.OnRoomYChanged();
				}
			}
		}
		
		[Column(Storage="_roomZ", Name="room_z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomZ
		{
			get
			{
				return this._roomZ;
			}
			set
			{
				if ((_roomZ != value))
				{
					this.OnRoomZChanging(value);
					this.SendPropertyChanging();
					this._roomZ = value;
					this.SendPropertyChanged("RoomZ");
					this.OnRoomZChanged();
				}
			}
		}
		
		[Column(Storage="_x", Name="x", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float X
		{
			get
			{
				return this._x;
			}
			set
			{
				if ((_x != value))
				{
					this.OnXChanging(value);
					this.SendPropertyChanging();
					this._x = value;
					this.SendPropertyChanged("X");
					this.OnXChanged();
				}
			}
		}
		
		[Column(Storage="_y", Name="y", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Y
		{
			get
			{
				return this._y;
			}
			set
			{
				if ((_y != value))
				{
					this.OnYChanging(value);
					this.SendPropertyChanging();
					this._y = value;
					this.SendPropertyChanged("Y");
					this.OnYChanged();
				}
			}
		}
		
		[Column(Storage="_z", Name="z", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Z
		{
			get
			{
				return this._z;
			}
			set
			{
				if ((_z != value))
				{
					this.OnZChanging(value);
					this.SendPropertyChanging();
					this._z = value;
					this.SendPropertyChanged("Z");
					this.OnZChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_accounts", OtherKey="AccountID", ThisKey="AccountID", Name="Accounts_Characters", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Accounts Accounts
		{
			get
			{
				return this._accounts.Entity;
			}
			set
			{
				if (((this._accounts.Entity == value) 
							== false))
				{
					if ((this._accounts.Entity != null))
					{
						Accounts previousAccounts = this._accounts.Entity;
						this._accounts.Entity = null;
						previousAccounts.Characters.Remove(this);
					}
					this._accounts.Entity = value;
					if ((value != null))
					{
						value.Characters.Add(this);
						_accountID = value.AccountID;
					}
					else
					{
						_accountID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="config")]
	public partial class Config : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _appDebugUrl;
		
		private string _appUrl;
		
		private string _clientUrl;
		
		private string _emailAddress;
		
		private string _emailHost;
		
		private string _emailPassword;
		
		private int _emailPort;
		
		private string _emailUserName;
		
		private int _id;
		
		private int _ircPort;
		
		private string _ircServer;
		
		private int _version;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAppDebugURLChanged();
		
		partial void OnAppDebugURLChanging(string value);
		
		partial void OnAppURLChanged();
		
		partial void OnAppURLChanging(string value);
		
		partial void OnClientURLChanged();
		
		partial void OnClientURLChanging(string value);
		
		partial void OnEmailAddressChanged();
		
		partial void OnEmailAddressChanging(string value);
		
		partial void OnEmailHostChanged();
		
		partial void OnEmailHostChanging(string value);
		
		partial void OnEmailPasswordChanged();
		
		partial void OnEmailPasswordChanging(string value);
		
		partial void OnEmailPortChanged();
		
		partial void OnEmailPortChanging(int value);
		
		partial void OnEmailUserNameChanged();
		
		partial void OnEmailUserNameChanging(string value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnIrcPortChanged();
		
		partial void OnIrcPortChanging(int value);
		
		partial void OnIrcServerChanged();
		
		partial void OnIrcServerChanging(string value);
		
		partial void OnVersionChanged();
		
		partial void OnVersionChanging(int value);
		#endregion
		
		
		public Config()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_appDebugUrl", Name="app_debug_url", DbType="varchar(256)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string AppDebugURL
		{
			get
			{
				return this._appDebugUrl;
			}
			set
			{
				if (((_appDebugUrl == value) 
							== false))
				{
					this.OnAppDebugURLChanging(value);
					this.SendPropertyChanging();
					this._appDebugUrl = value;
					this.SendPropertyChanged("AppDebugURL");
					this.OnAppDebugURLChanged();
				}
			}
		}
		
		[Column(Storage="_appUrl", Name="app_url", DbType="varchar(256)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string AppURL
		{
			get
			{
				return this._appUrl;
			}
			set
			{
				if (((_appUrl == value) 
							== false))
				{
					this.OnAppURLChanging(value);
					this.SendPropertyChanging();
					this._appUrl = value;
					this.SendPropertyChanged("AppURL");
					this.OnAppURLChanged();
				}
			}
		}
		
		[Column(Storage="_clientUrl", Name="client_url", DbType="varchar(256)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string ClientURL
		{
			get
			{
				return this._clientUrl;
			}
			set
			{
				if (((_clientUrl == value) 
							== false))
				{
					this.OnClientURLChanging(value);
					this.SendPropertyChanging();
					this._clientUrl = value;
					this.SendPropertyChanged("ClientURL");
					this.OnClientURLChanged();
				}
			}
		}
		
		[Column(Storage="_emailAddress", Name="email_address", DbType="varchar(256)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EmailAddress
		{
			get
			{
				return this._emailAddress;
			}
			set
			{
				if (((_emailAddress == value) 
							== false))
				{
					this.OnEmailAddressChanging(value);
					this.SendPropertyChanging();
					this._emailAddress = value;
					this.SendPropertyChanged("EmailAddress");
					this.OnEmailAddressChanged();
				}
			}
		}
		
		[Column(Storage="_emailHost", Name="email_host", DbType="varchar(256)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EmailHost
		{
			get
			{
				return this._emailHost;
			}
			set
			{
				if (((_emailHost == value) 
							== false))
				{
					this.OnEmailHostChanging(value);
					this.SendPropertyChanging();
					this._emailHost = value;
					this.SendPropertyChanged("EmailHost");
					this.OnEmailHostChanged();
				}
			}
		}
		
		[Column(Storage="_emailPassword", Name="email_password", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EmailPassword
		{
			get
			{
				return this._emailPassword;
			}
			set
			{
				if (((_emailPassword == value) 
							== false))
				{
					this.OnEmailPasswordChanging(value);
					this.SendPropertyChanging();
					this._emailPassword = value;
					this.SendPropertyChanged("EmailPassword");
					this.OnEmailPasswordChanged();
				}
			}
		}
		
		[Column(Storage="_emailPort", Name="email_port", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int EmailPort
		{
			get
			{
				return this._emailPort;
			}
			set
			{
				if ((_emailPort != value))
				{
					this.OnEmailPortChanging(value);
					this.SendPropertyChanging();
					this._emailPort = value;
					this.SendPropertyChanged("EmailPort");
					this.OnEmailPortChanged();
				}
			}
		}
		
		[Column(Storage="_emailUserName", Name="email_username", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EmailUserName
		{
			get
			{
				return this._emailUserName;
			}
			set
			{
				if (((_emailUserName == value) 
							== false))
				{
					this.OnEmailUserNameChanging(value);
					this.SendPropertyChanging();
					this._emailUserName = value;
					this.SendPropertyChanged("EmailUserName");
					this.OnEmailUserNameChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_ircPort", Name="irc_port", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int IrcPort
		{
			get
			{
				return this._ircPort;
			}
			set
			{
				if ((_ircPort != value))
				{
					this.OnIrcPortChanging(value);
					this.SendPropertyChanging();
					this._ircPort = value;
					this.SendPropertyChanged("IrcPort");
					this.OnIrcPortChanged();
				}
			}
		}
		
		[Column(Storage="_ircServer", Name="irc_server", DbType="varchar(128)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string IrcServer
		{
			get
			{
				return this._ircServer;
			}
			set
			{
				if (((_ircServer == value) 
							== false))
				{
					this.OnIrcServerChanging(value);
					this.SendPropertyChanging();
					this._ircServer = value;
					this.SendPropertyChanged("IrcServer");
					this.OnIrcServerChanged();
				}
			}
		}
		
		[Column(Storage="_version", Name="version", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Version
		{
			get
			{
				return this._version;
			}
			set
			{
				if ((_version != value))
				{
					this.OnVersionChanging(value);
					this.SendPropertyChanging();
					this._version = value;
					this.SendPropertyChanged("Version");
					this.OnVersionChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="energy_tanks")]
	public partial class EnergyTanks : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _energy;
		
		private int _energyTankID;
		
		private int _gameID;
		
		private int _ownership;
		
		private int _roomX;
		
		private int _roomY;
		
		private int _roomZ;
		
		private float _x;
		
		private float _y;
		
		private float _z;
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnEnergyChanged();
		
		partial void OnEnergyChanging(int value);
		
		partial void OnEnergyTankIDChanged();
		
		partial void OnEnergyTankIDChanging(int value);
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnOwnershipChanged();
		
		partial void OnOwnershipChanging(int value);
		
		partial void OnRoomXChanged();
		
		partial void OnRoomXChanging(int value);
		
		partial void OnRoomYChanged();
		
		partial void OnRoomYChanging(int value);
		
		partial void OnRoomZChanged();
		
		partial void OnRoomZChanging(int value);
		
		partial void OnXChanged();
		
		partial void OnXChanging(float value);
		
		partial void OnYChanged();
		
		partial void OnYChanging(float value);
		
		partial void OnZChanged();
		
		partial void OnZChanging(float value);
		#endregion
		
		
		public EnergyTanks()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_energy", Name="energy", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Energy
		{
			get
			{
				return this._energy;
			}
			set
			{
				if ((_energy != value))
				{
					this.OnEnergyChanging(value);
					this.SendPropertyChanging();
					this._energy = value;
					this.SendPropertyChanged("Energy");
					this.OnEnergyChanged();
				}
			}
		}
		
		[Column(Storage="_energyTankID", Name="energy_tank_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int EnergyTankID
		{
			get
			{
				return this._energyTankID;
			}
			set
			{
				if ((_energyTankID != value))
				{
					this.OnEnergyTankIDChanging(value);
					this.SendPropertyChanging();
					this._energyTankID = value;
					this.SendPropertyChanged("EnergyTankID");
					this.OnEnergyTankIDChanged();
				}
			}
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_ownership", Name="ownership", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Ownership
		{
			get
			{
				return this._ownership;
			}
			set
			{
				if ((_ownership != value))
				{
					this.OnOwnershipChanging(value);
					this.SendPropertyChanging();
					this._ownership = value;
					this.SendPropertyChanged("Ownership");
					this.OnOwnershipChanged();
				}
			}
		}
		
		[Column(Storage="_roomX", Name="room_x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomX
		{
			get
			{
				return this._roomX;
			}
			set
			{
				if ((_roomX != value))
				{
					this.OnRoomXChanging(value);
					this.SendPropertyChanging();
					this._roomX = value;
					this.SendPropertyChanged("RoomX");
					this.OnRoomXChanged();
				}
			}
		}
		
		[Column(Storage="_roomY", Name="room_y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomY
		{
			get
			{
				return this._roomY;
			}
			set
			{
				if ((_roomY != value))
				{
					this.OnRoomYChanging(value);
					this.SendPropertyChanging();
					this._roomY = value;
					this.SendPropertyChanged("RoomY");
					this.OnRoomYChanged();
				}
			}
		}
		
		[Column(Storage="_roomZ", Name="room_z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomZ
		{
			get
			{
				return this._roomZ;
			}
			set
			{
				if ((_roomZ != value))
				{
					this.OnRoomZChanging(value);
					this.SendPropertyChanging();
					this._roomZ = value;
					this.SendPropertyChanged("RoomZ");
					this.OnRoomZChanged();
				}
			}
		}
		
		[Column(Storage="_x", Name="x", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float X
		{
			get
			{
				return this._x;
			}
			set
			{
				if ((_x != value))
				{
					this.OnXChanging(value);
					this.SendPropertyChanging();
					this._x = value;
					this.SendPropertyChanged("X");
					this.OnXChanged();
				}
			}
		}
		
		[Column(Storage="_y", Name="y", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Y
		{
			get
			{
				return this._y;
			}
			set
			{
				if ((_y != value))
				{
					this.OnYChanging(value);
					this.SendPropertyChanging();
					this._y = value;
					this.SendPropertyChanged("Y");
					this.OnYChanged();
				}
			}
		}
		
		[Column(Storage="_z", Name="z", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Z
		{
			get
			{
				return this._z;
			}
			set
			{
				if ((_z != value))
				{
					this.OnZChanging(value);
					this.SendPropertyChanging();
					this._z = value;
					this.SendPropertyChanged("Z");
					this.OnZChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_EnergyTanks", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.EnergyTanks.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.EnergyTanks.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="game_events")]
	public partial class GameEvents : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _eventID;
		
		private System.DateTime _eventTime;
		
		private int _eventType;
		
		private int _gameID;
		
		private string _jsOnParameters;
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnEventIDChanged();
		
		partial void OnEventIDChanging(int value);
		
		partial void OnEventTimeChanged();
		
		partial void OnEventTimeChanging(System.DateTime value);
		
		partial void OnEventTypeChanged();
		
		partial void OnEventTypeChanging(int value);
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnJsonParametersChanged();
		
		partial void OnJsonParametersChanging(string value);
		#endregion
		
		
		public GameEvents()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_eventID", Name="event_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int EventID
		{
			get
			{
				return this._eventID;
			}
			set
			{
				if ((_eventID != value))
				{
					this.OnEventIDChanging(value);
					this.SendPropertyChanging();
					this._eventID = value;
					this.SendPropertyChanged("EventID");
					this.OnEventIDChanged();
				}
			}
		}
		
		[Column(Storage="_eventTime", Name="event_time", DbType="datetime", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public System.DateTime EventTime
		{
			get
			{
				return this._eventTime;
			}
			set
			{
				if ((_eventTime != value))
				{
					this.OnEventTimeChanging(value);
					this.SendPropertyChanging();
					this._eventTime = value;
					this.SendPropertyChanged("EventTime");
					this.OnEventTimeChanged();
				}
			}
		}
		
		[Column(Storage="_eventType", Name="event_type", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int EventType
		{
			get
			{
				return this._eventType;
			}
			set
			{
				if ((_eventType != value))
				{
					this.OnEventTypeChanging(value);
					this.SendPropertyChanging();
					this._eventType = value;
					this.SendPropertyChanged("EventType");
					this.OnEventTypeChanged();
				}
			}
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_jsOnParameters", Name="json_parameters", DbType="varchar(512)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string JsonParameters
		{
			get
			{
				return this._jsOnParameters;
			}
			set
			{
				if (((_jsOnParameters == value) 
							== false))
				{
					this.OnJsonParametersChanging(value);
					this.SendPropertyChanging();
					this._jsOnParameters = value;
					this.SendPropertyChanged("JsonParameters");
					this.OnJsonParametersChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_GameEvents", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.GameEvents.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.GameEvents.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="games")]
	public partial class Games : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _dungeonDifficulty;
		
		private int _dungeonSize;
		
		private int _gameID;
		
		private bool _ircEnAbed;
		
		private bool _ircEncryptionEnAbed;
		
		private string _ircEncryptionKey;
		
		private int _ircPort;
		
		private string _ircServer;
		
		private string _name;
		
		private int _ownerAccountID;
		
		private EntitySet<EnergyTanks> _energyTanks;
		
		private EntitySet<GameEvents> _gameEvents;
		
		private EntitySet<Loot> _loot;
		
		private EntitySet<Mobs> _mobs;
		
		private EntitySet<MobSpawners> _mobSpAwnErs;
		
		private EntitySet<Portals> _portals;
		
		private EntitySet<Rooms> _rooms;
		
		private EntityRef<Accounts> _accounts = new EntityRef<Accounts>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnDungeonDifficultyChanged();
		
		partial void OnDungeonDifficultyChanging(int value);
		
		partial void OnDungeonSizeChanged();
		
		partial void OnDungeonSizeChanging(int value);
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnIrcEnabedChanged();
		
		partial void OnIrcEnabedChanging(bool value);
		
		partial void OnIrcEncryptionEnabedChanged();
		
		partial void OnIrcEncryptionEnabedChanging(bool value);
		
		partial void OnIrcEncryptionKeyChanged();
		
		partial void OnIrcEncryptionKeyChanging(string value);
		
		partial void OnIrcPortChanged();
		
		partial void OnIrcPortChanging(int value);
		
		partial void OnIrcServerChanged();
		
		partial void OnIrcServerChanging(string value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		
		partial void OnOwnerAccountIDChanged();
		
		partial void OnOwnerAccountIDChanging(int value);
		#endregion
		
		
		public Games()
		{
			_energyTanks = new EntitySet<EnergyTanks>(new Action<EnergyTanks>(this.EnergyTanks_Attach), new Action<EnergyTanks>(this.EnergyTanks_Detach));
			_gameEvents = new EntitySet<GameEvents>(new Action<GameEvents>(this.GameEvents_Attach), new Action<GameEvents>(this.GameEvents_Detach));
			_loot = new EntitySet<Loot>(new Action<Loot>(this.Loot_Attach), new Action<Loot>(this.Loot_Detach));
			_mobs = new EntitySet<Mobs>(new Action<Mobs>(this.Mobs_Attach), new Action<Mobs>(this.Mobs_Detach));
			_mobSpAwnErs = new EntitySet<MobSpawners>(new Action<MobSpawners>(this.MobSpawners_Attach), new Action<MobSpawners>(this.MobSpawners_Detach));
			_portals = new EntitySet<Portals>(new Action<Portals>(this.Portals_Attach), new Action<Portals>(this.Portals_Detach));
			_rooms = new EntitySet<Rooms>(new Action<Rooms>(this.Rooms_Attach), new Action<Rooms>(this.Rooms_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_dungeonDifficulty", Name="dungeon_difficulty", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int DungeonDifficulty
		{
			get
			{
				return this._dungeonDifficulty;
			}
			set
			{
				if ((_dungeonDifficulty != value))
				{
					this.OnDungeonDifficultyChanging(value);
					this.SendPropertyChanging();
					this._dungeonDifficulty = value;
					this.SendPropertyChanged("DungeonDifficulty");
					this.OnDungeonDifficultyChanged();
				}
			}
		}
		
		[Column(Storage="_dungeonSize", Name="dungeon_size", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int DungeonSize
		{
			get
			{
				return this._dungeonSize;
			}
			set
			{
				if ((_dungeonSize != value))
				{
					this.OnDungeonSizeChanging(value);
					this.SendPropertyChanging();
					this._dungeonSize = value;
					this.SendPropertyChanged("DungeonSize");
					this.OnDungeonSizeChanged();
				}
			}
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_ircEnAbed", Name="irc_enabed", DbType="bit", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool IrcEnabed
		{
			get
			{
				return this._ircEnAbed;
			}
			set
			{
				if ((_ircEnAbed != value))
				{
					this.OnIrcEnabedChanging(value);
					this.SendPropertyChanging();
					this._ircEnAbed = value;
					this.SendPropertyChanged("IrcEnabed");
					this.OnIrcEnabedChanged();
				}
			}
		}
		
		[Column(Storage="_ircEncryptionEnAbed", Name="irc_encryption_enabed", DbType="bit", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool IrcEncryptionEnabed
		{
			get
			{
				return this._ircEncryptionEnAbed;
			}
			set
			{
				if ((_ircEncryptionEnAbed != value))
				{
					this.OnIrcEncryptionEnabedChanging(value);
					this.SendPropertyChanging();
					this._ircEncryptionEnAbed = value;
					this.SendPropertyChanged("IrcEncryptionEnabed");
					this.OnIrcEncryptionEnabedChanged();
				}
			}
		}
		
		[Column(Storage="_ircEncryptionKey", Name="irc_encryption_key", DbType="char(44)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string IrcEncryptionKey
		{
			get
			{
				return this._ircEncryptionKey;
			}
			set
			{
				if (((_ircEncryptionKey == value) 
							== false))
				{
					this.OnIrcEncryptionKeyChanging(value);
					this.SendPropertyChanging();
					this._ircEncryptionKey = value;
					this.SendPropertyChanged("IrcEncryptionKey");
					this.OnIrcEncryptionKeyChanged();
				}
			}
		}
		
		[Column(Storage="_ircPort", Name="irc_port", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int IrcPort
		{
			get
			{
				return this._ircPort;
			}
			set
			{
				if ((_ircPort != value))
				{
					this.OnIrcPortChanging(value);
					this.SendPropertyChanging();
					this._ircPort = value;
					this.SendPropertyChanged("IrcPort");
					this.OnIrcPortChanged();
				}
			}
		}
		
		[Column(Storage="_ircServer", Name="irc_server", DbType="varchar(128)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string IrcServer
		{
			get
			{
				return this._ircServer;
			}
			set
			{
				if (((_ircServer == value) 
							== false))
				{
					this.OnIrcServerChanging(value);
					this.SendPropertyChanging();
					this._ircServer = value;
					this.SendPropertyChanged("IrcServer");
					this.OnIrcServerChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="varchar(64)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) 
							== false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[Column(Storage="_ownerAccountID", Name="owner_account_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int OwnerAccountID
		{
			get
			{
				return this._ownerAccountID;
			}
			set
			{
				if ((_ownerAccountID != value))
				{
					this.OnOwnerAccountIDChanging(value);
					this.SendPropertyChanging();
					this._ownerAccountID = value;
					this.SendPropertyChanged("OwnerAccountID");
					this.OnOwnerAccountIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_energyTanks", OtherKey="GameID", ThisKey="GameID", Name="Games_EnergyTanks")]
		[DebuggerNonUserCode()]
		public EntitySet<EnergyTanks> EnergyTanks
		{
			get
			{
				return this._energyTanks;
			}
			set
			{
				this._energyTanks = value;
			}
		}
		
		[Association(Storage="_gameEvents", OtherKey="GameID", ThisKey="GameID", Name="Games_GameEvents")]
		[DebuggerNonUserCode()]
		public EntitySet<GameEvents> GameEvents
		{
			get
			{
				return this._gameEvents;
			}
			set
			{
				this._gameEvents = value;
			}
		}
		
		[Association(Storage="_loot", OtherKey="GameID", ThisKey="GameID", Name="Games_Loot")]
		[DebuggerNonUserCode()]
		public EntitySet<Loot> Loot
		{
			get
			{
				return this._loot;
			}
			set
			{
				this._loot = value;
			}
		}
		
		[Association(Storage="_mobs", OtherKey="GameID", ThisKey="GameID", Name="Games_Mobs")]
		[DebuggerNonUserCode()]
		public EntitySet<Mobs> Mobs
		{
			get
			{
				return this._mobs;
			}
			set
			{
				this._mobs = value;
			}
		}
		
		[Association(Storage="_mobSpAwnErs", OtherKey="GameID", ThisKey="GameID", Name="Games_MobSpAwnERS")]
		[DebuggerNonUserCode()]
		public EntitySet<MobSpawners> MobSpawners
		{
			get
			{
				return this._mobSpAwnErs;
			}
			set
			{
				this._mobSpAwnErs = value;
			}
		}
		
		[Association(Storage="_portals", OtherKey="GameID", ThisKey="GameID", Name="Games_Portals")]
		[DebuggerNonUserCode()]
		public EntitySet<Portals> Portals
		{
			get
			{
				return this._portals;
			}
			set
			{
				this._portals = value;
			}
		}
		
		[Association(Storage="_rooms", OtherKey="GameID", ThisKey="GameID", Name="Games_Rooms")]
		[DebuggerNonUserCode()]
		public EntitySet<Rooms> Rooms
		{
			get
			{
				return this._rooms;
			}
			set
			{
				this._rooms = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_accounts", OtherKey="AccountID", ThisKey="OwnerAccountID", Name="Accounts_Games", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Accounts Accounts
		{
			get
			{
				return this._accounts.Entity;
			}
			set
			{
				if (((this._accounts.Entity == value) 
							== false))
				{
					if ((this._accounts.Entity != null))
					{
						Accounts previousAccounts = this._accounts.Entity;
						this._accounts.Entity = null;
						previousAccounts.Games.Remove(this);
					}
					this._accounts.Entity = value;
					if ((value != null))
					{
						value.Games.Add(this);
						_ownerAccountID = value.AccountID;
					}
					else
					{
						_ownerAccountID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void EnergyTanks_Attach(EnergyTanks entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void EnergyTanks_Detach(EnergyTanks entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		
		private void GameEvents_Attach(GameEvents entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void GameEvents_Detach(GameEvents entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		
		private void Loot_Attach(Loot entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void Loot_Detach(Loot entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		
		private void Mobs_Attach(Mobs entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void Mobs_Detach(Mobs entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		
		private void MobSpawners_Attach(MobSpawners entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void MobSpawners_Detach(MobSpawners entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		
		private void Portals_Attach(Portals entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void Portals_Detach(Portals entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		
		private void Rooms_Attach(Rooms entity)
		{
			this.SendPropertyChanging();
			entity.Games = this;
		}
		
		private void Rooms_Detach(Rooms entity)
		{
			this.SendPropertyChanging();
			entity.Games = null;
		}
		#endregion
	}
	
	[Table(Name="loot")]
	public partial class Loot : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _gameID;
		
		private int _lootCount;
		
		private int _lootID;
		
		private string _lootType;
		
		private int _roomX;
		
		private int _roomY;
		
		private int _roomZ;
		
		private float _x;
		
		private float _y;
		
		private float _z;
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnLootCountChanged();
		
		partial void OnLootCountChanging(int value);
		
		partial void OnLootIDChanged();
		
		partial void OnLootIDChanging(int value);
		
		partial void OnLootTypeChanged();
		
		partial void OnLootTypeChanging(string value);
		
		partial void OnRoomXChanged();
		
		partial void OnRoomXChanging(int value);
		
		partial void OnRoomYChanged();
		
		partial void OnRoomYChanging(int value);
		
		partial void OnRoomZChanged();
		
		partial void OnRoomZChanging(int value);
		
		partial void OnXChanged();
		
		partial void OnXChanging(float value);
		
		partial void OnYChanged();
		
		partial void OnYChanging(float value);
		
		partial void OnZChanged();
		
		partial void OnZChanging(float value);
		#endregion
		
		
		public Loot()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_lootCount", Name="loot_count", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int LootCount
		{
			get
			{
				return this._lootCount;
			}
			set
			{
				if ((_lootCount != value))
				{
					this.OnLootCountChanging(value);
					this.SendPropertyChanging();
					this._lootCount = value;
					this.SendPropertyChanged("LootCount");
					this.OnLootCountChanged();
				}
			}
		}
		
		[Column(Storage="_lootID", Name="loot_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int LootID
		{
			get
			{
				return this._lootID;
			}
			set
			{
				if ((_lootID != value))
				{
					this.OnLootIDChanging(value);
					this.SendPropertyChanging();
					this._lootID = value;
					this.SendPropertyChanged("LootID");
					this.OnLootIDChanged();
				}
			}
		}
		
		[Column(Storage="_lootType", Name="loot_type", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string LootType
		{
			get
			{
				return this._lootType;
			}
			set
			{
				if (((_lootType == value) 
							== false))
				{
					this.OnLootTypeChanging(value);
					this.SendPropertyChanging();
					this._lootType = value;
					this.SendPropertyChanged("LootType");
					this.OnLootTypeChanged();
				}
			}
		}
		
		[Column(Storage="_roomX", Name="room_x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomX
		{
			get
			{
				return this._roomX;
			}
			set
			{
				if ((_roomX != value))
				{
					this.OnRoomXChanging(value);
					this.SendPropertyChanging();
					this._roomX = value;
					this.SendPropertyChanged("RoomX");
					this.OnRoomXChanged();
				}
			}
		}
		
		[Column(Storage="_roomY", Name="room_y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomY
		{
			get
			{
				return this._roomY;
			}
			set
			{
				if ((_roomY != value))
				{
					this.OnRoomYChanging(value);
					this.SendPropertyChanging();
					this._roomY = value;
					this.SendPropertyChanged("RoomY");
					this.OnRoomYChanged();
				}
			}
		}
		
		[Column(Storage="_roomZ", Name="room_z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomZ
		{
			get
			{
				return this._roomZ;
			}
			set
			{
				if ((_roomZ != value))
				{
					this.OnRoomZChanging(value);
					this.SendPropertyChanging();
					this._roomZ = value;
					this.SendPropertyChanged("RoomZ");
					this.OnRoomZChanged();
				}
			}
		}
		
		[Column(Storage="_x", Name="x", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float X
		{
			get
			{
				return this._x;
			}
			set
			{
				if ((_x != value))
				{
					this.OnXChanging(value);
					this.SendPropertyChanging();
					this._x = value;
					this.SendPropertyChanged("X");
					this.OnXChanged();
				}
			}
		}
		
		[Column(Storage="_y", Name="y", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Y
		{
			get
			{
				return this._y;
			}
			set
			{
				if ((_y != value))
				{
					this.OnYChanging(value);
					this.SendPropertyChanging();
					this._y = value;
					this.SendPropertyChanged("Y");
					this.OnYChanged();
				}
			}
		}
		
		[Column(Storage="_z", Name="z", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Z
		{
			get
			{
				return this._z;
			}
			set
			{
				if ((_z != value))
				{
					this.OnZChanging(value);
					this.SendPropertyChanging();
					this._z = value;
					this.SendPropertyChanged("Z");
					this.OnZChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_Loot", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.Loot.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.Loot.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="mobs")]
	public partial class Mobs : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _aiData;
		
		private float _angle;
		
		private int _energy;
		
		private int _gameID;
		
		private int _health;
		
		private int _mobID;
		
		private int _mobTypeID;
		
		private int _roomX;
		
		private int _roomY;
		
		private int _roomZ;
		
		private float _x;
		
		private float _y;
		
		private float _z;
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		private EntityRef<MobTypes> _mobTypes = new EntityRef<MobTypes>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnAiDataChanged();
		
		partial void OnAiDataChanging(string value);
		
		partial void OnAngleChanged();
		
		partial void OnAngleChanging(float value);
		
		partial void OnEnergyChanged();
		
		partial void OnEnergyChanging(int value);
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnHealthChanged();
		
		partial void OnHealthChanging(int value);
		
		partial void OnMobIDChanged();
		
		partial void OnMobIDChanging(int value);
		
		partial void OnMobTypeIDChanged();
		
		partial void OnMobTypeIDChanging(int value);
		
		partial void OnRoomXChanged();
		
		partial void OnRoomXChanging(int value);
		
		partial void OnRoomYChanged();
		
		partial void OnRoomYChanging(int value);
		
		partial void OnRoomZChanged();
		
		partial void OnRoomZChanging(int value);
		
		partial void OnXChanged();
		
		partial void OnXChanging(float value);
		
		partial void OnYChanged();
		
		partial void OnYChanging(float value);
		
		partial void OnZChanged();
		
		partial void OnZChanging(float value);
		#endregion
		
		
		public Mobs()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_aiData", Name="ai_data", DbType="text", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string AiData
		{
			get
			{
				return this._aiData;
			}
			set
			{
				if (((_aiData == value) 
							== false))
				{
					this.OnAiDataChanging(value);
					this.SendPropertyChanging();
					this._aiData = value;
					this.SendPropertyChanged("AiData");
					this.OnAiDataChanged();
				}
			}
		}
		
		[Column(Storage="_angle", Name="angle", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Angle
		{
			get
			{
				return this._angle;
			}
			set
			{
				if ((_angle != value))
				{
					this.OnAngleChanging(value);
					this.SendPropertyChanging();
					this._angle = value;
					this.SendPropertyChanged("Angle");
					this.OnAngleChanged();
				}
			}
		}
		
		[Column(Storage="_energy", Name="energy", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Energy
		{
			get
			{
				return this._energy;
			}
			set
			{
				if ((_energy != value))
				{
					this.OnEnergyChanging(value);
					this.SendPropertyChanging();
					this._energy = value;
					this.SendPropertyChanged("Energy");
					this.OnEnergyChanged();
				}
			}
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_health", Name="health", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Health
		{
			get
			{
				return this._health;
			}
			set
			{
				if ((_health != value))
				{
					this.OnHealthChanging(value);
					this.SendPropertyChanging();
					this._health = value;
					this.SendPropertyChanged("Health");
					this.OnHealthChanged();
				}
			}
		}
		
		[Column(Storage="_mobID", Name="mob_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobID
		{
			get
			{
				return this._mobID;
			}
			set
			{
				if ((_mobID != value))
				{
					this.OnMobIDChanging(value);
					this.SendPropertyChanging();
					this._mobID = value;
					this.SendPropertyChanged("MobID");
					this.OnMobIDChanged();
				}
			}
		}
		
		[Column(Storage="_mobTypeID", Name="mob_type_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobTypeID
		{
			get
			{
				return this._mobTypeID;
			}
			set
			{
				if ((_mobTypeID != value))
				{
					this.OnMobTypeIDChanging(value);
					this.SendPropertyChanging();
					this._mobTypeID = value;
					this.SendPropertyChanged("MobTypeID");
					this.OnMobTypeIDChanged();
				}
			}
		}
		
		[Column(Storage="_roomX", Name="room_x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomX
		{
			get
			{
				return this._roomX;
			}
			set
			{
				if ((_roomX != value))
				{
					this.OnRoomXChanging(value);
					this.SendPropertyChanging();
					this._roomX = value;
					this.SendPropertyChanged("RoomX");
					this.OnRoomXChanged();
				}
			}
		}
		
		[Column(Storage="_roomY", Name="room_y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomY
		{
			get
			{
				return this._roomY;
			}
			set
			{
				if ((_roomY != value))
				{
					this.OnRoomYChanging(value);
					this.SendPropertyChanging();
					this._roomY = value;
					this.SendPropertyChanged("RoomY");
					this.OnRoomYChanged();
				}
			}
		}
		
		[Column(Storage="_roomZ", Name="room_z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomZ
		{
			get
			{
				return this._roomZ;
			}
			set
			{
				if ((_roomZ != value))
				{
					this.OnRoomZChanging(value);
					this.SendPropertyChanging();
					this._roomZ = value;
					this.SendPropertyChanged("RoomZ");
					this.OnRoomZChanged();
				}
			}
		}
		
		[Column(Storage="_x", Name="x", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float X
		{
			get
			{
				return this._x;
			}
			set
			{
				if ((_x != value))
				{
					this.OnXChanging(value);
					this.SendPropertyChanging();
					this._x = value;
					this.SendPropertyChanged("X");
					this.OnXChanged();
				}
			}
		}
		
		[Column(Storage="_y", Name="y", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Y
		{
			get
			{
				return this._y;
			}
			set
			{
				if ((_y != value))
				{
					this.OnYChanging(value);
					this.SendPropertyChanging();
					this._y = value;
					this.SendPropertyChanged("Y");
					this.OnYChanged();
				}
			}
		}
		
		[Column(Storage="_z", Name="z", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Z
		{
			get
			{
				return this._z;
			}
			set
			{
				if ((_z != value))
				{
					this.OnZChanging(value);
					this.SendPropertyChanging();
					this._z = value;
					this.SendPropertyChanged("Z");
					this.OnZChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_Mobs", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.Mobs.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.Mobs.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		
		[Association(Storage="_mobTypes", OtherKey="MobTypeID", ThisKey="MobTypeID", Name="MobTypes_Mobs", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public MobTypes MobTypes
		{
			get
			{
				return this._mobTypes.Entity;
			}
			set
			{
				if (((this._mobTypes.Entity == value) 
							== false))
				{
					if ((this._mobTypes.Entity != null))
					{
						MobTypes previousMobTypes = this._mobTypes.Entity;
						this._mobTypes.Entity = null;
						previousMobTypes.Mobs.Remove(this);
					}
					this._mobTypes.Entity = value;
					if ((value != null))
					{
						value.Mobs.Add(this);
						_mobTypeID = value.MobTypeID;
					}
					else
					{
						_mobTypeID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="mob_spawners")]
	public partial class MobSpawners : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _gameID;
		
		private int _mobSpawnerID;
		
		private int _mobSpawnerTableID;
		
		private int _randomSeed;
		
		private int _remainingSpawnCount;
		
		private int _roomX;
		
		private int _roomY;
		
		private int _roomZ;
		
		private float _x;
		
		private float _y;
		
		private float _z;
		
		private EntityRef<MobSpawnTables> _mobSpawnTables = new EntityRef<MobSpawnTables>();
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnMobSpawnerIDChanged();
		
		partial void OnMobSpawnerIDChanging(int value);
		
		partial void OnMobSpawnerTableIDChanged();
		
		partial void OnMobSpawnerTableIDChanging(int value);
		
		partial void OnRandomSeedChanged();
		
		partial void OnRandomSeedChanging(int value);
		
		partial void OnRemainingSpawnCountChanged();
		
		partial void OnRemainingSpawnCountChanging(int value);
		
		partial void OnRoomXChanged();
		
		partial void OnRoomXChanging(int value);
		
		partial void OnRoomYChanged();
		
		partial void OnRoomYChanging(int value);
		
		partial void OnRoomZChanged();
		
		partial void OnRoomZChanging(int value);
		
		partial void OnXChanged();
		
		partial void OnXChanging(float value);
		
		partial void OnYChanged();
		
		partial void OnYChanging(float value);
		
		partial void OnZChanged();
		
		partial void OnZChanging(float value);
		#endregion
		
		
		public MobSpawners()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_mobSpawnerID", Name="mob_spawner_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobSpawnerID
		{
			get
			{
				return this._mobSpawnerID;
			}
			set
			{
				if ((_mobSpawnerID != value))
				{
					this.OnMobSpawnerIDChanging(value);
					this.SendPropertyChanging();
					this._mobSpawnerID = value;
					this.SendPropertyChanged("MobSpawnerID");
					this.OnMobSpawnerIDChanged();
				}
			}
		}
		
		[Column(Storage="_mobSpawnerTableID", Name="mob_spawner_table_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobSpawnerTableID
		{
			get
			{
				return this._mobSpawnerTableID;
			}
			set
			{
				if ((_mobSpawnerTableID != value))
				{
					this.OnMobSpawnerTableIDChanging(value);
					this.SendPropertyChanging();
					this._mobSpawnerTableID = value;
					this.SendPropertyChanged("MobSpawnerTableID");
					this.OnMobSpawnerTableIDChanged();
				}
			}
		}
		
		[Column(Storage="_randomSeed", Name="random_seed", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RandomSeed
		{
			get
			{
				return this._randomSeed;
			}
			set
			{
				if ((_randomSeed != value))
				{
					this.OnRandomSeedChanging(value);
					this.SendPropertyChanging();
					this._randomSeed = value;
					this.SendPropertyChanged("RandomSeed");
					this.OnRandomSeedChanged();
				}
			}
		}
		
		[Column(Storage="_remainingSpawnCount", Name="remaining_spawn_count", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RemainingSpawnCount
		{
			get
			{
				return this._remainingSpawnCount;
			}
			set
			{
				if ((_remainingSpawnCount != value))
				{
					this.OnRemainingSpawnCountChanging(value);
					this.SendPropertyChanging();
					this._remainingSpawnCount = value;
					this.SendPropertyChanged("RemainingSpawnCount");
					this.OnRemainingSpawnCountChanged();
				}
			}
		}
		
		[Column(Storage="_roomX", Name="room_x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomX
		{
			get
			{
				return this._roomX;
			}
			set
			{
				if ((_roomX != value))
				{
					this.OnRoomXChanging(value);
					this.SendPropertyChanging();
					this._roomX = value;
					this.SendPropertyChanged("RoomX");
					this.OnRoomXChanged();
				}
			}
		}
		
		[Column(Storage="_roomY", Name="room_y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomY
		{
			get
			{
				return this._roomY;
			}
			set
			{
				if ((_roomY != value))
				{
					this.OnRoomYChanging(value);
					this.SendPropertyChanging();
					this._roomY = value;
					this.SendPropertyChanged("RoomY");
					this.OnRoomYChanged();
				}
			}
		}
		
		[Column(Storage="_roomZ", Name="room_z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomZ
		{
			get
			{
				return this._roomZ;
			}
			set
			{
				if ((_roomZ != value))
				{
					this.OnRoomZChanging(value);
					this.SendPropertyChanging();
					this._roomZ = value;
					this.SendPropertyChanged("RoomZ");
					this.OnRoomZChanged();
				}
			}
		}
		
		[Column(Storage="_x", Name="x", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float X
		{
			get
			{
				return this._x;
			}
			set
			{
				if ((_x != value))
				{
					this.OnXChanging(value);
					this.SendPropertyChanging();
					this._x = value;
					this.SendPropertyChanged("X");
					this.OnXChanged();
				}
			}
		}
		
		[Column(Storage="_y", Name="y", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Y
		{
			get
			{
				return this._y;
			}
			set
			{
				if ((_y != value))
				{
					this.OnYChanging(value);
					this.SendPropertyChanging();
					this._y = value;
					this.SendPropertyChanged("Y");
					this.OnYChanged();
				}
			}
		}
		
		[Column(Storage="_z", Name="z", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float Z
		{
			get
			{
				return this._z;
			}
			set
			{
				if ((_z != value))
				{
					this.OnZChanging(value);
					this.SendPropertyChanging();
					this._z = value;
					this.SendPropertyChanged("Z");
					this.OnZChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_mobSpawnTables", OtherKey="MobSpawnTableID", ThisKey="MobSpawnerTableID", Name="MobSpawnTables_MobSpAwnERS", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public MobSpawnTables MobSpawnTables
		{
			get
			{
				return this._mobSpawnTables.Entity;
			}
			set
			{
				if (((this._mobSpawnTables.Entity == value) 
							== false))
				{
					if ((this._mobSpawnTables.Entity != null))
					{
						MobSpawnTables previousMobSpawnTables = this._mobSpawnTables.Entity;
						this._mobSpawnTables.Entity = null;
						previousMobSpawnTables.MobSpawners.Remove(this);
					}
					this._mobSpawnTables.Entity = value;
					if ((value != null))
					{
						value.MobSpawners.Add(this);
						_mobSpawnerTableID = value.MobSpawnTableID;
					}
					else
					{
						_mobSpawnerTableID = default(int);
					}
				}
			}
		}
		
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_MobSpAwnERS", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.MobSpawners.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.MobSpawners.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="mob_spawn_table_entries")]
	public partial class MobSpawnTableEntries : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _mobSpawnTableEntryID;
		
		private int _mobSpawnTableID;
		
		private int _referenceID;
		
		private bool _referenceIsMobType;
		
		private int _weight;
		
		private EntityRef<MobSpawnTables> _mobSpawnTables = new EntityRef<MobSpawnTables>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnMobSpawnTableEntryIDChanged();
		
		partial void OnMobSpawnTableEntryIDChanging(int value);
		
		partial void OnMobSpawnTableIDChanged();
		
		partial void OnMobSpawnTableIDChanging(int value);
		
		partial void OnReferenceIDChanged();
		
		partial void OnReferenceIDChanging(int value);
		
		partial void OnReferenceIsMobTypeChanged();
		
		partial void OnReferenceIsMobTypeChanging(bool value);
		
		partial void OnWeightChanged();
		
		partial void OnWeightChanging(int value);
		#endregion
		
		
		public MobSpawnTableEntries()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_mobSpawnTableEntryID", Name="mob_spawn_table_entry_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobSpawnTableEntryID
		{
			get
			{
				return this._mobSpawnTableEntryID;
			}
			set
			{
				if ((_mobSpawnTableEntryID != value))
				{
					this.OnMobSpawnTableEntryIDChanging(value);
					this.SendPropertyChanging();
					this._mobSpawnTableEntryID = value;
					this.SendPropertyChanged("MobSpawnTableEntryID");
					this.OnMobSpawnTableEntryIDChanged();
				}
			}
		}
		
		[Column(Storage="_mobSpawnTableID", Name="mob_spawn_table_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobSpawnTableID
		{
			get
			{
				return this._mobSpawnTableID;
			}
			set
			{
				if ((_mobSpawnTableID != value))
				{
					this.OnMobSpawnTableIDChanging(value);
					this.SendPropertyChanging();
					this._mobSpawnTableID = value;
					this.SendPropertyChanged("MobSpawnTableID");
					this.OnMobSpawnTableIDChanged();
				}
			}
		}
		
		[Column(Storage="_referenceID", Name="reference_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ReferenceID
		{
			get
			{
				return this._referenceID;
			}
			set
			{
				if ((_referenceID != value))
				{
					this.OnReferenceIDChanging(value);
					this.SendPropertyChanging();
					this._referenceID = value;
					this.SendPropertyChanged("ReferenceID");
					this.OnReferenceIDChanged();
				}
			}
		}
		
		[Column(Storage="_referenceIsMobType", Name="reference_is_mob_type", DbType="bit", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public bool ReferenceIsMobType
		{
			get
			{
				return this._referenceIsMobType;
			}
			set
			{
				if ((_referenceIsMobType != value))
				{
					this.OnReferenceIsMobTypeChanging(value);
					this.SendPropertyChanging();
					this._referenceIsMobType = value;
					this.SendPropertyChanged("ReferenceIsMobType");
					this.OnReferenceIsMobTypeChanged();
				}
			}
		}
		
		[Column(Storage="_weight", Name="weight", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Weight
		{
			get
			{
				return this._weight;
			}
			set
			{
				if ((_weight != value))
				{
					this.OnWeightChanging(value);
					this.SendPropertyChanging();
					this._weight = value;
					this.SendPropertyChanged("Weight");
					this.OnWeightChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_mobSpawnTables", OtherKey="MobSpawnTableID", ThisKey="MobSpawnTableID", Name="MobSpawnTables_MobSpawnTableEntries", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public MobSpawnTables MobSpawnTables
		{
			get
			{
				return this._mobSpawnTables.Entity;
			}
			set
			{
				if (((this._mobSpawnTables.Entity == value) 
							== false))
				{
					if ((this._mobSpawnTables.Entity != null))
					{
						MobSpawnTables previousMobSpawnTables = this._mobSpawnTables.Entity;
						this._mobSpawnTables.Entity = null;
						previousMobSpawnTables.MobSpawnTableEntries.Remove(this);
					}
					this._mobSpawnTables.Entity = value;
					if ((value != null))
					{
						value.MobSpawnTableEntries.Add(this);
						_mobSpawnTableID = value.MobSpawnTableID;
					}
					else
					{
						_mobSpawnTableID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="mob_spawn_tables")]
	public partial class MobSpawnTables : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _mobSpawnTableID;
		
		private string _mobSpawnTableName;
		
		private EntitySet<MobSpawners> _mobSpAwnErs;
		
		private EntitySet<MobSpawnTableEntries> _mobSpawnTableEntries;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnMobSpawnTableIDChanged();
		
		partial void OnMobSpawnTableIDChanging(int value);
		
		partial void OnMobSpawnTableNameChanged();
		
		partial void OnMobSpawnTableNameChanging(string value);
		#endregion
		
		
		public MobSpawnTables()
		{
			_mobSpAwnErs = new EntitySet<MobSpawners>(new Action<MobSpawners>(this.MobSpawners_Attach), new Action<MobSpawners>(this.MobSpawners_Detach));
			_mobSpawnTableEntries = new EntitySet<MobSpawnTableEntries>(new Action<MobSpawnTableEntries>(this.MobSpawnTableEntries_Attach), new Action<MobSpawnTableEntries>(this.MobSpawnTableEntries_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_mobSpawnTableID", Name="mob_spawn_table_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobSpawnTableID
		{
			get
			{
				return this._mobSpawnTableID;
			}
			set
			{
				if ((_mobSpawnTableID != value))
				{
					this.OnMobSpawnTableIDChanging(value);
					this.SendPropertyChanging();
					this._mobSpawnTableID = value;
					this.SendPropertyChanged("MobSpawnTableID");
					this.OnMobSpawnTableIDChanged();
				}
			}
		}
		
		[Column(Storage="_mobSpawnTableName", Name="mob_spawn_table_name", DbType="varchar(32)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string MobSpawnTableName
		{
			get
			{
				return this._mobSpawnTableName;
			}
			set
			{
				if (((_mobSpawnTableName == value) 
							== false))
				{
					this.OnMobSpawnTableNameChanging(value);
					this.SendPropertyChanging();
					this._mobSpawnTableName = value;
					this.SendPropertyChanged("MobSpawnTableName");
					this.OnMobSpawnTableNameChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_mobSpAwnErs", OtherKey="MobSpawnerTableID", ThisKey="MobSpawnTableID", Name="MobSpawnTables_MobSpAwnERS")]
		[DebuggerNonUserCode()]
		public EntitySet<MobSpawners> MobSpawners
		{
			get
			{
				return this._mobSpAwnErs;
			}
			set
			{
				this._mobSpAwnErs = value;
			}
		}
		
		[Association(Storage="_mobSpawnTableEntries", OtherKey="MobSpawnTableID", ThisKey="MobSpawnTableID", Name="MobSpawnTables_MobSpawnTableEntries")]
		[DebuggerNonUserCode()]
		public EntitySet<MobSpawnTableEntries> MobSpawnTableEntries
		{
			get
			{
				return this._mobSpawnTableEntries;
			}
			set
			{
				this._mobSpawnTableEntries = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void MobSpawners_Attach(MobSpawners entity)
		{
			this.SendPropertyChanging();
			entity.MobSpawnTables = this;
		}
		
		private void MobSpawners_Detach(MobSpawners entity)
		{
			this.SendPropertyChanging();
			entity.MobSpawnTables = null;
		}
		
		private void MobSpawnTableEntries_Attach(MobSpawnTableEntries entity)
		{
			this.SendPropertyChanging();
			entity.MobSpawnTables = this;
		}
		
		private void MobSpawnTableEntries_Detach(MobSpawnTableEntries entity)
		{
			this.SendPropertyChanging();
			entity.MobSpawnTables = null;
		}
		#endregion
	}
	
	[Table(Name="mob_types")]
	public partial class MobTypes : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _mobTypeData;
		
		private int _mobTypeID;
		
		private EntitySet<Mobs> _mobs;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnMobTypeDataChanged();
		
		partial void OnMobTypeDataChanging(string value);
		
		partial void OnMobTypeIDChanged();
		
		partial void OnMobTypeIDChanging(int value);
		#endregion
		
		
		public MobTypes()
		{
			_mobs = new EntitySet<Mobs>(new Action<Mobs>(this.Mobs_Attach), new Action<Mobs>(this.Mobs_Detach));
			this.OnCreated();
		}
		
		[Column(Storage="_mobTypeData", Name="mob_type_data", DbType="text", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string MobTypeData
		{
			get
			{
				return this._mobTypeData;
			}
			set
			{
				if (((_mobTypeData == value) 
							== false))
				{
					this.OnMobTypeDataChanging(value);
					this.SendPropertyChanging();
					this._mobTypeData = value;
					this.SendPropertyChanged("MobTypeData");
					this.OnMobTypeDataChanged();
				}
			}
		}
		
		[Column(Storage="_mobTypeID", Name="mob_type_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int MobTypeID
		{
			get
			{
				return this._mobTypeID;
			}
			set
			{
				if ((_mobTypeID != value))
				{
					this.OnMobTypeIDChanging(value);
					this.SendPropertyChanging();
					this._mobTypeID = value;
					this.SendPropertyChanged("MobTypeID");
					this.OnMobTypeIDChanged();
				}
			}
		}
		
		#region Children
		[Association(Storage="_mobs", OtherKey="MobTypeID", ThisKey="MobTypeID", Name="MobTypes_Mobs")]
		[DebuggerNonUserCode()]
		public EntitySet<Mobs> Mobs
		{
			get
			{
				return this._mobs;
			}
			set
			{
				this._mobs = value;
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region Attachment handlers
		private void Mobs_Attach(Mobs entity)
		{
			this.SendPropertyChanging();
			entity.MobTypes = this;
		}
		
		private void Mobs_Detach(Mobs entity)
		{
			this.SendPropertyChanging();
			entity.MobTypes = null;
		}
		#endregion
	}
	
	[Table(Name="portals")]
	public partial class Portals : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private float _bbOxX0;
		
		private float _bbOxX1;
		
		private float _bbOxY0;
		
		private float _bbOxY1;
		
		private int _gameID;
		
		private int _portalID;
		
		private int _portalType;
		
		private int _roomSide;
		
		private int _roomX;
		
		private int _roomY;
		
		private int _roomZ;
		
		private int _targetPortalID;
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnBboxX0Changed();
		
		partial void OnBboxX0Changing(float value);
		
		partial void OnBboxX1Changed();
		
		partial void OnBboxX1Changing(float value);
		
		partial void OnBboxY0Changed();
		
		partial void OnBboxY0Changing(float value);
		
		partial void OnBboxY1Changed();
		
		partial void OnBboxY1Changing(float value);
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnPortalIDChanged();
		
		partial void OnPortalIDChanging(int value);
		
		partial void OnPortalTypeChanged();
		
		partial void OnPortalTypeChanging(int value);
		
		partial void OnRoomSideChanged();
		
		partial void OnRoomSideChanging(int value);
		
		partial void OnRoomXChanged();
		
		partial void OnRoomXChanging(int value);
		
		partial void OnRoomYChanged();
		
		partial void OnRoomYChanging(int value);
		
		partial void OnRoomZChanged();
		
		partial void OnRoomZChanging(int value);
		
		partial void OnTargetPortalIDChanged();
		
		partial void OnTargetPortalIDChanging(int value);
		#endregion
		
		
		public Portals()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_bbOxX0", Name="bbox_x0", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float BboxX0
		{
			get
			{
				return this._bbOxX0;
			}
			set
			{
				if ((_bbOxX0 != value))
				{
					this.OnBboxX0Changing(value);
					this.SendPropertyChanging();
					this._bbOxX0 = value;
					this.SendPropertyChanged("BboxX0");
					this.OnBboxX0Changed();
				}
			}
		}
		
		[Column(Storage="_bbOxX1", Name="bbox_x1", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float BboxX1
		{
			get
			{
				return this._bbOxX1;
			}
			set
			{
				if ((_bbOxX1 != value))
				{
					this.OnBboxX1Changing(value);
					this.SendPropertyChanging();
					this._bbOxX1 = value;
					this.SendPropertyChanged("BboxX1");
					this.OnBboxX1Changed();
				}
			}
		}
		
		[Column(Storage="_bbOxY0", Name="bbox_y0", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float BboxY0
		{
			get
			{
				return this._bbOxY0;
			}
			set
			{
				if ((_bbOxY0 != value))
				{
					this.OnBboxY0Changing(value);
					this.SendPropertyChanging();
					this._bbOxY0 = value;
					this.SendPropertyChanged("BboxY0");
					this.OnBboxY0Changed();
				}
			}
		}
		
		[Column(Storage="_bbOxY1", Name="bbox_y1", DbType="real", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public float BboxY1
		{
			get
			{
				return this._bbOxY1;
			}
			set
			{
				if ((_bbOxY1 != value))
				{
					this.OnBboxY1Changing(value);
					this.SendPropertyChanging();
					this._bbOxY1 = value;
					this.SendPropertyChanged("BboxY1");
					this.OnBboxY1Changed();
				}
			}
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_portalID", Name="portal_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int PortalID
		{
			get
			{
				return this._portalID;
			}
			set
			{
				if ((_portalID != value))
				{
					this.OnPortalIDChanging(value);
					this.SendPropertyChanging();
					this._portalID = value;
					this.SendPropertyChanged("PortalID");
					this.OnPortalIDChanged();
				}
			}
		}
		
		[Column(Storage="_portalType", Name="portal_type", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int PortalType
		{
			get
			{
				return this._portalType;
			}
			set
			{
				if ((_portalType != value))
				{
					this.OnPortalTypeChanging(value);
					this.SendPropertyChanging();
					this._portalType = value;
					this.SendPropertyChanged("PortalType");
					this.OnPortalTypeChanged();
				}
			}
		}
		
		[Column(Storage="_roomSide", Name="room_side", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomSide
		{
			get
			{
				return this._roomSide;
			}
			set
			{
				if ((_roomSide != value))
				{
					this.OnRoomSideChanging(value);
					this.SendPropertyChanging();
					this._roomSide = value;
					this.SendPropertyChanged("RoomSide");
					this.OnRoomSideChanged();
				}
			}
		}
		
		[Column(Storage="_roomX", Name="room_x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomX
		{
			get
			{
				return this._roomX;
			}
			set
			{
				if ((_roomX != value))
				{
					this.OnRoomXChanging(value);
					this.SendPropertyChanging();
					this._roomX = value;
					this.SendPropertyChanged("RoomX");
					this.OnRoomXChanged();
				}
			}
		}
		
		[Column(Storage="_roomY", Name="room_y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomY
		{
			get
			{
				return this._roomY;
			}
			set
			{
				if ((_roomY != value))
				{
					this.OnRoomYChanging(value);
					this.SendPropertyChanging();
					this._roomY = value;
					this.SendPropertyChanged("RoomY");
					this.OnRoomYChanged();
				}
			}
		}
		
		[Column(Storage="_roomZ", Name="room_z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomZ
		{
			get
			{
				return this._roomZ;
			}
			set
			{
				if ((_roomZ != value))
				{
					this.OnRoomZChanging(value);
					this.SendPropertyChanging();
					this._roomZ = value;
					this.SendPropertyChanged("RoomZ");
					this.OnRoomZChanged();
				}
			}
		}
		
		[Column(Storage="_targetPortalID", Name="target_portal_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int TargetPortalID
		{
			get
			{
				return this._targetPortalID;
			}
			set
			{
				if ((_targetPortalID != value))
				{
					this.OnTargetPortalIDChanging(value);
					this.SendPropertyChanging();
					this._targetPortalID = value;
					this.SendPropertyChanged("TargetPortalID");
					this.OnTargetPortalIDChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_Portals", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.Portals.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.Portals.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="rooms")]
	public partial class Rooms : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _gameID;
		
		private int _randomSeed;
		
		private int _roomID;
		
		private string _staticData;
		
		private int _x;
		
		private int _y;
		
		private int _z;
		
		private EntityRef<Games> _games = new EntityRef<Games>();
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnGameIDChanged();
		
		partial void OnGameIDChanging(int value);
		
		partial void OnRandomSeedChanged();
		
		partial void OnRandomSeedChanging(int value);
		
		partial void OnRoomIDChanged();
		
		partial void OnRoomIDChanging(int value);
		
		partial void OnStaticDataChanged();
		
		partial void OnStaticDataChanging(string value);
		
		partial void OnXChanged();
		
		partial void OnXChanging(int value);
		
		partial void OnYChanged();
		
		partial void OnYChanging(int value);
		
		partial void OnZChanged();
		
		partial void OnZChanging(int value);
		#endregion
		
		
		public Rooms()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_gameID", Name="game_id", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int GameID
		{
			get
			{
				return this._gameID;
			}
			set
			{
				if ((_gameID != value))
				{
					this.OnGameIDChanging(value);
					this.SendPropertyChanging();
					this._gameID = value;
					this.SendPropertyChanged("GameID");
					this.OnGameIDChanged();
				}
			}
		}
		
		[Column(Storage="_randomSeed", Name="random_seed", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RandomSeed
		{
			get
			{
				return this._randomSeed;
			}
			set
			{
				if ((_randomSeed != value))
				{
					this.OnRandomSeedChanging(value);
					this.SendPropertyChanging();
					this._randomSeed = value;
					this.SendPropertyChanged("RandomSeed");
					this.OnRandomSeedChanged();
				}
			}
		}
		
		[Column(Storage="_roomID", Name="room_id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int RoomID
		{
			get
			{
				return this._roomID;
			}
			set
			{
				if ((_roomID != value))
				{
					this.OnRoomIDChanging(value);
					this.SendPropertyChanging();
					this._roomID = value;
					this.SendPropertyChanged("RoomID");
					this.OnRoomIDChanged();
				}
			}
		}
		
		[Column(Storage="_staticData", Name="static_data", DbType="text", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string StaticData
		{
			get
			{
				return this._staticData;
			}
			set
			{
				if (((_staticData == value) 
							== false))
				{
					this.OnStaticDataChanging(value);
					this.SendPropertyChanging();
					this._staticData = value;
					this.SendPropertyChanged("StaticData");
					this.OnStaticDataChanged();
				}
			}
		}
		
		[Column(Storage="_x", Name="x", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int X
		{
			get
			{
				return this._x;
			}
			set
			{
				if ((_x != value))
				{
					this.OnXChanging(value);
					this.SendPropertyChanging();
					this._x = value;
					this.SendPropertyChanged("X");
					this.OnXChanged();
				}
			}
		}
		
		[Column(Storage="_y", Name="y", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Y
		{
			get
			{
				return this._y;
			}
			set
			{
				if ((_y != value))
				{
					this.OnYChanging(value);
					this.SendPropertyChanging();
					this._y = value;
					this.SendPropertyChanged("Y");
					this.OnYChanged();
				}
			}
		}
		
		[Column(Storage="_z", Name="z", DbType="int", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Z
		{
			get
			{
				return this._z;
			}
			set
			{
				if ((_z != value))
				{
					this.OnZChanging(value);
					this.SendPropertyChanging();
					this._z = value;
					this.SendPropertyChanged("Z");
					this.OnZChanged();
				}
			}
		}
		
		#region Parents
		[Association(Storage="_games", OtherKey="GameID", ThisKey="GameID", Name="Games_Rooms", IsForeignKey=true)]
		[DebuggerNonUserCode()]
		public Games Games
		{
			get
			{
				return this._games.Entity;
			}
			set
			{
				if (((this._games.Entity == value) 
							== false))
				{
					if ((this._games.Entity != null))
					{
						Games previousGames = this._games.Entity;
						this._games.Entity = null;
						previousGames.Rooms.Remove(this);
					}
					this._games.Entity = value;
					if ((value != null))
					{
						value.Rooms.Add(this);
						_gameID = value.GameID;
					}
					else
					{
						_gameID = default(int);
					}
				}
			}
		}
		#endregion
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="room_templates")]
	public partial class RoomTemplates : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private byte[] _compressedNaVMesh;
		
		private byte[] _compressedVisibility;
		
		private int _id;
		
		private string _name;
		
		private string _xml;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnCompressedNavMeshChanged();
		
		partial void OnCompressedNavMeshChanging(byte[] value);
		
		partial void OnCompressedVisibilityChanged();
		
		partial void OnCompressedVisibilityChanging(byte[] value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		
		partial void OnXMLChanged();
		
		partial void OnXMLChanging(string value);
		#endregion
		
		
		public RoomTemplates()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_compressedNaVMesh", Name="compressed_nav_mesh", DbType="blob", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] CompressedNavMesh
		{
			get
			{
				return this._compressedNaVMesh;
			}
			set
			{
				if (((_compressedNaVMesh == value) 
							== false))
				{
					this.OnCompressedNavMeshChanging(value);
					this.SendPropertyChanging();
					this._compressedNaVMesh = value;
					this.SendPropertyChanged("CompressedNavMesh");
					this.OnCompressedNavMeshChanged();
				}
			}
		}
		
		[Column(Storage="_compressedVisibility", Name="compressed_visibility", DbType="blob", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public byte[] CompressedVisibility
		{
			get
			{
				return this._compressedVisibility;
			}
			set
			{
				if (((_compressedVisibility == value) 
							== false))
				{
					this.OnCompressedVisibilityChanging(value);
					this.SendPropertyChanging();
					this._compressedVisibility = value;
					this.SendPropertyChanged("CompressedVisibility");
					this.OnCompressedVisibilityChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="id", DbType="int", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="name", DbType="varchar(256)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) 
							== false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[Column(Storage="_xml", Name="xml", DbType="text", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string XML
		{
			get
			{
				return this._xml;
			}
			set
			{
				if (((_xml == value) 
							== false))
				{
					this.OnXMLChanging(value);
					this.SendPropertyChanging();
					this._xml = value;
					this.SendPropertyChanged("XML");
					this.OnXMLChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
