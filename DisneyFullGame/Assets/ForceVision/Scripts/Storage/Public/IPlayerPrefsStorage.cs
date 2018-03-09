namespace Disney.ForceVision
{
	public interface IPlayerPrefsStorage
	{

		/// <summary>
		/// Gets a key for accessing PlayerPrefs.
		/// All keys in Storage are prefixed by the name of StorageFolder + "_" + key
		/// </summary>
		/// <returns>The player prefs key.</returns>
		/// <param name="key">Key.</param>
		string GetPlayerPrefsKey(string key);

		/// <summary>
		/// Does the key exist in PlayerPrefs
		/// </summary>
		/// <returns><c>true</c>, if key exists was preferenced, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		bool PrefKeyExists(string key);

		/// <summary>
		/// Deletes the PlayerPref.
		/// </summary>
		/// <param name="key">Key.</param>
		void DeletePref(string key);

		/// <summary>
		/// Gets the PlayerPref string.
		/// </summary>
		/// <returns>The preference string.</returns>
		/// <param name="key">Key.</param>
		string GetPrefString(string key);

		/// <summary>
		/// Sets the PlayerPref string.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void SetPrefString(string key, string value);

		/// <summary>
		/// Gets the PlayerPref int.
		/// </summary>
		/// <returns>The preference int.</returns>
		/// <param name="key">Key.</param>
		/// <param name="defaultValue">Default value.</param>
		int GetPrefInt(string key, int defaultValue);

		/// <summary>
		/// Sets the PlayerPref int.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void SetPrefInt(string key, int value);

		/// <summary>
		/// Gets the PlayerPref float.
		/// </summary>
		/// <returns>The preference float.</returns>
		/// <param name="key">Key.</param>
		float GetPrefFloat(string key);

		/// <summary>
		/// Sets the PlayerPref float.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void SetPrefFloat(string key, float value);

	}
}