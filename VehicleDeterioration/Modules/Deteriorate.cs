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
		private Logger logger;

		private Random random;
		private Vector3 lastPosition;
		private Transform customSpace;
		private float distance = 0f;
		private string[] types;
		private bool deteriorated = false;
		private bool isVehicle = false;
		private int mode;

		/// <summary>
		/// Makeshift constructor as MonoBehaviour doesn't support constructors
		/// </summary>
		/// <param name="_logger">Logger instance</param>
		/// <param name="_random">Random instance</param>
		/// <param name="_types">Part types</param>
		public void Initialise(Logger _logger, Random _random, string[] _types, int _mode)
		{
			logger = _logger;
			random = _random;
			types = _types;
			mode = _mode;
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
			try
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
							if (types[0] == "gumi")
							{
								// Tires have a 1 in 6 chance of bursting at max condition.
								if (random.Next(6) == 0)
								{
									carscript car = gameObject.GetComponentInParent<carscript>();
									Vector3 position = gameObject.transform.position;

									partscript part = gameObject.GetComponentInChildren<partscript>();
									part.FallOFf();

									// Apply upwards force to the car at the position of the blown tire.
									if (car != null)
										car.RB.AddForceAtPosition(new Vector3(0f, 1500f, 0f), position, ForceMode.Impulse);
								}
							}
							else
							{
								// 1 in 10 chance of part falling off at max condition.
								if (random.Next(10) == 0)
								{
									// Ran out of conditions, drop the part off the vehicle.
									partscript part = gameObject.GetComponentInChildren<partscript>();
									part.FallOFf();
								}
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
			catch (Exception ex)
			{
				logger.Log($"Deteriorate update error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Get the distance check based off type
		/// </summary>
		/// <returns>Distance to check at as a float</returns>
		private float GetDistanceCheck()
		{
			// Debug mode.
			if (mode == 1)
				return 0.5f;

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
