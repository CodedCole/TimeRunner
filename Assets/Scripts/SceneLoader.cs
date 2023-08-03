using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIManagers;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using System;

public class SceneLoader : MonoBehaviour
{
    protected static SceneLoader instance;

    [SerializeField] private string _menuScene;
    [SerializeField] private string _raidScene;

    private void Awake()
    {
        if (instance != this)
            Destroy(instance);
        instance = this;

        //Load Main Menu if the persistent is the only scene loaded (aka scene initially loaded when the build is played)
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
        float unloadLoadProgress = 0;
        LoadingScreenUIManager.StartLoadingScreen(() => unloadLoadProgress);
        LoadingScreenUIManager.SetLoadingScreenLoadStatus("Loading Main Menu");
        UniTask loadMenu = UnloadAndLoadScenesAsync(_raidScene, _menuScene, (x) => unloadLoadProgress = x);
        await loadMenu;
        LoadingScreenUIManager.CancelLoadingScreen();
    }

    //External facing function for loading the raid
    public static void LoadRaid() { instance.LoadRaidOnInstance(); }
    protected void LoadRaidOnInstance() { UniTask.Void(LoadRaidOnInstanceAsync); }

    async UniTaskVoid LoadRaidOnInstanceAsync()
    {
        float unloadLoadProgress = 0;
        float raidSetupProgress = 0;
        LoadingScreenUIManager.StartLoadingScreen(() =>
        {
            return (unloadLoadProgress * 0.5f) + (raidSetupProgress * 0.5f);
        });

        LoadingScreenUIManager.SetLoadingScreenLoadStatus("Loading Raid");
        UniTask loadRaid = UnloadAndLoadScenesAsync(_menuScene, _raidScene, (x) => unloadLoadProgress = x);
        await loadRaid;

        LoadingScreenUIManager.SetLoadingScreenLoadStatus("Generating The Lowest Level");
        RaidManager raidManager = FindObjectOfType<RaidManager>();
        raidManager.RegisterOnRaidBegin(() => raidSetupProgress = 1);
        await UniTask.WaitUntil(() => raidSetupProgress == 1);
        LoadingScreenUIManager.CancelLoadingScreen();
    }

    /// <summary>
    /// Async operation for unloading one scene and loading another. The loaded scene is set as the active scene
    /// </summary>
    /// <param name="sceneToUnload">asset path of the scene to unload</param>
    /// <param name="sceneToLoad">asset path of the scene to load</param>
    /// <returns></returns>
    async UniTask UnloadAndLoadScenesAsync(string sceneToUnload, string sceneToLoad, Action<float> progress)
    {
        //unload scene if it is loaded
        AsyncOperation unload = null;
        if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
        {
            unload = SceneManager.UnloadSceneAsync(sceneToUnload);
            while (unload != null && !unload.isDone)
            {
                progress.Invoke(unload.progress * 0.5f);
                await UniTask.Yield();
            }
        }

        await UniTask.Delay(1000);

        //load scene
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        load.allowSceneActivation = false;
        while (load.progress < 0.9f)
        {
            if (unload != null)
                progress.Invoke(0.5f + (load.progress * 0.5f));
            else
                progress.Invoke(load.progress);
            await UniTask.Yield();
        }

        await UniTask.Delay(1000);

        load.allowSceneActivation = true;

        while (!load.isDone)
        {
            if (unload != null)
                progress.Invoke(0.5f + (load.progress * 0.5f));
            else
                progress.Invoke(load.progress);
            await UniTask.Yield();
        }

        //declare loading completed and set the loaded scene as the active scene
        progress.Invoke(1);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
    }
}
