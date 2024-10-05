using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Editor.Tools.FigmaImporter
{
    [Serializable]
    public class FigmaNodeConfig
    {
        [SerializeField]
        private string _unityExportPath;
        
        [SerializeField]
        private string _nodeToken;
        
        public string UnityExportPath => _unityExportPath;
        public string NodeToken => _nodeToken;
    }
    
    [CreateAssetMenu(menuName = "FigmaImporter/Node Importer", fileName = "FigmaNodeImporter", order = 0)]
    public class FigmaNodeImporter : ScriptableObject
    {
        [SerializeField]
        private string _figmaToken;

        [SerializeField]
        private string _figmaProjectId;

        [SerializeField]
        private FigmaNodeConfig[] _figmaNodeConfig;
        
        private const string TEXTURE_FORMAT = "png";

        private static CancellationTokenSource _cancellationTokenSource;

        public void Import()
        {
            Cancel();
            DoAsync().Forget();

            async UniTask DoAsync()
            {
                foreach (var nodeConfig in _figmaNodeConfig)
                {
                    await Do(nodeConfig);
                }
            }
        }

        private static void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            EditorUtility.ClearProgressBar();
        }

        private JObject FindDocumentNode(JObject jObject)
        {
            if (jObject.ContainsKey("document"))
            {
                return jObject["document"] as JObject;
            }

            foreach (var kvp in jObject)
            {
                if (kvp.Value is JObject jObjectValue)
                {
                    var result = FindDocumentNode(jObjectValue);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private async UniTask Do(FigmaNodeConfig figmaNodeConfig)
        {
            Debug.Log("---- Begin ----");
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                AssetDatabase.StartAssetEditing();
                //
                var existingFilesWithoutExtensions = Directory
                    .GetFiles(figmaNodeConfig.UnityExportPath, $"*.{TEXTURE_FORMAT}", SearchOption.AllDirectories)
                    .Select(path => Path.ChangeExtension(path, null))
                    .ToList();
                
                var figmaFile = await DownloadFigmaFile(figmaNodeConfig.NodeToken);
                
                var documentNode = FindDocumentNode(figmaFile);
                
                var imageUrls = await DownloadImageUrls(documentNode.ToObject<FigmaDocument>(), figmaNodeConfig.UnityExportPath);
                var total = imageUrls.Count;
                while (imageUrls.Count > 0)
                {
                    var (path, url) = imageUrls.First();
                    Debug.Log($"{path} {url}");
                    imageUrls.Remove(path);
                    var texture2D = await DownloadImage(url);
                    var progress = (total - imageUrls.Count) / (float) total;
                    EditorUtility.DisplayProgressBar($"Downloaded", $"{(int)(100 * progress)}% {path}", progress);
                    if (existingFilesWithoutExtensions.Contains(path))
                    {
                        existingFilesWithoutExtensions.Remove(path);
                        Debug.Log($"UPDATE {path}");
                    }
                    else
                    {
                        Debug.Log($"CREATE {path}");
                    }
                    Save(path, TEXTURE_FORMAT, texture2D.EncodeToPNG());
                }
                
                foreach (var path in existingFilesWithoutExtensions)
                {
                    Debug.LogWarning($"DELETE {path}");
                    File.Delete($"{path}.{TEXTURE_FORMAT}");
                    File.Delete($"{path}.{TEXTURE_FORMAT}.meta");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            Debug.Log("---- Done ----");
        }

        private async UniTask<JObject> DownloadFigmaFile(string node)
        {
            var request = UnityWebRequest.Get($"https://api.figma.com/v1/files/{_figmaProjectId}/nodes?ids={node}&depth=2");
            request.SetRequestHeader("X-Figma-Token", _figmaToken);
            
            EditorUtility.DisplayProgressBar($"Downloading figma file {_figmaProjectId}", "Downloading...", 0);
            await request.SendWebRequest().ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            EditorUtility.DisplayProgressBar($"Downloading figma file {_figmaProjectId}", "Downloading... done", 1);

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
            }

            throw new Exception($"Error downloading Figma project: {request.error}");
        }

        // full_path, url
        private async UniTask<Dictionary<string, string>> DownloadImageUrls(FigmaDocument figmaDocument, string unityExportPath)
        {
            var nodes = new List<FigmaNode>();
            foreach (var node in figmaDocument.Children)
            {
                CollectNodesOnDepth(node, 0, unityExportPath, nodes);
            }
            
            var commaSeparatedVisibleNodeIds = string.Join(',', nodes.Where(node => node.Visible).Select(x => x.Id));
            var request = UnityWebRequest.Get($"https://api.figma.com/v1/images/{_figmaProjectId}?ids={commaSeparatedVisibleNodeIds}&format=png");
            request.SetRequestHeader("X-Figma-Token", _figmaToken);
            
            EditorUtility.DisplayProgressBar($"Downloading images urls", "Downloading...", 0);
            await request.SendWebRequest().ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            EditorUtility.DisplayProgressBar($"Downloading images urls", "Downloading... done", 1);
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var data = request.downloadHandler.text;
                var figmaImages = JsonConvert.DeserializeObject<FigmaImages>(data);
                return figmaImages.Images
                    .ToDictionary(
                        kvp => nodes.First(node => node.Id == kvp.Key).Path,
                        kvp => kvp.Value);
            }

            throw new Exception($"Error downloading image:");
        }

        private void CollectNodesOnDepth(FigmaNode node, int depth, string path, List<FigmaNode> nodes)
        {
            var nodeName = GetNodeName(node);
            node.Path = Path.Combine(path, nodeName);

            if (depth >= 0 && node.Type != "COMPONENT_SET")
            {
                nodes.Add(node);
                return;
            }

            foreach (var child in node.Children)
            {
                CollectNodesOnDepth(child, depth + 1, node.Path, nodes);
            }
        }

        private string GetNodeName(FigmaNode node)
        {
            if (node.Type == "COMPONENT")
            {
                // "Property 1=active", => "active"
                return node.Name.Split('=').Skip(1).FirstOrDefault() ?? node.Name;
            }
            else
            {
                return node.Name;
            }
        }

        private async UniTask<Texture2D> DownloadImage(string url)
        {
            var request = UnityWebRequestTexture.GetTexture(url);
            await request.SendWebRequest().ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            if (request.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                return texture;
            }

            throw new Exception($"Error downloading image: {url} {request.error}");
        }

        private void Save(string fullPath, string extension, byte[] bytes)
        {
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fullPathWithExtension = $"{fullPath}.{extension}";
            File.WriteAllBytes(fullPathWithExtension, bytes);
        }
    }
}
