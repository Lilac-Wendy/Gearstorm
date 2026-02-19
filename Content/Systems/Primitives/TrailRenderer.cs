using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.Systems.Primitives
{
    public class TrailRenderer : ModSystem
    {
        private static readonly List<PrimitiveTrail> Trails = new();
        private static BasicEffect effect;
        private static void EnsureEffect()
        {
            if (effect != null)
                return;

            if (Main.instance?.GraphicsDevice == null)
                return;

            effect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true
            };
        }
        public static void Register(PrimitiveTrail trail)
        {
            if (!Trails.Contains(trail))
                Trails.Add(trail);
        }
        public static void Unregister(PrimitiveTrail trail)
        {
            Trails.Remove(trail);
        }
        public override void Load()
        {
            if (!Main.dedServ)
            {

            }
        }
        public override void PostUpdateEverything()
        {
            for (int i = Trails.Count - 1; i >= 0; i--)
            {
                Trails[i].Update();

                if (!Trails[i].IsAlive)
                    Trails.RemoveAt(i);
            }
        }
        public override void PostDrawTiles()
{
    if (Main.dedServ)
        return;

    EnsureEffect();
    if (effect == null)
        return;
    GraphicsDevice device = Main.instance.GraphicsDevice;
    
    // Configuramos os estados que são comuns aos dois passes
    device.RasterizerState = RasterizerState.CullNone;
    device.DepthStencilState = DepthStencilState.None;

    Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
    Vector2 worldCenter = Main.screenPosition + screenCenter;

    effect.View = Matrix.CreateTranslation(-worldCenter.X, -worldCenter.Y, 0) *
                  Matrix.CreateScale(Main.GameZoomTarget, Main.GameZoomTarget, 1) *
                  Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0);
    
    effect.Projection = Matrix.CreateOrthographicOffCenter(
        0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
    BlendState originalBlend = device.BlendState;

    foreach (var trail in Trails)
    {
        int count = trail.BuildVertexBuffer(out var buffer);
        if (count < 3)
            continue;

        device.BlendState = BlendState.AlphaBlend;
        
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, buffer, 0, count - 2);
        }
        // +
        device.BlendState = BlendState.Additive;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, buffer, 0, count - 2);
        }
    }
    device.BlendState = originalBlend;
}
        }

    }

