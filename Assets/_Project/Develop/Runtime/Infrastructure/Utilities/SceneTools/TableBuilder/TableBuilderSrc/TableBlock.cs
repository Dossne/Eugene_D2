using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure.Utilities
{
    public class TableBlock : MonoBehaviour
    {
        
        public enum Wall
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3,
        }

        public enum Corner
        {
            TopLeft = 0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3,
        }

        public enum Orientation
        {
            Forward = 0,
            Back    = 1,
            Left    = 2,
            Right   = 3,
        }

        [Serializable]
        public class WallElementSettings
        {
            public bool enabled                          = true;
            public bool auto                             = true;
            public bool ignoreClockwiseCornerDisable     = false;
            public bool ignoreAntiClockwiseCornerDisable = false;
            public TableBorder wall;
        }

        [Serializable]
        public class CornerElementSettings
        {
            public bool enabled = true;
            public bool auto    = true;
            public TableCorner corner;
        }

        [SerializeField][Min(0.5f)] private float width = 1;
        [SerializeField][Min(0.5f)] private float height = 1;
        [SerializeField] private bool isAutoUpdateOn = true;
        [SerializeField] private bool isExcludedFromGameplay = false;

        [Space]
        [Space]
        [SerializeField] private TablePrefabs prefabs;
        [SerializeField] private Transform wallParent;
        [SerializeField] private Transform cornerParent;

        [Space]
        [SerializeField] private TablePlane plane;
        [SerializeField] public SerializedDictionary<Wall, WallElementSettings> walls = new();
        [SerializeField] public SerializedDictionary<Corner, CornerElementSettings> corners = new();        

        
        private bool isClear = false;

        private void Start()
        {
#if !UNITY_EDITOR
            enabled = false;
#endif
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!isAutoUpdateOn || isClear)
                return;

            DelayedUpdate();
#endif
        }

        public void SetSize(float width, float heigth) 
        {
            this.width = width;
            this.height = heigth;
            OnValidate();
        }

        public void Validate() 
        {
            OnValidate();
        }

        public void SetWallEnabled(Wall wall, bool isEnabled)
        {
            walls[wall].enabled = isEnabled;
            var clockwiseCorner = wallBasePositionSettings[wall].clockwise;
            var antiClockwiseCorner = wallBasePositionSettings[wall].antiClockwise;
            corners[clockwiseCorner].enabled = isEnabled;
            corners[antiClockwiseCorner].enabled = isEnabled;
        }

        [TriInspector.Button]
        private void DivideTopBottom()
        {
#if UNITY_EDITOR
            if (transform.parent == null)
            {
                Debug.LogError($"Can't divide. Object must be child of parent object");
                return;
            }

            var halfWidth  = width / 2;
            var halfHeight = height / 2;

            Rect fullRect = new(new(plane.transform.position.x - halfWidth, plane.transform.position.z - halfHeight), new(width, height));
            Rect topRect  = new(fullRect.min + Vector2.up * halfHeight, new(width, halfHeight));
            Rect botRect  = new(fullRect.min, new(width, halfHeight));
            var topBlock  = CreateBlockByRect(topRect, transform.parent);
            var botBlock  = CreateBlockByRect(botRect, transform.parent);
            Dictionary<Wall, WallElementSettings> topWalls = 
                new()
                {
                    { Wall.Top   , walls[Wall.Top] },
                    { Wall.Right , new() { enabled = walls[Wall.Right].enabled, auto = walls[Wall.Right].auto, ignoreAntiClockwiseCornerDisable = walls[Wall.Right].ignoreAntiClockwiseCornerDisable} },
                    { Wall.Bottom, new() { enabled = false } },
                    { Wall.Left  , new() { enabled = walls[Wall.Left].enabled,  auto = walls[Wall.Left].auto,  ignoreClockwiseCornerDisable     = walls[Wall.Left].ignoreClockwiseCornerDisable} }
                };
            Dictionary<Corner, CornerElementSettings> topCorners =
                new()
                {
                    { Corner.TopRight   , corners[Corner.TopRight] },
                    { Corner.BottomRight, new() { enabled = false } },
                    { Corner.BottomLeft , new() { enabled = false } },
                    { Corner.TopLeft    , corners[Corner.TopLeft] }
                };
            topBlock.ApplyPreset(topWalls, topCorners);


            Dictionary<Wall, WallElementSettings> botWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = false } },
                    { Wall.Right , new() { enabled = walls[Wall.Right].enabled, auto = walls[Wall.Right].auto, ignoreClockwiseCornerDisable = walls[Wall.Right].ignoreClockwiseCornerDisable} },
                    { Wall.Bottom, walls[Wall.Bottom] },
                    { Wall.Left  , new() { enabled = walls[Wall.Left].enabled,  auto = walls[Wall.Left].auto,  ignoreAntiClockwiseCornerDisable = walls[Wall.Left].ignoreAntiClockwiseCornerDisable} }
                };
            Dictionary<Corner, CornerElementSettings> botCorners =
                new()
                {
                    { Corner.TopRight   , new() { enabled = false } },
                    { Corner.BottomRight, corners[Corner.BottomRight] },
                    { Corner.BottomLeft , corners[Corner.BottomLeft] },
                    { Corner.TopLeft    , new() { enabled = false } }
                };
            botBlock.ApplyPreset(botWalls, botCorners);

            DelayedDestroy(gameObject);
