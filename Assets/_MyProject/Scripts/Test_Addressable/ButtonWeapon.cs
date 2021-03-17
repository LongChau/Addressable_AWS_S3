using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Addressable_Test
{
    public class ButtonWeapon : MonoBehaviour
    {
        public AssetReference weaponRef;
        public Transform location;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void CreateWeapon()
        {
            weaponRef.InstantiateAsync(location);
        }
    }
}
