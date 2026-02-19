using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Gearstorm.Content.Systems
{
    public static class ShimmerPhaseSystem
    {
        /// <summary>
        /// Gerencia a lógica de atravessar paredes horizontalmente sem afetar a gravidade/chão.
        /// </summary>
        public static void HandleHorizontalPhasing(Projectile projectile, int timer)
        {
            if (timer <= 0)
            {
                // Se o efeito acabou, garantimos que a colisão volte ao normal
                if (!projectile.tileCollide) projectile.tileCollide = true;
                return;
            }

            // Partículas visuais
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.ShimmerSpark);
                d.velocity *= 0.1f;
                d.noGravity = true;
                d.alpha = 130;
                d.scale = 0.8f;
            }

            // LÓGICA DE FÍSICA
            // Checamos a colisão na posição futura X
            Vector2 horizontalCheck = projectile.position + new Vector2(projectile.velocity.X, 0);
            
            bool insideGround = Collision.SolidCollision(projectile.position, projectile.width, projectile.height);
            bool wallAhead = Collision.SolidCollision(horizontalCheck, projectile.width, projectile.height);

            if (wallAhead || insideGround)
            {
                // Desliga a colisão para permitir o movimento através do bloco
                projectile.tileCollide = false;
                
                // Se estiver dentro de um bloco, empurra para cima (Anti-Stuck)
                if (insideGround)
                {
                    projectile.position.Y -= 1.2f; 
                    if (projectile.velocity.Y > 0) projectile.velocity.Y *= 0.7f;
                }
            }
            else
            {
                // Se o caminho horizontal estiver limpo, volta a colidir (respeitando o chão)
                projectile.tileCollide = true;
            }
        }

        /// <summary>
        /// Deve ser chamado no OnTileCollide da base. 
        /// Retorna 'false' se a colisão deve ser ignorada (atravessar parede).
        /// </summary>
        public static bool ShouldIgnoreTileCollision(Projectile projectile, Vector2 oldVelocity, int timer)
        {
            if (timer > 0)
            {
                // Ignora colisão se for apenas horizontal (X mudou, mas Y bateu igual)
                if (projectile.velocity.X != oldVelocity.X && projectile.velocity.Y == oldVelocity.Y)
                {
                    return true; // Ignora
                }
            }
            return false; // Colisão normal
        }
    }
}