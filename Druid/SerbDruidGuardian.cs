using ReBot.API;
using System.Security.Cryptography;

namespace ReBot
{
	[Rotation ("Serb Druid Guardian SC", "Serb", WoWClass.Druid, Specialization.DruidGuardian, 5, 25)]

	public class SerbDruidGuardianSC : SerbDruid
	{
		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_agility_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/mark_of_the_wild,if=!aura.str_agi_int.up
			//	actions.precombat+=/bear_form
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/cenarion_ward

			return false;
		}

		public override void Combat ()
		{
			if (BearForm ())
				return;
			//	actions=auto_attack
			//	actions+=/skull_bash
			if (Range () > 6) {
				if (SkullBash ())
					return;
			}
			//	actions+=/savage_defense,if=buff.barkskin.down
			if (!Me.HasAura ("Barkskin")) {
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
			if (!Me.HasAura ("Bristling Fur"))
				Barkskin ();
			//	actions+=/bristling_fur,if=buff.barkskin.down&buff.savage_defense.down
			if (!Me.HasAura ("Barkskin") && !Me.HasAura ("Savage Defense"))
				BristlingFur ();
			//	actions+=/maul,if=buff.tooth_and_claw.react&incoming_damage_1s
			if (Me.HasAura ("Tooth and Claw") && (DamageTaken () / 10) > 0)
				Maul ();
			//	actions+=/berserk,if=buff.pulverize.remains>10
			//	actions+=/frenzied_regeneration,if=rage>=80
			//	actions+=/cenarion_ward
			//	actions+=/renewal,if=health.pct<30
			//	actions+=/heart_of_the_wild
			//	actions+=/rejuvenation,if=buff.heart_of_the_wild.up&remains<=3.6
			//	actions+=/natures_vigil
			//	actions+=/healing_touch,if=buff.dream_of_cenarius.react&health.pct<30
			//	actions+=/pulverize,if=buff.pulverize.remains<=3.6
			//	actions+=/lacerate,if=talent.pulverize.enabled&buff.pulverize.remains<=(3-dot.lacerate.stack)*gcd&buff.berserk.down
			//	actions+=/incarnation
			//	actions+=/lacerate,if=!ticking
			//	actions+=/thrash_bear,if=!ticking
			//	actions+=/mangle
			//	actions+=/thrash_bear,if=remains<=4.8
			//	actions+=/lacerate

		}
	}
}
