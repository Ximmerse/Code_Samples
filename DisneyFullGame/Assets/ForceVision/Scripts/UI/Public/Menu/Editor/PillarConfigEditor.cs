using UnityEditor;
using UnityEngine;
using System;
using SG.Lonestar;
using BSG.SWARTD;

namespace Disney.ForceVision.Internal
{
	[CustomEditor(typeof(PillarConfig), true)]
	public class PillarConfigEditor : Editor
	{
		/// <summary>
		/// Called each time the inspector's GUI is refreshed.
		/// </summary>
		public override void OnInspectorGUI()
		{
			PillarConfig myTarget = (PillarConfig)target;

			myTarget.Game = (Game)EditorGUILayout.Popup("Game: ", (int)myTarget.Game, Enum.GetNames(typeof(Game)));

			myTarget.IsBonusPlanet = EditorGUILayout.Toggle("Is Bonus Planet: ", myTarget.IsBonusPlanet);

			if (myTarget.IsBonusPlanet)
			{
				myTarget.BonusPlanet = (BonusPlanetType)EditorGUILayout.Popup("Bonus Planet: ", (int)myTarget.BonusPlanet, Enum.GetNames(typeof(BonusPlanetType)));
			}
			else
			{
				myTarget.Planet = (PlanetType)EditorGUILayout.Popup("Planet: ", (int)myTarget.Planet, Enum.GetNames(typeof(PlanetType)));
			}

			myTarget.PillarNumber = EditorGUILayout.IntField("Pillar Number: ", myTarget.PillarNumber);

			DrawLine();

			EditorGUILayout.LabelField("Interstitial Trigger or VO Event After Beat First Time");

			if (myTarget.Interstitial.Length != 3 && (myTarget.Game == Game.Assault || myTarget.Game == Game.Duel))
			{
				myTarget.Interstitial = new[] { false, false, false };
			}

			if (myTarget.InterstitialTrigger.Length != 3 && (myTarget.Game == Game.Assault || myTarget.Game == Game.Duel))
			{
				myTarget.InterstitialTrigger = new[] { "", "", "" };
			}

			if (myTarget.Interstitial.Length != 1 && (myTarget.Game == Game.HoloChess || myTarget.Game == Game.TowerDefense))
			{
				myTarget.Interstitial = new[] { false };
			}

			if (myTarget.InterstitialTrigger.Length != 1 && (myTarget.Game == Game.HoloChess || myTarget.Game == Game.TowerDefense))
			{
				myTarget.InterstitialTrigger = new[] { "" };
			}

			if (myTarget.LostVOTrigger.Length != 3 && (myTarget.Game == Game.Assault || myTarget.Game == Game.Duel))
			{
				myTarget.LostVOTrigger = new[] { "", "", "" };
			}

			if (myTarget.LostVOTrigger.Length != 1 && (myTarget.Game == Game.HoloChess || myTarget.Game == Game.TowerDefense))
			{
				myTarget.LostVOTrigger = new[] { "" };
			}

			for (int i = 0; i < myTarget.Interstitial.Length; i++)
			{
				EditorGUILayout.BeginHorizontal();
				myTarget.Interstitial[i] = EditorGUILayout.Toggle(Enum.GetNames(typeof(Difficulty))[i] + ": ", myTarget.Interstitial[i]);
				myTarget.InterstitialTrigger[i] = EditorGUILayout.TextField(myTarget.InterstitialTrigger[i]);
				EditorGUILayout.EndHorizontal();
			}

			DrawLine();

			EditorGUILayout.LabelField("VO Event After Lost First Time");

			for (int i = 0; i < myTarget.LostVOTrigger.Length; i++)
			{
				myTarget.LostVOTrigger[i] = EditorGUILayout.TextField(Enum.GetNames(typeof(Difficulty))[i] + ": ", myTarget.LostVOTrigger[i]);
			}

			DrawLine();

			switch (myTarget.Game)
			{
				case Game.Duel:
					myTarget.Duelist = (DuelAPI.Duelist)EditorGUILayout.Popup("Duelist: ", (int)myTarget.Duelist, Enum.GetNames(typeof(DuelAPI.Duelist)));
				break;

				case Game.TowerDefense:
					myTarget.Battle = (TDAPI.Battles)EditorGUILayout.Popup("Battle: ", (int)myTarget.Battle, Enum.GetNames(typeof(TDAPI.Battles)));
				break;
			}

			DrawLine();

			myTarget.PreviousConfig = (PillarConfig)EditorGUILayout.ObjectField("Locked until Beat: ", myTarget.PreviousConfig, typeof(PillarConfig), false);

			DrawLine();

			EditorUtility.SetDirty(target);
		}

		private static void DrawLine()
		{
			EditorGUILayout.Space();
			GUILayout.Box("", new [] {
				GUILayout.ExpandWidth(true),
				GUILayout.Height(1)
			});
			EditorGUILayout.Space();
		}
	}
}