﻿using OpenBveApi.Colors;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    internal static partial class Renderer
    {
        /// <summary>
        /// Performs a reset of OpenGL to the default state
        /// </summary>
        private static void ResetOpenGlState()
        {
            LastBoundTexture = null;
            GL.Enable(EnableCap.CullFace); CullEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Texture2D); TexturingEnabled = false;
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Blend); BlendEnabled = false;
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f }); EmissiveEnabled = false;
            SetAlphaFunc(AlphaFunction.Greater, 0.9f);
        }

        /// <summary>
        /// Specifies the OpenGL alpha function to perform
        /// </summary>
        /// <param name="Comparison">The comparison to use</param>
        /// <param name="Value">The value to compare</param>
        private static void SetAlphaFunc(AlphaFunction Comparison, float Value)
        {
            AlphaTestEnabled = true;
            AlphaFuncComparison = Comparison;
            AlphaFuncValue = Value;
            GL.AlphaFunc(Comparison, Value);
            GL.Enable(EnableCap.AlphaTest);
        }

        /// <summary>
        /// Disables OpenGL alpha testing
        /// </summary>
        private static void UnsetAlphaFunc()
        {
            AlphaTestEnabled = false;
            GL.Disable(EnableCap.AlphaTest);
        }

        /// <summary>
        /// Restores the OpenGL alpha function to it's previous state
        /// </summary>
        private static void RestoreAlphaFunc()
        {
            if (AlphaTestEnabled)
            {
                GL.AlphaFunc(AlphaFuncComparison, AlphaFuncValue);
                GL.Enable(EnableCap.AlphaTest);
            }
            else
            {
                GL.Disable(EnableCap.AlphaTest);
            }
        }

        /// <summary>
        /// Clears all currently registered OpenGL display lists
        /// </summary>
        internal static void ClearDisplayLists()
        {
            for (int i = 0; i < StaticOpaque.Length; i++)
            {
                if (StaticOpaque[i] != null)
                {
                    if (StaticOpaque[i].OpenGlDisplayListAvailable)
                    {
                        GL.DeleteLists(StaticOpaque[i].OpenGlDisplayList, 1);
                        StaticOpaque[i].OpenGlDisplayListAvailable = false;
                    }
                }
            }
            StaticOpaqueForceUpdate = true;
        }

        /// <summary>
        /// Resets the state of the renderer
        /// </summary>
        internal static void Reset()
        {
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
            Objects = new Object[256];
            ObjectCount = 0;
            StaticOpaque = new ObjectGroup[] { };
            StaticOpaqueForceUpdate = true;
            DynamicOpaque = new ObjectList();
            DynamicAlpha = new ObjectList();
            OverlayOpaque = new ObjectList();
            OverlayAlpha = new ObjectList();
            OptionLighting = true;
            OptionAmbientColor = new Color24(160, 160, 160);
            OptionDiffuseColor = new Color24(160, 160, 160);
            OptionLightPosition = new World.Vector3Df(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
            OptionLightingResultingAmount = 1.0f;
            OptionClock = false;
            OptionBrakeSystems = false;
        }

        /// <summary>
        /// Call this once to initialise the renderer
        /// </summary>
        internal static void Initialize()
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Hint(HintTarget.FogHint, HintMode.Fastest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
            GL.Disable(EnableCap.Dither);
            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.CullFace); CullEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Texture2D); TexturingEnabled = false;
            Interface.LoadHUD();
            string Path = Program.FileSystem.GetDataFolder("In-game");
            Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Path, "logo.png"), out TextureLogo);
            Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend); BlendEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Fog);
        }

        /// <summary>
        /// De-initialize the renderer, and clear all remaining OpenGL display lists
        /// </summary>
        internal static void Deinitialize()
        {
            ClearDisplayLists();
        }

        /// <summary>
        /// Initializes the lighting
        /// </summary>
        internal static void InitializeLighting()
        {
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
            GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            GL.CullFace(CullFaceMode.Front); CullEnabled = true; // possibly undocumented, but required for correct lighting
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
            GL.ShadeModel(ShadingModel.Smooth);
            float x = ((float)OptionAmbientColor.R + (float)OptionAmbientColor.G + (float)OptionAmbientColor.B);
            float y = ((float)OptionDiffuseColor.R + (float)OptionDiffuseColor.G + (float)OptionDiffuseColor.B);
            if (x < y) x = y;
            OptionLightingResultingAmount = 0.00208333333333333f * x;
            if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
            GL.Enable(EnableCap.Lighting); LightingEnabled = true;
            GL.DepthFunc(DepthFunction.Lequal);
        }
    }
}