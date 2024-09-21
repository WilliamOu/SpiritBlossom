using static Terraria.ModLoader.ModContent;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritBlossom.Items;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.UI.Elements;
using ReLogic.Utilities;
using StarsAbove.Buffs.Melee.Unforgotten;
using SpiritBlossom.Common.GlobalNPCs;
using System.Security.Cryptography;
using SpiritBlossom.Projectiles;

namespace SpiritBlossom
{
    public class SpiritBlossomPlayer : ModPlayer
    {
        // Yone's Abilities (See League of Legends Official Wiki for ability references)

        // [P] Way of the Hunter
        public const int CurrentSwingResetTime = 120;
        public int CurrentSwingResetTimer = 0;
        public bool CurrentSwing = true;

        // [Q] Mortal Steel
        public const int MortalSteelCooldown = 80;
        public const int MortalSteelUseTime = 20;
        public const int MortalSteelDashUseTime = 50;
        public const int MortalSteelDashDuration = 50;
        public const int MortalSteelDashWindup = 14;
        public const int MortalSteelDashFrameCount = 20;
        public const int MortalSteelGatheringStormStacksResetTime = 360;
        public const float MortalSteelDashDistancePerTick = 14f;
        public int MortalSteelFrame;
        public SBDash MortalSteelDash = new SBDash();
        public ActiveSound Q3ReadySound;
        public SlotId Q3ReadySlot;
        public SoundStyle Q3Cast = new SoundStyle("SpiritBlossom/Sounds/Q3Cast");
        public SoundStyle Q3Ready = new SoundStyle("SpiritBlossom/Sounds/Q3Ready");
        public SoundStyle Q3Hit = new SoundStyle("SpiritBlossom/Sounds/Q3Hit");
        public SoundStyle[] QCast = new SoundStyle[]
        {
            new SoundStyle("SpiritBlossom/Sounds/QCast0"),
            new SoundStyle("SpiritBlossom/Sounds/QCast1")
        };
        public SoundStyle[] QHit = new SoundStyle[]
        {
            new SoundStyle("SpiritBlossom/Sounds/QHit0"),
            new SoundStyle("SpiritBlossom/Sounds/QHit1")
        };

        // [W] Spirit Cleave
        public const int SpiritCleaveCooldown = 360;
        public const int SpiritCleaveShieldDuration = 90;
        public const int SpiritCleaveUseTime = 40;
        public const int SpiritCleavePrimaryHitShieldAmount = 200;
        public const int SpiritCleaveSecondaryHitShieldAmount = 25;
        public int CurrentShieldHealth = 0;
        public int ShieldFrame = 0;
        public bool ShieldWasApplied = false;
        public SoundStyle WCast = new SoundStyle("SpiritBlossom/Sounds/WCast");
        public SoundStyle WHit = new SoundStyle("SpiritBlossom/Sounds/WHit");
        public SoundStyle WShieldActivate = new SoundStyle("SpiritBlossom/Sounds/WShieldActivate");
        public SoundStyle WShieldDeactivate = new SoundStyle("SpiritBlossom/Sounds/WShieldDeactivate");

        // [E] Soul Unbound
        public const int SoulUnboundCooldown = 360;
        public const int SoulUnboundDuration = 300;
        public const int SoulUnboundUseTime = 20;
        public const int SoulUnboundDashFrameCount = 15;
        public const int SoulUnboundRecastWindup = 12;
        public const int SoulUnboundDeathMarkVerticalDrawOffset = -35;
        public const float SoulUnboundStoredDamageRatio = 0.35f;
        public const float SoulUnboundMinMovementSpeedBonus = 0.1f;
        public const float SoulUnboundMaxMovementSpeedBonus = 1.25f;
        public const float SoulUnboundDeathMarkSpriteScale = 1f;
        public const float SoulUnboundDashDistancePerTick = 14f;
        public const float SoulUnboundReturnSpeedPerTick = 60f;
        public int SoulUnboundFrame;
        public int SoulUnboundRecastDashFrameCount;
        public int SoulUnboundRecastFrame;
        public SBDash SoulUnboundDash = new SBDash();
        public Player SoulUnboundClone = null;
        public ActiveSound ETetherSound;
        public SlotId ETetherSlot;
        public SoundStyle EWarning = new SoundStyle("SpiritBlossom/Sounds/EWarning");
        public SoundStyle ERecast = new SoundStyle("SpiritBlossom/Sounds/ERecast");
        public SoundStyle EReturn = new SoundStyle("SpiritBlossom/Sounds/EReturn");
        public SoundStyle EDeactivate = new SoundStyle("SpiritBlossom/Sounds/EDeactivate");
        public SoundStyle ECast = new SoundStyle("SpiritBlossom/Sounds/ECast");
        public SoundStyle EMark = new SoundStyle("SpiritBlossom/Sounds/EMark");
        public SoundStyle EDetonate = new SoundStyle("SpiritBlossom/Sounds/EDetonate");
        public SoundStyle[] ETether = new SoundStyle[]
        {
            new SoundStyle("SpiritBlossom/Sounds/ETether0"),
            new SoundStyle("SpiritBlossom/Sounds/ETether1"),
            new SoundStyle("SpiritBlossom/Sounds/ETether2")
        };

