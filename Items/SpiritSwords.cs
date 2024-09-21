using static Terraria.ModLoader.ModContent;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using StarsAbove;
using StarsAbove.Buffs;
using StarsAbove.Items.Essences;
using StarsAbove.Items.Materials;
using StarsAbove.Items.Prisms;
using SpiritBlossom.Projectiles;
using static tModPorter.ProgressUpdate;
using Mono.Cecil;
using static Terraria.ModLoader.PlayerDrawLayer;
using StarsAbove.Projectiles.Melee.Unforgotten;
using System.Linq.Expressions;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Utilities;
using StarsAbove.Systems;
using StarsAbove.Dusts;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace SpiritBlossom.Items
{
    public class SpiritSwords : ModItem
    {
        private Vector2 playerDirection;
        private const int tapOrHoldThreshold = 8;
        private int leftClickTapOrHoldThresholdCharge = 0;
        private int rightClickTapOrHoldThresholdCharge = 0;

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 500;
            Item.crit = 60;
            Item.DamageType = DamageClass.Melee;
            Item.width = 65;
            Item.height = 70;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = 5;
            Item.noMelee = true; 
            Item.noUseGraphic = true;
            Item.knockBack = 4;
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = 0;
            Item.shootSpeed = 1f;
            Item.useTurn = true;
            Item.value = Item.buyPrice(gold: 100);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (sbPlayer.IsChannelingAbility(player)) { return false; }

            if (player.altFunctionUse == 2 && !player.HasBuff(BuffType<Buffs.FateSealedBuff>()))
            {
                if (player.controlUp && player.HasBuff(BuffType<Buffs.FateSealedCooldown>()))
                {
                    return false;
                }
                else if (player.HasBuff(BuffType<Buffs.SoulUnboundCooldown>()))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (!Main.dedServ && Item.UseSound.HasValue)
            {
                SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
            }

            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (leftClickTapOrHoldThresholdCharge >= tapOrHoldThreshold || player.HasBuff(BuffType<Buffs.MortalSteelCooldown>()))
            {
                int direction = (Main.MouseWorld.X > 0).ToDirectionInt();
                player.ChangeDir(direction);
                AlternateBetweenSteelAndAzakanaSwordSwings(player, sbPlayer);
                return true;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            UpdateTriggersAndInternalCooldowns(player);

            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (sbPlayer.IsChannelingAbility(player)) { return; }

            if (player.altFunctionUse == 2 && !player.HasBuff(BuffType<Buffs.FateSealedBuff>()))
            {
                if (player.controlUp)
                {
                    CastFateSealed(player, sbPlayer);
                }
                else
                {
                    CastSoulUnbound(player, sbPlayer);
                }
            }
            else if (player.channel)
            {
                if (player.controlUp)
                {
                    CastSpiritCleave(player, sbPlayer);
                }
                else
                {
                    leftClickTapOrHoldThresholdCharge = leftClickTapOrHoldThresholdCharge >= tapOrHoldThreshold ? tapOrHoldThreshold : ++leftClickTapOrHoldThresholdCharge;
                }
            }
            else
            {
                if (leftClickTapOrHoldThresholdCharge < tapOrHoldThreshold && leftClickTapOrHoldThresholdCharge != 0 && !player.HasBuff(BuffType<Buffs.FateSealedBuff>()))
                {
                    CastMortalSteel(player, sbPlayer);
                }
                leftClickTapOrHoldThresholdCharge = 0;
            }
        }

        private void AlternateBetweenSteelAndAzakanaSwordSwings(Player player, SpiritBlossomPlayer sbPlayer)
        {
            sbPlayer.CurrentSwingResetTimer = SpiritBlossomPlayer.CurrentSwingResetTime;
            if (sbPlayer.CurrentSwing)
            {
                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.MountedCenter.X, player.MountedCenter.Y, playerDirection.X * 59f, playerDirection.Y * 59f, ProjectileType<Projectiles.StarsAboveSource.SteelTempestSwing>(), Item.damage, 3, player.whoAmI);
            }
            else
            {
                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.MountedCenter.X, player.MountedCenter.Y, playerDirection.X * 59f, playerDirection.Y * 59f, ProjectileType<Projectiles.StarsAboveSource.SteelTempestSwing2>(), Item.damage, 0, player.whoAmI);
            }
            sbPlayer.CurrentSwing = !sbPlayer.CurrentSwing;
            /*int spriteDirection = ((Main.MouseWorld.X - player.Center.X) > 0) ? 1 : -1;
            player.ChangeDir(spriteDirection);*/
        }

        private void CastMortalSteel(Player player, SpiritBlossomPlayer sbPlayer)
        {
            if (player.HasBuff(BuffType<Buffs.MortalSteelCooldown>())) { return; }

            if (player.HasBuff(BuffType<Buffs.GatheringStormReady>()))
            {
                player.immune = true;
                player.immuneTime = 60;
                player.SetDummyItemTime(SpiritBlossomPlayer.MortalSteelDashUseTime);

                player.velocity = Vector2.Zero;

                sbPlayer.InitializeMortalSteelDashValues(player, playerDirection);
                player.ClearBuff(BuffType<Buffs.GatheringStormReady>());
                player.AddBuff(BuffType<Buffs.MortalSteelBuff>(), SpiritBlossomPlayer.MortalSteelDashDuration);

                SoundEngine.PlaySound(sbPlayer.Q3Cast with { Volume = SBUtils.GlobalSFXVolume });
                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, playerDirection, ProjectileType<MortalSteelWave>(), Item.damage * 2, 0, player.whoAmI);
            }
            else
            {
                player.SetDummyItemTime(SpiritBlossomPlayer.MortalSteelUseTime);

                SBUtils.SoundIndex = Main.rand.Next(2);
                SoundEngine.PlaySound(sbPlayer.QCast[SBUtils.SoundIndex] with { Volume = SBUtils.GlobalSFXVolume });

                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, playerDirection, ProjectileType<MortalSteel>(), Item.damage * 2, 0, player.whoAmI);
                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, playerDirection, ProjectileType<MortalSteelFlash>(), 0, 0, player.whoAmI);
                player.AddBuff(BuffType<Buffs.MortalSteelCooldown>(), SpiritBlossomPlayer.MortalSteelCooldown);
            }
        }

        private void CastSpiritCleave(Player player, SpiritBlossomPlayer sbPlayer)
        {
            if (player.HasBuff(BuffType<Buffs.SpiritCleaveCooldown>())) { return; }
            
            player.SetDummyItemTime(SpiritBlossomPlayer.SpiritCleaveUseTime);

            SoundEngine.PlaySound(sbPlayer.WCast with { Volume = SBUtils.GlobalSFXVolume });

            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, playerDirection, ProjectileType<SpiritCleave>(), Item.damage * 2, 0, player.whoAmI);
            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, playerDirection, ProjectileType<SpiritCleaveWave>(), 0, 0, player.whoAmI);

            player.AddBuff(BuffType<Buffs.SpiritCleaveCooldown>(), SpiritBlossomPlayer.SpiritCleaveCooldown);
        }

        private void CastSoulUnbound(Player player, SpiritBlossomPlayer sbPlayer)
        {
            if (player.HasBuff(BuffType<Buffs.SoulUnboundBuff>())) { 
                if (sbPlayer.SoulUnboundFrame > SpiritBlossomPlayer.SoulUnboundUseTime) {
                    player.ClearBuff(BuffType<Buffs.SoulUnboundBuff>());
                    sbPlayer.RecastSoulUnbound(player);
                }
                return;
            }

            if (player.HasBuff(BuffType<Buffs.SoulUnboundBuff>()) || player.HasBuff(BuffType<Buffs.SoulUnboundRecast>()) || player.HasBuff(BuffType<Buffs.SoulUnboundCooldown>())) { return; }

            sbPlayer.InitializeSoulUnboundDashValues(player, playerDirection);
            player.AddBuff(BuffType<Buffs.SoulUnboundBuff>(), SpiritBlossomPlayer.SoulUnboundDuration);

            player.immune = true;
            player.immuneTime = 30;
            player.SetDummyItemTime(SpiritBlossomPlayer.SoulUnboundUseTime);

            SBUtils.SoundIndex = Main.rand.Next(3);
            sbPlayer.ETetherSlot = SoundEngine.PlaySound(sbPlayer.ETether[SBUtils.SoundIndex] with { Volume = SBUtils.GlobalSFXVolume });
            SoundEngine.TryGetActiveSound(sbPlayer.ETetherSlot, out sbPlayer.ETetherSound);
            SoundEngine.PlaySound(sbPlayer.ECast with { Volume = SBUtils.GlobalSFXVolume });

            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.position, playerDirection, ProjectileType<SoulUnbound>(), 0, 0, player.whoAmI);
        }

        private void CastFateSealed(Player player, SpiritBlossomPlayer sbPlayer)
        {
            if (player.HasBuff(BuffType<Buffs.FateSealedBuff>()) || player.HasBuff(BuffType<Buffs.FateSealedCooldown>())) { return; }

            player.immune = true;
            player.immuneTime = 120;
            player.velocity = Vector2.Zero;
            player.SetDummyItemTime(SpiritBlossomPlayer.FateSealedUseTime);

            sbPlayer.InitializeFateSealedBlinkValues(player, playerDirection);
            player.AddBuff(BuffType<Buffs.FateSealedBuff>(), SpiritBlossomPlayer.FateSealedUseTime + 1);

            SoundEngine.PlaySound(sbPlayer.RCast with { Volume = SBUtils.GlobalSFXVolume });
            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, playerDirection, ProjectileType<FateSealed>(), Item.damage * 3, 0, player.whoAmI);
        }

        private void UpdateTriggersAndInternalCooldowns(Player player)
        {
            playerDirection = Vector2.Normalize(Main.MouseWorld - player.Center);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Cloud, 25);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddTile(TileID.LunarCraftingStation);
            if (ModLoader.TryGetMod("StarsAbove", out Mod starsAbove) && starsAbove.TryFind<ModItem>("PrismaticCore", out ModItem PrismaticCore) && starsAbove.TryFind<ModItem>("Unforgotten", out ModItem Unforgotten))
            {
                recipe.AddIngredient(PrismaticCore, 5);
                recipe.AddIngredient(Unforgotten, 1);
            }
            else
            {
                recipe.AddIngredient(ItemID.Muramasa, 1);
                recipe.AddIngredient(ItemID.Katana, 1);
            }
            recipe.Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int index = tooltips.FindIndex(line => line.Name == "Tooltip") + 6;

            bool shiftPressed = Main.keyState.PressingShift();

            bool key1 = Main.keyState.IsKeyDown(Keys.D1);
            bool key2 = Main.keyState.IsKeyDown(Keys.D2);
            bool key3 = Main.keyState.IsKeyDown(Keys.D3);
            bool key4 = Main.keyState.IsKeyDown(Keys.D4);
            bool key5 = Main.keyState.IsKeyDown(Keys.D5);

            string starsAboveSpatialIcon = (ModLoader.HasMod("StarsAbove")) ? "[i:StarsAbove/Spatial]" : "";

            if (!shiftPressed && !key1 && !key2 && !key3 && !key4 && !key5)
            {
                // Show summary
                TooltipLine summaryLine = new(Mod, "Summary",
                    "[c/a3a3a3:This weapon has a multitude of attacks and abilities]\n" +
                    GetWayOfTheHunterSummary() + "\n" +
                    GetMortalSteelSummary() + "\n" +
                    GetSpiritCleaveSummary() + "\n" +
                    GetSoulUnboundSummary() + "\n" +
                    GetFateSealedSummary() + "\n" +
                    "Hold [1], [2], [3], [4], or [5] for more detailed descriptions, or hold [Shift] to show all descriptions\n" +
                    starsAboveSpatialIcon
                );
                tooltips.Insert(index + 1, summaryLine);
                return;
            }

            string wayOfTheHunterDescription = (shiftPressed || key1) ? GetWayOfTheHunterExtended() : GetWayOfTheHunterSummary();
            string mortalSteelDescription = (shiftPressed || key2) ? GetMortalSteelExtended() : GetMortalSteelSummary();
            string spiritCleaveDescription = (shiftPressed || key3) ? GetSpiritCleaveExtended() : GetSpiritCleaveSummary();
            string soulUnboundDescription = (shiftPressed || key4) ? GetSoulUnboundExtended() : GetSoulUnboundSummary();
            string fateSealedDescription = (shiftPressed || key5) ? GetFateSealedExtended() : GetFateSealedSummary();

            TooltipLine headerLine = new(Mod, "Header", "[c/a3a3a3:This weapon has a multitude of attacks and abilities]");
            tooltips.Insert(index + 1, headerLine);

            int currentInsertIndex = index + 2;

            if (key1 || shiftPressed)
            {
                TooltipLine wayOfTheHunterLine = new(Mod, "WayOfTheHunter", "[Way of the Hunter]");
                wayOfTheHunterLine.OverrideColor = new Color(142, 96, 209);
                tooltips.Insert(currentInsertIndex++, wayOfTheHunterLine);
            }
            TooltipLine wayOfTheHunterDescLine = new(Mod, "WayOfTheHunterDesc", wayOfTheHunterDescription);
            tooltips.Insert(currentInsertIndex++, wayOfTheHunterDescLine);

            if (key2 || shiftPressed)
            {
                TooltipLine mortalSteelLine = new(Mod, "MortalSteel", "[Mortal Steel]");
                mortalSteelLine.OverrideColor = new Color(142, 96, 209);
                tooltips.Insert(currentInsertIndex++, mortalSteelLine);
            }
            TooltipLine mortalSteelDescLine = new(Mod, "MortalSteelDesc", mortalSteelDescription);
            tooltips.Insert(currentInsertIndex++, mortalSteelDescLine);

            if (key3 || shiftPressed)
            {
                TooltipLine spiritCleaveLine = new(Mod, "SpiritCleave", "[Spirit Cleave]");
                spiritCleaveLine.OverrideColor = new Color(142, 96, 209);
                tooltips.Insert(currentInsertIndex++, spiritCleaveLine);
            }
            TooltipLine spiritCleaveDescLine = new(Mod, "SpiritCleaveDesc", spiritCleaveDescription);
            tooltips.Insert(currentInsertIndex++, spiritCleaveDescLine);

            if (key4 || shiftPressed)
            {
                TooltipLine soulUnboundLine = new(Mod, "SoulUnbound", "[Soul Unbound]");
                soulUnboundLine.OverrideColor = new Color(142, 96, 209);
                tooltips.Insert(currentInsertIndex++, soulUnboundLine);
            }
            TooltipLine soulUnboundDescLine = new(Mod, "SoulUnboundDesc", soulUnboundDescription);
            tooltips.Insert(currentInsertIndex++, soulUnboundDescLine);

            if (key5 || shiftPressed)
            {
                TooltipLine fateSealedLine = new(Mod, "FateSealed", "[Fate Sealed]");
                fateSealedLine.OverrideColor = new Color(142, 96, 209);
                tooltips.Insert(currentInsertIndex++, fateSealedLine);
            }
            TooltipLine fateSealedDescLine = new(Mod, "FateSealedDesc", fateSealedDescription);
            tooltips.Insert(currentInsertIndex++, fateSealedDescLine);

            if (ModLoader.HasMod("StarsAbove"))
            {
                TooltipLine spatialIcon = new(Mod, "SpatialIcon", "[i:StarsAbove/Spatial]");
                tooltips.Insert(currentInsertIndex++, spatialIcon);
            }
        }

        private string GetWayOfTheHunterSummary()
        {
            return "[c/8E60D1:Way of the Hunter]: [Hold Primary Fire] Alternate between two katanas: [c/85C4DA:Steel Wind] and [c/F10079:Spirit Azakana]";
        }

        private string GetWayOfTheHunterExtended()
        {
            return "Hold your Primary Fire key to alternate between two katanas, [c/85C4DA:Steel Wind] and [c/F10079:Spirit Azakana]\n" +
                   "[c/85C4DA:Steel Wind] is guaranteed to critically strike, independent of other crit calculations\n" +
                   "[c/F10079:Spirit Azakana] has no knockback and cannot critically strike but repeats half the damage dealt as true damage";
        }

        private string GetMortalSteelSummary()
        {
            return "[c/8E60D1:Mortal Steel]: [Tap Primary Fire] Thrust forward, dashing forward instead on the third strike";
        }

        private string GetMortalSteelExtended()
        {
            return "Tap your Primary Fire key to thrust [c/85C4DA:Steel Wind] forward in a target direction, dealing 200% weapon damage\n" +
                   $"On hit, gain a stack of [c/85C4DA:Gathering Storm] for {SpiritBlossomPlayer.MortalSteelGatheringStormStacksResetTime / 60f} seconds\n" +
                   "At 2 stacks, instead dash forward with a gust of wind, dealing 200% weapon damage and knocking up enemies caught in its path";
        }

        private string GetSpiritCleaveSummary()
        {
            return "[c/8E60D1:Spirit Cleave]: [Tap Primary Fire + UP] Cleave in a cone, gaining a shield on hit";
        }

        private string GetSpiritCleaveExtended()
        {
            return "Tap your Primary Fire key while holding UP to cleave [c/F10079:Spirit Azakana] in a cone in the target direction for 200% weapon damage\n" +
                   $"On hit, gain a {SpiritBlossomPlayer.SpiritCleavePrimaryHitShieldAmount} health shield for {(SpiritBlossomPlayer.SpiritCleaveShieldDuration / 60f)} seconds, increased by {SpiritBlossomPlayer.SpiritCleaveSecondaryHitShieldAmount} for every additional target struck.";
        }

        private string GetSoulUnboundSummary()
        {
            return $"[c/8E60D1:Soul Unbound]: [Tap Alternate Fire] Enter [c/8E60D1:Spirit Form], gaining movement speed, then dealing damage and dashing back after {(SpiritBlossomPlayer.SoulUnboundDuration / 60f)} seconds";
        }

        private string GetSoulUnboundExtended()
        {
            return $"Tap your Alternate Fire key to sever your soul, discarding your body to dash forward, entering [c/8E60D1:Spirit Form]\n" +
                   $"[c/8E60D1:Spirit Form] lasts {(SpiritBlossomPlayer.SoulUnboundDuration / 60f)} seconds, during which you gain {(SpiritBlossomPlayer.SoulUnboundMinMovementSpeedBonus * 100)}% Movement Speed, ramping to {(SpiritBlossomPlayer.SoulUnboundMaxMovementSpeedBonus * 100)}% over its duration\n" +
                   $"Once [c/8E60D1:Soul Unbound] ends, dash back to your body, repeating {(int)(SpiritBlossomPlayer.SoulUnboundStoredDamageRatio * 100)}% of damage dealt to each target you struck while in [c/8E60D1:Spirit Form]";
        }

        private string GetFateSealedSummary()
        {
            return "[c/8E60D1:Fate Sealed]: [Tap Alternate Fire + UP] Channel a devastating strike that pull enemies towards you";
        }

        private string GetFateSealedExtended()
        {
            return "Tap your Alternate Fire key while holding UP to channel a devastating strike while briefly becoming immune to damage\n" +
                   "After a brief delay, blink behind the farthest enemy, stunning and marking all enemies caught in your path and dealing 300% weapon damage\n" +
                   "[c/8E60D1:Fate Sealed] then pulls all marked enemies towards you with a powerful gust of wind, for an additional 300% weapon damage\n" +
                   "\"No fate nor destiny! Only tomorrow!\"";
        }
    }
}