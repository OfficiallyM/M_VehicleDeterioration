using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace VehicleDeterioration.Modules
{
	[DisallowMultipleComponent]
	internal class Deteriorate : MonoBehaviour
	{
		private Vector3 lastPosition;
		private Transform customSpace;
		private float distance = 0f;
		private string[] types;
		private Logger logger;
		public bool deteriorated = false;
		private Random random;
		private bool isVehicle = false;

		/// <summary>
		/// Set the vehicle part types
		/// </summary>
		/// <param name="_types">The types to set</param>
		public void SetTypes(string[] _types)
		{
			types = _types;
		}

		/// <summary>
		/// Set the logger instance
		/// </summary>
		/// <param name="_logger">Existing logger instance</param>
		public void SetLogger(Logger _logger) { 
			logger = _logger; 
		}

		/// <summary>
		/// Set random generator
		/// </summary>
		/// <param name="_random">Existing random instance</param>
		public void SetRandom(Random _random)
		{
			random = _random;
		}

		/// <summary>
		/// Get the current distance
		/// </summary>
		/// <returns>Distance moved as a float</returns>
		public float GetDistance()
		{
			return distance;
		}

		public void Start()
		{
			if (gameObject.GetComponent<carscript>() != null)
				isVehicle = true;

			if (isVehicle)
				return;

			GameObject customSpaceGameObject = new GameObject();
			customSpace = customSpaceGameObject.transform;
			customSpaceGameObject.AddComponent<visszarako>();
			customSpaceGameObject.name = gameObject.name + " Custom Space";
			lastPosition = customSpace.InverseTransformPoint(transform.position);
		}

		public void OnDestroy()
		{
			if (customSpace != null)
				Destroy(customSpace.gameObject);
		}

		public void Update()
		{
			if (!isVehicle)
			{
				float distanceBetweenPoints = (customSpace.InverseTransformPoint(transform.position) - lastPosition).magnitude * (1f / 1000f);
				distance += distanceBetweenPoints;
			}
			else
				distance = gameObject.GetComponent<carscript>().distance;

			float distanceCheck = GetDistanceCheck();

			double distanceResult = Math.Round((float)Math.Round(distance, 2) % distanceCheck, 2);

			if (!deteriorated && distance >= 0.01f && distanceResult == 0)
			{
				partconditionscript condition = gameObject.GetComponentInChildren<partconditionscript>();
				if (condition != null)
				{
					deteriorated = true;
					if (condition.state + 1 > 4)
					{
						// 1 in 10 chance of part falling off at max rust.
						if (random.Next(10) == 0)
						{
							// Ran out of conditions, drop the part off the vehicle.
							partscript part = gameObject.GetComponentInChildren<partscript>();
							part.FallOFf();
						}
					}
					else
					{
						// 1 in 4 chance of part state changing.
						if (random.Next(4) == 0)
						{
							condition.state += 1;
							condition.Refresh();
						}
					}
				}
			}
			else if (deteriorated && distanceResult != 0)
				deteriorated = false;

			// Update last position.
			if (!isVehicle)
				lastPosition = customSpace.InverseTransformPoint(transform.position);
		}

		/// <summary>
		/// Get the distance check based off type
		/// </summary>
		/// <returns>Distance to check at as a float</returns>
		private float GetDistanceCheck()
		{
			switch (types[0])
			{
				// Tires.
				case "gumi":
					return 25f;
				case "kmh":
				case "radio":
				case "leftnapellenzo":
				case "rightnapellenzo":
					return 55f;
			}

			return 40f;
		}
	}
}
