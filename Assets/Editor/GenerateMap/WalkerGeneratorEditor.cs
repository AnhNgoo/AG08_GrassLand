using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WalkerGeneratorEditor : EditorWindow
{
    private bool isRunning = false;
    private WalkerGenerator walkerGenerator;
    private List<WalkerGenerator.TilemapConfig> tilemapConfigs = new List<WalkerGenerator.TilemapConfig>();
    private int mapWidth = 30;
    private int mapHeight = 30;
    private int maximumWalkers = 10;
    private float fillPercentage = 0.4f;
    private int seed = 0;
    private Vector2 scrollPos;

    [MenuItem("Tools/Walker Generator")]
    public static void ShowWindow()
    {
        GetWindow<WalkerGeneratorEditor>("Walker Generator");
    }

    void OnEnable()
    {
        // Tạo instance mới của WalkerGenerator (không cần ScriptableObject)
        walkerGenerator = new WalkerGenerator();
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Tilemap Configurations", EditorStyles.boldLabel);
        for (int i = 0; i < tilemapConfigs.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            tilemapConfigs[i].tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemapConfigs[i].tilemap, typeof(Tilemap), true);
            tilemapConfigs[i].type = (WalkerGenerator.Grid)EditorGUILayout.EnumPopup("Type", tilemapConfigs[i].type);
            tilemapConfigs[i].useRandomTile = EditorGUILayout.Toggle("Use Random Tile", tilemapConfigs[i].useRandomTile);

            if (tilemapConfigs[i].useRandomTile)
            {
                int size = EditorGUILayout.IntField("Tile Options Count", tilemapConfigs[i].tileOptions != null ? tilemapConfigs[i].tileOptions.Count : 0);
                if (tilemapConfigs[i].tileOptions == null) tilemapConfigs[i].tileOptions = new List<Tile>();
                while (tilemapConfigs[i].tileOptions.Count < size) tilemapConfigs[i].tileOptions.Add(null);
                while (tilemapConfigs[i].tileOptions.Count > size) tilemapConfigs[i].tileOptions.RemoveAt(tilemapConfigs[i].tileOptions.Count - 1);
                for (int j = 0; j < tilemapConfigs[i].tileOptions.Count; j++)
                {
                    tilemapConfigs[i].tileOptions[j] = (Tile)EditorGUILayout.ObjectField($"Tile {j}", tilemapConfigs[i].tileOptions[j], typeof(Tile), false);
                }
            }
            else
            {
                tilemapConfigs[i].specificTile = (Tile)EditorGUILayout.ObjectField("Specific Tile", tilemapConfigs[i].specificTile, typeof(Tile), false);
            }

            if (GUILayout.Button("Remove"))
            {
                tilemapConfigs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Tilemap Config"))
        {
            tilemapConfigs.Add(new WalkerGenerator.TilemapConfig());
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Settings", EditorStyles.boldLabel);
        mapWidth = EditorGUILayout.IntField("Map Width", mapWidth);
        mapHeight = EditorGUILayout.IntField("Map Height", mapHeight);
        maximumWalkers = EditorGUILayout.IntField("Maximum Walkers", maximumWalkers);
        fillPercentage = EditorGUILayout.Slider("Fill Percentage", fillPercentage, 0f, 1f);
        seed = EditorGUILayout.IntField("Seed", seed);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate"))
        {
            if (tilemapConfigs.Count == 0 || tilemapConfigs.Exists(config => config.tilemap == null))
            {
                Debug.LogError("TilemapConfigs is empty or contains null Tilemaps! Please assign valid Tilemaps.");
                return;
            }

            // Gán các giá trị từ EditorWindow vào WalkerGenerator
            walkerGenerator.tilemapConfigs = tilemapConfigs;
            walkerGenerator.MapWidth = mapWidth;
            walkerGenerator.MapHeight = mapHeight;
            walkerGenerator.MaximumWalkers = maximumWalkers;
            walkerGenerator.FillPercentage = fillPercentage;
            walkerGenerator.Seed = seed;

            walkerGenerator.StartGenerate();
            isRunning = true;
        }

        if (isRunning)
        {
            if (walkerGenerator.GenerateStep())
            {
                foreach (var config in tilemapConfigs)
                {
                    if (config.tilemap != null)
                    {
                        EditorUtility.SetDirty(config.tilemap.gameObject);
                    }
                }
                Repaint();
            }
            else
            {
                isRunning = false;
            }
        }

        EditorGUILayout.EndScrollView();
    }
}