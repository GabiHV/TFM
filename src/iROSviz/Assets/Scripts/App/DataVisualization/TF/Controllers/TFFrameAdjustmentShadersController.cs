using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class TFFrameAdjustmentShadersController : TFFrameShadersController
{

    public Shader adjustmentShader;

    private Dictionary<Transform, Dictionary<Material, Shader>> adjustmentShaders;

    void Start()
    {
        StoreOriginalShaders();
        GetAdjustmentShaders();
    }

    private void GetAdjustmentShaders()
    {
        var orgShaders = 
            GameObjectHelper.GetChildrenShaders(TFFrameObj, shaderExceptions.ToHashSet());
        var newShaders = GameObjectHelper.ChangeChildrenShaders(orgShaders, adjustmentShader);
        adjustmentShaders = newShaders;
    }

    public void Highlight() =>
        GameObjectHelper.ApplyShaderToChildrenGameObject(TFFrameObj, adjustmentShaders);
}