        // [R] Fate Sealed
        public const int FateSealedCooldown = 720;
        public const int FateSealedDuration = 40;
        public const int FateSealedUseTime = 65;
        public const int FateSealedWindup = 39;
        public const int FateSealedDistanceBehindTargetToPullTo = 60;
        public const int FateSealedDistanceBehindTargetToBlinkTo = 100;
        public const float FateSealedStandardBlinkDistance = 540f;
        public int FateSealedFrame;
        public float FateSealedAdjustedBlinkDistance;
        public Vector2 FateSealedCastPosition;
        public Vector2 FateSealedBlinkDirection;
        public Vector2 ProjectionOfFarthestEnemyPositionToFateSealedForwardVector;
        public Vector2 PointBehindFarthestEnemyProjectionThatEnemiesArePulledTo;
        public Vector2 PointBehindFarthestEnemyProjectionWhichPlayerBlinksTo;
        public SBDash FateSealedBlink = new SBDash();
        public Tuple<float, NPC> FarthestEnemyFromPlayerDuringFateSealedCast;
        public SoundStyle RCast = new SoundStyle("SpiritBlossom/Sounds/RCast");
        public SoundStyle RBlink = new SoundStyle("SpiritBlossom/Sounds/RBlink");
        public SoundStyle RInitialHit = new SoundStyle("SpiritBlossom/Sounds/RInitialHit");
        public SoundStyle RResidualHit = new SoundStyle("SpiritBlossom/Sounds/RResidualHit");

        public override void OnEnterWorld()
        {
            ResetAbilities(Player);
        }

        public override void OnRespawn()
        {
            // In most cases debuffs are cleared on death, but this is in case another mod prevents that
            ResetAbilities(Player);
        }

        public override void PreUpdate()
        {
            if (CurrentSwingResetTimer <= 0) { CurrentSwing = true; }

            Cooldown(ref CurrentSwingResetTimer);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Player.HasBuff<Buffs.SoulUnboundBuff>()) { return; }

            OnSoulUnboundHit(target, damageDone);
        }

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            base.ModifyMaxStats(out health, out mana);

            if (CurrentShieldHealth > 0)
            {
                health.Base += CurrentShieldHealth;
                if (!ShieldWasApplied) {
                    Player.statLife += CurrentShieldHealth;
                    ShieldWasApplied = true;
                }
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            base.OnHurt(info);

            if (CurrentShieldHealth > 0)
            {
                int shieldAbsorbedDamage = Math.Min(CurrentShieldHealth, info.Damage);
                CurrentShieldHealth -= shieldAbsorbedDamage;
                info.Damage -= shieldAbsorbedDamage;

                Color shieldTextColor = new Color(200, 200, 200);
                CombatText.NewText(Player.getRect(), shieldTextColor, $"({shieldAbsorbedDamage})");

                if (info.Damage <= 0)
                {
                    info.Damage = 0;
                }
            }
        }

        public void OnMortalSteelHit(Player player)
        {
            if (!player.HasBuff(BuffType<Buffs.GatheringStorm>()))
            {
                player.AddBuff(BuffType<Buffs.GatheringStorm>(), MortalSteelGatheringStormStacksResetTime);
            }
            else 
            {
                player.ClearBuff(BuffType<Buffs.GatheringStorm>());
                player.AddBuff(BuffType<Buffs.GatheringStormReady>(), MortalSteelGatheringStormStacksResetTime);

                Q3ReadySlot = SoundEngine.PlaySound(Q3Ready with { Volume = SBUtils.GlobalSFXVolume });
                SoundEngine.TryGetActiveSound(Q3ReadySlot, out Q3ReadySound);
            }
        }

