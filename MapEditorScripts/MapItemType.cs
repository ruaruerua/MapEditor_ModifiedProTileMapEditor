using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapItemType : MonoBehaviour
{
    public enum ItemType
    {
        BaseGrid,
        GridEffect,
        ScenesItem,
        MySetArea,
        EnemyRandomArea,
    }

    public ItemType curItemType = ItemType.BaseGrid;
    [FormerlySerializedAs("TerrainID")]
    public int MainID = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
