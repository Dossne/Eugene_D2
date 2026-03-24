using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class TableBuilder : MonoBehaviour
    {
        [Serializable]
        public class Edge
        {
            public Transform a;
            public Transform b;
            public Edge(Transform a, Transform b)
            {
                this.a = a;
                this.b = b;
            }
        }

        [Serializable]
        public class RectInfo
        {
            public Rect top = default;
            public Rect bottom = default;
            public Rect left = default;
            public Rect right = default;
        }


        [SerializeField] private List<Transform> points = new List<Transform>();
        [SerializeField] private List<Edge> edges = new();
        [SerializeField] private List<Rect> xRects = new();
        [SerializeField] private List<Rect> rects = new();
        [SerializeField] private float step = 5;

        [SerializeField] private Transform tableParent;
        [SerializeField] private TablePoint pointPf;
        [SerializeField] private TableBlock blockPf;


        private Dictionary<Rect, RectInfo> rectInfo = new();
        private Dictionary<Rect, List<Rect>> rectsForOptimization = new();


        [TriInspector.Button]
        private void AddPoint()
        {
#if UNITY_EDITOR
            GameObject newPoint = (GameObject)PrefabUtility.InstantiatePrefab(pointPf.gameObject, transform);
            points.Add(newPoint.transform);
            int index = 0;
            foreach (var item in points)
            {
                item.name = $"point_{index}";
                index++;
            }

            edges.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                if (points.Count - 1 == i)
                    edges.Add(new(points[i], points[0]));
                else if (points.Count - 1 > i)
                    edges.Add(new(points[i], points[i + 1]));
            }
#endif
        }

        [TriInspector.Button]
        private void GenerateEdges()
        {
#if UNITY_EDITOR
            for (int i = 0; i < points.Count; i++)
            {
                var pos = points[i].localPosition;
                pos.x = Mathf.RoundToInt(points[i].localPosition.x / step) * step;
                pos.z = Mathf.RoundToInt(points[i].localPosition.z / step) * step;
                points[i].SetLocalPositionAndRotation(pos, Quaternion.identity);
            }

            int index = 0;
            foreach (var item in points)
            {
                item.name = $"point_{index}";
                item.SetSiblingIndex(index);
                index++;
            }

            for (int i = 0; i < points.Count; i++)
            {
                var curA = points[i].position;
                var curB = i == points.Count - 1 ? points[0].position : points[i + 1].position;

                if (Math.Abs(curA.x - curB.x) > float.Epsilon && Math.Abs(curA.z - curB.z) > float.Epsilon)
                {
                    Debug.LogWarning($"{points[i].name} must be on same line with next point");
                }
            }

            edges.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                if (points.Count - 1 == i)
                    edges.Add(new(points[i], points[0]));
                else if (points.Count - 1 > i)
                    edges.Add(new(points[i], points[i + 1]));
            }
