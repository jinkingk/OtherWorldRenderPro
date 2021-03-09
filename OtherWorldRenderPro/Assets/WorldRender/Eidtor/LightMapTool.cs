using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Eidtor
{
    class LightMapTool
    {
        private const string Path_HEADER = "lightmap_";
        private const string LightingDatasPath = "Assets/WorldRender/Game/LightingDatas";

        public static LightmapsManager CreateLightmapsManager(bool other)
        {
            string goName = other ? "other_lightmap" : "main_lightmap";
            GameObject target = GameObject.Find(goName);
            if (target == null)
                target = new GameObject(goName);

            LightmapsManager lightmapManager = target.GetComponent<LightmapsManager>();
            if (lightmapManager == null)
            {
                lightmapManager = target.AddComponent<LightmapsManager>();
            }
            lightmapManager.lightingData = CreateLightingData(other);

            //target.layer = 31;

            return lightmapManager;
        }

        public static LightingData CreateLightingData(bool other)
        {
            string goName = other ? "other_lightmap" : "main_lightmap";
            GameObject target = GameObject.Find(goName);
            if (target == null)
                target = new GameObject(goName);

            LightingData lightingData = target.GetComponent<LightingData>();
            if (lightingData == null)
            {
                lightingData = target.AddComponent<LightingData>();
            }

            //target.layer = 31;

            return lightingData;
        }
        //拷贝烘焙数据并保存
        public static void CopyLightmapsData(bool other, Light dirLight)
        {
            Scene sc = SceneManager.GetActiveScene();
            string sceneName = sc.name;
            string dir = Path.Combine(LightingDatasPath, sceneName);
            string lightmap_dir = Path.Combine(dir, "main_lightmap");
            if (other)
            {
                lightmap_dir = Path.Combine(dir, "other_lightmap");
            }
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (Directory.Exists(lightmap_dir))
            {
                Directory.Delete(lightmap_dir, true);
            }
            Directory.CreateDirectory(lightmap_dir);
            
            var lightmaps = LightmapSettings.lightmaps;
            LightmapsTexture[] lightmapTices = new LightmapsTexture[lightmaps.Length];
            for (int index = 0; index < lightmaps.Length; index++)
            {
                LightmapData item = lightmaps[index];
                LightmapsTexture texName = new LightmapsTexture();
                if (item.lightmapColor)
                {
                    string light_name = Path_HEADER + index + "_light";
                    texName.color = SaveTexEXR(lightmap_dir, light_name, item.lightmapColor);
                }
                if (item.lightmapDir)
                {
                    string dir_name = Path_HEADER + index + "_dir";
                    texName.dir = SaveTexPNG(lightmap_dir, dir_name, item.lightmapDir);
                }
                if (item.shadowMask)
                {
                    string sm_name = Path_HEADER + index + "_sm";
                    texName.shadowmask = SaveTexPNG(lightmap_dir, sm_name, item.shadowMask);
                }
                lightmapTices[index] = texName;
            }

            LightmapsManager lightmapManager = CreateLightmapsManager(other);
            lightmapManager.dirLight = dirLight;
            lightmapManager.lightmapsRenderDatas = lightmapManager.GetLightmapsRenderData();
            var lightingData = lightmapManager.lightingData;
            lightingData.LightmapTexs = lightmapTices;
            lightingData.lightmapMode = LightmapSettings.lightmapsMode;

            var prefabName = Path.Combine(dir, other ? "other_lightmap" : "main_lightmap");
            prefabName = prefabName + ".prefab";
            prefabName = prefabName.Replace('\\', '/');
            if (File.Exists(prefabName))
                File.Delete(prefabName);
            PrefabUtility.SaveAsPrefabAsset(lightingData.gameObject, prefabName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        //应用烘焙数据
        public static void ApplyLightmaps(bool other)
        {
            string goName = other ? "other_lightmap" : "main_lightmap";
            GameObject target = GameObject.Find(goName);
            if (target == null)
            {
                Debug.LogError("no lightmaps baked");
                return;
            }

            LightmapsManager lightmapManager = target.GetComponent<LightmapsManager>();
            lightmapManager.Apply();
        }

        public static void ClearBakeData()
        {
            Lightmapping.ClearLightingDataAsset();
            Lightmapping.ClearDiskCache();
            Lightmapping.Clear();
        }

        private static Texture2D SaveTexPNG(string path, string name, Texture2D tex)
        {
            string filename = path + "/" + name + ".png";
            Debug.Log("save tex:" + filename);
            return CopyTex(filename, tex);
        }

        private static Texture2D SaveTexEXR(string path, string name, Texture2D tex)
        {
            string filename = path + "/" + name + ".exr";
            Debug.Log("save tex:" + filename);
            return CopyTex(filename, tex);

        }

        private static Texture2D CopyTex(string dst, Texture2D tex)
        {
            string src = AssetDatabase.GetAssetPath(tex);
            if (File.Exists(src))
            {
                AssetDatabase.CopyAsset(src, dst);
            }
            //shadowmask只有R通道
            if (dst.Contains("_sm"))
            {
                var tim = AssetImporter.GetAtPath(dst) as TextureImporter;
                var defaultSetting = tim.GetDefaultPlatformTextureSettings();
                defaultSetting.format = TextureImporterFormat.R8;
                tim.SetPlatformTextureSettings(defaultSetting);
                tim.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(dst);
        }
    }
}
