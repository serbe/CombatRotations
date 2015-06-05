// Need update

using ReBot.API;
using System.Linq;
using System;

namespace ReBot
{
	[Rotation ("Serb Druid Guardian", "Serb", WoWClass.Druid, Specialization.DruidGuardian, 5, 25)]

	public class SerbDruidGuardian : SerbDruid
	{
		public 	SerbDruidGuardian ()
		{
			GroupBuffs = new[] {
				"Mark of the Wild"
			};
			PullSpells = new[] {
				"Maul",
				"Mangle"
//				"Faerie Swarm",
//				"Faerie Fire"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_agility_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/mark_of_the_wild,if=!aura.str_agi_int.up
			//	actions.precombat+=/bear_form
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/cenarion_ward
			if (MarkoftheWild (Me))
				return true;

			if (Health (Me) < 0.9) {
				if (Rejuvenation (Me))
					return true;
			}
			if (Health (Me) < 0.6) {
				if (HealingTouch (Me))
					return true;
			}

			if (InCombat) {
				InCombat = false;
			}

			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (Interrupt ())
				return;

			if (BearForm ())
				return;

			if (Health (Me) < 0.9)
				HealTank ();

			if (ActiveEnemies (8) == 1)
				SingleTarget ();
			else
				MultiTarget ();

		}

		public void SingleTarget ()
		{
			if (Mangle ())
				return;
			if (!Target.HasAura ("Pulverize") && !Target.HasAura ("Lacerate", true, 3)) {
				if (Lacerate ())
					return;
			}
			if (Target.HasAura ("Lacerate", true, 3)) {
				if (Pulverize ())
					return;
			}
			if (!Target.HasAura ("Thrash")) {
				if (Thrash ())
					return;
			}
			if (Me.HasAura ("Tooth and Claw") || Rage > MaxPower - 20 || DamageTaken (1000) > 0)
				Maul ();
		}

		public void MultiTarget ()
		{
			if (Usable ("Thrash")) {
				Unit = Enemy.Where (u => Range (8, u) && !u.HasAura ("Thrash")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Thrash ())
					return;
			}
			if (!Target.HasAura ("Pulverize") && !Target.HasAura ("Lacerate", true, 3)) {
				if (Lacerate ())
					return;
			}
			if (Target.HasAura ("Lacerate", true, 3)) {
				if (Pulverize ())
					return;
			}
			if (Mangle ())
				return;
		}

		public void SC ()
		{
			//	actions=auto_attack
			//	actions+=/skull_bash
			if (Target.CombatRange > 6) {
				if (SkullBash ())
					return;
			}
			//	actions+=/savage_defense,if=buff.barkskin.down
			if (Health (Me) < 0.5 && !Me.HasAura ("Barkskin")) {
				if (SavageDefense ())
					return;
			}
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/use_item,slot=trinket2
			//	actions+=/barkskin,if=buff.bristling_fur.down
			if (Health (Me) < 0.65 && !Me.HasAura ("Bristling Fur"))
				Barkskin ();
			//	actions+=/bristling_fur,if=buff.barkskin.down&buff.savage_defense.down
			if (Health (Me) < 0.3 && !Me.HasAura ("Barkskin") && !Me.HasAura ("Savage Defense"))
				BristlingFur ();
			//	actions+=/maul,if=buff.tooth_and_claw.react&incoming_damage_1s
			if (Me.HasAura ("Tooth and Claw") && DamageTaken (1000) > 0)
				Maul ();
			//	actions+=/berserk,if=buff.pulverize.remains>10
			if (Me.AuraTimeRemaining ("Pulverize") > 10)
				Berserk ();
			//	actions+=/frenzied_regeneration,if=rage>=80
			if (Health (Me) < 0.5 && Rage >= 80)
				FrenziedRegeneration ();
			//	actions+=/cenarion_ward
			if (Health (Me) < 0.9) {
				if (CenarionWard (Me))
					return;
			}
			//	actions+=/renewal,if=health.pct<30
			if (Health (Me) < 0.3)
				Renewal ();
			//	actions+=/heart_of_the_wild
			if (Health (Me) < 0.5)
				HeartoftheWild ();
			//	actions+=/rejuvenation,if=buff.heart_of_the_wild.up&remains<=3.6
			if (Me.HasAura ("Heart of the Wild") && Me.AuraTimeRemaining ("Rejuvenation") <= 3.6) {
				if (Rejuvenation (Me))
					return;
			}
			//	actions+=/natures_vigil
			NaturesVigil ();
			//	actions+=/healing_touch,if=buff.dream_of_cenarius.react&health.pct<30
			if (Me.HasAura ("Dream of Cenarius") && Health (Me) < 0.3) {
				if (HealingTouch (Me))
					return;
			}
			//	actions+=/pulverize,if=buff.pulverize.remains<=3.6
			if (Me.AuraTimeRemaining ("Pulverize") <= 3.6)
				Pulverize ();
			//	actions+=/lacerate,if=talent.pulverize.enabled&buff.pulverize.remains<=(3-dot.lacerate.stack)*gcd&buff.berserk.down
			if (HasSpell ("Pulverize") && Me.AuraTimeRemaining ("Pulverize") <= (3 - AuraStackCount ("Lacerate")) * 1.5 && !Me.HasAura ("Berserk")) {
				if (Lacerate ())
					return;
			}
			//	actions+=/incarnation
			if (IncarnationSonofUrsoc ())
				return;
			//	actions+=/lacerate,if=!ticking
			if (!Target.HasAura ("Lacerate")) {
				if (Lacerate ())
					return;
			}
			//	actions+=/thrash_bear,if=!ticking
			if (!Target.HasAura ("Thrash")) {
				if (Thrash ())
					return;
			}

			if (ActiveEnemies (10) > 1) {
				Unit = Enemy.Where (u => Range (8, u) && !u.HasAura ("Thrash")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Thrash (Unit))
					return;
			}
			//	actions+=/mangle
			if (Mangle ())
				return;
			//	actions+=/thrash_bear,if=remains<=4.8
			if (Target.AuraTimeRemaining ("Thrash") <= 4.8) {
				if (Thrash ())
					return;
			}
			//	actions+=/lacerate
			if (Lacerate ())
				return;
		}

	}
}
