using System;
using System.Collections.Generic;
using TLDLoader;
using UnityEngine;
using VehicleDeterioration.Core;
using VehicleDeterioration.Modules;
using Logger = VehicleDeterioration.Modules.Logger;
using Random = System.Random;

namespace VehicleDeterioration
{
	public class M_VehicleDeterioration : Mod
	{
		// Mod meta stuff.
		public override string ID => Meta.ID;
		public override string Name => Meta.Name;
		public override string Author => Meta.Author;
		public override string Version => Meta.Version;

		// Initialise modules.
		private readonly Logger logger = new Logger();

		// Variables.
		private readonly bool debug = false;

		private List<string> blacklistedTypes = new List<string>()
		{
			"coolanttank",
		};
		private Random random;

		public override void OnLoad()
		{
			random = new Random();
		}

		public override void Update()
		{
			carscript car = mainscript.M.player.Car;

			// Return early if player isn't in a vehicle.
			if (car == null)
				return;

			// Add deteriorate to vehicle body.
			if (car.gameObject.GetComponent<Deteriorate>() == null)
			{
				Deteriorate deteriorate = car.gameObject.AddComponent<Deteriorate>();
				deteriorate.Initialise(logger, random, new string[] { "body" }, debug ? 1 : 0);
			}

			foreach (partslotscript slot in car.GetComponent<tosaveitemscript>().partslotscripts)
			{
				if (slot.part != null && slot.part.GetComponent<Deteriorate>() == null)
					AddToParts(slot);
			}
		}

		/// <summary>
		/// Attach deteriorate class to each part and any sub parts.
		/// </summary>
		/// <param name="slot">The slot to search through for parts</param>
		private void AddToParts(partslotscript slot)
		{
			// Can't find slot or slot has no mounted part, continue.
			if (slot == null || slot.part == null)
				return;

			Deteriorate deteriorate = slot.part.GetComponent<Deteriorate>();

			if (deteriorate == null)
			{
				foreach (var type in slot.tipus)
				{
					// Return early if type is blacklisted.
					if (blacklistedTypes.Contains(type))
						return;
				}

				deteriorate = slot.part.gameObject.AddComponent<Deteriorate>();
				deteriorate.Initialise(logger, random, slot.tipus, debug ? 1 : 0);
			}

			foreach (var subslot in slot.part.tosaveitem.partslotscripts)
			{
				AddToParts(subslot);
			}
		}
	}
}
