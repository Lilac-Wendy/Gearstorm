using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Players
{
    public class AirTrekPlayer : ModPlayer
    {
        // ==================== PROPRIEDADES ====================
        public bool AirTrekActive = false;
        public float Momentum = 0f;
        public int ComboPoints = 0;
        public int ComboCounter = 0;

        public bool isOnRail = false;
        public bool isOnChain = false;
        public bool isStomping = false;

        private int chainReleaseCooldown = 0;
        private int stompCooldown = 0;
        private const float MAX_MOMENTUM = 100f;

        public override void ResetEffects()
        {
            if (!AirTrekActive)
            {
                isOnRail = false;
                isOnChain = false;
                isStomping = false;
                Momentum = 0f;
            }
        }

        public override void PostUpdate()
        {
            if (!AirTrekActive) return;

            if (chainReleaseCooldown > 0) chainReleaseCooldown--;
            if (stompCooldown > 0) stompCooldown--;

            if (Player.velocity.Length() < 1f && !isOnRail && !isOnChain)
                Momentum = Math.Max(Momentum - 0.2f, 0f);
        }

        public override void PostUpdateRunSpeeds()
        {
            if (!AirTrekActive || Player.mount.Active) return;

            // Lógica de Chão (Botas de Velocidade)
            if (Player.velocity.Y == 0 && !isOnRail && !isOnChain)
            {
                float boost = 1f + (Momentum / 35f);
                Player.accRunSpeed += boost;
                Player.maxRunSpeed += boost * 2.5f;

                if (Math.Abs(Player.velocity.X) > 4f)
                    Momentum = Math.Min(Momentum + 0.4f, MAX_MOMENTUM);
            }

            if (isOnRail) { Player.runSlowdown = 0f; Player.runAcceleration = 0f; }
            if (isOnChain) { Player.gravity = 0f; Player.maxFallSpeed = 0f; }
        }

        public override void PreUpdateMovement()
        {
            if (!AirTrekActive || Player.mount.Active) return;

            HandleChainLogic();
            HandleRailLogic();
            HandleStompLogic();
        }

        // ==================== LÓGICA DE CORDAS (REVISADA) ====================
        private void HandleChainLogic()
        {
            if (chainReleaseCooldown > 0) { isOnChain = false; return; }

            Point tileCoords = Player.Center.ToTileCoordinates();
            Tile tile = Framing.GetTileSafely(tileCoords);
            
            // VERIFICAÇÃO SEGURA DE CORDA (Evita o IndexOutOfRangeException)
            bool isRope = tile.HasTile && tile.TileType < Main.tileRope.Length && Main.tileRope[tile.TileType];

            if (isRope)
            {
                // 1. DASH DE SAÍDA (SHIFT)
                if (isOnChain && Player.controlHook)
                {
                    isOnChain = false;
                    chainReleaseCooldown = 25;
                    float power = 11f + (Momentum / 8f);
                    Player.velocity.X = Player.direction * power;
                    Player.velocity.Y = -4f;
                    Momentum = Math.Max(0, Momentum - 15f);
                    SoundEngine.PlaySound(SoundID.Item38, Player.Center);
                    return;
                }

                // 2. ENTRADA AUTOMÁTICA (AIR GEAR STYLE)
                if (!isOnChain)
                {
                    bool fastEnough = Math.Abs(Player.velocity.X) > 5f || isOnRail;
                    if (fastEnough || Player.controlDown)
                    {
                        isOnChain = true;
                        float speed = 9f + (Momentum / 10f);
                        Player.velocity.Y = Player.controlDown ? speed : -speed;
                        Player.velocity.X = 0;
                        SoundEngine.PlaySound(SoundID.Item34 with { Pitch = 0.3f });
                    }
                }

                // 3. ESTADO ATIVO NA CORRETE
                if (isOnChain)
                {
                    Player.pulley = false;
                    
                    // Snap horizontal e Spin visual
                    float targetX = tileCoords.X * 16f + 8f - (Player.width / 2f);
                    Player.position.X = MathHelper.Lerp(Player.position.X, targetX, 0.3f);
                    
                    if (Math.Abs(Player.velocity.Y) > 1f && Main.GameUpdateCount % 5 == 0)
                        Player.direction *= -1;

                    // 4. LÓGICA DE TOPO (SAÍDA AUTOMÁTICA)
                    Tile tileAbove = Framing.GetTileSafely(tileCoords.X, tileCoords.Y - 1);
                    bool ropeAbove = tileAbove.HasTile && tileAbove.TileType < Main.tileRope.Length && Main.tileRope[tileAbove.TileType];
                    
                    if (!ropeAbove && Player.velocity.Y < 0 && Player.position.Y < tileCoords.Y * 16)
                    {
                        isOnChain = false;
                        chainReleaseCooldown = 12;
                    }
                    
                    // Se parar, cai
                    if (Math.Abs(Player.velocity.Y) < 0.5f && !Player.controlUp && !Player.controlDown)
                        isOnChain = false;
                }
            }
            else { isOnChain = false; }
        }

        // ==================== TRILHOS E STOMP ====================
        private void HandleRailLogic()
        {
            if (isOnChain) { isOnRail = false; return; }

            Point tilePos = (Player.Bottom + new Vector2(0, -2)).ToTileCoordinates();
            Tile tile = Framing.GetTileSafely(tilePos);

            if (tile.HasTile && tile.TileType == TileID.MinecartTrack)
            {
                isOnRail = true;
                float targetY = tilePos.Y * 16f - Player.height + 2f;
                Player.position.Y = MathHelper.Lerp(Player.position.Y, targetY, 0.5f);
                Player.velocity.Y = 0;

                if (Player.controlRight) Player.velocity.X += 0.45f;
                else if (Player.controlLeft) Player.velocity.X -= 0.45f;
                else Player.velocity.X *= 0.985f;

                float maxR = 26f + (Momentum / 8f);
                Player.velocity.X = MathHelper.Clamp(Player.velocity.X, -maxR, maxR);

                if (Player.controlJump)
                {
                    Player.velocity.Y = -10.5f;
                    isOnRail = false;
                    AddCombo(5);
                }
            }
            else { isOnRail = false; }
        }

        private void HandleStompLogic()
        {
            if (isOnChain || isOnRail) { isStomping = false; return; }

            if (!isStomping && Player.velocity.Y > 5f && Player.controlDown && stompCooldown == 0)
                isStomping = true;

            if (isStomping)
            {
                Player.velocity.Y = 22f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && Player.Hitbox.Intersects(npc.Hitbox))
                    {
                        var hit = npc.CalculateHitInfo(50 + (int)Momentum, Player.direction, false, 6f);
                        npc.StrikeNPC(hit);
                        Player.velocity.Y = -13f;
                        isStomping = false;
                        stompCooldown = 35;
                        AddCombo(15);
                        SoundEngine.PlaySound(SoundID.Item14, Player.Center);
                        break;
                    }
                }
                if (Player.velocity.Y == 0) isStomping = false;
            }
        }

        public void AddCombo(int p)
        {
            ComboCounter += p;
            if (ComboCounter >= 100) { ComboPoints++; ComboCounter = 0; }
            Momentum = Math.Min(Momentum + (p * 0.25f), MAX_MOMENTUM);
        }
    }
}