#endif
        }

        [TriInspector.Button]
        private void DivideLeftRight()
        {
#if UNITY_EDITOR
            if (transform.parent == null)
            {
                Debug.LogError($"Can't divide. Object must be child of parent object");
                return;
            }

            var halfWidth = width / 2;
            var halfHeight = height / 2;

            Rect fullRect  = new(new(plane.transform.position.x - halfWidth, plane.transform.position.z - halfHeight), new(width, height));
            Rect leftRect  = new(fullRect.min, new(halfWidth, height));
            Rect rightRect = new(fullRect.min + Vector2.right * halfWidth, new(halfWidth, height));
            var leftBlock  = CreateBlockByRect(leftRect, transform.parent);
            var rightBlock = CreateBlockByRect(rightRect, transform.parent);

            Dictionary<Wall, WallElementSettings> leftWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = walls[Wall.Top].enabled,     auto = walls[Wall.Top].auto,     ignoreAntiClockwiseCornerDisable = walls[Wall.Top].ignoreAntiClockwiseCornerDisable} },
                    { Wall.Right , new() { enabled = false } },
                    { Wall.Bottom, new() { enabled = walls[Wall.Bottom].enabled,  auto = walls[Wall.Bottom].auto,  ignoreClockwiseCornerDisable     = walls[Wall.Bottom].ignoreClockwiseCornerDisable} },
                    { Wall.Left  , walls[Wall.Left] } 
                };
            Dictionary<Corner, CornerElementSettings> leftCorners =
                new()
                {
                    { Corner.TopRight   , new() { enabled = false } },
                    { Corner.BottomRight, new() { enabled = false } },
                    { Corner.BottomLeft , corners[Corner.BottomLeft]},
                    { Corner.TopLeft    , corners[Corner.TopLeft]   }
                };
            leftBlock.ApplyPreset(leftWalls, leftCorners);


            Dictionary<Wall, WallElementSettings> rightWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = walls[Wall.Top].enabled,     auto = walls[Wall.Top].auto,     ignoreClockwiseCornerDisable     = walls[Wall.Top].ignoreClockwiseCornerDisable} },
                    { Wall.Right , walls[Wall.Right] },
                    { Wall.Bottom, new() { enabled = walls[Wall.Bottom].enabled,  auto = walls[Wall.Bottom].auto,  ignoreAntiClockwiseCornerDisable = walls[Wall.Bottom].ignoreAntiClockwiseCornerDisable} },
                    { Wall.Left  , new() { enabled = false } }
                };
            Dictionary<Corner, CornerElementSettings> rightCorners =
                new()
                {
                    { Corner.TopRight   , corners[Corner.TopRight]    },
                    { Corner.BottomRight, corners[Corner.BottomRight] },
                    { Corner.BottomLeft , new() { enabled = false }   },
                    { Corner.TopLeft    , new() { enabled = false }   }
                };
            rightBlock.ApplyPreset(rightWalls, rightCorners);

            DelayedDestroy(gameObject);
