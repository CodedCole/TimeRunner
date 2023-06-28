using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public class Pattern
    {
        private int[] _tiles;                   //tiles used in the pattern from bottom left to top right
        private int _size;                      //width and height of the pattern in tiles
        private HashSet<Pattern>[] _neighbors;  //patterns that overlap this pattern in each of the four cardinal directions

        public Pattern(int[] tiles, int size)
        {
            _tiles = tiles;
            _size = size;
            _neighbors = new HashSet<Pattern>[4];
        }

        public HashSet<Pattern> GetNeighborsInDirection(EDirection dir) { return _neighbors[(int)dir]; }


    }

    public class PatternWFC
    {

    }
}
