using System;
using System.Runtime.Serialization;

[Serializable]
public class SavedPlayerData
{
	public const string USER_NAME_PROPERTY = "username";

	public const string DISPLAY_NAME_PROPERTY = "displayName";

	public const string PLAYER_SWID_PROPERTY = "playerSwid";

	public string UserName;

	public string DisplayName;

	public string Swid;

	public byte[] PaperDollBytes;

	[OptionalField]
	public long PlayerId;

	[OptionalField]
	public int PenguinColor;

	[OptionalField]
	public int HighScore;

	[OptionalField]
	public bool IsLocalAccount;

	[OptionalField]
	public bool HasGoldenHelmet;

	[OptionalField]
	public string StoredPassword;

	[NonSerialized]
	public string Password;
}
