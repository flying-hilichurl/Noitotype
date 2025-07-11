using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utility
{
    public static class MapUtility
    {
        private enum TileStatus
        {
                UnChecked,
                Checked
        }

        /// <summary>
        /// 获取到地图中的每一个”孤岛“。(洪水填充）
        /// </summary>
        /// <param name="tilemap">检索的地图</param>
        /// <param name="target">要查找的tileBase</param>
        /// <returns>一个孤岛中的所有瓦片在tilemap中的坐标</returns>
        public static List<List<Vector3Int>> FindAllTargetTiles(Tilemap tilemap, [CanBeNull] TileBase target)
        {
            BoundsInt bounds = tilemap.cellBounds;
            List<List<Vector3Int>> islands = new List<List<Vector3Int>>();
            TileStatus[,] statusMap = new TileStatus[bounds.size.x, bounds.size.y];

            for (int i = 0; i < bounds.size.x; i++)
            { 
                for (int j = 0; j < bounds.size.y; j++)
                {
                    Vector3Int position = new Vector3Int(bounds.position.x+i, bounds.position.y+j);
                    if (tilemap.GetTile(position) == target && statusMap[i,j] == TileStatus.UnChecked)
                    {
                        Vector3Int mapPosition = new Vector3Int(i, j);
                        List<Vector3Int> list = new List<Vector3Int>();
                        FloodFill(tilemap, position, mapPosition, target, statusMap, list);
                        islands.Add(list);
                    }
                }
            }
            
            return islands;
        }
        
        /// <summary>
        /// 通过洪水填充找到所有相连的同一瓦片
        /// </summary>
        private static void FloodFill(Tilemap tilemap, Vector3Int position, Vector3Int mapPosition, TileBase target, TileStatus[,] statusMap, List<Vector3Int> island)
        {
            //未越界且是目标瓦片且未被检查过
            if (mapPosition.x >= 0 && mapPosition.x < statusMap.GetLength(0) && mapPosition.y >= 0 && mapPosition.y < statusMap.GetLength(1) &&
                tilemap.GetTile(position) == target && statusMap[mapPosition.x, mapPosition.y] == TileStatus.UnChecked)
            {
                //添加瓦片并标记为已检查
                island.Add(position);
                statusMap[mapPosition.x, mapPosition.y] = TileStatus.Checked;
                
                //递归检查周围节点
                FloodFill(tilemap, position + Vector3Int.right,mapPosition+Vector3Int.right, target, statusMap, island);
                FloodFill(tilemap, position + Vector3Int.left, mapPosition+Vector3Int.left,target, statusMap, island);
                FloodFill(tilemap, position + Vector3Int.up, mapPosition+Vector3Int.up,target, statusMap, island);
                FloodFill(tilemap, position + Vector3Int.down, mapPosition+Vector3Int.down,target, statusMap, island);
            }
        }
    }
}