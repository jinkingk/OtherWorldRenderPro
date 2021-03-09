using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LightmapsRenderData
{
	public MeshRenderer render;
	public int lightmapIndex;
	public Vector4 lightmapOffsetScale;
}
class LightmapsManager :MonoBehaviour
{
	public LightmapsRenderData[] lightmapsRenderDatas;
	public LightingData lightingData;
	public Light dirLight;
	public LightmapsRenderData[] GetLightmapsRenderData()
	{
		MeshRenderer[] renders = FindObjectsOfType<MeshRenderer>();
		var rendererItems = new List<LightmapsRenderData>();

		for (int i = 0; i < renders.Length; i++)
		{
			if (renders[i] != null)
			{
				MeshRenderer item = renders[i];
				LightmapsRenderData data = new LightmapsRenderData();
				data.render = item;
				data.lightmapIndex = item.lightmapIndex;
				data.lightmapOffsetScale = item.lightmapScaleOffset;
				rendererItems.Add(data);
			}
		}
		return rendererItems.ToArray();
	}

	private void ApplyLightmapsRenderData()
	{
		var lmrDatas = lightmapsRenderDatas;
		if (lmrDatas != null)
		{
			for (int i = 0; i < lmrDatas.Length; ++i)
			{
				LightmapsRenderData item = lmrDatas[i];
				MeshRenderer render = item.render;
				if (render != null)
				{
					render.lightmapIndex = item.lightmapIndex;
					render.lightmapScaleOffset = item.lightmapOffsetScale;
				}
			}
		}
	}

	public void Apply()
    {
		lightingData.ApplyLightmapsTex();
		ApplyLightmapsRenderData();
        //下面代码不设置会没有烘焙的影子
        LightBakingOutput bakingOutput = new LightBakingOutput();
        bakingOutput.isBaked = true;
        bakingOutput.lightmapBakeType = LightmapBakeType.Mixed;
        bakingOutput.mixedLightingMode = MixedLightingMode.Shadowmask;
		dirLight.bakingOutput = bakingOutput;
    }
}

