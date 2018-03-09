using UnityEngine;

namespace Disney.ForceVision
{

	public class PlayerPrefsStorage : IPlayerPrefsStorage
	{

		/// <summary>
		/// The player prefs prefix is the PlayerPrefsPrefix + "_".
		/// </summary>
		private string playerPrefsPrefix;

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.PlayerPrefsStorage"/> class.
		/// </summary>
		/// <param name="game">Game.</param>
		public PlayerPrefsStorage(Game game)
		{
			this.playerPrefsPrefix = game.ToString("f");
		}

		/// <summary>
		/// Gets a key for accessing PlayerPrefs.
		/// All keys in Storage are prefixed by the name of StorageFolder + "_" + key
		/// </summary>
		/// <returns>The player prefs key.</returns>
		/// <param name="key">Key.</param>
		public string GetPlayerPrefsKey(string key)
		{
			return playerPrefsPrefix + "_" + key;
		}

		/// <summary>
		/// Does the key exist in PlayerPrefs
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="key">Key.</param>
		public bool PrefKeyExists(string key)
		{
			return PlayerPrefs.HasKey(GetPlayerPrefsKey(key));
		}

		/// <summary>
		/// Deletes the PlayerPref.
		/// </summary>
		/// <param name="key">Key.</param>
		public void DeletePref(string key)
		{
			PlayerPrefs.DeleteKey(GetPlayerPrefsKey(key));
		}

		/// <summary>
		/// Gets the PlayerPref string.
		/// </summary>
		/// <returns>The preference string.</returns>
		/// <param name="key">Key.</param>
		public string GetPrefString(string key)
		{
			return PlayerPrefs.GetString(GetPlayerPrefsKey(key));
		}

		/// <summary>
		/// Sets the PlayerPref string.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetPrefString(string key, string value)
		{
			PlayerPrefs.SetString(GetPlayerPrefsKey(key), value);
		}

		/// <summary>
		/// Gets the PlayerPref int.
		/// </summary>
		/// <returns>The preference int.</returns>
		/// <param name="key">Key.</param>
		/// <param name="defaultValue">Default value.</param>
		public int GetPrefInt(string key, int defaultValue = 0)
		{
			return PlayerPrefs.GetInt(GetPlayerPrefsKey(key), defaultValue);
		}

		/// <summary>
		/// Sets the PlayerPref int.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetPrefInt(string key, int value)
		{
			PlayerPrefs.SetInt(GetPlayerPrefsKey(key), value);
		}

		/// <summary>
		/// Gets the PlayerPref float.
		/// </summary>
		/// <returns>The preference float.</returns>
		/// <param name="key">Key.</param>
		public float GetPrefFloat(string key)
		{
			return PlayerPrefs.GetFloat(GetPlayerPrefsKey(key));
		}

		/// <summary>
		/// Sets the PlayerPref float.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetPrefFloat(string key, float value)
		{
			PlayerPrefs.SetFloat(GetPlayerPrefsKey(key), value);
		}

	}
}