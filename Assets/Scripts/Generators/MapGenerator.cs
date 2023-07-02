using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using WaveFunctionCollapse;

public class MapGenerator : MonoBehaviour
{
    public struct Room
    {
        public Vector2Int topRight;
        public Vector2Int bottomLeft;
        public bool canExpandUp;
        public bool canExpandDown;
        public bool canExpandLeft;
        public bool canExpandRight;

        public Vector2Int RoomSize { get { return (topRight - bottomLeft) + Vector2Int.one; } }

        /// <summary>
        /// Checks if the position 'pos' is within or on the bounds of the room.
        /// </summary>
        /// <param name="pos">the position</param>
        /// <returns>Whether the position is inside the room</returns>
        public bool Contains(Vector2Int pos)
        {
            return (pos.x >= bottomLeft.x && pos.x <= topRight.x) && (pos.y >= bottomLeft.y && pos.y <= topRight.y);
        }

        /// <summary>
        /// Checks if the room overlaps the rectangle defined by BL and TR.
        /// </summary>
        /// <param name="BL">The bottom left of the compared rectangle</param>
        /// <param name="TR">The top right of the compared rectangle</param>
        /// <returns>Whether the rectangle overlaps the room</returns>
        public bool Overlaps(Vector2Int BL, Vector2Int TR)
        {
            //overlap on the x axis
            if ((TR.y >= bottomLeft.y && TR.y <= topRight.y) || (BL.y >= bottomLeft.y && BL.y <= topRight.y) 
                || (bottomLeft.y >= BL.y && bottomLeft.y <= TR.y) || (topRight.y >= BL.y && topRight.y <= TR.y))
            {
                //overlap on the y axis
                if ((TR.x >= bottomLeft.x && TR.x <= topRight.x) || (BL.x >= bottomLeft.x && BL.x <= topRight.x)
                    || (bottomLeft.x >= BL.x && bottomLeft.x <= TR.x) || (topRight.x >= BL.x && topRight.x <= TR.x))
                {
                    return true;
                }
            }
            return false;
        }

        public Vector3Int[] AllPositionsWithin()
        {
            Vector3Int[] result = new Vector3Int[RoomSize.x * RoomSize.y];
            for (int y = 0; y < RoomSize.y; y++)
            {
                for (int x = 0; x < RoomSize.x; x++)
                {
                    result[x + (y * RoomSize.x)] = (Vector3Int)bottomLeft + new Vector3Int(x, y);
                }
            }
            return result;
        }

        public Vector3Int[] Border()
        {
            const int MIN = -1;
            const int MAX = 0;

            Vector3Int[] result = new Vector3Int[(RoomSize.x + RoomSize.y + ((Mathf.Abs(MIN) + Mathf.Abs(MAX)) * 2)) * 2];
            int index = 0;

            for (int x = MIN; x <= RoomSize.x + MAX; x++)
            {
                for (int y = MIN; y <= RoomSize.y + MAX; y++)
                {
                    if (y == MIN || y == RoomSize.y + MAX || x == MIN || x == RoomSize.x + MAX)
                    {
                        result[index] = (Vector3Int)bottomLeft + new Vector3Int(x, y);
                        index++;
                    }
                }
            }
            return result;
        }
    }

    [SerializeField] private Color north = Color.red;
    [SerializeField] private Color south = Color.blue;
    [SerializeField] private Color east = Color.green;
    [SerializeField] private Color west = Color.yellow;
    [SerializeField] private Tile tileToFill;
    [SerializeField] private Vector2Int size;

    [Header("Rooms")]
    [SerializeField] private int _maxSize = 10;
    [SerializeField] private int _count = 4;
    [SerializeField] private Gradient _roomColors;
    [SerializeField] private int _tryCount = 10;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _sample;
    [SerializeField] private Tilemap _level;
    [SerializeField] private WFCTemplate _template;
    private Grid _grid;

    [Header("WFC")]
    [SerializeField] private Vector2Int _startPos;
    [SerializeField] private Vector2Int _size;
    [SerializeField] private bool _debug;