#endif
        }

        [TriInspector.Button]
        private void MakeHole()
        {
#if UNITY_EDITOR
            if (transform.parent == null)
            {
                Debug.LogError($"Can't divide. Object must be child of parent object");
                return;
            }

            var halfWidth  = width  / 2;
            var halfHeight = height / 2;

            var thirdWidth  = width  / 3;
            var thirdHeight = height / 3;
            Vector2 thirdSize = new(thirdWidth, thirdHeight);

            Rect fullRect = new(new(plane.transform.position.x - halfWidth, plane.transform.position.z - halfHeight), new(width, height));


            Rect leftBotRect  = new(fullRect.min                               , thirdSize);
            Rect leftMidRect  = new(fullRect.min + Vector2.up * thirdHeight    , thirdSize);
            Rect leftTopRect  = new(fullRect.min + Vector2.up * thirdHeight * 2, thirdSize);
                              
            Rect botRect      = new(fullRect.min + Vector2.right * thirdWidth                               , thirdSize);
            Rect topRect      = new(fullRect.min + Vector2.right * thirdWidth + Vector2.up * thirdHeight * 2, thirdSize);

            Rect rightBotRect = new(fullRect.min + Vector2.right * thirdWidth * 2                               , thirdSize);
            Rect rightMidRect = new(fullRect.min + Vector2.right * thirdWidth * 2 + Vector2.up * thirdHeight    , thirdSize);
            Rect rightTopRect = new(fullRect.min + Vector2.right * thirdWidth * 2 + Vector2.up * thirdHeight * 2, thirdSize);

            var leftBotBlock  = CreateBlockByRect(leftBotRect , transform.parent);
            var leftMidBlock  = CreateBlockByRect(leftMidRect , transform.parent);
            var leftTopBlock  = CreateBlockByRect(leftTopRect , transform.parent);        
            var botBlock      = CreateBlockByRect(botRect     , transform.parent);
            var topBlock      = CreateBlockByRect(topRect     , transform.parent);    
            var rightBotBlock = CreateBlockByRect(rightBotRect, transform.parent);
            var rightMidBlock = CreateBlockByRect(rightMidRect, transform.parent);
            var rightTopBlock = CreateBlockByRect(rightTopRect, transform.parent);


            Dictionary<Wall, WallElementSettings> leftBotWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = false } },
                    { Wall.Right , new() { enabled = false } },
                    { Wall.Bottom, new() { enabled = walls[Wall.Bottom].enabled,  auto = walls[Wall.Bottom].auto,  ignoreClockwiseCornerDisable = walls[Wall.Bottom].ignoreClockwiseCornerDisable} },
                    { Wall.Left  , walls[Wall.Left] }
                };
            Dictionary<Corner, CornerElementSettings> leftBotCorners =
                new()
                {
                    { Corner.TopRight   , new() { enabled = true } },
                    { Corner.BottomRight, new() { enabled = false } },
                    { Corner.BottomLeft , corners[Corner.BottomLeft]},
                    { Corner.TopLeft    , new() { enabled = false } }
                };
            leftBotBlock.ApplyPreset(leftBotWalls, leftBotCorners);

            Dictionary<Wall, WallElementSettings> leftMidWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = false } },
                    { Wall.Right , new() { enabled = true, ignoreClockwiseCornerDisable = true, ignoreAntiClockwiseCornerDisable = true } },
                    { Wall.Bottom, new() { enabled = false } },
                    { Wall.Left  , walls[Wall.Left] }
                };
            Dictionary<Corner, CornerElementSettings> leftMidCorners = noCornersPreset;
            leftMidBlock.ApplyPreset(leftMidWalls, leftMidCorners);

            Dictionary<Wall, WallElementSettings> leftTopWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = walls[Wall.Top].enabled, auto = walls[Wall.Top].auto, ignoreAntiClockwiseCornerDisable = walls[Wall.Top].ignoreAntiClockwiseCornerDisable} },
                    { Wall.Right , new() { enabled = false } },
                    { Wall.Bottom, new() { enabled = false } },
                    { Wall.Left  , walls[Wall.Left] }
                };
            Dictionary<Corner, CornerElementSettings> leftTopCorners =
                new()
                {
                    { Corner.TopRight   , new() { enabled = false }},
                    { Corner.BottomRight, new() { enabled = true  }},
                    { Corner.BottomLeft , new() { enabled = false }},
                    { Corner.TopLeft    , corners[Corner.TopLeft]  }
                };
            leftTopBlock.ApplyPreset(leftTopWalls, leftTopCorners);


            Dictionary<Wall, WallElementSettings> botWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = true, ignoreClockwiseCornerDisable = true, ignoreAntiClockwiseCornerDisable = true } },
                    { Wall.Right , new() { enabled = false } },
                    { Wall.Bottom, new() { enabled = true  } },
                    { Wall.Left  , new() { enabled = false } }
                };
            Dictionary<Corner, CornerElementSettings> botCorners = noCornersPreset;
            botBlock.ApplyPreset(botWalls, botCorners);

            Dictionary<Wall, WallElementSettings> topWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = true  } },
                    { Wall.Right , new() { enabled = false } },
                    { Wall.Bottom, new() { enabled = true, ignoreClockwiseCornerDisable = true, ignoreAntiClockwiseCornerDisable = true } },
                    { Wall.Left  , new() { enabled = false } }
                };
            Dictionary<Corner, CornerElementSettings> topCorners = noCornersPreset;
            topBlock.ApplyPreset(topWalls, topCorners);


            Dictionary<Wall, WallElementSettings> rightBotWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = false } },
                    { Wall.Right , walls[Wall.Right] },
                    { Wall.Bottom, new() { enabled = walls[Wall.Bottom].enabled, auto = walls[Wall.Bottom].auto,  ignoreClockwiseCornerDisable = walls[Wall.Bottom].ignoreClockwiseCornerDisable} },
                    { Wall.Left  , new() { enabled = false } }
                };
            Dictionary<Corner, CornerElementSettings> rightBotCorners =
                new()
                {
                    { Corner.TopRight   , new() { enabled = false }   },
                    { Corner.BottomRight, corners[Corner.BottomRight] },
                    { Corner.BottomLeft , new() { enabled = false }   },
                    { Corner.TopLeft    , new() { enabled = true  }   }
                };
            rightBotBlock.ApplyPreset(rightBotWalls, rightBotCorners);

            Dictionary<Wall, WallElementSettings> rightMidWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = false } },
                    { Wall.Right , walls[Wall.Right] },
                    { Wall.Bottom, new() { enabled = false } },
                    { Wall.Left  , new() { enabled = true, ignoreClockwiseCornerDisable = true, ignoreAntiClockwiseCornerDisable = true } }
                };
            Dictionary<Corner, CornerElementSettings> rightMidCorners = noCornersPreset;
            rightMidBlock.ApplyPreset(rightMidWalls, rightMidCorners);

            Dictionary<Wall, WallElementSettings> rightTopWalls =
                new()
                {
                    { Wall.Top   , new() { enabled = walls[Wall.Top].enabled, auto = walls[Wall.Top].auto, ignoreClockwiseCornerDisable = walls[Wall.Top].ignoreClockwiseCornerDisable} },
                    { Wall.Right , walls[Wall.Right] },
                    { Wall.Bottom, new() { enabled = false } },
                    { Wall.Left  , new() { enabled = false } }
                };
            Dictionary<Corner, CornerElementSettings> rightTopCorners =
                new()
                {
                    { Corner.TopRight   , corners[Corner.TopRight] },
                    { Corner.BottomRight, new() { enabled = false }},
                    { Corner.BottomLeft , new() { enabled = true  }},
                    { Corner.TopLeft    , new() { enabled = false }}
                };
            rightTopBlock.ApplyPreset(rightTopWalls, rightTopCorners);

            DelayedDestroy(gameObject);
