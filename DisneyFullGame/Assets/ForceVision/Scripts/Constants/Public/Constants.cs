namespace Disney.ForceVision
{
	#region Enums

	public enum Game
	{
		ForceVision,
		Duel,
		TowerDefense,
		HoloChess,
		Assault,
		None,
		PorgIsland
	}

	#endregion

	public class Constants
	{

		#region Public Constant Properties

		/// <summary>
		/// The saber color player preference key.
		/// </summary>
		public const string SaberColorPlayerPrefKey = "SaberColor";

		/// <summary>
		/// The galaxy loaded player preference key.
		/// </summary>
		public const string GalaxyLoadedPlayerPrefKey = "GalaxyHasLoadedOnce";

		/// <summary>
		/// If we are using spatialization.
		/// </summary>
		public const string UseSpatialization = "UseSpatialization";

		/// <summary>
		/// The use profile selection.
		/// </summary>
		public const string UseProfileSelection = "UseProfileSelection";

		#if IS_DEMO_BUILD
		/// <summary>
		/// All progression unlocked.
		/// </summary>
		public const string AllProgressionUnlocked = "AllProgressionUnlocked";

		/// <summary>
		/// The go pro mode.
		/// </summary>
		public const string GoProMode = "GoProMode";
		#endif

		/// <summary>
		/// The scene fade out time.
		/// </summary>
		public const float SceneFadeOutTime = 1.5f;

		/// <summary>
		/// The galaxy hide time.
		/// </summary>
		public const float GalaxyHideTime = 0.5f;

		/// <summary>
		/// Crait unlocked.
		/// </summary>
		public const string CraitUnlocked = "CraitUnlockedOnce";

		/// <summary>
		/// The porg unlocked.
		/// </summary>
		public const string PorgUnlocked = "PorgUnlockedOnce";

		public const string ML = "xfxH";

		#endregion

		#region Level Triggers

		// Non Config Based Animation Triggers
		public const string LoseCoreDuelHard = "STORY_DX_Arch_Combat_Hard_DarkPath";

		public const string WinCoreDuelHard = "archivistInterstitial_defeatArchivist_light";

		public const string EarnAdvancedCombat = "archivistInterstitial_advancedCombat";

		public const string EarnAllTrailMedals = "archivistInterstitial_unlockDarkivist";

		public const string RandomDuelLoss = "STORY_DX_Arch_Combat_Lose_Advice";

		#endregion

	}
}