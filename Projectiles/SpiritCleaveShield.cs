using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Items;
using ReLogic.Content;

namespace SpiritBlossom.Projectiles
{
    public class SpiritCleaveShield : ModProjectile
    {
        static Texture2D baseLayer;
        static Texture2D blurLayer;
        private int frameCount = 103;
        private int ticksPerFrame = 1;
        private int currentFrame = 0;
        private float scale = 0.3f;

        /*private static void LoadTextures()
        {
            if (baseLayer == null)
            {
                baseLayer = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/SpiritCleaveShieldBaseDrawLayer", AssetRequestMode.ImmediateLoad).Value;
            }
            if (blurLayer == null)
            {
                blurLayer = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/SpiritCleaveShieldGuassianBlurLayer", AssetRequestMode.ImmediateLoad).Value;
            }
        }*/

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 0;
            Projectile.scale = scale;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = frameCount * ticksPerFrame;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        /*public override void Load()
        {
            LoadTextures();
        }*/

        public override bool PreAI()
        {
            currentFrame++;

            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            Projectile.position = player.Center;

            if (++Projectile.frameCounter % ticksPerFrame == 0)
            {
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);

            // Stops the shield deactivation from rendering if the shield is reapplied
            if (sbPlayer.ShieldFrame > 103)
            {
                Projectile.timeLeft = 1;
            }

            SBUtils.PrintCurrentFrame(currentFrame);
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (sbPlayer.ShieldFrame > 103) { return false; }

            SBUtils.DrawFrame(player.Center, 0, scale, TextureAssets.Projectile[Projectile.type].Value, sbPlayer.ShieldFrame, ticksPerFrame, Color.White, false, 10, 11);

            /*Color additiveWhite = Color.White with { A = 0 };
            LoadTextures();
            SBUtils.DrawFrame(player.Center, 0, scale, baseLayer, sbPlayer.ShieldFrame, ticksPerFrame, Color.White, false, 10, 11);
            SBUtils.DrawFrame(player.Center, 0, scale, blurLayer, sbPlayer.ShieldFrame, ticksPerFrame, additiveWhite, false, 10, 11);*/

            sbPlayer.ShieldFrame++;
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}