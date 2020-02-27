using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CursorHandler 
{
    static Texture2D[] cursor;
     static CursorHandler()
    {
        Sprite[] sprites = SpriteHandler.GetCursors;
        cursor = new Texture2D[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            Rect r = sprites[i].rect;
            Texture2D sourse = sprites[i].texture;
            int w = (int)r.width, h = (int)r.height;
            int x = (int)r.x, y = (int)r.y;
            cursor[i] = new Texture2D(w, h, TextureFormat.RGBA32,false);
            cursor[i].SetPixels(sourse.GetPixels(x, y, w, h));
           // cursor[i].alphaIsTransparency = true;
        }
    }
    public static void SetCursor(CursorType type)
    {
        Texture2D s = null;
        switch(type)
        {
            case CursorType.Default:s = cursor[0];break;
            case CursorType.Select: s = cursor[1];break;
            case CursorType.MeleeAttack: s = cursor[2];break;
            case CursorType.RangeAttack: s = cursor[3];break;
            case CursorType.SiegeAttack: s = cursor[4];break;
            case CursorType.CastSkill: s = cursor[5];break;
            case CursorType.CanCastSkill: s = cursor[6];break;
        }
        Cursor.SetCursor(s, new Vector2(0, 0), CursorMode.Auto);
    }
}
public enum CursorType
{
    Default,
    Select,
    MeleeAttack,
    RangeAttack,
    SiegeAttack,
    CastSkill,
    CanCastSkill
}
