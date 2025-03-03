using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.Content.BehaviorOverrides.BossAIs.Destroyer.DestroyerHeadBehaviorOverride;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Destroyer
{
    public class DestroyerBodyBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.TheDestroyerBody;

        public override bool PreAI(NPC npc) => DoBehavior(npc);

        public static bool DoBehavior(NPC npc)
        {
            NPC aheadSegment = Main.npc[(int)npc.ai[1]];
            if (!aheadSegment.active || npc.realLife <= -1)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return false;

                npc.life = 0;
                npc.HitEffect();
                npc.active = false;
                npc.netUpdate = true;
            }

            // Reset the segment scale if necessary.
            ResetScale(npc);

            // Inherit various attributes from the ahead segment.
            // This code will go upstream across every segment, until it reaches the head.
            NPC head = Main.npc[npc.realLife];
            npc.Opacity = aheadSegment.Opacity;
            npc.chaseable = true;
            npc.friendly = false;
            npc.dontTakeDamage = aheadSegment.dontTakeDamage;
            npc.damage = npc.dontTakeDamage ? 0 : npc.defDamage;
            npc.Calamity().DR = 0.5f;
            npc.defense = 12;

            // Completely disable the pink light effect that Calamity's rev+ destroyer AI uses.
            npc.Calamity().newAI[1] = 600f;

            npc.buffImmune[ModContent.BuffType<CrushDepth>()] = true;

            Vector2 directionToNextSegment = aheadSegment.Center - npc.Center;
            if (aheadSegment.rotation != npc.rotation)
                directionToNextSegment = directionToNextSegment.RotatedBy(WrapAngle(aheadSegment.rotation - npc.rotation) * 0.03f);

            directionToNextSegment = directionToNextSegment.SafeNormalize(Vector2.Zero);

            npc.rotation = directionToNextSegment.ToRotation() + PiOver2;
            npc.Center = aheadSegment.Center - directionToNextSegment * npc.width * npc.scale * 0.725f;
            npc.spriteDirection = (directionToNextSegment.X > 0).ToDirectionInt();

            if (!Main.npc.IndexInRange(npc.realLife) || !head.active)
            {
                npc.active = false;
                return false;
            }

            float segmentNumber = npc.Infernum().ExtraAI[0];
            float headAttackTimer = head.ai[2];
            Player target = Main.player[head.target];
            if (head.ai[1] == (int)DestroyerAttackType.DiveBombing && headAttackTimer - 45f == segmentNumber && segmentNumber % 6f == 5f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(npc.Center, npc.SafeDirectionTo(target.Center) * 30f, ModContent.ProjectileType<EnergySpark2>(), EnergySparkDamage, 0f);
            }

            if (head.ai[1] == (int)DestroyerAttackType.EnergyBlasts && head.Infernum().ExtraAI[0] == 2f && headAttackTimer - 45f == segmentNumber)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(npc.Center, npc.SafeDirectionTo(target.Center) * 42f, ModContent.ProjectileType<EnergySpark2>(), EnergySparkDamage, 0f);
            }

            if (head.ai[1] == (int)DestroyerAttackType.LaserSpin && head.Infernum().ExtraAI[0] == segmentNumber && headAttackTimer % 8f == 7f && headAttackTimer > 45f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(npc.Center, npc.SafeDirectionTo(target.Center) * 26f, ModContent.ProjectileType<EnergySpark2>(), EnergySparkDamage, 0f);
            }
            return false;
        }
    }
}