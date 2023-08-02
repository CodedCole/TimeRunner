using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIManagers;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

public class SceneLoader : MonoBehaviour
{
    protected static SceneLoader instance;

    [SerializeField] private string _menuScene;
    [SerializeField] private string _raidScene;
    [SerializeField] private Camera _backupCamera;

    private void Awake()
    {
        if (instance != this)
            Destroy(instance);
        instance = this;

        if(SceneManager.sceneCount < 2)
        {
            LoadMenuOnInstance();
        }
    }

    //External facing function for loading the menu
    public static void LoadMenu() { instance.LoadMenuOnInstance(); }
    protected void LoadMenuOnInstance() { UniTask.Void(LoadMenuOnInstanceAsync); }

    async UniTaskVoid LoadMenuOnInstanceAsync()
    {
        UniTask loadMenu = UnloadAndLoadScenesAsync(_raidScene, _menuScene);
        await loadMenu;
    }

    //External facing function for loading the raid
    public static void LoadRaid() { instance.LoadRaidOnInstance(); }
    protected void LoadRaidOnInstance() { UniTask.Void(LoadRaidOnInstanceAsync); }

    async UniTaskVoid LoadRaidOnInstanceAsync()
    {
        UniTask loadRaid = UnloadAndLoadScenesAsync(_menuScene, _raidScene);
        await loadRaid;
    }

    /// <summary>
    /// Async operation for unloading one scene and loading another. The loaded scene is set as the active scene
    /// </summary>
    /// <param name="sceneToUnload">asset path of the scene to unload</param>
    /// <param name="sceneToLoad">asset path of the scene to load</param>
    /// <returns></returns>
    async UniTask UnloadAndLoadScenesAsync(string sceneToUnload, string sceneToLoad)
    {
        AsyncOperation unload = null;
        AsyncOperation load = null;

        //enable loading screen and loading screen camera
        _backupCamera.gameObject.SetActive(true);
        LoadingScreenUIManager.StartLoadingScreen(() =>
        {
            if (load != null && unload == null)
            {
                return load.progress;
            }
            else
            {
                float progress = 0;
                if (load != null)
                    progress += load.progress * 0.5f;
                if (unload != null)
                    progress += unload.progress * 0.5f;
                return progress;
            }
        });

        //unload scene if it is loaded
        if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
        {
            unload = SceneManager.UnloadSceneAsync(sceneToUnload);
            while (unload != null && !unload.isDone)
            {
                await UniTask.Yield();
            }
        }

        //load scene
        load = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        load.allowSceneActivation = false;
        while (load.progress < 0.9f)
        {
            await UniTask.Yield();
        }

        load.allowSceneActivation = true;

        while (!load.isDone)
        {
            await UniTask.Yield();
        }

        //set as active and disable loading screen camera
        _backupCamera.gameObject.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
    }
}
