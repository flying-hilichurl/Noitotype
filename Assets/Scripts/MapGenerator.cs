using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]private Vector2 startPosition;     //地图起始点（左上角）
    [SerializeField]private Vector2 size;              //地图大小
    [Range(0,1f)]
    [SerializeField]private float emptyProbability;    //空洞占整个地图的比例
    private float _scale = 0.1f;                       //瓦片坐标在柏林噪声图中步长的缩放
    private int _seed;                                  //地图种子
    [HideInInspector][SerializeField]private List<float> _tileValue = new List<float>();       //每个瓦片就噪声得出的值，与emptyProbability比较，决定该处是空洞还是地面

    [SerializeField]private Tilemap groundTilemap;             //整个瓦片地图
    [SerializeField] private TileBase tile;     //填充地图使用的规则瓦片

    private void Awake()
    {
        //是每次的随机数都不一样
        _seed = Time.time.GetHashCode();
        Random.InitState(_seed);
        
        //获取地图示例
        groundTilemap = FindFirstObjectByType<Tilemap>();
    }

    //生成通过柏林噪声一个随机地图
    public void GenerateMap()
    {
        ClearMap();
        GenerateMapData();
        GenerateTileMap();
    }

    private void GenerateMapData()
    {
        int offest = Random.Range(-10000, 10000);
        for(int i = 0; i<size.x;i++)
        {
            for(int j = 0; j<size.y;j++)
            {
                float value = Mathf.PerlinNoise(offest + i * _scale, offest + j * _scale);
                _tileValue.Add(value);
            }
        }
    }

    private void GenerateTileMap()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (_tileValue[i * (int)size.y + j] > emptyProbability)
                {
                    groundTilemap.SetTile(new Vector3Int((int)startPosition.x+i,(int)startPosition.y+j),tile);
                }
            }
        }
    }
    
    //清除地图
    public void ClearMap()
    {
        _tileValue.Clear();
        groundTilemap.ClearAllTiles();
    }
}