#endif
        }


        private TableBlock CreateBlockByRect(Rect rect, Transform parent) 
        {
#if UNITY_EDITOR
            GameObject blockObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.tableBlockPf.gameObject, parent);
            blockObject.transform.position = new(rect.center.x, 0, rect.center.y);
            var block = blockObject.GetComponent<TableBlock>();
            block.SetSize(rect.width, rect.height);
            return block;
#else
            return null;
#endif
        }

        public void ApplyPreset(Dictionary<Wall, WallElementSettings> presetWalls, Dictionary<Corner, CornerElementSettings> presetCorners) 
        {
            foreach (var item in corners)
            {
                item.Value.enabled = presetCorners[item.Key].enabled;
                item.Value.auto    = presetCorners[item.Key].auto;
            }

            foreach (var item in walls)
            {
                item.Value.enabled                          = presetWalls[item.Key].enabled;
                item.Value.auto                             = presetWalls[item.Key].auto;
                item.Value.ignoreClockwiseCornerDisable     = presetWalls[item.Key].ignoreClockwiseCornerDisable;
                item.Value.ignoreAntiClockwiseCornerDisable = presetWalls[item.Key].ignoreAntiClockwiseCornerDisable;
            }
        }


        private void DelayedDestroy(GameObject gameObject)
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                DestroyImmediate(gameObject);
            };
