using System;

namespace Disney.ForceVision
{
	public class MenuNodeEventArgs : EventArgs
	{
		#region Properties

		public MenuNodeType NodeType { get; private set; }
		public PlanetType? Planet { get; private set; }
		public Game? GameToLaunch { get; private set; }
		public BonusPlanetType? BonusPlanet { get; private set; }

		#endregion

		#region Constructor

		public MenuNodeEventArgs(MenuNodeType nodeType, PlanetType planet)
		{
			NodeType = nodeType;
			Planet = planet;
			GameToLaunch = null;
		}

		public MenuNodeEventArgs(MenuNodeType nodeType, BonusPlanetType bonusPlanet)
		{
			NodeType = nodeType;
			BonusPlanet = bonusPlanet;
			GameToLaunch = null;
		}

		public MenuNodeEventArgs(MenuNodeType nodeType, Game gameToLaunch)
		{
			NodeType = nodeType;
			Planet = null;
			GameToLaunch = gameToLaunch;
		}

		#endregion
	}
}