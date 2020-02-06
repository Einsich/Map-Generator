using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDataBase", menuName = "SpriteDataBase", order = 51)]
public class SpriteDataBase : ScriptableObject
{

    [SerializeField] public Sprite[] RegimentTypes;
}

