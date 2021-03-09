using Assets.Eidtor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BakeToolWindow : EditorWindow
{
	public enum WorldState
    {
		Main = 1,
		Other = 2,
    }
	private WorldState worldState = WorldState.Main;
	public Light mainLight = null;
	public Light otherLight = null;
	void Awake()
    {
		Debug.Log("awake");
		Lightmapping.bakeCompleted += OnBakeCompleted;

		GameObject dLightObject = GameObject.Find("DirectionLight");
		if (dLightObject == null)
			return;
		Transform mainLightTs = dLightObject.transform.Find("MainLight");
		Transform otherLightTs = dLightObject.transform.Find("OtherLight");
		if (mainLightTs == null || otherLightTs == null)
        {
			Debug.LogError("Please set light!");
        }
        else
        {
			mainLight = mainLightTs.GetComponent<Light>();
			otherLight = otherLightTs.GetComponent<Light>();
		}
	}
    private void OnGUI()
    {
		if (GUILayout.Button("BakeMain"))
		{
			worldState = WorldState.Main;
			SetBakeDirectionLight(true);
			Lightmapping.BakeAsync();
		}
		if (GUILayout.Button("BakeOther"))
		{
			worldState = WorldState.Other;
			SetBakeDirectionLight(true);
			Lightmapping.BakeAsync();
		}
		if (GUILayout.Button("ShowMain"))
		{
			worldState = WorldState.Main;
			SetBakeDirectionLight(false);
			LightMapTool.ApplyLightmaps(false);
		}
		if (GUILayout.Button("ShowOther"))
		{
			worldState = WorldState.Other;
			SetBakeDirectionLight(false);
			LightMapTool.ApplyLightmaps(true);
		}
		mainLight = EditorGUILayout.ObjectField("main light",
															mainLight,
															typeof(Light),
															true) as Light;
		otherLight = EditorGUILayout.ObjectField("other light",
												otherLight,
												typeof(Light),
												true) as Light;
	}
	//
	// 摘要:
	//		设置灯光是否投射影子。只在烘焙时开启影子，运行时不会投射影子。
	//
	// 参数:
	//   bakeShadow:
	//		是否是烘焙的影子
	private void SetBakeDirectionLight(bool bakeShadow)
    {
		mainLight.gameObject.SetActive(false);
		otherLight.gameObject.SetActive(false);
		if (worldState == WorldState.Main)
        {
			mainLight.gameObject.SetActive(true);
			mainLight.shadows = bakeShadow ? LightShadows.Hard : LightShadows.None;
        }
        else
        {
			otherLight.gameObject.SetActive(true);
			otherLight.shadows = bakeShadow ? LightShadows.Hard : LightShadows.None;
		}
    }

	//private void OnEnable()
 //   {
	//	Debug.Log("OnEnable");
 //   }

 //   private void OnDisable()
 //   {
	//	Debug.Log("OnDisable");
	//}

    private void OnDestroy()
    {
		Debug.Log("OnDestroy");
		Lightmapping.bakeCompleted -= OnBakeCompleted;
	}

	//void OnBecameVisible()
	//{
	//	Debug.Log("OnBecameVisible");
	//}

	//void OnBecameInvisible()
	//{
	//	Debug.Log("OnBecameInvisible");
	//}

	//烘焙参数，按照实际需求修改
	private void SetBakeLightMap()
    {
		Lightmapping.bakedGI = true;
		Lightmapping.realtimeGI = false;
		LightmapEditorSettings.mixedBakeMode = MixedLightingMode.Shadowmask;

		LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
		//Prioritize View
		LightmapEditorSettings.prioritizeView = true;
		LightmapEditorSettings.directSampleCount = 32;
		LightmapEditorSettings.indirectSampleCount = 500;
		LightmapEditorSettings.bounces = 2;

		LightmapEditorSettings.bakeResolution = 40f;

		LightmapEditorSettings.padding = 2;
		LightmapEditorSettings.enableAmbientOcclusion = false;
		LightmapEditorSettings.maxAtlasSize = 1024;
		LightmapEditorSettings.textureCompression = true;
		
		LightmapEditorSettings.filteringMode = LightmapEditorSettings.FilterMode.Auto;
		LightmapEditorSettings.lightmapsMode = LightmapsMode.NonDirectional;

		UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
	}

	private void OnBakeCompleted()
    {
		LightMapTool.CopyLightmapsData(worldState == WorldState.Other ? true : false,
			worldState == WorldState.Other ? otherLight : mainLight);
		LightMapTool.ClearBakeData();
		SetBakeDirectionLight(false);
		LightMapTool.ApplyLightmaps(worldState == WorldState.Other ? true : false);
		Debug.Log("BakeComplete");
    }

	[MenuItem("Tools/BakeTool")]
	static void OpenWindow()
	{
		var wnd = EditorWindow.GetWindow<BakeToolWindow>(false, "BakeTool");
		wnd.minSize = new Vector2(260f, 400f);
		wnd.maxSize = new Vector2(400f, 1200f);
		wnd.Show();
	}
}