#endif
        }

        private void DelayedUpdate()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (this == null)
                    return;

                foreach (var item in walls)
                    DelayedUpdateWall(item.Key);

                foreach (var item in corners)
                    DelayedUpdateCorner(item.Key);

                var pos = transform.position;
                pos.y = 0;
                transform.position = pos;

                plane.SetSize(width, height);
                plane.SetExcludedFromGameplay(isExcludedFromGameplay);

                var wallData = GetWallData(width, height);

                foreach (var item in walls)
                    AutoUpdateWall(item.Key, wallData[item.Key].size, plane.transform.position + wallData[item.Key].pos);

                foreach (var item in corners)
                {
                    var (clockwise, antiClockwise, angle, name) = cornerBasePositionSettings[item.Key];
                    AutoUpdateCorners(item.Key, plane.transform.position + wallData[clockwise].pos + wallData[antiClockwise].pos);
                }
            };
#endif
        }

        private void DelayedUpdateWall(Wall wall) 
        {
            walls[wall].wall.SetObjectActive(walls[wall].enabled);
        }

        private void DelayedUpdateCorner(Corner corner)
        {
            var clockwiseWall = cornerBasePositionSettings[corner].clockwise;
            var antiClockwiseWall = cornerBasePositionSettings[corner].antiClockwise;
            corners[corner].corner.SetObjectActive(corners[corner].enabled);
            corners[corner].corner.SetOrientation(GetOrientaion(clockwiseWall, antiClockwiseWall));
        }



        private void AutoUpdateWall(Wall wall, float size, Vector3 position)
        {
#if UNITY_EDITOR
            if (!walls[wall].auto)
                return;

            var clockwiseCorner = wallBasePositionSettings[wall].clockwise;
            var antiClockwiseCorner = wallBasePositionSettings[wall].antiClockwise;

            walls[wall].wall.SetSize(size,         
                                     corners[clockwiseCorner].corner.CurrentOrientation,
                                     corners[clockwiseCorner].enabled || walls[wall].ignoreClockwiseCornerDisable,
                                     corners[antiClockwiseCorner].corner.CurrentOrientation,
                                     corners[antiClockwiseCorner].enabled || walls[wall].ignoreAntiClockwiseCornerDisable);
            walls[wall].wall.SetPosition(position,
                                         corners[clockwiseCorner].corner.CurrentOrientation,
                                         corners[clockwiseCorner].enabled || walls[wall].ignoreClockwiseCornerDisable,
                                         corners[antiClockwiseCorner].corner.CurrentOrientation,
                                         corners[antiClockwiseCorner].enabled || walls[wall].ignoreAntiClockwiseCornerDisable);
#endif
        }

        private void AutoUpdateCorners(Corner corner, Vector3 position)
        {
#if UNITY_EDITOR
            if (!corners[corner].auto)
                return;
            
            corners[corner].corner.SetPosition(position);
#endif
        }

        private TableCorner.Orientation GetOrientaion(Wall clockwiseWall, Wall antiClockwiseWall) 
        {
            if (!walls[clockwiseWall].enabled && !walls[antiClockwiseWall].enabled)
                return TableCorner.Orientation.Inner;

            if (walls[clockwiseWall].enabled && !walls[antiClockwiseWall].enabled)
                return TableCorner.Orientation.Clockwise;

            if (!walls[clockwiseWall].enabled && walls[antiClockwiseWall].enabled)
                return TableCorner.Orientation.AntiClockwise;

            return TableCorner.Orientation.Outer;
        }

        private Dictionary<Wall, (Vector3 pos, float size)> GetWallData(float width, float height)
        {
            Dictionary<Wall, (Vector3 pos, float size)> result = new();

            foreach (var item in wallBasePositionSettings)
            {
                var value      = item.Key is Wall.Top or Wall.Bottom ? height : width;
                var otherValue = item.Key is Wall.Left or Wall.Right ? height : width;
                result.Add(item.Key, (value * GetWallDirection(wallBasePositionSettings[item.Key].orientation), otherValue));
            }

            return result;
        }

        private Vector3 GetWallDirection(Orientation orientation)
        {
            return orientation switch
            {
                Orientation.Forward => 0.5f  * transform.forward,
                Orientation.Back    => -0.5f * transform.forward,
                Orientation.Left    => -0.5f * transform.right,
                Orientation.Right   => 0.5f  * transform.right,
                _                   => 0.5f  * transform.forward
            };
        }

        private readonly static Dictionary<Wall, (Corner clockwise, Corner antiClockwise, float angle, Orientation orientation, string name)> wallBasePositionSettings = new()
        {
            { Wall.Top   , (Corner.TopRight,    Corner.TopLeft,     0,   Orientation.Forward, "TableBorderTop") },
            { Wall.Right , (Corner.BottomRight, Corner.TopRight,    90,  Orientation.Right  , "TableBorderRight") },
            { Wall.Bottom, (Corner.BottomLeft,  Corner.BottomRight, 180, Orientation.Back   , "TableBorderBottom") },
            { Wall.Left  , (Corner.TopLeft,     Corner.BottomLeft,  -90, Orientation.Left   , "TableBorderLeft") }
        };

        private readonly static Dictionary<Corner, (Wall clockwise, Wall antiClockwise, float angle, string name)> cornerBasePositionSettings = new()
        {
            { Corner.TopRight   , (Wall.Right,  Wall.Top,    90 , "TableCornerTopRight") },
            { Corner.BottomRight, (Wall.Bottom, Wall.Right,  180, "TableCornerBottomRight") },
            { Corner.BottomLeft , (Wall.Left,   Wall.Bottom, -90, "TableCornerBottomLeft") },
            { Corner.TopLeft    , (Wall.Top,    Wall.Left,   0  , "TableCornerTopLeft") }
        };

        private static readonly Dictionary<Corner, CornerElementSettings> noCornersPreset = new()
        {
            { Corner.TopRight   , new() { enabled = false } },
            { Corner.BottomRight, new() { enabled = false } },
            { Corner.BottomLeft , new() { enabled = false } },
            { Corner.TopLeft    , new() { enabled = false } }
        };        
    }
}


