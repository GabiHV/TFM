using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace App.Utilities
{
    public static class GameObjectHelper
    {
        /// <summary>
        /// Function that applies a specific shader to all childer GameObject.
        /// </summary>
        /// <param name="go">GameObject to apply shader</param>
        /// <param name="shaders">Dictionary that contains each material shader from a specific object</param>
        public static void ApplyShaderToChildrenGameObject(
            GameObject go, 
            Dictionary<Transform, Dictionary<Material, Shader>> shaders
        )
        {
            Dictionary<Transform, Renderer> renderers = GetChildrenRenderers(go);
            foreach (var (transform, renderer) in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];

                    if(shaders.ContainsKey(transform) && 
                        shaders[transform].ContainsKey(material))
                            material.shader = shaders[transform][material];
                }
            }
        }

        /// <summary>
        /// Function that gets children shaders
        /// </summary>
        /// <param name="go">Game Objects to get children shaders</param>
        /// <param name="exceptions">Skipped branches</param>
        /// <returns>Dictionawy with material shader for each child transform</returns>
        public static Dictionary<Transform, Dictionary<Material, Shader>> GetChildrenShaders(
            GameObject go,
            HashSet<Transform> exceptions = null
        )
        {
            Dictionary<Transform, Dictionary<Material, Shader>> shaders = new();

            GetChildrenShadersRecursive(go, exceptions, shaders);

            return shaders;
        }

        /// <summary>
        /// Function that changes children shaders
        /// </summary>
        /// <param name="currentShaders">Dictionary with each material shader for each child transform</param>
        /// <param name="newShader">New shader to apply</param>
        /// <returns></returns>
        public static Dictionary<Transform, Dictionary<Material, Shader>> ChangeChildrenShaders(
            Dictionary<Transform, Dictionary<Material, Shader>> currentShaders,
            Shader newShader
        )
        {
            Dictionary<Transform, Dictionary<Material, Shader>> newShaders = new();
            foreach (var (transform, materialShader) in currentShaders)
            {
                if(!newShaders.ContainsKey(transform))
                    newShaders.Add(transform, new());

                foreach (var (material, _) in materialShader)
                {
                    if(!newShaders[transform].ContainsKey(material))
                        newShaders[transform].Add(material, newShader);
                }
            }
            return newShaders;
        }

        private static void GetChildrenShadersRecursive(
            GameObject go,
            HashSet<Transform> exceptions,
            Dictionary<Transform, Dictionary<Material, Shader>> dict
        )
        {
            if(go == null) return;

            Transform t = go.transform;

            if(exceptions != null && exceptions.Contains(t)) return;

            AddObjectMaterialShaderToDict(t, dict);

            if(t.childCount == 0) return;

            foreach(Transform child in t)
            {
                GetChildrenShadersRecursive(child?.gameObject, exceptions, dict);
            }
        }

        private static void AddObjectMaterialShaderToDict(
            Transform t, 
            Dictionary<Transform, Dictionary<Material, Shader>> dict
        )
        {
            if(!t.TryGetComponent<Renderer>(out Renderer renderer)) return;

            int transformID = t.GetInstanceID();

            if(!dict.ContainsKey(t))
                dict.Add(t, new());
            
            foreach (Material m in renderer.materials)
            {
                if(!dict[t].ContainsKey(m))
                    dict[t].Add(m, m.shader);          
            }
        }

        private static Dictionary<Transform, Renderer> GetChildrenRenderers(GameObject go)
        {
            Dictionary<Transform, Renderer> renderers = new();

            GetChildrenRenderersRecursive(go, renderers);

            return renderers;
        }

        private static void GetChildrenRenderersRecursive(
            GameObject go, 
            Dictionary<Transform, Renderer> dict
        )
        {
            if(go == null) return;
            Transform t = go.transform;

            AddObjectRendererToDict(t, dict);
            
            if(t.childCount == 0) return;
            foreach (Transform child in t)
            {
                GetChildrenRenderersRecursive(child?.gameObject, dict);
            }

        }

        private static void AddObjectRendererToDict(
            Transform t, 
            Dictionary<Transform, Renderer> dict
        )
        {
            if(!t.TryGetComponent<Renderer>(out Renderer renderer)) return;

            if(!dict.ContainsKey(t))
                dict.Add(t, renderer);
        }
    }
}
