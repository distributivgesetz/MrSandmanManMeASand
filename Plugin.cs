using System.Collections;
using System.Linq;
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
		
		private void Start()
		{
			_gc = NewMovement.Instance.gc;
			_nm = NewMovement.Instance;
			_prefab = AssetBundle.GetAllLoadedAssetBundles().First(p => p.name == "common")
															.LoadAsset<GameObject>("HotSand");
			StartCoroutine(nameof(DamagePlayer));
		}

		private IEnumerator DamagePlayer()
		{
			while (true)
			{
				if (_gc.onGround)
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
	}
}