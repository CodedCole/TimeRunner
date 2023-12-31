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
        public Item keyToNextLevel;
        public string crateTypeForKey;
    }

    public struct BuiltLevel
    {
        public string name;
        public LevelLayout layout;
        public GameObject parent;
        public ZoneGenerator zoneGenerator;
        public Tilemap tilemap;
        public Item keyToNextLevel;
        public string crateTypeForKey;
    }

    [SerializeField] private Difficulty[] _difficultyProgression;
    [SerializeField] private Level[] _levels;
    [SerializeField] private float _switchLevelDuration = 2.0f;
    [SerializeField] private Grid _tilemapGrid;
    [SerializeField] private int _tilemapLayer;
    [SerializeField] private Gradient _zoneColors;
    [SerializeField] private TileBase _tile;
    [SerializeField] private TileBase _wall;
    [SerializeField] private ZoneData _defaultZone;
    [SerializeField] private int _floorDepth = 1;
    [SerializeField] private bool _debug;
    private List<BuiltLevel> _builtLevels; 
    private int _currentLevel;

    private Action onRaidBegin;
    public void RegisterOnRaidBegin(Action action) { onRaidBegin += action; }
    public void UnregisterOnRaidBegin(Action action) { onRaidBegin -= action; }

    private Action onRaidEnd;
    public void RegisterOnRaidEnd(Action action) { onRaidEnd += action; }
    public void UnregisterOnRaidEnd(Action action) { onRaidEnd -= action; }

    public event Action<int> onLevelLeft;
    public event Action<int> onLevelEntered;

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
        bl.keyToNextLevel = _levels[levelIndex].keyToNextLevel;
        bl.crateTypeForKey= _levels[levelIndex].crateTypeForKey;
        bl.parent = new GameObject(bl.name);
        bl.zoneGenerator = bl.parent.AddComponent<ZoneGenerator>();
        bl.zoneGenerator._zoneColors = _zoneColors;
        bl.zoneGenerator._tile = _tile;
        bl.zoneGenerator._wall = _wall;
        bl.zoneGenerator._defaultZone = _defaultZone;
        bl.zoneGenerator._floorDepth = _floorDepth;
        bl.tilemap = new GameObject(bl.name + "_tilemap").AddComponent<Tilemap>();
        bl.tilemap.gameObject.layer = _tilemapLayer;
        bl.tilemap.transform.SetParent(_tilemapGrid.transform);
        bl.tilemap.AddComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;
        bl.tilemap.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        bl.tilemap.AddComponent<CompositeCollider2D>();
        bl.tilemap.AddComponent<TilemapCollider2D>().usedByComposite = true;
        //center the tower at (0,0)
        bl.tilemap.transform.position = Vector3.down * _tilemapGrid.cellSize.y * bl.layout.map.Size().y * 0.5f;

        //hide until ready
        bl.tilemap.gameObject.SetActive(false);
        yield return StartCoroutine(bl.zoneGenerator.Generate(bl.layout, bl.tilemap, _debug));

        new TileReplacer(bl.tilemap, bl.layout.tileReplaceLibrary, bl.parent.transform).ReplaceTiles(bl.tilemap.cellBounds.min, bl.tilemap.cellBounds.max);

        if (bl.keyToNextLevel != null && bl.crateTypeForKey != string.Empty)
            LootManager.TryPutItemInCrate(bl.keyToNextLevel, bl.crateTypeForKey);

        _builtLevels.Add(bl);
    }

    IEnumerator BeginRaid()
    {
        yield return StartCoroutine(BuildLevel(0));

        _builtLevels[0].tilemap.gameObject.SetActive(true);

        FindObjectOfType<Player>().GetComponent<Health>().RegisterOnDeath(() => EndRaid(false));

        if (onRaidBegin != null)
            onRaidBegin();
        if (onLevelEntered != null)
            onLevelEntered(0);
    }

    public void EndRaid(bool playerReturned)
    {
        if (playerReturned)
            FindObjectOfType<RaidEndedScreen>().SuccessReveal();
        else
            FindObjectOfType<RaidEndedScreen>().FailReveal();

        if (onRaidEnd != null)
            onRaidEnd();
    }

    public BuiltLevel? GetActiveLevel()
    {
        if (_builtLevels.Count == 0)
            return null;
        return _builtLevels[_currentLevel];
    }

    public Item GetKeyToNextLevel()
    {
        if (_builtLevels.Count == 0)
            return null;
        return _builtLevels[_currentLevel].keyToNextLevel;

    }

    public bool MoveToLevel(int targetLevel)
    {
        if (targetLevel >= _levels.Length || targetLevel < 0 || _builtLevels.Count < targetLevel || targetLevel == _currentLevel)
            return false;

        StartCoroutine(MoveToLevelAsync(targetLevel));

        return true;
    }

    IEnumerator MoveToLevelAsync(int targetLevel)
    {
        if (onLevelLeft != null)
            onLevelLeft(_currentLevel);

        _builtLevels[_currentLevel].parent.SetActive(false);
        _builtLevels[_currentLevel].tilemap.gameObject.SetActive(false);

        //check if a new level needs to be made
        if (_builtLevels.Count <= targetLevel)
        {
            yield return StartCoroutine(BuildLevel(targetLevel));
        }

        yield return new WaitForSeconds(_switchLevelDuration);

        _builtLevels[targetLevel].parent.SetActive(true);
        _builtLevels[targetLevel].tilemap.gameObject.SetActive(true);

        _currentLevel = targetLevel;

        if (onLevelEntered != null)
            onLevelEntered(targetLevel);
    }
}
