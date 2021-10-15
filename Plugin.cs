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
			_ply = NewMovement.Instance.gameObject;
			if (!_ply)
				return;
			if (_ply.GetComponent<FloorHurt>() == null)
			{
				if (_isEnabled)
				{
					_ply.AddComponent<FloorHurt>();
				}
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
		private bool damaging;
		
		private void Start()
		{
			_gc = NewMovement.Instance.gc;
			_nm = NewMovement.Instance;
			_prefab = AssetBundle.GetAllLoadedAssetBundles().First(p => p.name == "common")
															.LoadAsset<GameObject>("HotSand");
		}

		private void Update()
		{
			if (_gc.onGround && !damaging)
			{
				damaging = true;
				StartCoroutine(nameof(DamagePlayer));
			}
			else if(!_gc.onGround && damaging)
			{
				damaging = false;
				StopCoroutine(nameof(DamagePlayer));
			}
		}

		private IEnumerator DamagePlayer()
		{
			while (true)
			{
				_nm.GetHurt(10, false);
				Instantiate(_prefab, _nm.gameObject.transform.position, Quaternion.identity);
				yield return new WaitForSeconds(1f);
			}
		}
	}
}