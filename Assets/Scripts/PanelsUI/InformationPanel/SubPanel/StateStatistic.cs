using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStatistic : MonoBehaviour
{
    public Renderer canvas;
    private MaterialPropertyBlock block;
    void Start()
    {
        block = new MaterialPropertyBlock();
        canvas.GetPropertyBlock(block);
    }

    public void UpdateState(State state)
    {
       // Vector4[] 
        //block.SetVectorArray();
    }
}
