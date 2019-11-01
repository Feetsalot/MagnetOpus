using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New MetalFrag List", menuName = "Tile Data", order = 51)]
public class MetalFragData : ScriptableObject
{
    public List<GameObject> metalFragPrefabs;
}
