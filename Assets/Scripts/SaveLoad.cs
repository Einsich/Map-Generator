using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SaveLoad : MonoBehaviour {

	public static void Save(byte SeaLevel, Texture2D[] landTex,Texture2D[] landNorm,byte[]terrain,byte[] HeightArr,int[,]provArr,List<Region> regions,List<State>st,string path)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(SeaLevel);
            int h = provArr.GetLength(0);
            int w = provArr.GetLength(1);
            writer.Write((byte)(h / MapMetrics.Tile));
            writer.Write((byte)(w / MapMetrics.Tile));
            for (int i = 0; i < 4; i++)
            {
                byte[] png = landTex[i].EncodeToPNG();
                writer.Write(png.Length);
                writer.Write(png);

                png = landNorm[i].EncodeToPNG();
                writer.Write(png.Length);
                writer.Write(png);
            }

            writer.Write(terrain);
            writer.Write(HeightArr);

            byte[] IdArr = new byte[h * w * 2];
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    IdArr[(i * w + j) * 2] = (byte)(provArr[i, j] >> 8);
                    IdArr[(i * w + j) * 2 + 1] = (byte)(provArr[i, j]);
                }
            writer.Write(IdArr);

            writer.Write((short)regions.Count);
            for (int i = 0; i < regions.Count; i++)
            {
                writer.Write(regions[i].name);
                writer.Write((short)regions[i].Capital.x);
                writer.Write((short)regions[i].Capital.y);
                writer.Write(regions[i].iswater);
                writer.Write((short)regions[i].portIdto);

                writer.Write((byte)regions[i].neib.Length);
                foreach (Region x in regions[i].neib)
                    writer.Write((short)x.id);
            }
            writer.Write((short)st.Count);
            for (int i = 0; i < st.Count; i++)
            {
                //writer.Write((short)st[i].originalId);
                writer.Write((short)st[i].regions[0].id);
                writer.Write((byte)st[i].fraction);
                writer.Write((short)st[i].regions.Count);
                foreach (Region x in st[i].regions)
                    writer.Write((short)x.id);
            }

            for(int i=0;i<regions.Count;i++)
            {
                regions[i].data.Save(writer);
            }
        }
    }
    public static void Load(out byte SeaLevel,out Texture2D[] landTex,out Texture2D[] landNorm,out byte[]terrain,out byte[] HeightArr, out int[,] provArr, out List<Region> regions, out List<State> states,string path)
    {
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            SeaLevel = reader.ReadByte();
            int h = reader.ReadByte() * MapMetrics.Tile;
            int w = reader.ReadByte() * MapMetrics.Tile;

            landTex = new Texture2D[4];
            landNorm = new Texture2D[4];
            for (int i = 0; i < 4; i++)
            {
                landTex[i] = new Texture2D(w, h);
                int pngL = reader.ReadInt32();
                landTex[i].LoadImage(reader.ReadBytes(pngL));

                landNorm[i] = new Texture2D(w, h);
                pngL = reader.ReadInt32();
                landNorm[i].LoadImage(reader.ReadBytes(pngL));
            }
            terrain = reader.ReadBytes(w * h);

            HeightArr = reader.ReadBytes(h * w);

            byte[] topr = reader.ReadBytes(h * w * 2);
            provArr = new int[h, w];
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    provArr[i, j] = (topr[(i * w + j) * 2] << 8) + topr[(i * w + j) * 2 + 1];

            regions = new List<Region>();
            int regcount = reader.ReadInt16();
            for (int i = 0; i < regcount; i++)
                regions.Add(new Region());
            for (int i = 0; i < regions.Count; i++)
            {
                regions[i].id = i;
                regions[i].name = reader.ReadString();
                regions[i].Capital= new Vector2Int( reader.ReadInt16(), reader.ReadInt16());
                regions[i].iswater = reader.ReadBoolean();
                regions[i].portIdto = reader.ReadInt16();
                int l = reader.ReadByte();
                regions[i].neib = new Region[l];
                //regions[i].border = new List<GameObject>[l];
                //regions[i].arrow = new GameObject[l];
                for (int j = 0; j < l; j++)
                {
                    int k = reader.ReadInt16();
                    regions[i].neib[j] = regions[k];
                }
            }
            Texture2D colors = new Texture2D(32, 32);
            colors.LoadImage(File.ReadAllBytes( "Assets/Texture/Terrain/StateColor.png"));

            string[] names = File.ReadAllLines("Assets/Textes/States.txt");

            int stcount = reader.ReadInt16();
            states = new List<State>();
            for (int i = 0; i < stcount; i++)
                states.Add(new State());
            for (int i = 0; i < states.Count; i++)
            {
                int a0=reader.ReadInt16();
               // states[i].originalId = a0;
                if (i == 0) states[i].mainColor = new Color(0, 0, 0, 0);
                else
                states[i].mainColor = colors.GetPixel(a0 % 32, a0 / 32);
                states[i].name = names[a0];
                states[i].flag = new Texture2D(128, 128);
                states[i].flag.LoadImage(File.ReadAllBytes("Assets/Texture/flags/(" + a0 + ").png"));              

                states[i].Capital = regions[reader.ReadInt16()];
                states[i].fraction = (FractionType)reader.ReadByte();

                int l = reader.ReadInt16();
                for (int j = 0; j < l; j++)
                {
                    int k = reader.ReadInt16();
                    states[i].regions.Add(regions[k]);
                    regions[k].owner = states[i];
                }
            }

            for(int i=0;i<regions.Count;i++)
            {
                FractionType frac = regions[i].owner.fraction;
                regions[i].data = new ProvinceData(frac,regions[i]);
                regions[i].data.Load(reader);
            }

        }
    }
    public static void WriteColorList(List<Color> colors, string path)
    {
        int size = (int)Mathf.Sqrt(colors.Count) + 1;

        Texture2D map = new Texture2D(size, size);
        for (int i = 0; i < colors.Count; i++)
            map.SetPixel(i % size, i / size, colors[i]);
        map.Apply();
        var bytes = map.EncodeToJPG(100);
        File.WriteAllBytes(path, bytes);
    }
    public static List<Color> ReadColorList()
    {
        List<Color> colors = new List<Color>();
        Texture2D map = SpriteHandler.StateColors;
        Color[] c = map.GetPixels();
        foreach (var a in c)
            if (a != Color.white)
                colors.Add(a);
        return colors;
    }
}
