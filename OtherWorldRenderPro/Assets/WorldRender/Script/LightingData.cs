using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LightmapsTexture
{
    public Texture2D color;
    public Texture2D dir;
    public Texture2D shadowmask;
}
public class LightingData : MonoBehaviour
{
	public LightmapsTexture[] LightmapTexs;
    public LightmapsMode lightmapMode = LightmapsMode.CombinedDirectional;

	public void ApplyLightmapsTex()
	{
		var lightmap_tex = LightmapTexs;
		int itemLen = 0;
		if (LightmapTexs != null && LightmapTexs.Length > 0)
		{
			LightmapData[] datas = new LightmapData[LightmapTexs.Length];
			for (int i = 0; i < LightmapTexs.Length; i++)
			{
				LightmapData dataL = new LightmapData();
				LightmapsTexture lightmap = LightmapTexs[i];
				dataL.lightmapColor = lightmap.color;
				dataL.lightmapDir = lightmap.dir;
				dataL.shadowMask = lightmap.shadowmask;
				datas[i] = dataL;
			}
			itemLen = LightmapTexs.Length;
			LightmapSettings.lightmaps = datas;
			LightmapSettings.lightmapsMode = lightmapMode;
		}
	}

}
