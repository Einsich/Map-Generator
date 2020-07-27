using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDataBase", menuName = "SpriteDataBase", order = 51)]
public class SpriteDataBase : ScriptableObject
{
    [SerializeField] public Sprite[] PersonTypes;
    [SerializeField] public Sprite[] Skills;
    [SerializeField] public Sprite[] Cursors;
    [SerializeField] public Texture2D[] Buildings;
    [SerializeField] public Sprite[] BuildFrames;
}

