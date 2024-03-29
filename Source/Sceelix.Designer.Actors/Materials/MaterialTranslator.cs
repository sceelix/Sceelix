﻿using DigitalRune.Graphics.SceneGraph;
using Sceelix.Actors.Data;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Actors.Materials
{
    public abstract class MaterialTranslator
    {
        public abstract SceneNode CreateSceneNode(ContentLoader contentLoader, Material sceelixMaterial, object data);
    }
}