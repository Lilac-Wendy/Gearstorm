﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Gearstorm.Content.Systems.Primitives
{
    public class PrimitiveLighting : PrimitiveTrail
    {
        private Vector2 start;
        private Vector2 end;
        private Color color;

        private Vector2[] points;

        private int segments;
        private float thickness;
        private int lifetime;
        private int timeLeft;

        private float noiseStrength;
        private float seed;
        public new bool IsAlive => timeLeft > 0;

        public PrimitiveLighting(
            Vector2 start,
            Vector2 end,
            Color color,
            int segments = 20,
            float thickness = 14f,
            int lifetime = 8,
            float noiseStrength = 30f)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.segments = segments;
            this.thickness = thickness;
            this.lifetime = lifetime;
            this.timeLeft = lifetime;
            this.noiseStrength = noiseStrength;

            seed = Main.rand.NextFloat(0f, 1000f);

            points = new Vector2[segments + 1];

            RebuildPoints();

            TrailRenderer.Register(this);
        }

        public override void Update()
        {
            timeLeft--;

            if (timeLeft <= 0)
            {
                return;
            }
            RebuildPoints();
        }

        private void RebuildPoints()
        {
            Vector2 direction = end - start;
            float length = direction.Length();

            if (length <= 0.001f)
                return;

            direction.Normalize();
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);

            for (int i = 0; i <= segments; i++)
            {
                float progress = i / (float)segments;

                Vector2 basePos = Vector2.Lerp(start, end, progress);

                float centerFade = 1f - Math.Abs(progress - 0.5f) * 2f;

                float noise = (float)Math.Sin(
                    progress * 12f +
                    seed +
                    Main.GlobalTimeWrappedHourly * 25f);

                float offset = noise * noiseStrength * centerFade;

                points[i] = basePos + normal * offset;
            }
        }

        internal override int BuildVertexBuffer(out VertexPositionColor[] vertices)
        {
            vertices = null;

            if (points == null || points.Length < 2)
                return 0;

            int vertCount = points.Length * 2;
            vertices = new VertexPositionColor[vertCount];

            for (int i = 0; i < points.Length; i++)
            {
                float progress = i / (float)(points.Length - 1);

                Vector2 dir;
                if (i == points.Length - 1)
                    dir = points[i] - points[i - 1];
                else
                    dir = points[i + 1] - points[i];

                if (dir != Vector2.Zero)
                    dir.Normalize();

                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float width = thickness * (1f - progress * 0.8f);

                Vector2 left = points[i] - normal * width * 0.5f;
                Vector2 right = points[i] + normal * width * 0.5f;

                Color segmentColor = Color.Lerp(Color.White, color, progress);
                segmentColor *= (float)timeLeft / lifetime;

                vertices[i * 2] = new VertexPositionColor(
                    new Vector3(left, 0f),
                    segmentColor);

                vertices[i * 2 + 1] = new VertexPositionColor(
                    new Vector3(right, 0f),
                    segmentColor);
            }

            return vertCount;
        }
    }
}