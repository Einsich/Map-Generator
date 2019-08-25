using UnityEngine;
using System.Collections.Generic;


namespace StaticFunction
  {
  public static partial class staticFunc
    {
    public static Color rgb(byte r,byte g,byte b){const float mul=1/255f; return new Color(r*mul,g*mul,b*mul);}

    }
  }
