using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;

namespace SpiritBlossom.Buffs
{
    public class SpiritBlossomCrowdControl : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            SpiritBlossomCrowdControlGlobalNPCs globalNPC = npc.GetGlobalNPC<SpiritBlossomCrowdControlGlobalNPCs>();

            if (globalNPC.CurrentEffect == SpiritBlossomCrowdControlGlobalNPCs.CrowdControl.MortalSteel)
            {
                MortalSteelKnockup(npc, globalNPC, ref buffIndex);
            }
            else if (globalNPC.CurrentEffect == SpiritBlossomCrowdControlGlobalNPCs.CrowdControl.FateSealedStun)
            {
                FateSealedStun(npc, globalNPC, ref buffIndex);
            }
            else
            {
                FateSealedPull(npc, globalNPC, ref buffIndex);
            }
        }

        private void MortalSteelKnockup(NPC npc, SpiritBlossomCrowdControlGlobalNPCs globalNPC, ref int buffIndex)
        {
            npc.velocity = Vector2.Zero;

            if (globalNPC.MortalSteelKnockupDuration > 0)
            {
                globalNPC.MortalSteelCurrentPosition.Y += globalNPC.MortalSteelCurrentVelocity;
                globalNPC.MortalSteelCurrentVelocity -= globalNPC.Gravity;
            }

            npc.position = globalNPC.MortalSteelCurrentPosition;

            globalNPC.MortalSteelKnockupDuration--;
            globalNPC.MortalSteelStunDuration--;

            if (globalNPC.MortalSteelStunDuration <= 0)
            {
                npc.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        private void FateSealedStun(NPC npc, SpiritBlossomCrowdControlGlobalNPCs globalNPC, ref int buffIndex)
        {
            npc.velocity = Vector2.Zero;
            npc.position = globalNPC.FateSealedStunPosition;
        }

        private void FateSealedPull(NPC npc, SpiritBlossomCrowdControlGlobalNPCs globalNPC, ref int buffIndex)
        {
            npc.velocity = Vector2.Zero;

            // SB_Projectile.PrintMessage($"Current Pull Duration Tick: {globalNPC.FateSealedPullDuration}");
            if (globalNPC.FateSealedPullDuration > 0)
            {
                globalNPC.FateSealedCurrentPosition += globalNPC.FateSealedDirectionToEndPosition * globalNPC.FateSealedNPCPullSpeedPerTick;
                // SB_Projectile.PrintMessage($"Pulling NPC: Current Position: {globalNPC.FateSealedCurrentPosition}, Direction: {globalNPC.FateSealedDirectionToEndPosition}, Speed per Tick: {globalNPC.FateSealedNPCPullSpeedPerTick}, Duration: {globalNPC.FateSealedPullDuration}");
            }
            else if (globalNPC.FateSealedPullDuration == 0)
            {
                globalNPC.FateSealedCurrentPosition = globalNPC.FateSealedEndPosition;
                globalNPC.FateSealedPullDuration = -1;
                // SB_Projectile.PrintMessage($"Pull Completed: Final Position: {globalNPC.FateSealedCurrentPosition}");
            }

            npc.position = globalNPC.FateSealedCurrentPosition;

            globalNPC.FateSealedPullDuration--;
            globalNPC.FateSealedStunDuration--;

            if (globalNPC.FateSealedStunDuration <= 0)
            {
                npc.DelBuff(buffIndex);
                buffIndex--;
                // SB_Projectile.PrintMessage($"Stun Ended: NPC Position: {npc.position}");
            }
        }
    }
}