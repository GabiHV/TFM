using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using App.Utilities;

public class TFFrameMeasurementShadersController : TFFrameShadersController
{
    public Shader measurementShader;

    private Dictionary<Transform, Dictionary<Material, Shader>> measurementShaders;

    void Start()
    {
        StoreOriginalShaders();
        GetMeasurementShaders();
    }

    private void GetMeasurementShaders()
    {
        var orgShaders = 
            GameObjectHelper.GetChildrenShaders(TFFrameObj, shaderExceptions.ToHashSet());
        var newShaders = GameObjectHelper.ChangeChildrenShaders(orgShaders, measurementShader);
        measurementShaders = newShaders;
    }

    public void Highlight() =>
        GameObjectHelper.ApplyShaderToChildrenGameObject(TFFrameObj, measurementShaders);
}
