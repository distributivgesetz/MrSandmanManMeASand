using System.Collections;
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
			
			_isEnabled = GUI.Toggle(new Rect(10, Screen.height - 30, 200, 20), _isEnabled, "MrSandman enabled");
		}
	}

	[DefaultExecutionOrder(10000)]
	internal class FloorHurt : MonoBehaviour
	{
		private GroundCheck _gc;
		private NewMovement _nm;
		private GameObject _prefab;

		private static readonly string[] DamageBlacklist =
		{
			// Common
			"FirstRoom", "FinalRoom", "Secret FirstRoom", "FinalRoom 2", "Prime FirstRoom", "Prime FinalRoom",
			// 2-4
			"0 - Tram", 
			// 3-2
			"1 - Opening", "2 - Thin Walkway", "3 - Other Room", "3B - Drop",
			// 4-4
			"1 - Underground", "2 - Elevator 1", "3 - Ground Floor", "3B - Secret Shortcut", "3A - Elevator 2", "4 - Pit Bridge",
			"5 - Window Hallway", "6 - Boss Entrance"
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
				var col = (f_gc_currentCol?.GetValue(_gc) as Collider)?.gameObject;
				if (_gc.onGround && DamageBlacklist.All(p => !IsParent(col, p)))
				{
					if (SceneManager.GetActiveScene().name == "Level 3-2" && IsParent(col, "DoorMouth") ||
					    SceneManager.GetActiveScene().name == "Level 4-4" && (IsParent(col, "DoorGreed") || IsParent(col, "Exterior")))
					{
						yield return null;
						continue;
					}
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