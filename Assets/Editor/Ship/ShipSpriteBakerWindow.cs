using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class ShipSpriteBakerWindow : EditorWindow
{
	[MenuItem("Tools/Ships/Ship Sprite Baker")] 
	private static void OpenWindow()
	{
		var window = GetWindow<ShipSpriteBakerWindow>(true, "Ship Sprite Baker", true);
		window.minSize = new Vector2(420, 520);
		window.Show();
	}

	[SerializeField] private GameObject shipPrefab;
	[SerializeField] private int frameSize = 256;
	[SerializeField] private int columns = 36;
	[SerializeField] private bool orthographic = false;
	[SerializeField] private float cameraFOV = 25f;
	[SerializeField] private float cameraPadding = 1.15f;
	[SerializeField] private Vector3 modelEulerOffset = Vector3.zero;
	[SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0);
	[SerializeField] private string outputFolder = "Assets/Textures";
	[SerializeField] private string baseFileName = "Ship360";

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Source Prefab", EditorStyles.boldLabel);
		shipPrefab = (GameObject)EditorGUILayout.ObjectField("Ship Prefab", shipPrefab, typeof(GameObject), false);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Camera & Render", EditorStyles.boldLabel);
		frameSize = Mathf.Clamp(EditorGUILayout.IntField("Frame Size (px)", frameSize), 32, 4096);
		columns = Mathf.Clamp(EditorGUILayout.IntField("Columns", columns), 1, 360);
		orthographic = EditorGUILayout.Toggle("Orthographic Camera", orthographic);
		if (!orthographic)
		{
			cameraFOV = Mathf.Clamp(EditorGUILayout.FloatField("Camera FOV", cameraFOV), 1f, 100f);
		}
		cameraPadding = Mathf.Clamp(EditorGUILayout.FloatField("Camera Padding", cameraPadding), 1f, 3f);
		modelEulerOffset = EditorGUILayout.Vector3Field("Model Euler Offset", modelEulerOffset);
		backgroundColor = EditorGUILayout.ColorField("Background", backgroundColor);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
		if (GUILayout.Button("...", GUILayout.Width(30)))
		{
			string picked = EditorUtility.OpenFolderPanel("Pick Output Folder (inside Assets)", Application.dataPath, "");
			if (!string.IsNullOrEmpty(picked))
			{
				string normPicked = picked.Replace('\\', '/').TrimEnd('/');
				string normData = Application.dataPath.Replace('\\', '/').TrimEnd('/');
				if (normPicked.StartsWith(normData, StringComparison.OrdinalIgnoreCase))
				{
					outputFolder = "Assets" + normPicked.Substring(normData.Length);
					if (string.IsNullOrEmpty(outputFolder)) outputFolder = "Assets";
				}
				else
				{
					EditorUtility.DisplayDialog("Invalid Folder", "Folder must be inside the project's Assets.", "OK");
				}
			}
		}
		EditorGUILayout.EndHorizontal();
		baseFileName = EditorGUILayout.TextField("Base File Name", baseFileName);

		EditorGUILayout.Space();
		using (new EditorGUI.DisabledScope(shipPrefab == null))
		{
			if (GUILayout.Button("Bake 360 Sprite Sheet"))
			{
				Bake();
			}
		}
	}

	private void Bake()
	{
		if (shipPrefab == null)
		{
			EditorUtility.DisplayDialog("Missing Prefab", "Please assign a ship prefab.", "OK");
			return;
		}
		if (!EnsureFolderExists(outputFolder))
		{
			EditorUtility.DisplayDialog("Invalid Output Folder", "The output folder must be inside Assets.", "OK");
			return;
		}

		int totalFrames = 360;
		int rows = Mathf.CeilToInt(totalFrames / (float)columns);
		int sheetWidth = frameSize * columns;
		int sheetHeight = frameSize * rows;

		var sheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false, true);
		sheet.name = baseFileName + "_Sheet";

		var pr = new PreviewRenderUtility();
		try
		{
			pr.camera.clearFlags = CameraClearFlags.SolidColor;
			pr.camera.backgroundColor = backgroundColor;
			pr.camera.allowHDR = true;
			pr.camera.orthographic = orthographic;
			if (!orthographic) pr.camera.fieldOfView = cameraFOV;

			// Lighting: align with Sun light direction if present
			var sun = RenderSettings.sun;
			if (sun == null)
			{
				foreach (var l in UnityEngine.Object.FindObjectsOfType<Light>())
				{
					if (l.type == LightType.Directional) { sun = l; break; }
				}
			}
			var lightGO = new GameObject("TempDirectionalLight");
			var light = lightGO.AddComponent<Light>();
			light.type = LightType.Directional;
			if (sun != null)
			{
				light.transform.rotation = sun.transform.rotation;
				light.color = sun.color;
				light.intensity = sun.intensity;
			}
			else
			{
				light.transform.rotation = Quaternion.Euler(50, 30, 0);
				light.color = Color.white;
				light.intensity = 1.2f;
			}
			pr.AddSingleGO(lightGO);

			// Instantiate prefab into preview world
			var instance = (GameObject)PrefabUtility.InstantiatePrefab(shipPrefab);
			instance.transform.position = Vector3.zero;
			instance.transform.rotation = Quaternion.identity;
			instance.transform.localScale = Vector3.one;
			pr.AddSingleGO(instance);

			// Fixed camera viewpoint as requested
			pr.camera.nearClipPlane = 0.01f;
			pr.camera.farClipPlane = 2000f;
			pr.camera.aspect = 1f; // square frames
			pr.camera.transform.position = new Vector3(0f, 20f, -20f);
			pr.camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

			var pixelBlock = new Color[frameSize * frameSize];

			// Create one RT and reuse it for all frames (more stable, less GC)
			var rt = new RenderTexture(frameSize, frameSize, 24, RenderTextureFormat.ARGB32);
			rt.antiAliasing = 1; // keep it simple and stable
			rt.Create();
			var prevActive = RenderTexture.active;
			pr.camera.targetTexture = rt;

			for (int angle = 0; angle < totalFrames; angle++)
			{
				instance.transform.rotation = Quaternion.Euler(modelEulerOffset) * Quaternion.Euler(0, angle, 0);
				if (sun != null)
				{
					light.transform.rotation = sun.transform.rotation; // keep sun consistent
				}

				// Render the camera into RT
				pr.camera.aspect = 1f;
				pr.camera.Render();

				RenderTexture.active = rt;
				var frame = new Texture2D(frameSize, frameSize, TextureFormat.RGBA32, false, true);
				frame.ReadPixels(new Rect(0, 0, frameSize, frameSize), 0, 0);
				frame.Apply();
				RenderTexture.active = prevActive;

				// Copy into sheet
				int col = angle % columns;
				int row = rows - 1 - (angle / columns);
				pixelBlock = frame.GetPixels();
				sheet.SetPixels(col * frameSize, row * frameSize, frameSize, frameSize, pixelBlock);
			}

			pr.camera.targetTexture = null;
			rt.Release();

			sheet.Apply(false, false);

			// Save PNG
			var pngBytes = sheet.EncodeToPNG();
			string pngPath = Path.Combine(outputFolder, baseFileName + ".png");
			File.WriteAllBytes(pngPath, pngBytes);
			AssetDatabase.ImportAsset(pngPath);

			// Configure importer for multiple sprites
			var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.mipmapEnabled = false;
			importer.alphaIsTransparency = true;
			importer.filterMode = FilterMode.Bilinear;
			importer.textureCompression = TextureImporterCompression.Uncompressed;

			var metaList = new List<SpriteMetaData>(totalFrames);
			for (int i = 0; i < totalFrames; i++)
			{
				int col = i % columns;
				int row = rows - 1 - (i / columns);
				var meta = new SpriteMetaData
				{
					name = $"Angle_{i:000}",
					rect = new Rect(col * frameSize, row * frameSize, frameSize, frameSize),
					alignment = (int)SpriteAlignment.Center,
					pivot = new Vector2(0.5f, 0.5f)
				};
				metaList.Add(meta);
			}
			importer.spritesheet = metaList.ToArray();
			importer.SaveAndReimport();

			// Create ScriptableObject with references
			var sheetAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
			var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(pngPath);
			var spriteList = new List<Sprite>();
			foreach (var o in sprites)
			{
				if (o is Sprite s) spriteList.Add(s);
			}
			// Ensure order by name Angle_000..Angle_359
			spriteList.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

			var sheetSO = CreateInstance<SpriteSheet360>();
			sheetSO.texture = sheetAsset;
			sheetSO.frames = spriteList.ToArray();
			sheetSO.frameWidth = frameSize;
			sheetSO.frameHeight = frameSize;
			sheetSO.columns = columns;
			sheetSO.rows = rows;
			sheetSO.frameCount = totalFrames;
			sheetSO.degreesPerFrame = 1;
			sheetSO.zeroDegreesFacing = 0; // 0 = +Z direction

			string soPath = Path.Combine(outputFolder, baseFileName + ".asset");
			AssetDatabase.CreateAsset(sheetSO, soPath);
			EditorUtility.SetDirty(sheetSO);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorUtility.DisplayDialog("Ship Sprite Baker", "Baking completed successfully!\n\nSaved to:\n" + pngPath + "\n" + soPath, "OK");
		}
		catch (Exception ex)
		{
			Debug.LogError("Ship Sprite Baker failed: " + ex);
			EditorUtility.DisplayDialog("Error", ex.Message, "OK");
		}
		finally
		{
			pr.Cleanup();
			RenderTexture.active = null;
		}
	}

	private static Bounds CalculateRenderableBounds(GameObject go)
	{
		var renderers = go.GetComponentsInChildren<Renderer>();
		Bounds b = new Bounds(go.transform.position, Vector3.zero);
		bool has = false;
		foreach (var r in renderers)
		{
			if (!has)
			{
				b = r.bounds;
				has = true;
			}
			else
			{
				b.Encapsulate(r.bounds);
			}
		}
		if (!has) b = new Bounds(go.transform.position, Vector3.one);
		return b;
	}

	private static bool EnsureFolderExists(string assetPath)
	{
		if (string.IsNullOrEmpty(assetPath)) return false;
		if (AssetDatabase.IsValidFolder(assetPath)) return true;
		if (!assetPath.StartsWith("Assets", StringComparison.Ordinal)) return false;
		string[] parts = assetPath.Replace('\\', '/').Split('/');
		string current = "Assets";
		for (int i = 1; i < parts.Length; i++)
		{
			string next = parts[i];
			if (string.IsNullOrEmpty(next)) continue;
			string combined = current + "/" + next;
			if (!AssetDatabase.IsValidFolder(combined))
			{
				AssetDatabase.CreateFolder(current, next);
			}
			current = combined;
		}
		return AssetDatabase.IsValidFolder(assetPath);
	}
}


