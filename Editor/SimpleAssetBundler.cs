using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SimpleAssetBundler
{
	public class SimpleAssetBundler : EditorWindow
	{
		private const string defaultAssetBundleDirectory = "Assets\\Bundles\\";
		private const string defaultOutputDirectory = "AssetBundles";

		[MenuItem("Assets/AssetBundles/Simple Asset Bundler")]
		private static void OpenAssetBundlesWindow()
		{
			var newWindow = GetWindow<SimpleAssetBundler>();
			newWindow.Show();
		}

		private static List<AssetBundleBuild> BuildAllAssetBundles(BuildAssetBundleOptions assetBundleOptions)
		{
			List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
			string assetBundleDirectory = EditorPrefs.GetString("SimpleBundlerBundleDir", defaultAssetBundleDirectory);
			string outputDirectory = EditorPrefs.GetString("SimpleBundlerOutputDir", defaultOutputDirectory);

			if (!Directory.Exists(assetBundleDirectory))
			{
				Directory.CreateDirectory(assetBundleDirectory);
			}

			if (!Directory.Exists(outputDirectory))
			{
				Directory.CreateDirectory(outputDirectory);
			}

			if (EditorPrefs.GetBool("SimpleBundlerClearOnBuild", false))
			{
				ClearDirectory(defaultOutputDirectory);
			}

			string[] directories = Directory.GetDirectories(assetBundleDirectory);
			foreach (var directory in directories)
			{
				var build = new AssetBundleBuild();
				List<string> filesInBuild = new List<string>();
				List<string> bundleAccessPaths = new List<string>();
				string bundleName = directory.Replace(assetBundleDirectory, "").Remove(0, 1);
				int variantStart = bundleName.IndexOf(".");

				string variant = null;
				if ((variantStart != -1) && ((variantStart + 1) < bundleName.Length))
				{
					variant = bundleName.Substring(variantStart + 1);
					bundleName = bundleName.Remove(variantStart);
				}
				build.assetBundleVariant = variant;

				build.assetBundleName = bundleName;
				AssignAssetBundles(build, filesInBuild, bundleAccessPaths, directory);
				build.assetNames = filesInBuild.ToArray();
				build.addressableNames = bundleAccessPaths.ToArray();
				builds.Add(build);
			}
			BuildPipeline.BuildAssetBundles(outputDirectory, builds.ToArray(), (BuildAssetBundleOptions)((int)assetBundleOptions / 2), EditorUserBuildSettings.activeBuildTarget);
			return builds;
		}

		private static void ClearDirectory(string directory)
		{
			var subdirectories = Directory.GetDirectories(directory);
			foreach (var sub in subdirectories)
			{
				ClearDirectory(sub);
				Directory.Delete(sub);
			}

			foreach (var file in Directory.GetFiles(directory))
			{
				File.Delete(file);
			}
		}

		private static void AssignAssetBundles(AssetBundleBuild build, List<string> filesInBuild, List<string> fileAccessPaths, string path)
		{
			path = path.Replace('/', '\\');
			var directories = Directory.GetDirectories(path);

			var files = Directory.GetFiles(path);
			foreach (var file in files)
			{
				string bundlePath = file;
				bundlePath = bundlePath.Remove(0, file.IndexOf(build.assetBundleName));
				if (!string.IsNullOrEmpty(build.assetBundleVariant))
				{
					bundlePath = bundlePath.Remove(bundlePath.IndexOf(build.assetBundleVariant) - 1, build.assetBundleVariant.Length);
				}

				AssetImporter importer = AssetImporter.GetAtPath(file);
				if (importer == null)
					continue;

				importer.assetBundleName = bundlePath;
				importer.assetBundleVariant = build.assetBundleVariant;
				fileAccessPaths.Add(importer.assetBundleName);
				filesInBuild.Add(file);
			}

			foreach (var directory in directories)
			{
				AssignAssetBundles(build, filesInBuild, fileAccessPaths, directory);
			}
		}

		private List<CachedBuildInfo> cachedInfo;

		private Vector2 buildResultsScroll;

		private BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;

		private BuildTarget targetPlatform;

		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Bundle Folder: ", GUILayout.MaxWidth(100));
				EditorPrefs.SetString("SimpleBundlerBundleDir", EditorGUILayout.TextField(EditorPrefs.GetString("SimpleBundlerBundleDir", defaultAssetBundleDirectory)));
				if (GUILayout.Button("...", GUILayout.Width(30)))
					EditorPrefs.SetString("SimpleBundlerBundleDir", (EditorUtility.OpenFolderPanel("Select an Asset Bundle Directory", EditorPrefs.GetString("SimpleBundlerBundleDir", defaultAssetBundleDirectory), "").Remove(0, Directory.GetCurrentDirectory().Length + 1)));
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Output Folder: ", GUILayout.MaxWidth(100));
				EditorPrefs.SetString("SimpleBundlerOutputDir", EditorGUILayout.TextField(EditorPrefs.GetString("SimpleBundlerOutputDir", defaultOutputDirectory)));
				if (GUILayout.Button("...", GUILayout.Width(30)))
				{
					EditorPrefs.SetString("SimpleBundlerOutputDir", (EditorUtility.OpenFolderPanel("Select an Output Directory", EditorPrefs.GetString("SimpleBundlerOutputDir", defaultAssetBundleDirectory), "").Remove(0, Directory.GetCurrentDirectory().Length + 1)));
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Clear Output Folder on Build:", GUILayout.MaxWidth(200));
				EditorPrefs.SetBool("SimpleBundlerClearOnBuild", EditorGUILayout.Toggle(EditorPrefs.GetBool("SimpleBundlerClearOnBuild", true)));
			}
			EditorGUILayout.EndHorizontal();

			buildOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumMaskPopup("Build Options: ", buildOptions);

			if (GUILayout.Button("Build AssetBundles"))
			{
				var buildInfo  = BuildAllAssetBundles(buildOptions);
				cachedInfo = new List<CachedBuildInfo>();
				for (var i = 0; i < buildInfo.Count; i++)
				{
					var info = new CachedBuildInfo();
					info.BundlePaths = new List<string>();
					info.LocalPaths = new List<string>();
					info.BuildName = buildInfo[i].assetBundleName;
					info.Variant = buildInfo[i].assetBundleVariant;
					for (var j = 0; j < buildInfo[i].addressableNames.Length; j++)
					{
						info.BundlePaths.Add(buildInfo[i].addressableNames[j]);
						info.LocalPaths.Add(buildInfo[i].assetNames[j]);
					}
					cachedInfo.Add(info);
				}
			}

			DisplayResults();
		}

		private void DisplayResults()
		{
			if (cachedInfo != null)
			{
				buildResultsScroll = EditorGUILayout.BeginScrollView(buildResultsScroll);
				GUIStyle buildNameLabel = new GUIStyle();
				buildNameLabel.fontSize = 16;
				buildNameLabel.fontStyle = FontStyle.Bold;

				foreach (var buildInfo in cachedInfo)
				{
					buildInfo.IsFoldedOut = (string.IsNullOrEmpty(buildInfo.Variant)) ?
						EditorGUILayout.Foldout(buildInfo.IsFoldedOut, buildInfo.BuildName) :
						EditorGUILayout.Foldout(buildInfo.IsFoldedOut, buildInfo.BuildName + " [" + buildInfo.Variant + "]");
					if (buildInfo.IsFoldedOut)
					{
						buildInfo.Scroll = EditorGUILayout.BeginScrollView(buildInfo.Scroll, true, true);
						{
							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField("Local Path", EditorStyles.boldLabel);
								EditorGUILayout.LabelField("Bundle Path", EditorStyles.boldLabel);
							}
							EditorGUILayout.EndHorizontal();

							for (var i = 0; i < buildInfo.LocalPaths.Count; i++)
							{
								EditorGUILayout.BeginHorizontal();
								{
									EditorGUILayout.LabelField(new GUIContent(buildInfo.LocalPaths[i], buildInfo.LocalPaths[i]));
									EditorGUILayout.LabelField(new GUIContent(buildInfo.BundlePaths[i], buildInfo.BundlePaths[i]));
								}
								EditorGUILayout.EndHorizontal();

							}
						}
						EditorGUILayout.EndScrollView();
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}

		[Serializable]
		public class CachedBuildInfo
		{
			public string BuildName { get; set; }
			public string Variant { get; set; }
			public List<string> LocalPaths { get; set; }
			public List<string> BundlePaths { get; set; }
			public Vector2 Scroll { get; set; }
			public bool IsFoldedOut { get; set; }
		}

	}
}
