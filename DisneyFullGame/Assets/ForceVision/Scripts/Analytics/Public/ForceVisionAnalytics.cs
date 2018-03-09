using DCPI.Platforms.SwrveManager.Analytics;

namespace Disney.ForceVision
{
	/// <summary>
	/// Methods for analytics calls specific to Force Vision app
	/// </summary>
	public static class ForceVisionAnalytics
	{
		private static float timeGameStarted = 0;

		/// <summary>
		/// Logs a game starting. Call this when a game starts.
		/// This is called in PillarMenuNode when a game is first started, and doesn't need to be called by each game.
		/// </summary>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		public static void LogGameStart(bool multiplayer = false)
		{
			LogGameStartOrRestart(false, multiplayer);
		}

		/// <summary>
		/// Logs a game restarting.
		/// </summary>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		public static void LogGameRestart(bool multiplayer = false)
		{
			LogGameQuit(multiplayer);
			LogGameStartOrRestart(true, multiplayer);
		}

		/// <summary>
		/// Logs the game finish, either by winning or losing.
		/// </summary>
		/// <param name="rating">Rating.</param>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		public static void LogGameFinish(int rating, bool multiplayer = false)
		{
			string result = (rating > 0) ? "success" : "fail";
			LogGameResult(result, rating, multiplayer);
		}

		/// <summary>
		/// Logs quitting a game.
		/// </summary>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		public static void LogGameQuit(bool multiplayer = false)
		{
			LogGameResult("quit", 0, multiplayer);
		}

		/// <summary>
		/// Logs the Lenovo one-time call to our analytics.
		/// This should be called alongside Lenovo's one-time call.
		/// </summary>
		/// <param name="deviceID">Device ID.</param>
		/// <param name="channel">Distribution Channel (iTunes / Google Play).</param>
		/// <param name="country">Country.</param>
		/// <param name="bundleCode">Bundle code.</param>
		/// <param name="osVersion">OS version.</param>
		/// <param name="sdkVersion">Sdk version.</param>
		public static void LogOneTimeCall(string deviceID,
		                                  string channel,
		                                  string country,
		                                  string bundleCode,
		                                  string osVersion,
		                                  string sdkVersion)
		{
			ActionAnalytics action = new ActionAnalytics(deviceID,
			                                             "new_activation",
			                                             0,
			                                             channel,
			                                             country);
			Analytics.LogAction(action);
			LogToConsole("LogOneTimeCall 1/2");

			action = new ActionAnalytics(bundleCode, 
			                             "hlog",
			                             0,
			                             osVersion,
			                             sdkVersion);
			Analytics.LogAction(action);
			LogToConsole("LogOneTimeCall 2/2");
		}

		/// <summary>
		/// Logs unlocking a pawn / force power, etc.
		/// </summary>
		/// <param name="unlock">Unlock.</param>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		public static void LogUnlock(string unlock, bool multiplayer = false)
		{
			PillarConfig config = MenuController.ConfigToLoad;

			if (config == null)
			{
				return;
			}

			ActionAnalytics action = new ActionAnalytics("unit_unlocked",
			                                             GameModeFromConfig(config) + "unit_unlocked",
			                                             config.PillarNumber,
			                                             unlock,
			                                             multiplayer ? "multiplayer" : "single_player",
			                                             MenuController.DifficultyToLoad.ToString(),
			                                             config.ContextForAnalytics());
			Analytics.LogAction(action);
			LogToConsole("LogUnlock: " + unlock);
		}

		/// <summary>
		/// Logs the language select.
		/// </summary>
		/// <param name="language">Language.</param>
		public static void LogLanguageSelect(string language)
		{
			ActionAnalytics action = new ActionAnalytics(language,
			                                             "language_select",
			                                             0,
			                                             null,
			                                             null,
			                                             null,
			                                             "language_select");
			Analytics.LogAction(action);
			LogToConsole("LogLanguageSelect: " + language);
		}

		/// <summary>
		/// Logs a game starting or restarting.
		/// </summary>
		/// <param name="restart">Whether the game is starting normally, or restarting via "restart" in the pause menu.</param>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		private static void LogGameStartOrRestart(bool restart, bool multiplayer)
		{
			// MenuController.ConfigToLoad won't get set while testing specific games in editor
			if (MenuController.ConfigToLoad == null)
			{
				return;
			}

			PillarConfig config = MenuController.ConfigToLoad;
			ActionAnalytics action = new ActionAnalytics("game_start",
			                                             GameModeFromConfig(config) + "game_start",
			                                             config.PillarNumber,
			                                             restart ? "retry" : null,
			                                             multiplayer ? "multiplayer" : "single_player",
			                                             MenuController.DifficultyToLoad.ToString(),
			                                             config.ContextForAnalytics());
			timeGameStarted = UnityEngine.Time.time;
			Analytics.LogAction(action);
			LogToConsole("LogGameStartOrRestart: restart=" + restart.ToString());
		}

		/// <summary>
		/// Logs the game result.
		/// </summary>
		/// <param name="result">Result.</param>
		/// <param name="rating">Rating.</param>
		/// <param name="multiplayer">If set to <c>true</c> multiplayer.</param>
		private static void LogGameResult(string result, int rating, bool multiplayer)
		{
			// MenuController.ConfigToLoad won't get set while testing specific games in editor
			if (MenuController.ConfigToLoad == null)
			{
				return;
			}

			PillarConfig config = MenuController.ConfigToLoad;
			ActionAnalytics action = new ActionAnalytics(result,
			                                             GameModeFromConfig(config) + result,
			                                             config.PillarNumber,
			                                             rating.ToString(),
			                                             multiplayer ? "multiplayer" : "single_player",
			                                             MenuController.DifficultyToLoad.ToString(),
			                                             config.ContextForAnalytics());

			string stepName = result;
			if (config.Game == Game.Assault || config.Game == Game.Duel)
			{
				// difficulty is only used for Duel and Assault. Not for Holochess and Tower Defense.
				stepName += "_" + MenuController.DifficultyToLoad.ToString();
			}
			TimingAnalytics timingAction = new TimingAnalytics((int)(UnityEngine.Time.time - timeGameStarted),
			                                                   GameModeFromConfig(config).TrimEnd('.'),
			                                                   config.ContextForAnalytics(),
			                                                   config.PillarNumber,
			                                                   stepName);

			Analytics.LogAction(action);
			Analytics.LogTimingAction(timingAction);
			LogToConsole("LogGameResult: " + result + " " + rating.ToString());
		}

		/// <summary>
		/// Returns a "game mode" string from a PillarConfig
		/// </summary>
		/// <returns>The mode from config.</returns>
		/// <param name="config">Config.</param>
		private static string GameModeFromConfig(PillarConfig config)
		{
			switch (config.Game)
			{
				case Game.Assault:
				case Game.Duel:
					return "CombatChallenge.";
				case Game.HoloChess:
					return "InsightChallenge.";
				case Game.TowerDefense:
					return "LeadershipChallenge.";
				default:
					return "UnknownGameMode.";
			}
		}

		/// <summary>
		/// Logs to console in editor.
		/// </summary>
		/// <param name="output">Output.</param>
		private static void LogToConsole(string output)
		{
			if (!UnityEngine.Application.isEditor)
			{
				return;
			}

			Log.Debug("<color=cyan>ANALYTICS:</color> " + output);
		}
	}
}

