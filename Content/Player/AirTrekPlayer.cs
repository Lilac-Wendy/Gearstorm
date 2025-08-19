using System;
using Gearstorm.Content.Items.AirTreks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Player
{
    public class AirTrekPlayer : ModPlayer
    {
        public bool hasAirTrekEquipped; 
        

        public override void UpdateEquips()
        {
            // Checa todos os slots de acessório
            for (int i = 3; i < 8 + Player.extraAccessorySlots; i++)
            {
                if (Player.armor[i].type == ModContent.ItemType<AirTrek>())
                {
                    hasAirTrekEquipped = true;
                    break;
                }
            }
        }
    
        public bool active = false;
        public float momentum = 0f;
        public int comboPoints = 0; // Each point = +1 flat damage
        public int comboCounter = 0; // accum to 100 => +1 comboPoints
        public int slideTimer = 0;
        public int wallJumpCooldown = 0;
        private int onTrackTimer = 0;
        private bool isDashingDown = false;
        private int dashDownTimer = 0;
        private const int dashDownDuration = 15;
        private const float dashDownSpeed = 20f;
        private const int dashDownDamage = 40;
        public const float MaxMomentum = 600f;
        private const float MomentumGain = 0.1f;
        private const float MomentumDecay = 0.05f;
        private bool canStomp = false;  
        private int stompCooldown = 0;
        private bool isClimbingChain = false;
        private float chainClimbSpeed = 3f;
        private const int stompCooldownDuration = 120; 
        private const int RailJumpPoints = 15;
        private const int WallJumpPoints = 20;
        private const int ChainBoostPoints = 25;
        private const int StompPoints = 10;
        // === Flags de tricks ===
        public bool isOnRail;            // True se grindando em rail
        public bool didWallJumpRecently; // True se fez wall jump
        public bool isChainBoosting;     // True se usando corrente

        
        
        
        public override void ResetEffects()
        {
            hasAirTrekEquipped = false;
            isOnRail = false;
            didWallJumpRecently = false;
            isChainBoosting = false;
            canStomp = false;
            if (!active)
            {
                momentum = 0f;
            }
            else
            {
                Player.slippy = true;
                Player.runAcceleration *= 0.5f;
            }
        }

        public override void PostUpdate()
        {
            if (wallJumpCooldown > 0) wallJumpCooldown--;
            if (slideTimer > 0) slideTimer--;
        }

        public override void PreUpdateMovement()
        {
            if (!active && !hasAirTrekEquipped) return;

            HandleMomentum();
            HandleSlide();
            HandleWallJump();
            ApplyJumpBoost();
            HandleVerticalDash();
            bool onTrack = false;
            CheckMinecartTrackCollision(ref onTrack);

            HandleChainRide();

            if (onTrack)
            {
                if (Player.controlJump && Player.velocity.Y == 0)
                {
                    canStomp = true;
                    Player.velocity.Y = -12f; // boost no pulo na rail
                    momentum = MaxMomentum;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item7, Player.Center);
                    CombatText.NewText(Player.Hitbox, Color.Cyan, "Rail Jump!");
                }

                onTrackTimer++;
                if (onTrackTimer >= 60) // 60 ticks = 1 segundo
                {
                    AddComboPoints(1);
                    onTrackTimer = 0;
                }
            }
            else
            {
                onTrackTimer = 0;
            }
        }

        // ----------------------
        // Continuous acceleration
        private void HandleMomentum()
        {
            bool moving = Player.controlRight || Player.controlLeft;
            bool grounded = Player.velocity.Y == 0;

            if (moving)
            {
                float gain = grounded ? MomentumGain * 1.5f : MomentumGain;
                momentum = Math.Min(momentum + gain, MaxMomentum);
            }
            else
            {
                float decay = (slideTimer > 0) ? MomentumDecay * 0.5f : MomentumDecay;
                momentum = Math.Max(momentum - decay, 0f);
            }

            Player.moveSpeed += momentum * 0.1f;
            Player.maxRunSpeed += momentum * 0.2f;

            if (Main.rand.NextBool(10) && momentum > 5f)
            {
                Dust.NewDustPerfect(
                    Player.Bottom + new Vector2(0, 4),
                    DustID.Electric,
                    new Vector2(-Player.direction * momentum * 0.1f, -1f),
                    100, default, 0.7f
                );
            }
        }

        // ----------------------
        // Slide + i-frames
        private void HandleSlide()
        {
            if (Player.controlDown && Player.velocity.Y == 0 && slideTimer <= 0)
            {
                slideTimer = 30;
                Player.immune = true;
                Player.immuneTime = 30;
            }

            if (slideTimer > 0)
            {
                momentum = Math.Max(momentum - 0.2f, 0f);
                Player.velocity.X *= 0.85f;
                Player.slippy = true;

                if (Main.rand.NextBool(3))
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.Cloud, 0, 0, 150, default, 1.5f);
                }
            }
        }

        private void HandleWallJump()
        {
            if (wallJumpCooldown > 0) return;

            int tileX = (int)(Player.position.X / 16);
            int tileY = (int)(Player.position.Y / 16);

            bool touchingWall = false;
            bool enoughSpace = true;

            if (Player.direction > 0)
            {
                touchingWall = Collision.SolidTiles(tileX + Player.width / 16, tileX + Player.width / 16 + 1, tileY, tileY + Player.height / 16);
                enoughSpace = !Collision.SolidTiles(tileX + Player.width / 16, tileX + Player.width / 16 + 1, tileY - 3, tileY);
            }
            else
            {
                touchingWall = Collision.SolidTiles(tileX - 1, tileX, tileY, tileY + Player.height / 16);
                enoughSpace = !Collision.SolidTiles(tileX - 1, tileX, tileY - 3, tileY);
            }

            if (touchingWall && enoughSpace && Player.controlJump)
            {
                Player.velocity.Y = -6f;
                Player.velocity.X = -Player.direction * 6f;
                Player.immune = true;
                Player.immuneTime = 20;
                wallJumpCooldown = 20;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item56, Player.Center);
                CombatText.NewText(Player.Hitbox, Color.LightBlue, "Wall Jump!");
                canStomp = true;
            }
        }

        private void ApplyJumpBoost()
        {
            if (active)
            {
                Player.jumpSpeedBoost += 1.5f + (momentum / MaxMomentum) * 2f;
            }
        }

        private void HandleChainRide()
        {
            int tileX = (int)(Player.Center.X / 16);
            int tileY = (int)(Player.Center.Y / 16);
            Tile tile = Framing.GetTileSafely(tileX, tileY);

            if (tile.TileType == TileID.Chain)
            {
                if (Player.pulley && momentum > 5f)
                {
                    float horizontalSpeed = Math.Abs(Player.velocity.X);
                    Player.velocity.X = 0f;
                    Player.velocity.Y = -Math.Min(horizontalSpeed, 3f);

                    Player.fullRotation += Player.direction * 0.3f;
                    momentum = Math.Max(momentum - 0.1f, 0f);

                    canStomp = true;
                    comboPoints += ChainBoostPoints;
                    comboCounter++;
                
                    CombatText.NewText(Player.Hitbox, Color.LightGreen, "Chain Boost!");
                }
                
                else
                {
                    isClimbingChain = false;
                }
            }
            else
            {
                isClimbingChain = false;
            }
        }

        private void CheckMinecartTrackCollision(ref bool onTrack)
        {
            Vector2[] detectionPoints = {
                Player.Bottom,
                Player.Bottom + new Vector2(-8, 0),
                Player.Bottom + new Vector2(8, 0)
            };

            bool isOnTrack = false;
            Vector2 trackPosition = Vector2.Zero;
            Vector2 trackNormal = Vector2.Zero;
            bool foundValidTrack = false;

            foreach (Vector2 point in detectionPoints)
            {
                Point tilePos = point.ToTileCoordinates();
                if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                    continue;

                Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);
                if (tile.HasTile && tile.TileType == TileID.MinecartTrack)
                {
                    isOnTrack = true;
                    trackPosition = new Vector2(tilePos.X * 16, tilePos.Y * 16);

                    switch (tile.Slope)
                    {
                        case SlopeType.SlopeDownLeft:
                            trackNormal = new Vector2(-1, 1);
                            foundValidTrack = true;
                            break;
                        case SlopeType.SlopeDownRight:
                            trackNormal = new Vector2(1, 1);
                            foundValidTrack = true;
                            break;
                        case SlopeType.SlopeUpLeft:
                            trackNormal = new Vector2(-1, -1);
                            foundValidTrack = true;
                            break;
                        case SlopeType.SlopeUpRight:
                            trackNormal = new Vector2(1, -1);
                            foundValidTrack = true;
                            break;
                        case SlopeType.Solid:
                            trackNormal = Vector2.UnitY;
                            foundValidTrack = true;
                            break;
                    }

                    if (foundValidTrack)
                        break;
                }
            }

            if (isOnTrack && foundValidTrack)
            {
                onTrack = true;
                SnapToTrack(trackPosition, trackNormal);
                ApplyTrackPhysics(trackNormal);
                PlayTrackEffects();
            }
            else
            {
                onTrack = false;
            }
        }

        private void SnapToTrack(Vector2 trackPosition, Vector2 trackNormal)
        {
            float baseHeight = 2f;

            float targetY = trackPosition.Y - Player.height + baseHeight;
            if (Player.velocity.Y <= 1f)
            {
                float yDiff = targetY - Player.position.Y;
                Player.position.Y += yDiff * 0.3f; 
            }
            float targetX = trackPosition.X + 8 - Player.width / 2f;
            float xDiff = targetX - Player.position.X;
            Player.position.X += xDiff * 0.3f;
        }



        private void ApplyTrackPhysics(Vector2 trackNormal)
        {
            if (trackNormal != Vector2.Zero)
                trackNormal.Normalize();

            float acceleration = 0.5f; // mais agressivo para acelerar rápido
            float maxSpeed = 20f;      // velocidade maior

            if (Player.velocity.X > 0)
            {
                Player.velocity.X = Math.Min(Player.velocity.X + acceleration, maxSpeed);
            }
            else if (Player.velocity.X < 0)
            {
                Player.velocity.X = Math.Max(Player.velocity.X - acceleration, -maxSpeed);
            }
            else
            {
                Player.velocity.X = acceleration * Math.Sign(Main.player[Player.whoAmI].Center.X - Player.Center.X);
            }

            if (Math.Abs(trackNormal.Y) > 0.1f)
            {
                Player.velocity.Y = -trackNormal.Y * Math.Abs(Player.velocity.X) * 0.4f;
            }
            else
            {
                Player.velocity.Y = 0;
            }

            Player.velocity *= 0.9f; // menos fricção para evitar travar
        }

        private void PlayTrackEffects()
        {
            if (Main.rand.NextBool(30))
            {
                var soundStyle = new Terraria.Audio.SoundStyle("Terraria/Sounds/Item_55")
                {
                    Volume = 0.3f,
                    Pitch = Main.rand.NextFloat(-0.2f, 0.2f),
                    PitchVariance = 0.15f
                };
                Terraria.Audio.SoundEngine.PlaySound(soundStyle, Player.position);
            }

            if (Main.rand.NextBool(5))
            {
                int dustType = DustID.Smoke;
                Vector2 dustVelocity = new Vector2(-Math.Sign(Player.velocity.X) * 0.8f, -0.5f);
                Dust.NewDustPerfect(Player.Bottom, dustType, dustVelocity, 100, default, 0.7f);
            }
        }
        

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            AddComboPoints(10);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            AddComboPoints(10);
        }
        
        private void HandleVerticalDash()
        {
            if (stompCooldown > 0)
                stompCooldown--;

            // Somente pode stomp se canStomp estiver true
            if (Player.controlDown && !Player.controlJump && Player.velocity.Y != 0 && !isDashingDown && momentum > 5f && canStomp && stompCooldown == 0)
            {
                isDashingDown = true;
                dashDownTimer = dashDownDuration;
                Player.velocity.Y = dashDownSpeed;
                Player.immune = true;
                Player.immuneTime = dashDownDuration;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, Player.Center);
                CombatText.NewText(Player.Hitbox, Color.Cyan, "Stomp!");

                stompCooldown = stompCooldownDuration;
                canStomp = false; // resetar flag até novo salto válido
            }

            if (isDashingDown)
            {
                dashDownTimer--;

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && Player.Hitbox.Intersects(npc.Hitbox))
                    {
                        var hitInfo = new NPC.HitInfo()
                        {
                            Damage = (dashDownDamage / 4) + comboPoints, // reduzir dano drasticamente
                            Knockback = 2f, // reduzir knockback
                            HitDirection = Player.direction,
                            Crit = false
                        };
                        npc.StrikeNPC(hitInfo);
                        npc.AddBuff(BuffID.Confused, 120);
                        npc.AddBuff(BuffID.Slow, 120);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit4, npc.Center);
                        CombatText.NewText(npc.Hitbox, Color.OrangeRed, $"Stomp!");

                        Player.velocity.Y = -14f;

                        isDashingDown = false;
                        dashDownTimer = 0;
                        break;
                    }
                }

                if (dashDownTimer <= 0)
                    isDashingDown = false;
            }
        }

        public override void OnHurt(Terraria.Player.HurtInfo info)
        {
            ResetCombo();
        }

        // ----------------------
        // Combo system
        public void AddComboPoints(int points)
        {
            if (!active && !hasAirTrekEquipped) return;
            comboCounter += points;
            if (comboCounter >= 100)
            {
                comboPoints++;
                comboCounter -= 100;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item43, Player.Center);
                CombatText.NewText(Player.Hitbox, Color.Yellow, $"Combo +1! Total: {comboPoints}");
                Main.NewText($"Combo aumentado! Pontos: {comboPoints}", Color.Yellow);
            }
        }

        private void ResetCombo()
        {
            comboPoints = 0;
            comboCounter = 0;
        }
    }
}
