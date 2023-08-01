using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RaidEndedScreen : MonoBehaviour
{
    [SerializeField] private string _hiddenClass;
    [SerializeField] private string _successClass;
    [SerializeField] private string[] _successMessages;
    [SerializeField] private AudioClip[] _successStings;
    [SerializeField] private string _failClass;
    [SerializeField] private string[] _failMessages;
    [SerializeField] private AudioClip[] _failStings;
    [SerializeField] private float _stingDelay = 0.5f;

    Label _raidResultText;
    AudioClip _sting;

    public void SuccessReveal()
    {
        SetupEndScreen();

        _raidResultText.text = _successMessages[Random.Range(0, _successMessages.Length)];
        _raidResultText.AddToClassList(_successClass);
        _sting = _successStings[Random.Range(0, _successStings.Length)];
        Invoke("PlaySting", _stingDelay);
    }

    public void FailReveal()
    {
        SetupEndScreen();

        _raidResultText.text = _failMessages[Random.Range(0, _failMessages.Length)];
        _raidResultText.AddToClassList(_failClass);
        _sting = _failStings[Random.Range(0, _failStings.Length)];
        Invoke("PlaySting", _stingDelay);
    }

    void PlaySting()
    {
        GameObject.Find("GlobalAudioSource").GetComponent<AudioSource>().PlayOneShot(_sting);
    }

    void SetupEndScreen()
    {
        UIDocument document = GetComponent<UIDocument>();
        VisualElement endScreenRoot = document.rootVisualElement.Q<VisualElement>("RaidEndedScreen");
        endScreenRoot.RemoveFromClassList(_hiddenClass);

        VisualElement viewStatsButton = endScreenRoot.Q<VisualElement>("view-stats-button");
        VisualElement vcstat = endScreenRoot.Q<VisualElement>("value-collected-stat");

        vcstat.RegisterCallback((NavigationMoveEvent ev) => {
            switch (ev.direction)
            {
                case NavigationMoveEvent.Direction.Down:
                    viewStatsButton.Focus();
                    ev.PreventDefault();
                    break;
            }
        });

        viewStatsButton.RegisterCallback((NavigationMoveEvent ev) =>
        {
            if (ev.direction == NavigationMoveEvent.Direction.Up)
            {
                vcstat.Focus();
                ev.PreventDefault();
            }
        });

        Button returnToOutskirtsButton = endScreenRoot.Q<Button>("return-to-outskirts-button");
        returnToOutskirtsButton.clicked += SceneLoader.LoadMenu;

        _raidResultText = endScreenRoot.Q<Label>("raid-result-text");
    }
}
