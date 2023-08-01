using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RaidManager : MonoBehaviour
{
    [Serializable]
    public struct Difficulty
    {
        public string name;
        public float duration;
    }

    [Serializable]
    public struct Level
    {
        public string name;
        public LevelLayout[] levelLayouts;
    }

    public struct BuiltLevel
    {
        public string name;
        public LevelLayout layout;
        public GameObject parent;
        public ZoneGenerator zoneGenerator;
        public Tilemap tilemap;
    }

    [SerializeField] private Difficulty[] _difficultyProgression;
    [SerializeField] private Level[] _levels;
    [SerializeField] private Grid _tilemapGrid;
    [SerializeField] private int _tilemapLayer;
    [SerializeField] private Gradient _zoneColors;
    [SerializeField] private TileBase _tile;
    [SerializeField] private TileBase _wall;
    [SerializeField] private bool _debug;
    private List<BuiltLevel> _builtLevels; 
    private int _currentLevel;

    private Action onRaidBegin;
    public void RegisterOnRaidBegin(Action action) { onRaidBegin += action; }
    public void UnregisterOnRaidBegin(Action action) { onRaidBegin -= action; }

    private Action onRaidEnd;
    public void RegisterOnRaidEnd(Action action) { onRaidEnd += action; }
    public void UnregisterOnRaidEnd(Action action) { onRaidEnd -= action; }

    private Action<int> onLevelLeft;
    private Action<int> onLevelEntered;

    private void Awake()
    {
        _builtLevels = new List<BuiltLevel>();
        _currentLevel = 0;
        StartCoroutine(BeginRaid());
    }

    IEnumerator BuildLevel(int levelIndex)
    {
        BuiltLevel bl = new BuiltLevel();
        bl.name = _levels[levelIndex].name;
        bl.layout = _levels[levelIndex].levelLayouts[UnityEngine.Random.Range(0, _levels[levelIndex].levelLayouts.Length)];
        bl.parent = new GameObject(bl.name);
        bl.zoneGenerator = bl.parent.AddComponent<ZoneGenerator>();
        bl.zoneGenerator._zoneColors = _zoneColors;
        bl.zoneGenerator._tile = _tile;
        bl.zoneGenerator._wall = _wall;
        bl.tilemap = new GameObject(bl.name + "_tilemap").AddComponent<Tilemap>();
        bl.tilemap.gameObject.layer = _tilemapLayer;
        bl.tilemap.transform.SetParent(_tilemapGrid.transform);
        bl.tilemap.AddComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;
        bl.tilemap.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        bl.tilemap.AddComponent<CompositeCollider2D>();
        bl.tilemap.AddComponent<TilemapCollider2D>().usedByComposite = true;
        yield return StartCoroutine(bl.zoneGenerator.Generate(bl.layout, bl.tilemap, _debug));
        _builtLevels.Add(bl);
    }

    IEnumerator BeginRaid()
    {
        yield return StartCoroutine(BuildLevel(0));

        FindObjectOfType<Player>().GetComponent<Health>().RegisterOnDeath(EndRaid);

        if (onRaidBegin != null)
            onRaidBegin();
        if (onLevelEntered != null)
            onLevelEntered(0);
    }

    public void EndRaid()
    {
        FindObjectOfType<RaidEndedScreen>().FailReveal();

        if (onRaidEnd != null)
            onRaidEnd();
    }

    public ZoneGenerator GetActiveLevel()
    {
        return _builtLevels[_currentLevel].zoneGenerator;
    }
}
