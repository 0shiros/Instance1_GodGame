using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brush
{
    public class Shaper : MonoBehaviour
    {
        public static Shaper Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public List<Vector3Int> CircleShape(int _Size, Vector3Int _CenterShape)
        {
            List<Vector3Int> _cells = new List<Vector3Int>();


            int _limitSize = _Size * _Size;
            for (int _x = -_Size; _x <= _Size; _x++)
            {
                for (int _y = -_Size; _y <= _Size; _y++)
                {
                    if (_x * _x + _y * _y <= _limitSize)
                    {
                        _cells.Add(new Vector3Int(_CenterShape.x + _x, _CenterShape.y + _y, _CenterShape.z));
                    }
                }
            }

            return _cells;
        }

        public List<Vector3Int> SquareShape(int _Size, Vector3Int _CenterShape)
        {
            List<Vector3Int> _cells = new List<Vector3Int>();

            for (int _x = -_Size; _x <= _Size; _x++)
            {
                for (int _y = -_Size; _y <= _Size; _y++)
                {
                    _cells.Add(new Vector3Int(_CenterShape.x + _x, _CenterShape.y + _y, _CenterShape.z));
                }
            }

            return _cells;
        }
    }
}