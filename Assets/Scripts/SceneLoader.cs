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

        if(!SceneManager.GetSceneByName(_menuScene).isLoaded && !SceneManager.GetSceneByName(_raidScene).isLoaded)
        {
            LoadMenuOnInstance();
        }
    }

    public static void LoadMenu()
    {
        instance.LoadMenuOnInstance();
    }

    protected void LoadMenuOnInstance()
    {
        UniTask.Void(LoadMenuOnInstanceAsync);
    }

    async UniTaskVoid LoadMenuOnInstanceAsync()
    {
        AsyncOperation unloadRaid = null;
        AsyncOperation loadMenu = null;

        _backupCamera.gameObject.SetActive(true);

        LoadingScreenUIManager.StartLoadingScreen(() =>
        {
            if (loadMenu != null && unloadRaid == null)
            {
                return loadMenu.progress;
            }
            else
            {
                float progress = 0;
                if (loadMenu != null)
                    progress += loadMenu.progress * 0.5f;
                if (unloadRaid != null)
                    progress += unloadRaid.progress * 0.5f;
                return progress;
            }
        });

        if (SceneManager.GetSceneByName(_raidScene).isLoaded)
        {
            unloadRaid = SceneManager.UnloadSceneAsync(_raidScene);
            while (!unloadRaid.isDone)
            {
                await UniTask.Yield();
            }
        }

        loadMenu = SceneManager.LoadSceneAsync(_menuScene, LoadSceneMode.Additive);
        loadMenu.allowSceneActivation = false;
        while (loadMenu.progress < 0.9f)
        {
            await UniTask.Yield();
        }

        loadMenu.allowSceneActivation = true;

        while (!loadMenu.isDone)
        {
            await UniTask.Yield();
        }

        _backupCamera.gameObject.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_menuScene));

        Debug.Log("done");
    }

    public static void LoadRaid()
    {
        instance.LoadRaidOnInstance();
    }

    protected void LoadRaidOnInstance()
    {
        UniTask.Void(LoadRaidOnInstanceAsync);
    }

    async UniTaskVoid LoadRaidOnInstanceAsync()
    {
        AsyncOperation unloadMenu = null;
        AsyncOperation loadRaid = null;

        _backupCamera.gameObject.SetActive(true);

        LoadingScreenUIManager.StartLoadingScreen(() =>
        {
            if (loadRaid != null && unloadMenu == null)
            {
                return loadRaid.progress;
            }
            else
            {
                float progress = 0;
                if (loadRaid != null)
                    progress += loadRaid.progress * 0.5f;
                if (unloadMenu != null)
                    progress += unloadMenu.progress * 0.5f;
                return progress;
            }
        });

        if (SceneManager.GetSceneByName(_menuScene).isLoaded)
        {
            unloadMenu = SceneManager.UnloadSceneAsync(_menuScene);
            while (unloadMenu != null && !unloadMenu.isDone)
            {
                await UniTask.Yield();
            }
        }

        loadRaid = SceneManager.LoadSceneAsync(_raidScene, LoadSceneMode.Additive);
        loadRaid.allowSceneActivation = false;
        while (loadRaid.progress < 0.9f)
        {
            await UniTask.Yield();
        }

        loadRaid.allowSceneActivation = true;

        while (!loadRaid.isDone)
        {
            await UniTask.Yield();
        }

        _backupCamera.gameObject.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_raidScene));
        Debug.Log("raid scene loaded");
    }
}
