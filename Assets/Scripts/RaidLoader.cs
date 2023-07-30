using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaidLoader : MonoBehaviour
{
    [SerializeField] private string _raidScene;
    public event Action onRaidLoaded = () => { };

    private AsyncOperation sceneLoadOp;

    public void LoadRaid()
    {
        Debug.Log("Loading Raid");

        sceneLoadOp = SceneManager.LoadSceneAsync(_raidScene, LoadSceneMode.Additive);
        sceneLoadOp.allowSceneActivation = false;
        sceneLoadOp.completed += RaidLoaded;
    }

    void RaidLoaded(AsyncOperation ao)
    {
        Debug.Log("Raid Loaded");
        sceneLoadOp.allowSceneActivation = true;
        onRaidLoaded();
    }

    /// <summary>
    /// Returns the active progress of the raid load operation. Returns 0 if no raid is being loaded
    /// </summary>
    public float GetLoadProgress()
    {
        if (sceneLoadOp != null)
            return sceneLoadOp.progress;
        else
            return 0.0f;
    }
}