#endif
        }

        [TriInspector.Button]
        private void GenerateRects()
        {
#if UNITY_EDITOR
            if (HasIntersections())
                Debug.LogError("Shape has edge intersections");

            List<float> xCoords = points.Select(x => x.position.x).Distinct().ToList();
            xCoords.Sort((left, right) => {
                if (left < right) return -1;
                if (left > right) return 1;
                return 0;
            });


            xRects.Clear();
            for (int i = 0; i < xCoords.Count - 1; i++)
            {
                float xLeft = xCoords[i];
                float xRight = xCoords[i + 1];
                float xMid = (xLeft + xRight) * 0.5f;

                List<float> zIntersections = new List<float>();
                foreach (var edge in edges)
                {
                    if (Mathf.Approximately(edge.a.position.z, edge.b.position.z))
                    {
                        float z = edge.a.position.z;
                        float xMin = Mathf.Min(edge.a.position.x, edge.b.position.x);
                        float xMax = Mathf.Max(edge.a.position.x, edge.b.position.x);

                        if (xMid > xMin + 0.1 && xMid < xMax - 0.1)
                            zIntersections.Add(z);
                    }
                }

                zIntersections.Sort();
                for (int j = 0; j < zIntersections.Count - 1; j += 2)
                {
                    float zBottom = zIntersections[j];
                    float zTop = zIntersections[j + 1];
                    float width = xRight - xLeft;
                    float height = zTop - zBottom;
                    xRects.Add(new Rect(xLeft, zBottom, width, height));
                }
            }


            List<float> zCoords = points.Select(x => x.position.z).Distinct().ToList();
            zCoords.Sort((left, right) => {
                if (left < right) return -1;
                if (left > right) return 1;
                return 0;
            });

            rects.Clear();
            for (int i = 0; i < xRects.Count; i++)
            {
                var zBetween = zCoords.FindAll(z => z > xRects[i].min.y + 0.1 && z < xRects[i].max.y - 0.1);
                if (zBetween.Count == 0)
                {
                    rects.Add(xRects[i]);
                    continue;
                }

                float xMin = xRects[i].min.x;
                float zMin = xRects[i].min.y;
                float width = xRects[i].width;
                for (int j = 0; j < zBetween.Count; j++)
                {
                    rects.Add(new Rect(xMin, zMin, width, zBetween[j] - zMin));
                    zMin = zBetween[j];
                }
                rects.Add(new Rect(xMin, zMin, width, xRects[i].max.y - zMin));
            }

            OptimizeRects();
#endif
        }

        [TriInspector.Button]
        private void GenerateBlocks()
        {
#if UNITY_EDITOR
            if (HasIntersections())
                Debug.LogError("Shape has edge intersections");

            for (int i = tableParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(tableParent.GetChild(i).gameObject);

            Dictionary<Rect, TableBlock> blockByRect = new();
            Dictionary<Rect, (Rect topLeft, 
                              Rect topRight, 
                              Rect botLeft, 
                              Rect botRight, 
                              Rect topLeftTop,  
                              Rect topLeftLeft,
                              Rect topRightTop, 
                              Rect topRightRight,
                              Rect botLeftBot,  
                              Rect botLeftLeft,
                              Rect botRightBot, 
                              Rect botRightRight)> nearRects = new();

            for (int i = 0; i < rects.Count; i++)
            {
                GameObject newBlock = (GameObject)PrefabUtility.InstantiatePrefab(blockPf.gameObject, tableParent);
                newBlock.transform.position = new(rects[i].center.x, 0, rects[i].center.y);
                var tableBlock = newBlock.GetComponent<TableBlock>();
                tableBlock.SetSize(rects[i].width, rects[i].height);
                nearRects.Add(
                              rects[i],
                              new()
                              {
                                  topLeft       = rects.Find(x => x.Contains(rects[i].center + Vector2.up    * (rects[i].height / 2 + 0.1f * step) + Vector2.left  * (rects[i].width / 2 + 0.1f * step))),
                                  topRight      = rects.Find(x => x.Contains(rects[i].center + Vector2.up    * (rects[i].height / 2 + 0.1f * step) + Vector2.right * (rects[i].width / 2 + 0.1f * step))),
                                  botLeft       = rects.Find(x => x.Contains(rects[i].center + Vector2.down  * (rects[i].height / 2 + 0.1f * step) + Vector2.left  * (rects[i].width / 2 + 0.1f * step))),
                                  botRight      = rects.Find(x => x.Contains(rects[i].center + Vector2.down  * (rects[i].height / 2 + 0.1f * step) + Vector2.right * (rects[i].width / 2 + 0.1f * step))),
                                  topLeftTop    = rects.Find(x => x.Contains(rects[i].center + Vector2.up    * (rects[i].height / 2 + 0.1f * step) + Vector2.left  * (rects[i].width / 2 - 0.1f * step))),
                                  topLeftLeft   = rects.Find(x => x.Contains(rects[i].center + Vector2.up    * (rects[i].height / 2 - 0.1f * step) + Vector2.left  * (rects[i].width / 2 + 0.1f * step))),
                                  topRightTop   = rects.Find(x => x.Contains(rects[i].center + Vector2.up    * (rects[i].height / 2 + 0.1f * step) + Vector2.right * (rects[i].width / 2 - 0.1f * step))),
                                  topRightRight = rects.Find(x => x.Contains(rects[i].center + Vector2.up    * (rects[i].height / 2 - 0.1f * step) + Vector2.right * (rects[i].width / 2 + 0.1f * step))),
                                  botLeftBot    = rects.Find(x => x.Contains(rects[i].center + Vector2.down  * (rects[i].height / 2 + 0.1f * step) + Vector2.left  * (rects[i].width / 2 - 0.1f * step))),
                                  botLeftLeft   = rects.Find(x => x.Contains(rects[i].center + Vector2.down  * (rects[i].height / 2 - 0.1f * step) + Vector2.left  * (rects[i].width / 2 + 0.1f * step))),
                                  botRightBot   = rects.Find(x => x.Contains(rects[i].center + Vector2.down  * (rects[i].height / 2 + 0.1f * step) + Vector2.right * (rects[i].width / 2 - 0.1f * step))),
                                  botRightRight = rects.Find(x => x.Contains(rects[i].center + Vector2.down  * (rects[i].height / 2 - 0.1f * step) + Vector2.right * (rects[i].width / 2 + 0.1f * step)))
                              }
                             );

                if (nearRects[rects[i]].topLeftTop != default && nearRects[rects[i]].topRightTop != default)
                    tableBlock.SetWallEnabled(TableBlock.Wall.Top, false);

                if (nearRects[rects[i]].botLeftBot != default && nearRects[rects[i]].botRightBot != default)
                    tableBlock.SetWallEnabled(TableBlock.Wall.Bottom, false);

                if (nearRects[rects[i]].topLeftLeft != default && nearRects[rects[i]].botLeftLeft != default)
                    tableBlock.SetWallEnabled(TableBlock.Wall.Left, false);

                if (nearRects[rects[i]].topRightRight != default && nearRects[rects[i]].botRightRight != default)
                    tableBlock.SetWallEnabled(TableBlock.Wall.Right, false);                               

                blockByRect.Add(rects[i], tableBlock);
            }
            foreach (var item in blockByRect)
                item.Value.Validate();

            for (int i = 0; i < rects.Count; i++)
            {
                (Rect topLeft,
                 Rect topRight,
                 Rect botLeft,
                 Rect botRight,
                 Rect topLeftTop,
                 Rect topLeftLeft,
                 Rect topRightTop,
                 Rect topRightRight,
                 Rect botLeftBot,
                 Rect botLeftLeft,
                 Rect botRightBot,
                 Rect botRightRight) = nearRects[rects[i]];
                if (topLeft  != default 
                 && topRight != default 
                 && botLeft  != default 
                 && botRight != default)
                    continue;

                if (topLeft == default && topLeftTop != default && topLeftLeft != default)
                {
                    var curBlock  = blockByRect[rects[i]];
                    var topBlock  = blockByRect[topLeftTop];
                    var leftBlock = blockByRect[topLeftLeft];

                    curBlock.corners[TableBlock.Corner.TopLeft].enabled = true;
                    topBlock.walls[TableBlock.Wall.Left].ignoreAntiClockwiseCornerDisable = true;
                    topBlock.corners[TableBlock.Corner.BottomLeft].enabled = false;
                    leftBlock.walls[TableBlock.Wall.Top].ignoreClockwiseCornerDisable = true;
                    leftBlock.corners[TableBlock.Corner.TopRight].enabled = false;
                }

                if (topRight == default && topRightTop != default && topRightRight != default)
                {
                    var curBlock = blockByRect[rects[i]];
                    var topBlock = blockByRect[topRightTop];
                    var rightBlock = blockByRect[topRightRight];

                    curBlock.corners[TableBlock.Corner.TopRight].enabled = true;
                    topBlock.walls[TableBlock.Wall.Right].ignoreClockwiseCornerDisable = true;
                    topBlock.corners[TableBlock.Corner.BottomRight].enabled = false;
                    rightBlock.walls[TableBlock.Wall.Top].ignoreAntiClockwiseCornerDisable = true;
                    rightBlock.corners[TableBlock.Corner.TopLeft].enabled = false;
                }

                if (botLeft == default && botLeftBot != default && botLeftLeft != default)
                {
                    var curBlock = blockByRect[rects[i]];
                    var botBlock = blockByRect[botLeftBot];
                    var leftBlock = blockByRect[botLeftLeft];

                    curBlock.corners[TableBlock.Corner.BottomLeft].enabled = true;
                    botBlock.walls[TableBlock.Wall.Left].ignoreClockwiseCornerDisable = true;
                    botBlock.corners[TableBlock.Corner.TopLeft].enabled = false;
                    leftBlock.walls[TableBlock.Wall.Bottom].ignoreAntiClockwiseCornerDisable = true;
                    leftBlock.corners[TableBlock.Corner.BottomRight].enabled = false;
                }

                if (botRight == default && botRightBot != default && botRightRight != default)
                {
                    var curBlock = blockByRect[rects[i]];
                    var botBlock = blockByRect[botRightBot];
                    var rightBlock = blockByRect[botRightRight];

                    curBlock.corners[TableBlock.Corner.BottomRight].enabled = true;
                    botBlock.walls[TableBlock.Wall.Right].ignoreAntiClockwiseCornerDisable = true;
                    botBlock.corners[TableBlock.Corner.TopRight].enabled = false;
                    rightBlock.walls[TableBlock.Wall.Bottom].ignoreClockwiseCornerDisable = true;
                    rightBlock.corners[TableBlock.Corner.BottomLeft].enabled = false;
                }
            }

            foreach (var item in blockByRect)
                item.Value.Validate();
#endif
        }

        [TriInspector.Button]
        private void ResetBuilder()
        {
#if UNITY_EDITOR
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            edges.Clear();
            xRects.Clear();
            rects.Clear();
            DelayedDestroyObjects();
#endif
        }

        private static bool IntersectBounds(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Bounds boundsAB = new(a, Vector3.zero);
            boundsAB.Encapsulate(b);
            Bounds boundsCD = new(c, Vector3.zero);
            boundsCD.Encapsulate(d);
            return boundsAB.Intersects(boundsCD);
        }
                
        private static int OrientationXZ(Vector3 a, Vector3 b, Vector3 t)
        {
            float val = (b.x - a.x) * (t.z - a.z) - (t.x - a.x) * (b.z - a.z);
            if (Math.Abs(val) < float.Epsilon)
                return 0;
            return (val > 0) ? 1 : -1;
        }


        private static bool Intersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return IntersectBounds(a, b, c, d)
                && OrientationXZ(a, b, c) * OrientationXZ(a, b, d) <= 0
                && OrientationXZ(c, d, a) * OrientationXZ(c, d, b) <= 0;
        }


        private void DelayedDestroyObjects()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                foreach (var item in points)
                   DestroyImmediate(item.gameObject);

                points.Clear();
                for (int i = tableParent.childCount - 1 ; i >= 0; i--)
                    DestroyImmediate(tableParent.GetChild(i).gameObject);
            };
