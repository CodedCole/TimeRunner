using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        /// <summary>
        /// Checks if the position 'pos' is within or on the bounds of the room.
        /// </summary>
        /// <param name="pos">the position</param>
        /// <returns>Whether the position is inside the room</returns>
        public bool Contains(Vector2Int pos)
        {
            return (pos.x >= bottomLeft.x && pos.x <= topRight.x) && (pos.y >= bottomLeft.y && pos.y <= topRight.y);
        }

        public bool Overlaps(Vector2Int BL, Vector2Int TR)
        {
            //overlap on the x axis
            if ((TR.y >= bottomLeft.y && TR.y <= topRight.y) || (BL.y >= bottomLeft.y && BL.y <= topRight.y))
            {
                //overlap on the y axis
                if ((TR.x >= bottomLeft.x && TR.x <= topRight.x) || (BL.x >= bottomLeft.x && BL.x <= topRight.x))
                {
                    return true;
                }
            }
            return false;
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

    private Grid _grid;
    private Tilemap _level;

    // Start is called before the first frame update
    void Start()
    {
        _grid = GetComponent<Grid>();
        _level = GetComponentInChildren<Tilemap>();
        _level.ClearAllTiles();
        _level.origin = Vector3Int.zero;
        _level.size = new Vector3Int(size.x + 2, size.y + 2, 2);
        _level.ResizeBounds();

        _level.BoxFill(Vector3Int.zero, tileToFill, 0, 0, size.x - 1, size.y - 1);
        for (int i = 0; i < size.x; i++)
        {
            _level.SetTileFlags(Vector3Int.right * i, TileFlags.None);
            _level.SetColor(Vector3Int.right * i, south);

            _level.SetTileFlags((Vector3Int.right * i) + (Vector3Int.up * (size.y - 1)), TileFlags.None);
            _level.SetColor((Vector3Int.right * i) + (Vector3Int.up * (size.y - 1)), north);
        }
        for (int i = 1; i < size.y - 1; i++)
        {
            _level.SetTileFlags(Vector3Int.up * i, TileFlags.None);
            _level.SetColor(Vector3Int.up * i, west);

            _level.SetTileFlags((Vector3Int.up * i) + (Vector3Int.right * (size.x - 1)), TileFlags.None);
            _level.SetColor((Vector3Int.up * i) + (Vector3Int.right * (size.x - 1)), east);
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
                            rooms[i].topRight += Vector2Int.up;
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
                            rooms[i].bottomLeft += Vector2Int.down;
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
                            rooms[i].bottomLeft += Vector2Int.left;
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
                        if (rooms[i].bottomLeft.x + 1 < size.x && !OverlapsWithOtherRooms(rooms[i].bottomLeft, rooms[i].topRight + Vector2Int.right, i))
                            rooms[i].topRight += Vector2Int.right;
                        else
                            rooms[i].canExpandRight = false;
                    }
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
            _level.BoxFill(new Vector3Int(rooms[i].bottomLeft.x + 2, rooms[i].bottomLeft.y + 2, -1), tileToFill, rooms[i].bottomLeft.x + 2, rooms[i].bottomLeft.y + 2 , rooms[i].topRight.x + 2, rooms[i].topRight.y + 2);
            for (int x = rooms[i].bottomLeft.x + 2; x <= rooms[i].topRight.x + 2; x++)
            {
                for (int y = rooms[i].bottomLeft.y + 2; y <= rooms[i].topRight.y + 2; y++)
                {
                    _level.SetTileFlags(Vector3Int.back + (Vector3Int.right * x) + (Vector3Int.up * y), TileFlags.None);
                    _level.SetColor(Vector3Int.back + (Vector3Int.right * x) + (Vector3Int.up * y), roomColor);
                }
            }
            _level.BoxFill(new Vector3Int(rooms[i].bottomLeft.x + 3, rooms[i].bottomLeft.y + 3, -1), null, rooms[i].bottomLeft.x + 3, rooms[i].bottomLeft.y + 3, rooms[i].topRight.x + 1, rooms[i].topRight.y + 1);
        }

        yield return null;
    }
}
