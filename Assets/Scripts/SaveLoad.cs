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
    static List<int> colorHardcoded =new List<int> {
        0x5d8aa8, 0xe32636, 0xe52b50, 0xffbf00, 0x9966cc, 0xa4c639, 0xcd9575, 0xfaebd7, 0xb8860b, 0x8db600, 0x4b5320, 0xe9d66b, 0xff9966, 0xa52a2a, 0x013220, 0xbdb76b,
        0xfdee00, 0x007fff, 0xffe135, 0xfae7b5, 0xfe6f5e, 0x318ce7, 0x0d98ba, 0x8a2be2, 0xe3dac9, 0xcc0000, 0x006a4e, 0x873260, 0x0070ff, 0xb5a642, 0x1dacd6, 0x66ff00,
        0xc32148, 0x08e8de, 0xd19fe8, 0x004225, 0xcd7f32, 0xa52a2a, 0xe7feff, 0x480607, 0x800020, 0xcc5500, 0xe97451, 0xbd33a4, 0x702963, 0x007aa5, 0xe03c31, 0x5f9ea0,
        0x91a3b0, 0x006b3c, 0xed872d, 0xe30022, 0xfff600, 0x1e4d2b, 0x78866b, 0xffff99, 0xc41e3a, 0x00cc99, 0x99badd, 0xb31b1b, 0xed9121, 0xace1af, 0x4997d0, 0x007ba7,
        0x2a52be, 0xfad6a5, 0xd2691e, 0xffa700, 0xe34234, 0x0047ab, 0x002e63, 0x893f45, 0xfbec5d, 0xb31b1b, 0x6495ed, 0xffbcd9, 0xdc143c, 0x990000, 0x5d3954, 0x08457e,
        };
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
        if (true)
        {
            Texture2D map = SpriteHandler.StateColors;
            Color[] c = map.GetPixels();
            foreach (var a in c)
                if (a != Color.white)
                    colors.Add(a);
        } else
        {
            foreach (var a in colorHardcoded)
            {
                int r = (a >> 16) & 0xff;
                int g = (a >> 8) & 0xff;
                int b = (a >> 0) & 0xff;
                float k = 1f / 0xff;
                colors.Add(new Color(r * k, g * k, b * k).gamma);
            }
        }
        
        return colors;
    }
}
