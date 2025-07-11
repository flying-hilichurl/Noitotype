using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Enums;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utility;
using Random = UnityEngine.Random;

namespace MapFactory
{
    public class IrregularMapFactory : MapAbstractFactory
    {
        #region Field
        //常量
        private const float MinDistance = 40;           //起点（原点）与终点之间的最小距离
        private const float MaxDistance = 60;           //起点（原点）与终点之间的最大距离
        private const float Radius = 60;                //地图由起点和终点向外延伸的距离
        private const float Scale = 0.15f;              //柏林噪声图中步长的缩放
        private const float EmptyProbability = 0.4f;    //空洞在地图中所占的比例
        private const int CaveThreshold = 100;           //小于该值的空洞将会被清除
        private static readonly Vector2Int MapBorder = new Vector2Int(2, 2);    //地图外墙壁的厚度
        
        private int _seed;                              //地图种子
        private Vector2Int _endPosition;                //玩家在该地图的终点(以瓦片为单位)
        private float[,] _tileValue;                    //每个瓦片所在点的噪声值
        private Tilemap _groundTilemap;                 //地面所在的瓦片地图
        private TileBase _groundTile;                   //填充地面所用的规则瓦片
        
        private Dictionary<MapFeaturesEnum, GameObject> _featuresDict = new Dictionary<MapFeaturesEnum, GameObject>();  //key:地点类型枚举  value:地图特征地点
        #endregion
        
        private void Awake()
        {
            _seed = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            Random.InitState(_seed);
        }
        
        private async void Start()
        {
            try
            {
                //获取地图实例
                _groundTilemap = GameObject.FindWithTag("groundTile").GetComponent<Tilemap>();

                if (_groundTilemap == null)
                {
                    throw new NullReferenceException();
                }

                await LoadAllMapAssetsAsync();
            }
            catch (NullReferenceException nullReferenceException)
            {
                Debug.LogError("Not found TileMpa with groundTile tag: " + nullReferenceException.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("资源异步加载失败: "+e.Message);
            }
            finally
            {
                Application.Quit(1);
            }
        }

        /// <summary>
        /// 统一异步加载地图创建需要的资源。
        /// </summary>
        private async Task LoadAllMapAssetsAsync()
        {
            //加载创建地图时需要使用的资源
            _groundTile = await AddressableUtility.LoadAssetByNameAsync<TileBase>("GroundRuleTile");
            GameObject entrance = await AddressableUtility.LoadAssetByNameAsync<GameObject>("Entrance");
            GameObject exit = await AddressableUtility.LoadAssetByNameAsync<GameObject>("Exit");
            _featuresDict.Add(MapFeaturesEnum.Entrance, entrance);
            _featuresDict.Add(MapFeaturesEnum.Exit, exit);
        }

        /// <summary>
        /// 通过柏林噪声生成一个以起点和终点为圆心延伸、连接的不规则地图
        /// </summary>
        public override void CreateMap()
        {
            CreateEndPoint();
            CreateRectData();
            CreateTileMap();
            ClearSmallCave();
        }

        /// <summary>
        /// 清除地图
        /// </summary>
        public override void ClearMap()
        {
            _groundTilemap.ClearAllTiles();
        }

        /// <summary>
        /// 在随机的范围内得到一个终点，将会分别以起点和终点为圆心生成地图
        /// </summary>
        private void CreateEndPoint()
        {
            //利用极坐标，确定距离和角度，确定终点
            float distance = Random.Range(MinDistance, MaxDistance);
            float radian = Random.Range(0, Mathf.PI * 2);
            _endPosition = new Vector2Int((int)(distance * Mathf.Cos(radian)), (int)(distance * Mathf.Sin(radian)));
            Debug.Log("终点创建完成"+_endPosition);
        }

        /// <summary>
        /// 生成每个瓦片对应的噪声值
        /// </summary>
        private void CreateRectData()
        {
            Vector2Int size = new Vector2Int((int)(Mathf.Abs(_endPosition.x)+2*Radius) + (MapBorder.x<<1),(int)(Mathf.Abs(_endPosition.y)+2*Radius) + (MapBorder.y<<1));
            _tileValue = new float[size.x, size.y];
            float offest = Random.Range(-10000, 10000);     //柏林噪声图的偏移量

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    _tileValue[i, j] = Mathf.PerlinNoise(offest + i*Scale, offest + j*Scale);
                }
            }
            
            Debug.Log("地图数据创建完成"+size);
        }

        /// <summary>
        /// 根据噪声值在特定范围生成瓦片地图
        /// </summary>
        private void CreateTileMap()
        {
            Vector2Int leftTop = new Vector2Int((int)(_endPosition.x>0?-Radius:_endPosition.x-Radius),(int)(_endPosition.y<0?Radius:_endPosition.y+Radius));    //地图左上角的坐标
            leftTop += new Vector2Int(-MapBorder.x, MapBorder.y);
            
            for (int i = 0; i < _tileValue.GetLength(0); i++)
            {
                for (int j = 0; j < _tileValue.GetLength(1); j++)
                {
                    Vector2Int tilePositon = new Vector2Int(leftTop.x+i, leftTop.y-j);
                    
                    //如果该瓦片到起点或终点的距离在半径内，且噪声值小于空洞概率，则不生成瓦片
                    if (Vector2Int.Distance(tilePositon, _endPosition) <= Radius ||
                        Vector2Int.Distance(tilePositon, Vector2Int.zero) <= Radius)
                    {
                        if(_tileValue[i,j]>EmptyProbability)
                            _groundTilemap.SetTile(new Vector3Int(leftTop.x+i,leftTop.y-j,0),_groundTile);
                    }
                    else
                        _groundTilemap.SetTile(new Vector3Int(leftTop.x+i,leftTop.y-j,0),_groundTile);
                }
            }
            
            Debug.Log("地图创建完成"+leftTop);
        }

        /// <summary>
        /// 清除地图中的细小空洞
        /// </summary>
        private void ClearSmallCave()
        {
            List<List<Vector3Int>> caves = MapUtility.FindAllTargetTiles(_groundTilemap, null);
            foreach (List<Vector3Int> cave in caves)
            {
                if (cave.Count <= CaveThreshold)
                {
                    foreach (Vector3Int tilePosition in cave)
                    {
                        _groundTilemap.SetTile(tilePosition,_groundTile);
                    }
                }
            }
            
            Debug.Log("地图碎片清除完毕！");
        }
    }
}