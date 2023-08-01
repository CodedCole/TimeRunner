using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIManagers
{
    public class LoadingScreenUIManager : MonoBehaviour
    {
        protected static LoadingScreenUIManager instance;

        [SerializeField] private string _hiddenClass;
        [SerializeField] private float _disableDocumentDelay;

        private UIDocument _loadingScreenDocument;
        private VisualElement _loadingScreenRoot;
        private Label _loadingStatusLabel;
        private ProgressBar _loadingScreenProgress;

        private Func<float> _progressFunc;
        private bool _loadingScreenInProgress;

        private void Awake()
        {
            if (instance != null)
                Destroy(instance);
            instance = this;

            _loadingScreenDocument = GetComponent<UIDocument>();
            _loadingScreenRoot = _loadingScreenDocument.rootVisualElement.Q<VisualElement>("background", "loading-screen-background");

            _loadingScreenInProgress = false;
            _loadingScreenRoot.AddToClassList(_hiddenClass);
            _loadingScreenDocument.enabled = false;
        }

        public static bool StartLoadingScreen(Func<float> loadProgress)
        {
            return instance.StartLoadingScreenOnInstance(loadProgress);
        }

        protected bool StartLoadingScreenOnInstance(Func<float> loadProgress)
        {
            if (_loadingScreenInProgress)
                return false;
            
            _progressFunc = loadProgress;
            UniTask.Void(RunLoadingScreen);

            return true;
        }

        public static void SetLoadingScreenLoadStatus(string status)
        {
            instance.SetLoadingScreenLoadStatusOnInstance(status);
        }

        protected void SetLoadingScreenLoadStatusOnInstance(string status)
        {
            _loadingStatusLabel.text = status;
        }

        async UniTaskVoid RunLoadingScreen()
        {
            //show loading screen
            _loadingScreenDocument.enabled = true;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            //re-initialize from UIDocument disabling
            _loadingScreenRoot = _loadingScreenDocument.rootVisualElement.Q<VisualElement>("background", "loading-screen-background");
            _loadingStatusLabel = _loadingScreenRoot.Q<Label>("load-status-text");
            _loadingScreenProgress = _loadingScreenRoot.Q<ProgressBar>("load-progress-bar");

            _loadingScreenRoot.RemoveFromClassList(_hiddenClass);

            _loadingScreenInProgress = true;

            //update progress bar
            float progress = _progressFunc();
            while (progress < 1)
            {
                progress = _progressFunc();
                _loadingScreenProgress.value = progress;
                _loadingStatusLabel.text = "Hello";
                await UniTask.Yield();
            }

            _loadingScreenInProgress = false;

            //hide loading screen
            _loadingScreenRoot.AddToClassList(_hiddenClass);
            await UniTask.Delay(Mathf.CeilToInt(_disableDocumentDelay * 1000));
            _loadingScreenDocument.enabled = false;
        }
    }
}
