using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class AddressablesSceneLoader : MonoBehaviour
{
    [SerializeField] private AssetReference sceneReference = null;

    private SceneInstance _scene;

    IEnumerator Start()
    {
        yield return Addressables.InitializeAsync();
        //yield return new WaitForSeconds(1);
        var asyncOperation = sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
        yield return asyncOperation;
        _scene = asyncOperation.Result;
        yield return _scene;
        //yield return new WaitForSeconds(3);
        //yield return Addressables.UnloadSceneAsync(_scene);
    }
}