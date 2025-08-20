using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.Data.Runtime;
using tasks_and_resources.Core;

namespace Game.Data.Editor
{
    public static class CreateGameActionsAndResources
    {
        [MenuItem("Tools/Create Game Actions and Resources")]
        public static void Create()
        {
            ResourceRegistry.Init();

            var createdGameResource = CreateGameResources();

            if (CreateGameActions(createdGameResource)) return;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static bool CreateGameActions(Dictionary<string, GameResource> createdGameResources)
        {
            string gameActionsFolder = $"{GetSelectedFolderPath()}/GameActions";
            if (string.IsNullOrEmpty(gameActionsFolder))
            {
                Debug.LogError("Could not resolve a target folder.");
                return true;
            }

            // Example: create one asset per resource in your registry
            for (int i = 0; i < ResourceRegistry.TaskRegistry.Count; i++)
            {
                var task = ResourceRegistry.TaskRegistry.ElementAt(i);

                // TODO: replace `GameResource` with your ScriptableObject type
                var so = ScriptableObject.CreateInstance<GameAction>();

                so.consumes = new Dictionary<GameResource, int>();
                foreach (var consumes in task.Value.Consumes)
                {
                    so.consumes.Add(createdGameResources[consumes.Key.Name], consumes.Value);
                }
                so.produces = new Dictionary<GameResource, int>();
                foreach (var produces in task.Value.Produces)
                {
                    so.produces.Add(createdGameResources[produces.Key.Name], produces.Value);
                }
                
                // Optional: initialize fields on `so` from `resource` here

                string fileName = $"{Sanitize(task.Key)}.asset";
                string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(gameActionsFolder, fileName));
                AssetDatabase.CreateAsset(so, path);
            }

            return false;
        }
        static Dictionary<string, GameResource> CreateGameResources()
        {
            Dictionary<string, GameResource> createdGameResources = new Dictionary<string, GameResource>();
            
            string gameResourcesFolder = $"{GetSelectedFolderPath()}/GameResources";
            if (string.IsNullOrEmpty(gameResourcesFolder))
            {
                Debug.LogError("Could not resolve a target folder.");
                return null;
            }

            // Example: create one asset per resource in your registry
            for (int i = 0; i < ResourceRegistry.ResourcesRegistry.Count; i++)
            {
                var resource = ResourceRegistry.ResourcesRegistry.ElementAt(i);

                // TODO: replace `GameResource` with your ScriptableObject type
                var so = ScriptableObject.CreateInstance<GameResource>();
                createdGameResources.Add(resource.Key, so);
                // Optional: initialize fields on `so` from `resource` here

                string fileName = $"{Sanitize(resource.Key)}.asset";
                string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(gameResourcesFolder, fileName));
                AssetDatabase.CreateAsset(so, path);
            }

            return createdGameResources;
        }

        /// <summary>
        /// Returns a project-relative folder path like "Assets/Some/Subfolder".
        /// If a file is selected, returns its containing folder.
        /// If nothing valid is selected, returns "Assets".
        /// </summary>
        static string GetSelectedFolderPath()
        {
            var obj = Selection.activeObject;
            if (obj == null)
                return "Assets";

            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
                return "Assets";

            if (Directory.Exists(path)) // a folder is selected
                return path;

            // an asset is selected -> get its parent folder
            string dir = Path.GetDirectoryName(path)?.Replace('\\', '/');
            return string.IsNullOrEmpty(dir) ? "Assets" : dir;
        }

        static string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "_");
            return name;
        }
        
    }
}
