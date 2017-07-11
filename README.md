# SimpleAssetBundler
A simple editor script for Unity3D that creates AssetBundles with a hierarchy mirroring the folder they're in.

## Installation:
Just copy the entire SimpleAssetBundler folder into the Assets folder of your project. A subfolder of the Assets folder is also okay.

## Usage:
Once the script is installed, you can open the window by going to the "Assets/AssetBundles/Simple Asset Bundler" menu which should now be visible in your Unity3D editor. The bundler only has four settings:
#### Bundle Folder: 
This is the root folder where your assets are kept. It is a path relative to your game's project folder. Each folder in this root folder will be turned into a bundle.
#### Output Folder: 
Once the bundles are assembled, they will be placed in a folder at this path. If it does not exist, it will be created.
#### Clear Output Folder On Build:
If this box is checked, all files in the output folder will be deleted before new ones are created. This keeps your output folder clean.
#### Build Options:
Any number of these options may be selected. Unity will warn you if you select incompatable options. For more information on these options, please refer to the Unity documentation at https://docs.unity3d.com/ScriptReference/BuildAssetBundleOptions.html

Once you press the "Build AssetBundles" button, the AssetBundles will be built for the target platform you have selected in your build settings. For each AssetBundle, a foldout will appear where you can view the files that were placed in the bundle, and their path inside the bundle. The path to the asset in the bundle will mirror the structure of your folders. So, for example, your asset is in "Assets/Bundles/Base/Prefabs/Character.Prefab", and "Assets/Bundles" is set as your Bundle Folder path, the script will create a bundle named Base, and your asset would be in that bundle at "Prefabs/Character.Prefab".

## Variants:
Variants are simple to add. Just add a period to your bundle's root folder, followed by the name of the variant. For example, if you want your bundle to be named "base", and you have two variants, "highres" and "lowres", your bundle folders would be named "base.highres" and "base.lowres" respectively. To function properly with code, the folder structure of each variant of a bundle should be the same, with the files and folders named exactly the same other than the root folder.
