using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Addressable_Test
{
    public class AddressableManager : MonoBehaviour
    {
        public bool dontDestroyOnLoad;
        public AssetReference _weaponRef;
        public AssetReference[] _arrAssetRefs;

        private AddressableManager _instance;

        public AddressableManager Instance
        {
            get
            {
                if (_instance == null)                                          // if _instance is null...
                {
                    _instance = FindObjectOfType<AddressableManager>();         // Find singleton in the scene.
                    if (_instance == null)                                      // Still null?
                    {
                        var newObj = new GameObject("AddressableManager");      // Create a new singleton object and reference to it.
                        _instance = newObj.AddComponent<AddressableManager>();  
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else Destroy(this);
            if (dontDestroyOnLoad) DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        [ContextMenu("LoadWeapon")]
        private void LoadWeapon()
        {
            _weaponRef.InstantiateAsync();
        }

        [ContextMenu("LoadAllWeapons")]
        public async Task LoadAllWeapons()
        {
            for (int i = 0; i < _arrAssetRefs.Length; i++)
            {
                var asset = _arrAssetRefs[i];
                var handle = Addressables.LoadAssetAsync<GameObject>(asset);
                await handle.Task; // The task is complete. Be sure to check the Status is successful before storing the Result.
                if (handle.IsDone)
                {
                }
            }
        }
    }
}