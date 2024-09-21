using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace SpiritBlossom.System
{
    public class ShaderSystem : ModSystem
    {
        public static Asset<Effect> SoulUnboundTrailShader;
        public static Asset<Effect> SoulUnboundCloneShader;

        public override void Load()
        {
            SoulUnboundTrailShader = ModContent.Request<Effect>("SpiritBlossom/Common/Effects/SoulUnboundTrailShader", AssetRequestMode.ImmediateLoad);
            SoulUnboundCloneShader = ModContent.Request<Effect>("SpiritBlossom/Common/Effects/SoulUnboundCloneShader", AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            SoulUnboundTrailShader = null;
            SoulUnboundCloneShader = null;
        }
    }
}