using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class TFFrameShadersController : MonoBehaviour
{
    private Dictionary<Transform, Dictionary<Material, Shader>> defaultShaders;

    public GameObject TFFrameObj;
    public List<Transform> shaderExceptions;

    void Start() =>
        StoreOriginalShaders();

    protected void StoreOriginalShaders() =>
        defaultShaders = GameObjectHelper.GetChildrenShaders(TFFrameObj, shaderExceptions.ToHashSet());
    
    public void Unhighlight() =>
        GameObjectHelper.ApplyShaderToChildrenGameObject(TFFrameObj, defaultShaders);
}