        public void OnSpiritCleaveHit(Player player, int shieldAmount)
        {
            CurrentShieldHealth = shieldAmount;
            ShieldFrame = 0;
            ShieldWasApplied = false;
            player.AddBuff(BuffType<Buffs.SpiritCleaveShield>(), SpiritCleaveShieldDuration);
            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, new Vector2(1f, 0f), ProjectileType<SpiritCleaveShield>(), 0, 0, player.whoAmI);

            Color shieldTextColor = new Color(255, 40, 100);
            CombatText.NewText(player.getRect(), shieldTextColor, $"{shieldAmount}");
        }

        public void ClearSpiritCleaveShield(Player player)
        {
            SoundEngine.PlaySound(WShieldDeactivate with { Volume = SBUtils.GlobalSFXVolume });
            if (CurrentShieldHealth > 0) { player.statLife -= CurrentShieldHealth; }
            CurrentShieldHealth = 0;
        }

        public void OnFateSealedHit(Player player, NPC target)
        {
            float distanceToPlayer = Vector2.Distance(player.Center, target.Center);
            if (FarthestEnemyFromPlayerDuringFateSealedCast.Item2 == null || distanceToPlayer > FarthestEnemyFromPlayerDuringFateSealedCast.Item1)
            {
                FarthestEnemyFromPlayerDuringFateSealedCast = new Tuple<float, NPC>(distanceToPlayer, target);
            }
        }

        public void OnSoulUnboundHit(NPC target, int damageDone)
        {
            if (!target.HasBuff(BuffType<Buffs.DeathMark>()))
            {
                target.GetGlobalNPC<DeathMarkGlobalNPCs>().Initialize(Player, target);
                target.AddBuff(BuffType<Buffs.DeathMark>(), SoulUnboundDuration - SoulUnboundFrame);
                SoundEngine.PlaySound(EMark with { Volume = SBUtils.GlobalSFXVolume * 2f });
            }
            target.GetGlobalNPC<DeathMarkGlobalNPCs>().StackDamage(damageDone * SoulUnboundStoredDamageRatio, target);
        }

        public void InitializeMortalSteelDashValues(Player player, Vector2 dashDirection)
        {
            MortalSteelDash.Set(null, player, dashDirection, player.position, MortalSteelDashWindup, MortalSteelDashFrameCount, 1, MortalSteelDashDistancePerTick, false, true, ref MortalSteelFrame);
        }

        public void InitializeSoulUnboundDashValues(Player player, Vector2 dashDirection)
        {
            SoulUnboundDash.Set(null, player, dashDirection, player.position, 0, SoulUnboundDashFrameCount, 0, SoulUnboundDashDistancePerTick, true, true, ref SoulUnboundFrame);
        }

        public void InitializeSoulUnboundRecastDashValues(Player player)
        {
            if (SoulUnboundClone == null) { return; }

            Vector2 returnDirection = Vector2.Normalize(SoulUnboundClone.position - player.position);
            float distanceBetweenPlayerAndAnchor = Vector2.Distance(SoulUnboundClone.position, player.position);
            SoulUnboundRecastDashFrameCount = (int)(distanceBetweenPlayerAndAnchor / SoulUnboundReturnSpeedPerTick);

            SoulUnboundDash.Set(null, player, returnDirection, player.position, SoulUnboundRecastWindup, SoulUnboundRecastDashFrameCount, 0, SoulUnboundReturnSpeedPerTick, false, true, ref SoulUnboundRecastFrame);
        }

        public void InitializeFateSealedBlinkValues(Player player, Vector2 dashDirection)
        {
            FarthestEnemyFromPlayerDuringFateSealedCast = new Tuple<float, NPC>(-1, null);
            FateSealedCastPosition = player.position;
            FateSealedBlinkDirection = dashDirection;
            FateSealedAdjustedBlinkDistance = FateSealedBlink.Set(null, player, dashDirection, player.position, FateSealedWindup, 1, FateSealedUseTime - FateSealedWindup - 1, 0, false, true, ref FateSealedFrame);
        }

        public void RecastSoulUnbound(Player player)
        {
            if (player.HasBuff(BuffType<Buffs.SoulUnboundRecast>())) { return; }

            InitializeSoulUnboundRecastDashValues(player);

            bool anyNPCIsMarkedForDeath = false;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.HasBuff(BuffType<Buffs.DeathMark>()))
                {
                    anyNPCIsMarkedForDeath = true;
                    npc.GetGlobalNPC<DeathMarkGlobalNPCs>().DetonateMark(npc);
                }
            }

            player.immune = true;
            player.immuneTime = SoulUnboundRecastDashFrameCount + SoulUnboundRecastWindup + 30;
            player.SetDummyItemTime(SoulUnboundRecastDashFrameCount + SoulUnboundRecastWindup);

            player.AddBuff(BuffType<Buffs.SoulUnboundRecast>(), SoulUnboundRecastDashFrameCount + SoulUnboundRecastWindup + 1);

            SoundEngine.PlaySound(ERecast with { Volume = SBUtils.GlobalSFXVolume });
            if (anyNPCIsMarkedForDeath) { SoundEngine.PlaySound(EDetonate with { Volume = SBUtils.GlobalSFXVolume * 1.2f }); }

            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, new Vector2(1f, 0f), ProjectileType<SoulUnboundRecastFlash>(), 0, 0, player.whoAmI);
        }

        public void EndSoulUnbound(Player player)
        {
            player.AddBuff(BuffType<Buffs.SoulUnboundCooldown>(), SoulUnboundCooldown);
            Player.ChangeDir(SoulUnboundClone.direction);
            SoundEngine.PlaySound(EReturn with { Volume = SBUtils.GlobalSFXVolume });
            SoundEngine.PlaySound(EDeactivate with { Volume = SBUtils.GlobalSFXVolume / 2 });
        }

        public bool IsChannelingAbility(Player player)
        {
            bool isChanneling = (player.HasBuff(BuffType<Buffs.FateSealedBuff>()) || player.HasBuff(BuffType<Buffs.MortalSteelBuff>()) || player.HasBuff(BuffType<Buffs.SoulUnboundRecast>()));
            return isChanneling;
        }

        public void ResetAbilities(Player player)
        {
            if (player.HasBuff(BuffType<Buffs.MortalSteelBuff>())) { player.ClearBuff(BuffType<Buffs.MortalSteelBuff>()); }
            if (player.HasBuff(BuffType<Buffs.GatheringStorm>())) { player.ClearBuff(BuffType<Buffs.GatheringStorm>()); }
            if (player.HasBuff(BuffType<Buffs.GatheringStormReady>())) { player.ClearBuff(BuffType<Buffs.GatheringStormReady>()); }
            if (player.HasBuff(BuffType<Buffs.SpiritCleaveShield>())) { player.ClearBuff(BuffType<Buffs.SpiritCleaveShield>()); }
            if (player.HasBuff(BuffType<Buffs.SoulUnboundBuff>())) { player.ClearBuff(BuffType<Buffs.SoulUnboundBuff>()); }
            if (player.HasBuff(BuffType<Buffs.SoulUnboundRecast>())) { player.ClearBuff(BuffType<Buffs.SoulUnboundRecast>()); }
            if (player.HasBuff(BuffType<Buffs.FateSealedBuff>())) { player.ClearBuff(BuffType<Buffs.FateSealedBuff>()); }
            /*if (player.HasBuff(BuffType<Buffs.MortalSteelCooldown>())) { player.ClearBuff(BuffType<Buffs.MortalSteelCooldown>()); }
            if (player.HasBuff(BuffType<Buffs.SpiritCleaveCooldown>())) { player.ClearBuff(BuffType<Buffs.SpiritCleaveCooldown>()); }
            if (player.HasBuff(BuffType<Buffs.SoulUnboundCooldown>())) { player.ClearBuff(BuffType<Buffs.SoulUnboundCooldown>()); }
            if (player.HasBuff(BuffType<Buffs.FateSealedCooldown>())) { player.ClearBuff(BuffType<Buffs.FateSealedCooldown>()); }*/
        }

        public void ThreePointCalculation(Player player)
        {
            // If no enemies are hit
            if (FarthestEnemyFromPlayerDuringFateSealedCast.Item2 == null)
            {
                FateSealedAdjustedBlinkDistance = SBUtils.SafeLocationLinearSearch(FateSealedStandardBlinkDistance, FateSealedBlinkDirection, FateSealedCastPosition, player.width, player.height);

                // Error handling, in case a stray enemy is somehow caught in the second hit without triggering the first hit (which would mean the farthest target would still be null)
                // Theoretically, only the point that enemies are pulled to needs to be defined, but just in case, the rest are defined as well
                float defaultPullDistance = (FateSealedAdjustedBlinkDistance - 30 > 0) ? FateSealedAdjustedBlinkDistance - 30 : 0;
                Vector2 defaultPullPosition = FateSealedCastPosition + FateSealedBlinkDirection * defaultPullDistance;
                ProjectionOfFarthestEnemyPositionToFateSealedForwardVector = defaultPullPosition;
                PointBehindFarthestEnemyProjectionThatEnemiesArePulledTo = defaultPullPosition;
                PointBehindFarthestEnemyProjectionWhichPlayerBlinksTo = defaultPullPosition;
                return;
            }

            // Project the farthest enemy's position onto the attack's forward vector to ensure all subsequent positions (player blink and enemy pull) lie along the attack's path
            // This maintains the directionality of the attack and prevents the player from blinking to unintended lateral positions
            Vector2 playerToEnemy = FarthestEnemyFromPlayerDuringFateSealedCast.Item2.Center - FateSealedCastPosition;
            float projectionLength = Vector2.Dot(playerToEnemy, FateSealedBlinkDirection);
            ProjectionOfFarthestEnemyPositionToFateSealedForwardVector = FateSealedCastPosition + FateSealedBlinkDirection * projectionLength;

            PointBehindFarthestEnemyProjectionThatEnemiesArePulledTo = ProjectionOfFarthestEnemyPositionToFateSealedForwardVector + FateSealedBlinkDirection * FateSealedDistanceBehindTargetToPullTo;

            PointBehindFarthestEnemyProjectionWhichPlayerBlinksTo = ProjectionOfFarthestEnemyPositionToFateSealedForwardVector + FateSealedBlinkDirection * FateSealedDistanceBehindTargetToBlinkTo;

            // Store the calculated blink distance
            float blinkDistance = Vector2.Distance(FateSealedCastPosition, PointBehindFarthestEnemyProjectionWhichPlayerBlinksTo);
            FateSealedAdjustedBlinkDistance = SBUtils.SafeLocationLinearSearch(blinkDistance, FateSealedBlinkDirection, FateSealedCastPosition, player.width, player.height);
        }

        private void Cooldown(ref int cooldown)
        {
            if (cooldown > 0)
            {
                cooldown--;
            }
        }
    }

    public class SBDash
    {
        private NPC npc;
        private Player player;

        private Vector2 dashDirection;
        private Vector2 dashOffset;
        private Vector2 initialPosition;
        private int windUpFrameCount;
        private int dashFrameCount;
        private int windDownFrameCount;
        private int totalFrameCount;
        private float originalDashDistancePerTick;
        private float dashDistancePerTick;
        private bool useEndSpeedTaper;
        private bool zeroVelocity;
        private bool isPlayer;
        private bool useCollideAndSlide;

        private float safeDashDistance;
        private float maxDistance;
        private float remainingDashDistance;
        private float totalDistanceCovered;

        private int dummyFrame;

        public SBDash()
        {
            Set(null, null, Vector2.Zero, Vector2.Zero, 0, 0, 0, 0, false, false, ref dummyFrame);
        }

        public SBDash(NPC npc, Player player, Vector2 dashDirection, Vector2 initialPosition, int windupFrameCount, int dashFrameCount, int windDownFrameCount, float originalDashDistancePerTick, bool useEndSpeedTaper, bool zeroVelocity, ref int currentFrame)
        {
            Set(npc, player, dashDirection, initialPosition, windupFrameCount, dashFrameCount, windDownFrameCount, originalDashDistancePerTick, useEndSpeedTaper, zeroVelocity, ref currentFrame);
        }

        public float Set(NPC npc, Player player, Vector2 dashDirection, Vector2 initialPosition, int windUpFrameCount, int dashFrameCount, int windDownFrameCount, float originalDashDistancePerTick, bool useEndSpeedTaper, bool zeroVelocity, ref int currentFrame)
        {
            this.npc = npc;
            this.player = player;
            this.dashDirection = dashDirection;
            this.initialPosition = initialPosition;
            this.windUpFrameCount = windUpFrameCount;
            this.dashFrameCount = dashFrameCount;
            this.windDownFrameCount = windDownFrameCount;
            this.originalDashDistancePerTick = originalDashDistancePerTick;
            this.useEndSpeedTaper = useEndSpeedTaper;
            this.zeroVelocity = zeroVelocity;

            dashOffset = Vector2.Zero;
            totalFrameCount = windUpFrameCount + dashFrameCount + windDownFrameCount;
            isPlayer = (player != null);
            currentFrame = 0;

            int frameAdjustmentToCompensateForWindDown = (useEndSpeedTaper) ? 2 : 0;
            int adjustedDashFrameCount = Math.Max(1, dashFrameCount - frameAdjustmentToCompensateForWindDown);

            if (npc == null && player == null)
            {
                SBUtils.PrintMessage("Improper SBDash.Set() invocation");
                return 0f;
            }

            int entityWidth = isPlayer ? player.width : npc.width;
            int entityHeight = isPlayer ? player.height : npc.height;

            maxDistance = originalDashDistancePerTick * adjustedDashFrameCount;

            safeDashDistance = SBUtils.SafeLocationLinearSearch(maxDistance, dashDirection, initialPosition, entityWidth, entityHeight);

            remainingDashDistance = maxDistance - safeDashDistance;

            useCollideAndSlide = remainingDashDistance > 0;

            dashDistancePerTick = maxDistance / adjustedDashFrameCount;

            totalDistanceCovered = 0f;

            return dashDistancePerTick;
        }

        public void Dash(ref int currentFrame)
        {
            currentFrame++;
            if (currentFrame > totalFrameCount) { return; }

            SetVelocity();

            Vector2 positionOffset = Vector2.Zero;

            if (currentFrame < windUpFrameCount)
            {
                SetPosition(initialPosition);
                return;
            }
            else if (useEndSpeedTaper && totalFrameCount - windDownFrameCount - currentFrame < 4)
            {
                float multiplier = (5 - (totalFrameCount - windDownFrameCount - currentFrame)) / 5f;
                positionOffset = dashDirection * dashDistancePerTick * multiplier;
            }
            else if (currentFrame <= totalFrameCount - windDownFrameCount)
            {
                positionOffset = dashDirection * dashDistancePerTick;
            }

            float distanceThisTick = positionOffset.Length();
            float newTotalDistanceCovered = totalDistanceCovered + distanceThisTick;

            Vector2 currentPosition = GetCurrentPosition();

            if (newTotalDistanceCovered <= safeDashDistance)
            {
                dashOffset += positionOffset;
                SetPosition(initialPosition + dashOffset);
            }
            else
            {
                float distanceInSafePhase = safeDashDistance - totalDistanceCovered;

                if (distanceInSafePhase > 0)
                {
                    Vector2 safePhaseOffset = positionOffset * (distanceInSafePhase / distanceThisTick);
                    dashOffset += safePhaseOffset;
                    SetPosition(initialPosition + dashOffset);

                    Vector2 remainingOffset = positionOffset * ((distanceThisTick - distanceInSafePhase) / distanceThisTick);

                    if (useCollideAndSlide)
                    {
                        if (isPlayer && player != null)
                        {
                            remainingOffset = Collision.TileCollision(currentPosition + safePhaseOffset, remainingOffset, player.width, player.height);
                        }
                        else if (npc != null)
                        {
                            remainingOffset = Collision.TileCollision(currentPosition + safePhaseOffset, remainingOffset, npc.width, npc.height);
                        }

                        SetPosition(currentPosition + safePhaseOffset + remainingOffset);
                    }
                }
                else
                {
                    if (useCollideAndSlide)
                    {
                        if (isPlayer && player != null)
                        {
                            positionOffset = Collision.TileCollision(currentPosition, positionOffset, player.width, player.height);
                        }
                        else if (npc != null)
                        {
                            positionOffset = Collision.TileCollision(currentPosition, positionOffset, npc.width, npc.height);
                        }

                        SetPosition(currentPosition + positionOffset);
                    }
                }
            }

            totalDistanceCovered = newTotalDistanceCovered;
        }

        public void DashToPosition(Vector2 finalPosition, ref int currentFrame)
        {
            Dash(ref currentFrame);

            if (currentFrame >= totalFrameCount - windDownFrameCount)
            {
                SetPosition(finalPosition);
            }
        }

        private void SetVelocity()
        {
            if (!zeroVelocity) { return; }

            if (isPlayer && player != null)
            {
                player.velocity = Vector2.Zero;
            }
            else if (npc != null)
            {
                npc.velocity = Vector2.Zero;
            }
        }

        private void SetPosition(Vector2 newPosition)
        {
            if (isPlayer && player != null)
            {
                player.position = newPosition;
            }
            else if (npc != null)
            {
                npc.position = newPosition;
            }
        }

        private Vector2 GetCurrentPosition()
        {
            if (isPlayer && player != null)
            {
                return player.position;
            }
            else if (npc != null)
            {
                return npc.position;
            }
            else
            {
                return initialPosition + dashOffset;
            }
        }
    }

    // Credit to Photonic0 for DrawFrame(), RectVisualizer, and AABBvLineVisualizer()
    public static class SBUtils
    {
        public static int SoundIndex = 0;
        public static float GlobalSFXVolume = 0.2f;

        public static bool DrawRectangularHitboxes = false;
        public static bool DrawLineHitboxes = false;
        public static bool DrawConeHitboxes = false;
        public static bool PrintMessages = false;

        // Draws the sprite in full brightness and makes it unaffected by outside lighting sources
        public static bool DrawFrame(Projectile projectile, int frameCount, int ticksPerFrame, float horizontalOffset = 0f, float verticalOffset = 0f)
        {
            Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
            Rectangle frame = tex.Frame(1, frameCount, 0, projectile.frameCounter / ticksPerFrame % frameCount);

            SpriteEffects flipIfFacingLeft = (projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, new Vector2(frame.Width / 2 + horizontalOffset, frame.Height / 2 + verticalOffset), projectile.scale, flipIfFacingLeft);
            return false;
        }
        public static bool DrawFrame(Projectile projectile, int frameCount, int ticksPerFrame, Color color, int horizontalFrameCount = 1, int verticalFrameCount = 1, float horizontalOffset = 0f, float verticalOffset = 0f)
        {
            Texture2D tex = TextureAssets.Projectile[projectile.type].Value;

            // Calculate the current frame index
            int totalFrames = horizontalFrameCount * verticalFrameCount;
            int currentFrame = projectile.frameCounter / ticksPerFrame % totalFrames;

            // Calculate the frame's X and Y positions on the spritesheet
            int frameX = currentFrame % horizontalFrameCount;
            int frameY = currentFrame / horizontalFrameCount;

            // Get the correct frame rectangle
            Rectangle frame = tex.Frame(horizontalFrameCount, verticalFrameCount, frameX, frameY);

            SpriteEffects flipIfFacingLeft = (projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, projectile.Center - Main.screenPosition, frame, color, projectile.rotation, new Vector2(frame.Width / 2 + horizontalOffset, frame.Height / 2 + verticalOffset), projectile.scale, flipIfFacingLeft);
            return false;
        }

        public static bool DrawFrame(Vector2 position, float rotation, float scale, Texture2D tex, int inputFrame, int ticksPerFrame, Color color, bool useFlip, int horizontalFrameCount = 1, int verticalFrameCount = 1, float horizontalOffset = 0f, float verticalOffset = 0f)
        {
            int totalFrames = horizontalFrameCount * verticalFrameCount;
            int currentFrame = inputFrame / ticksPerFrame % totalFrames;

            // Calculate the frame's X and Y positions on the spritesheet
            int frameX = currentFrame % horizontalFrameCount;
            int frameY = currentFrame / horizontalFrameCount;

            // Get the correct frame rectangle
            Rectangle frame = tex.Frame(horizontalFrameCount, verticalFrameCount, frameX, frameY);

            SpriteEffects flipIfFacingLeft = (useFlip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, position - Main.screenPosition, frame, color, rotation, new Vector2(frame.Width / 2 + horizontalOffset, frame.Height / 2 + verticalOffset), scale, flipIfFacingLeft);
            return false;
        }

        public static void RectVisualizer(Rectangle rectangle)
        {
            if (!DrawRectangularHitboxes) { return; }

            Texture2D blankTexture = Terraria.GameContent.TextureAssets.Extra[195].Value;
            Vector2 texScale = rectangle.Size() * 0.00390625f; // 1/256, texture is 256x256
            Main.EntitySpriteDraw(blankTexture, rectangle.Location.ToVector2() - Main.screenPosition, null, Color.Red * 0.25f, 0, Vector2.Zero, texScale, SpriteEffects.None, 1);
        }

        public static void LineVisualizer(Vector2 lineStart, Vector2 lineEnd, float lineWidth)
        {
            if (!DrawLineHitboxes) { return; }

            AABBvLineVisualizer(lineStart, lineEnd, lineWidth);
        }

        private static void AABBvLineVisualizer(Vector2 lineStart, Vector2 lineEnd, float lineWidth)
        {
            Texture2D blankTexture = Terraria.GameContent.TextureAssets.Extra[195].Value;
            Vector2 texScale = new Vector2((lineStart - lineEnd).Length(), lineWidth) * 0.00390625f; // 1/256, texture is 256x256
            Main.EntitySpriteDraw(blankTexture, (lineStart) - Main.screenPosition, null, Color.Red * 0.25f, (lineEnd - lineStart).ToRotation(), new Vector2(0, 128), texScale, SpriteEffects.None);
        }

        public static void ConeVisualizer(Vector2 coneCenter, float coneLength, float coneRotation, float maximumAngle)
        {
            if (!DrawConeHitboxes) { return; }

            Vector2 direction = new Vector2(MathF.Cos(coneRotation), MathF.Sin(coneRotation));
            Vector2 edge1 = new Vector2(MathF.Cos(coneRotation - maximumAngle), MathF.Sin(coneRotation - maximumAngle));
            Vector2 edge2 = new Vector2(MathF.Cos(coneRotation + maximumAngle), MathF.Sin(coneRotation + maximumAngle));

            Vector2 lineEnd1 = coneCenter + direction * coneLength;
            Vector2 lineEnd2 = coneCenter + edge1 * coneLength;
            Vector2 lineEnd3 = coneCenter + edge2 * coneLength;

            AABBvLineVisualizer(coneCenter, lineEnd1, 5f);
            AABBvLineVisualizer(coneCenter, lineEnd2, 5f);
            AABBvLineVisualizer(coneCenter, lineEnd3, 5f);
        }

        public static void PrintCurrentFrame(int currentFrame)
        {
            if (!PrintMessages) { return; }

            PrintMessage("Current Frame: " + currentFrame);
        }

        public static void PrintMessage(string message)
        {
            if (!PrintMessages) { return; }

            Main.NewText(message, 255, 255, 255);
        }

        public static float SafeLocationLinearSearch(float distance, Vector2 direction, Vector2 startingPosition, int playerWidth, int playerHeight)
        {
            Vector2 endingPosition = distance * direction + startingPosition;

            int xStart = (int)(endingPosition.X / 16);
            int xEnd = (int)((endingPosition.X + playerWidth - 1) / 16);
            int yStart = (int)(endingPosition.Y / 16);
            int yEnd = (int)((endingPosition.Y + playerHeight - 1) / 16);

            bool isSolid = Collision.SolidTiles(xStart, xEnd, yStart, yEnd);
            if (!isSolid)
            {
                return distance;
            }

            // March forward
            for (int i = 1; i <= 4; i++)
            {
                Vector2 farEndingPosition = distance * direction + startingPosition + (i * 16 * direction);

                xStart = (int)(farEndingPosition.X / 16);
                xEnd = (int)((farEndingPosition.X + playerWidth - 1) / 16);
                yStart = (int)(farEndingPosition.Y / 16);
                yEnd = (int)((farEndingPosition.Y + playerHeight - 1) / 16);

                isSolid = Collision.SolidTiles(xStart, xEnd, yStart, yEnd);
                if (!isSolid)
                {
                    return distance + (16 * i);
                }
            }

            // March backward
            float closeDistance = distance;
            while (closeDistance > 32)
            {
                closeDistance -= 16f;
                Vector2 closeEndingPosition = closeDistance * direction + startingPosition;

                xStart = (int)(closeEndingPosition.X / 16);
                xEnd = (int)((closeEndingPosition.X + playerWidth - 1) / 16);
                yStart = (int)(closeEndingPosition.Y / 16);
                yEnd = (int)((closeEndingPosition.Y + playerHeight - 1) / 16);

                isSolid = Collision.SolidTiles(xStart, xEnd, yStart, yEnd);
                if (!isSolid)
                {
                    return closeDistance;
                }
            }

            return 0f;
        }
    }
}