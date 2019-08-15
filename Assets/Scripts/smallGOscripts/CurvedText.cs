using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CurvedText : Text {

    public float radius = 0.5f;
    public float wrapAngle = 360.0f;
    public float scaleFactor = 100.0f;
    //  public float begX=-50, begY=-20, endX=50, endY=-10;
    Vector3 Begin, End;
    private float circumference
    {
        get
        {
            if (_radius != radius || _scaleFactor != scaleFactor)
            {
                _circumference = 2.0f * Mathf.PI * radius * scaleFactor;
                _radius = radius;
                _scaleFactor = scaleFactor;
            }

            return _circumference;
        }
    }
    private float _radius = -1;
    private float _scaleFactor = -1;
    private float _circumference = -1;
    

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);

        List<UIVertex> stream = new List<UIVertex>();

       // begX = -50; begY = -20; endX = 50; endY = -10;

        vh.GetUIVertexStream(stream);
        minx = stream[0].position.x; 
        maxx = stream[stream.Count-3].position.x;
       // Vector3 Begin = new Vector3(begX, begY);
        //Vector3 End = new Vector3(endX, endY);
        
        for (int i = 0; i < stream.Count; i+=6)
        {
            float q = Intpol(stream, i);
            Vector3 A = Begin + q * (-Begin);
            Vector3 B =  q * (End);
            Vector3 a = B - A;
            Vector3 P = A + a * q;
            Correct(stream,i,P,a);
            
        }

        vh.AddUIVertexTriangleStream(stream);
    }
    float minx, maxx;
    float Intpol(List<UIVertex> list,int i)
    {
        float x = (list[i + 1].position.x + list[i].position.x) * 0.5f;
        
        return (x - minx) / (maxx - minx);
    }

    void Correct(List<UIVertex> list, int i,Vector3 p,Vector3 a)
    {
        Vector3 locP = (list[i].position + list[i + 1].position) * 0.5f;
        locP.y = 0;
        a.Normalize();
        Vector3 n = new Vector3(-a.y, a.x);
        
        for (int j = i; j < i + 6; j++)
        {
            Vector3 d = list[j].position - locP;
            UIVertex buf = list[j];
            buf.position = p + d.x * a + d.y * n;
            buf.position.z = -buf.position.z;
            list[j] = buf;
        }
    }
    public void SetProperies(string name,Vector3 begin,Vector3 end,int fontS)
    {
        text = name;
        Begin = begin;
        End = end;
        fontSize = fontS;
    }

}