#endif
        }

        private void OptimizeRects()
        {
#if UNITY_EDITOR
            rectInfo.Clear();
            for (int i = 0; i < rects.Count; i++)
            {
                rectInfo.Add(rects[i],
                            new()
                            {
                                top = rects.Find(x => x.Contains(rects[i].center + Vector2.up * (rects[i].height / 2 + 0.1f))),
                                bottom = rects.Find(x => x.Contains(rects[i].center + Vector2.down * (rects[i].height / 2 + 0.1f))),
                                left = rects.Find(x => x.Contains(rects[i].center + Vector2.left * (rects[i].width / 2 + 0.1f))),
                                right = rects.Find(x => x.Contains(rects[i].center + Vector2.right * (rects[i].width / 2 + 0.1f)))
                            });
            }

            rectsForOptimization.Clear();
            Dictionary<Rect, Rect> optimizedTo = new();
            for (int i = 0; i < rects.Count; i++)
            {
                RectInfo curInfo = rectInfo[rects[i]];
                if (curInfo.top == default)
                    continue;
                RectInfo topInfo = rectInfo[curInfo.top];
                bool leftSame = curInfo.left == topInfo.left || curInfo.left != default && topInfo.left != default;
                bool rightSame = curInfo.right == topInfo.right || curInfo.right != default && topInfo.right != default;

                if (!leftSame || !rightSame)
                    continue;

                if (optimizedTo.TryGetValue(rects[i], out var optimizedKey))
                {
                    rectsForOptimization[optimizedKey].Add(curInfo.top);
                    optimizedTo.Add(curInfo.top, optimizedKey);
                    continue;
                }

                if (!rectsForOptimization.TryGetValue(rects[i], out var rectList))
                {
                    rectsForOptimization.Add(rects[i], new());
                    rectsForOptimization[rects[i]].Add(curInfo.top);
                    optimizedTo.Add(curInfo.top, rects[i]);
                }
                else
                {
                    rectList.Add(curInfo.top);
                    optimizedTo.Add(curInfo.top, rects[i]);
                }
            }

            foreach (var rect in rectsForOptimization)
            {
                List<Rect> rectForOpt = new List<Rect>();
                rectForOpt.Add(rect.Key);
                rectForOpt.AddRange(rect.Value);
                rectForOpt = rectForOpt.OrderBy(x => x.xMin).ToList();
                var resultRect = new Rect(rectForOpt[0].min, new Vector2(rectForOpt[^1].max.x - rectForOpt[0].min.x, rectForOpt[^1].max.y - rectForOpt[0].min.y));
                var insertPos = rects.IndexOf(rect.Key);
                rects.Insert(insertPos, resultRect);
                rects.RemoveAll(x => rectForOpt.Contains(x));
            }
#endif
        }


        private bool HasIntersections()
        {
            if (points.Count < 4)
                return false;

            points.RemoveAll(x => x == null);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (Math.Abs(i - j) <= 1 || Math.Abs(i - j) == points.Count - 1)
                        continue;

                    var curA = points[i].position;
                    var curB = i == points.Count - 1 ? points[0].position : points[i + 1].position;

                    var curC = points[j].position;
                    var curD = j == points.Count - 1 ? points[0].position : points[j + 1].position;

                    if (Intersect(curA, curB, curC, curD))
                        return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {

            if (points.Count < 4 || HasIntersections())
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            for (int i = 0; i < edges.Count; i++)
                if (edges[i].a != null && edges[i].b != null)
                    Gizmos.DrawLine(edges[i].a.position, edges[i].b.position);

            Gizmos.color = Color.red;
            for (int i = 0; i < rects.Count; i++)
            {
                Gizmos.DrawLine(new(rects[i].min.x, 0, rects[i].min.y), new(rects[i].min.x, 0, rects[i].max.y));
                Gizmos.DrawLine(new(rects[i].max.x, 0, rects[i].min.y), new(rects[i].max.x, 0, rects[i].max.y));
                Gizmos.DrawLine(new(rects[i].min.x, 0, rects[i].min.y), new(rects[i].max.x, 0, rects[i].min.y));
                Gizmos.DrawLine(new(rects[i].min.x, 0, rects[i].max.y), new(rects[i].max.x, 0, rects[i].max.y));
            }
        }
#endif
    }
}