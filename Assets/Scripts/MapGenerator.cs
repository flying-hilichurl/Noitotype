using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using Utility;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]private Vector2 startPosition;     //地图起始点（左上角）
    [SerializeField]private Vector2 size;              //地图大小
    [Range(0,1f)]
    [SerializeField]private float emptyProbability;    //空洞占整个地图的比例
    private readonly float _scale = 0.1f;              //瓦片坐标在柏林噪声图中步长的缩放
    private int _seed;                                 //地图种子
    [SerializeField][HideInInspector] private float[,] _tileValue;    //每个瓦片就噪声得出的值，与emptyProbability比较，决定该处是空洞还是地面

    [SerializeField]private Tilemap groundTilemap;     //整个瓦片地图
    [SerializeField] private TileBase tile;            //填充地图使用的规则瓦片

    private Dictionary<MapFeaturesEnum, GameObject> _featuresDir = new Dictionary<MapFeaturesEnum, GameObject>();

    private void Awake()
    {
        _tileValue = new float[(int)size.x,(int)size.y];
        
        //使每次的随机数都不一样
        _seed = Time.time.GetHashCode();
        Random.InitState(_seed);
    }
    private async void Start()
    {
        try
        {
            //获取地图实例
            groundTilemap = FindFirstObjectByType<Tilemap>();
                 
            //加载创建地图时需要使用的资源
            GameObject entrance = await AddressableUtility.LoadAssetByNameAsync<GameObject>("Entrance");
            GameObject exit = await AddressableUtility.LoadAssetByNameAsync<GameObject>("Exit");
            _featuresDir.Add(MapFeaturesEnum.Entrance,entrance);
            _featuresDir.Add(MapFeaturesEnum.Exit,exit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Application.Quit(1);
        }
        
    }

    //生成通过柏林噪声一个随机地图
    public void GenerateMap()
    {
        ClearMap();
        GenerateMapData();
        GenerateGroundTileMap();
    }

    //生成柏林噪声地图数据.用于阈值控制
    private void GenerateMapData()
    {
        int offest = Random.Range(-10000, 10000);
        for(int i = 0; i<size.x;i++)        //纵向
        {
            for(int j = 0; j<size.y;j++)    //横向
            {
                float value = Mathf.PerlinNoise(offest + j * _scale, offest + i * _scale);
                _tileValue[i, j] = value;
            }
        }
    }

    //生成实体的瓦片地图
    private void GenerateGroundTileMap()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (_tileValue[i,j] > emptyProbability)
                {
                    groundTilemap.SetTile(new Vector3Int((int)startPosition.x+i,(int)startPosition.y+j),tile);
                }
            }
        }
    }

    //生成地图中的特征元素
    private void GenerateFeatures()
    {
        
    }
    
    //清除地图
    public void ClearMap()
    {
        _tileValue = new float[(int)size.x, (int)size.y];
        groundTilemap.ClearAllTiles();
    }
}
