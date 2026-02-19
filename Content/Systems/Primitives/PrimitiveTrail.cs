using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;


namespace Gearstorm.Content.Systems.Primitives
{
    public class PrimitiveTrail
    {
        private struct TrailPoint
        {
            public Vector2 Position;
            public float Life;
            public float MaxLife;
            public float Opacity => Life / MaxLife;
        }

        private readonly List<TrailPoint> points = new();
        private VertexPositionColor[] vertexBuffer = Array.Empty<VertexPositionColor>();

        private const int MAX_POINTS = 128;

        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;

        public bool IsAlive => points.Count > 0;

        public PrimitiveTrail()
        {
            TrailRenderer.Register(this);
        }

        public void Dispose()
        {
            TrailRenderer.Unregister(this);
        }

        public void AddPoint(Vector2 position, float lifetime = 280f)
        {
            if (points.Count >= MAX_POINTS)
                points.RemoveAt(0);

            points.Add(new TrailPoint
            {
                Position = position,
                Life = lifetime,
                MaxLife = lifetime
            });
        }

        public virtual void Update()
        {
            for (int i = points.Count - 1; i >= 0; i--)
            {
                TrailPoint p = points[i];
                p.Life--;

                if (p.Life <= 0)
                    points.RemoveAt(i);
                else
                    points[i] = p;
            }
        }

        internal virtual int BuildVertexBuffer(out VertexPositionColor[] buffer)
        {
            
            buffer = Array.Empty<VertexPositionColor>();

            if (points.Count < 2)
                return 0;

            int neededVertices = points.Count * 2;

            if (vertexBuffer.Length < neededVertices)
                vertexBuffer = new VertexPositionColor[neededVertices];

            int idx = 0;

            for (int i = 0; i < points.Count; i++)
            {
                float progress = i / (float)(points.Count - 1);

                Vector2 current = points[i].Position;

                Vector2 direction;

                if (i == 0)
                    direction = points[1].Position - points[0].Position;
                else if (i == points.Count - 1)
                    direction = points[i].Position - points[i - 1].Position;
                else
                    direction = points[i + 1].Position - points[i - 1].Position;

                if (direction.LengthSquared() < 0.001f)
                    direction = Vector2.UnitX;

                direction.Normalize();

                Vector2 normal = new Vector2(-direction.Y, direction.X);

                float width = WidthFunction?.Invoke(progress) ?? 10f;
                Color baseColor = ColorFunction?.Invoke(progress) ?? Color.White;
                float pointOpacity = points[i].Opacity; 
                
                float finalAlpha = (baseColor.A / 255f) * pointOpacity;
                Color finalColor = baseColor * finalAlpha; 

                Vector2 offset = normal * (width * 0.5f);

                vertexBuffer[idx++] = new VertexPositionColor(new Vector3(current - offset, 0f), finalColor);
                vertexBuffer[idx++] = new VertexPositionColor(new Vector3(current + offset, 0f), finalColor);
            }


            buffer = vertexBuffer;
            return idx;
        }
    }
}