    [Header("Generation")]
    [SerializeField] private bool reseed;
    [SerializeField] private int seed;

    // Start is called before the first frame update
    void Start()
    {
        if (reseed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log("seed: " + seed);
        }
        Random.InitState(seed);

        _level.origin = Vector3Int.zero;
        _level.size = (Vector3Int)size + Vector3Int.one;
        _level.ResizeBounds();
        _sample.gameObject.SetActive(false);
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                _level.SetTile(pos, tileToFill);
                _level.SetTileFlags(pos, TileFlags.None);
                _level.SetColor(pos, Color.grey);
            }
        }
        StartCoroutine(BuildRooms());
    }

    IEnumerator BuildRooms()
    {
        //make rooms
        Room[] rooms = new Room[_count];
        for (int i = 0; i < _count; i++)
        {
            //only try so many times
            for (int tryNum = 0; tryNum < _tryCount; tryNum++)
            {
                //new room start position
                Vector2Int origin = new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));

                //check if room already exists
                bool canPlaceRoom = true;
                for (int j = 0; j < i; j++)
                {
                    if (rooms[j].topRight == origin)
                    {
                        canPlaceRoom = false;
                        break;
                    }
                }

                //if room doesn't already exist, create it
                if (canPlaceRoom)
                {
                    rooms[i].bottomLeft = origin;
                    rooms[i].topRight = origin;
                    rooms[i].canExpandUp = true;
                    rooms[i].canExpandDown = true;
                    rooms[i].canExpandLeft = true;
                    rooms[i].canExpandRight = true;
                    break;
                }
            }
        }

        //expand rooms
        for(int expandCount = 0; expandCount < _maxSize; expandCount++)
        {
            //do one expand iteration of each room
            for (int i = 0; i < _count; i++)
            {
                bool changed = false;
                //try expanding up
                if (rooms[i].canExpandUp)
                {
                    //check room size limit
                    if ((rooms[i].topRight.y - rooms[i].bottomLeft.y) + 1 > _maxSize)
                    {
                        rooms[i].canExpandUp = false;
                        rooms[i].canExpandDown = false;
                    }
                    else
                    {
                        //check border and room overlap
                        if (rooms[i].topRight.y + 1 < size.y && !OverlapsWithOtherRooms(rooms[i].bottomLeft, rooms[i].topRight + Vector2Int.up, i))
                        {
                            rooms[i].topRight += Vector2Int.up;
                            changed = true;
                        }
                        else
                            rooms[i].canExpandUp = false;
                    }
                }

                //try expanding down
                if (rooms[i].canExpandDown)
                {
                    //check room size limit
                    if ((rooms[i].topRight.y - rooms[i].bottomLeft.y) + 1 > _maxSize)
                    {
                        rooms[i].canExpandUp = false;
                        rooms[i].canExpandDown = false;
                    }
                    else
                    {
                        //check border and room overlap
                        if (rooms[i].bottomLeft.y - 1 >= 0 && !OverlapsWithOtherRooms(rooms[i].bottomLeft + Vector2Int.down, rooms[i].topRight, i))
                        {
                            rooms[i].bottomLeft += Vector2Int.down;
                            changed = true;
                        }
                        else
                            rooms[i].canExpandDown = false;
                    }
                }

                //try expanding left
                if (rooms[i].canExpandLeft)
                {
                    //check room size limit
                    if ((rooms[i].topRight.x - rooms[i].bottomLeft.x) + 1 > _maxSize)
                    {
                        rooms[i].canExpandLeft = false;
                        rooms[i].canExpandRight = false;
                    }
                    else
                    {
                        //check border and room overlap
                        if (rooms[i].bottomLeft.x - 1 >= 0 && !OverlapsWithOtherRooms(rooms[i].bottomLeft + Vector2Int.left, rooms[i].topRight, i))
                        {
                            rooms[i].bottomLeft += Vector2Int.left;
                            changed = true;
                        }
                        else
                            rooms[i].canExpandLeft = false;
                    }
                }

                //try expanding right
                if (rooms[i].canExpandRight)
                {
                    //check room size limit
                    if ((rooms[i].topRight.x - rooms[i].bottomLeft.x) + 1 > _maxSize)
                    {
                        rooms[i].canExpandLeft = false;
                        rooms[i].canExpandRight = false;
                    }
                    else
                    {
                        //check border and room overlap
                        if (rooms[i].topRight.x + 1 < size.x && !OverlapsWithOtherRooms(rooms[i].bottomLeft, rooms[i].topRight + Vector2Int.right, i))
                        {
                            rooms[i].topRight += Vector2Int.right;
                            changed = true;
                        }
                        else
                            rooms[i].canExpandRight = false;
                    }
                }

                if (_debug && changed)
                {
                    BuildRooms(rooms);
                    yield return null;
                }
            }
        }

        //local function for room expanding
        bool OverlapsWithOtherRooms(Vector2Int BL, Vector2Int TR, int room)
        {
            for (int i = 0; i < _count; i++)
            {
                if (i != room && rooms[i].Overlaps(BL, TR))
                    return true;
            }
            return false;
        }

        //place on tilemap
        for (int i = 0; i < _count; i++)
        {
            Color roomColor = _roomColors.Evaluate(((float)i)/_count);
            _level.BoxFill(new Vector3Int(rooms[i].bottomLeft.x, rooms[i].bottomLeft.y), tileToFill, rooms[i].bottomLeft.x, rooms[i].bottomLeft.y, rooms[i].topRight.x, rooms[i].topRight.y);
            if ((rooms[i].topRight - rooms[i].bottomLeft).x > 1 && (rooms[i].topRight - rooms[i].bottomLeft).y > 1)
            {
                TileWFC roomWFC;
                if (_template != null)
                {
                    //_template.Log();
                    roomWFC = new TileWFC(_level, _template, rooms[i].AllPositionsWithin(), _debug);
                    Dictionary<Vector3Int, HashSet<int>> tileRestrictions = new Dictionary<Vector3Int, HashSet<int>>();
                    foreach (var b in rooms[i].Border())
                    {
                        HashSet<int> restriction = new HashSet<int>{ 0 };
                        tileRestrictions.Add(b, restriction);
                    }
                    //tileRestrictions.Add((Vector3Int)rooms[i].bottomLeft + new Vector3Int(2, 2), new HashSet<int> { 1, 6 });
                    roomWFC.SetTileRestrictions(tileRestrictions);
                }
                else
                    roomWFC = new TileWFC(_sample, _level, rooms[i].bottomLeft + Vector2Int.one, (rooms[i].topRight - rooms[i].bottomLeft) - Vector2Int.one, _debug);
                yield return StartCoroutine(roomWFC.GenerateCoroutine(this));
            }

            //recolor room
            for (int x = rooms[i].bottomLeft.x; x <= rooms[i].topRight.x; x++)
            {
                for (int y = rooms[i].bottomLeft.y; y <= rooms[i].topRight.y; y++)
                {
                    _level.SetTileFlags((Vector3Int.right * x) + (Vector3Int.up * y), TileFlags.None);
                    _level.SetColor((Vector3Int.right * x) + (Vector3Int.up * y), roomColor);
                }
            }
        }

        yield return null;
    }

    void BuildRooms(Room[] rooms)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            Vector2Int roomSize = (rooms[i].topRight - rooms[i].bottomLeft) + Vector2Int.one;
            Color roomColor = _roomColors.Evaluate(((float)i) / _count);
            for (int x = 0; x < roomSize.x; x++)
            {
                for (int y = 0; y < roomSize.y; y++)
                {
                    Vector3Int pos = (Vector3Int)rooms[i].bottomLeft + new Vector3Int(x, y);
                    _level.SetTile(pos, tileToFill);
                    _level.SetTileFlags(pos, TileFlags.None);
                    _level.SetColor(pos, roomColor);
                }
            }
        }
    }
}
