using UnityEngine;
using SG.ProtoFramework;
using System;

namespace Disney.ForceVision
{
	public enum NodeName
	{
		First,
		Second,
		Thrid
	}

	public class PlanetNodes : MonoBehaviour
	{
		/// <summary>
		/// List of nodes for this planet
		/// </summary>
		[EnumMappedList(typeof(NodeName))]
		public PlanetNode[] Nodes = new PlanetNode[Enum.GetNames(typeof(NodeName)).Length];

		/// <summary>
		/// Sets the progress for a given node.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="completed">Completed.</param>
		public void SetProgress(NodeName node, DifficultyComplete completed)
		{
			if (Nodes[(int)node] != null)
			{
				Nodes[(int)node].SetProgress(completed);
			}
		}

		/// <summary>
		/// Turn on the tooltip for a given node
		/// </summary>
		/// <param name="node">Node.</param>
		public void SetTooltip(NodeName node)
		{
			Nodes[(int)node].Tooltip.SetActive(true);
		}
	}
}