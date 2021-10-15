﻿using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MrSandmanManMeASand
{
	[BepInPlugin("net.distrilul.mrsandman", "MrSandmanManMeASand", "1.0")]
	[BepInProcess("ULTRAKILL.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private GameObject _ply;
		
		private bool _isEnabled;

		private void Awake()
		{
			SceneManager.sceneLoaded += (scene, mode) =>
			{
				if (!_isEnabled)
					return;

				ObjectActivator trigger;
				
				switch (scene.name)
				{
					case "Level 2-4":
						trigger = GameObject.Find("FirstRoom/Room/Cube (1)").GetComponent<ObjectActivator>();
						trigger.events.onActivate.AddListener(() =>
							HudMessageReceiver.Instance.SendHudMessage("The floor is unbearably hot, but it " +
							                                           "seems like the tram is safe for you to stand on."));
						break;
					case "Level 3-2":
						trigger = GameObject.Find("FirstRoom/Room/Cube (1)").GetComponent<ObjectActivator>();
						trigger.events.onActivate.AddListener(() => 
							HudMessageReceiver.Instance.SendHudMessage("The room feels cool, but you can feel " +
																							"the blistering heat ahead."));
						break;
					case "Level 4-4":
						trigger = GameObject.Find("FirstRoom/Room/Cube (1)").GetComponent<ObjectActivator>();
						trigger.events.onActivate.AddListener(() => 
							HudMessageReceiver.Instance.SendHudMessage("The air is hot and humid, but the floor " +
																						"seems to be fine to walk on."));
						break;
				}
			};
		}

		private void Update()
		{
			if (!_isEnabled)
				return;
			
			if (!NewMovement.Instance)
				return;
			_ply = NewMovement.Instance.gameObject;
			
			if (_ply.GetComponent<FloorHurt>() == null)
			{
				_ply.AddComponent<FloorHurt>();
			}
		}
		
		private void OnGUI()
		{
			if (SceneManager.GetActiveScene().name != "Main Menu")
			{
				if (_isEnabled)
					GUI.Label(new Rect(10, Screen.height - 30, 200, 20), "MrSandman is enabled.");
				return;
			}
			
			_isEnabled = GUI.Toggle(new Rect(10, Screen.height - 30, 200, 20), _isEnabled, "Enable MrSandman");
		}
	}

	[DefaultExecutionOrder(10000)]
	internal class FloorHurt : MonoBehaviour
	{
		private GroundCheck _gc;
		private NewMovement _nm;
		private GameObject _prefab;

		private static readonly (string, string[])[] DamageBlacklist =
		{
			// Common
			(null, new[] { "FirstRoom", "FinalRoom", "Secret FirstRoom", "FinalRoom 2", "Prime FirstRoom", "Prime FinalRoom" }),
			("Level 0-1", new[] { "1 -  Starting Room", "2A - Hallway", "2B - Hallway", "1Alt -  Short Starting Room", "2Alt - Short Hallway" }),
			// 2-4
			("Level 2-4", new[] {"0 - Tram"}), 
			// 3-2
			("Level 3-2", new[] {"1 - Opening", "2 - Thin Walkway", "3 - Other Room", "3B - Drop", "DoorMouth" }),
			// 4-4
			("Level 4-4", new[] { "1 - Underground", "2 - Elevator 1", "3 - Ground Floor", "3B - Secret Shortcut", "3A - Elevator 2", 
									"4 - Pit Bridge", "5 - Window Hallway", "6 - Boss Entrance", "DoorGreed", "Exterior" })
		};
		
		private void Start()
		{
			_gc = NewMovement.Instance.gc;
			_nm = NewMovement.Instance;
			_prefab = AssetBundle.GetAllLoadedAssetBundles().First(p => p.name == "common")
															.LoadAsset<GameObject>("HotSand");
			StartCoroutine(nameof(DamagePlayer));
		}

		private static readonly FieldInfo f_gc_currentCol = typeof(GroundCheck)
			.GetField("currentCol", BindingFlags.NonPublic | BindingFlags.Instance); 
		
		private IEnumerator DamagePlayer()
		{
			while (true)
			{
				if (SceneManager.GetActiveScene().name == "Tutorial")
				{
					yield return null;
					continue;
				}
				var col = (f_gc_currentCol?.GetValue(_gc) as Collider)?.gameObject;
				if (_gc.onGround && !DamageBlacklist
					.Where(p => p.Item1 == null || p.Item1 == SceneManager.GetActiveScene().name)
					.SelectMany(p => p.Item2)
					.Any(p => IsParent(col, p)))
				{
					_nm.GetHurt(10, false);
					Instantiate(_prefab, transform.position, Quaternion.identity);
					yield return new WaitForSeconds(1f);
				}
				else
				{
					yield return null;
				}
			}
		}

		private void OnDestroy()
		{
			StopCoroutine(nameof(DamagePlayer));
		}

		private static bool IsParent(GameObject current, string name)
		{
			var parent = current;
			while (parent != null)	
			{
				if (parent.name.StartsWith(name))
				{
					return true;
				}

				parent = parent.transform.parent != null ? parent.transform.parent.gameObject : null;
			}

			return false;
		}
	}
}