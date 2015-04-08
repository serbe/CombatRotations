using System;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Blood Death Knight SC", "Serb", WoWClass.DeathKnight, Specialization.DeathknightBlood, 5, 25)]

	public class SerbDeathKnightBloodSC : DeathKnight
	{
		public SerbDeathKnightBloodSC ()
		{
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/blood_presence
			//	actions.precombat+=/horn_of_winter
			if (HornofWinter ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_armor
			//	actions.precombat+=/bone_shield
			BoneShield ();
			//	actions.precombat+=/army_of_the_dead

			return false;
		}

		public override void Combat ()
		{
			//	actions=auto_attack
			//	actions+=/blood_fury,if=target.time_to_die>120|buff.draenic_armor_potion.remains<=buff.blood_fury.duration
			if (TimeToDie (Target) > 120)
				BloodFury ();
			//	actions+=/berserking,if=buff.dancing_rune_weapon.up
			if (Me.HasAura ("Dancing Rune Weapon"))
				Berserking ();
			//	actions+=/dancing_rune_weapon,if=target.time_to_die>90|buff.draenic_armor_potion.remains<=buff.dancing_rune_weapon.duration
			if (TimeToDie (Target) > 120)
				DancingRuneWeapon ();
			//	actions+=/potion,name=draenic_armor,if=target.time_to_die<(buff.draenic_armor_potion.duration+13)
			//	actions+=/blood_fury,if=buff.blast_furnace.up
			if (Me.HasAura ("Blast Furnace"))
				BloodFury ();
			//	actions+=/dancing_rune_weapon,if=target.time_to_die<90&buff.blast_furnace.up
			if (TimeToDie (Target) < 90 && Me.HasAura ("Blast Furnace"))
				DancingRuneWeapon ();
			//	actions+=/potion,name=draenic_armor,if=buff.blast_furnace.up&dot.soul_reaper.ticking&target.time_to_die<120
			//	actions+=/use_item,name=vial_of_convulsive_shadows,if=target.time_to_die>120|buff.draenic_armor_potion.remains<21
			//	actions+=/bone_shield,if=buff.army_of_the_dead.down&buff.bone_shield.down&buff.dancing_rune_weapon.down&buff.icebound_fortitude.down&buff.rune_tap.down
			if (!Me.HasAura ("Army of the Dead") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Rune Tap")) {
				if (BoneShield ())
					return;
			}
			//	actions+=/lichborne,if=health.pct<30
			if (Health < 0.3)
				Lichborne ();
			//	actions+=/vampiric_blood,if=health.pct<40
			if (Heal < 0.4)
				VampiricBlood ();
			//	actions+=/icebound_fortitude,if=health.pct<30&buff.army_of_the_dead.down&buff.dancing_rune_weapon.down&buff.bone_shield.down&buff.rune_tap.down
			if (Health < 0.3 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Rune Tap"))
				IceboundFortitude ();
			//	actions+=/death_pact,if=health.pct<30
			if (Health < 0.3)
				DeathPact ();
			//	actions+=/run_action_list,name=last,if=target.time_to_die<8|target.time_to_die<13&cooldown.empower_rune_weapon.remains<4
			if (TimeToDie (Target) < 8 || TimeToDie (Target) < 13 && Cooldown ("Empower Rune Weapon") < 4) {
				if (Last ())
					return;
			}
			//	actions+=/run_action_list,name=bos,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (Bos ())
					return;
			}
			//	actions+=/run_action_list,name=nbos,if=!dot.breath_of_sindragosa.ticking&cooldown.breath_of_sindragosa.remains<4
			if (!Me.HasAura ("Breath of Sindragosa") && Cooldown ("Breath of Sindragosa") < 4) {
				if (Nbos ())
					return;
			}
			//	actions+=/run_action_list,name=cdbos,if=!dot.breath_of_sindragosa.ticking&cooldown.breath_of_sindragosa.remains>=4
			if (!Me.HasAura ("Breath of Sindragosa") && Cooldown ("Breath of Sindragosa") >= 4) {
				if (Cdbos ())
					return;
			}		}

		public bool Bos ()
		{
			//	actions.bos=blood_tap,if=buff.blood_charge.stack>=11
			if (BloodCharge >= 11)
				BloodTap ();
			//	actions.bos+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<35&runic_power>5
			if (Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) < 35 && RunicPower > 5) {
				if (SoulReaper ())
					return true;
			}
			//	actions.bos+=/blood_tap,if=buff.blood_charge.stack>=9&runic_power>80&(blood.frac>1.8|frost.frac>1.8|unholy.frac>1.8)
			if (BloodCharge >= 9 && RunicPower > 80 && (BloodFrac > 1.8 || FrostFrac > 1.8 || UnholyFrac > 1.8))
				BloodTap ();
			//	actions.bos+=/death_coil,if=runic_power>80&(blood.frac>1.8|frost.frac>1.8|unholy.frac>1.8)
			if (RunicPower > 80 && (BloodFrac > 1.8 || FrostFrac > 1.8 || UnholyFrac > 1.8)) {
				if (DeathCoil ())
					return true;
			}
			//	actions.bos+=/blood_tap,if=buff.blood_charge.stack>=9&runic_power>85&(buff.convulsive_shadows.remains>5|buff.convulsive_shadows.remains>2&buff.bloodlust.up)
			if (BloodCharge >= 9 && RunicPower > 85 && (Me.AuraTimeRemaining ("Convulsive Shadows") > 5 || Me.AuraTimeRemaining ("Convulsive Shadows") > 2 && Me.HasAura ("Bloodlust")))
				BloodTap ();
			//	actions.bos+=/death_coil,if=runic_power>85&(buff.convulsive_shadows.remains>5|buff.convulsive_shadows.remains>2&buff.bloodlust.up)
			//	actions.bos+=/outbreak,if=(!dot.blood_plague.ticking|!dot.frost_fever.ticking)&runic_power>21
			//	actions.bos+=/chains_of_ice,if=!dot.frost_fever.ticking&glyph.icy_runes.enabled&runic_power<90
			//	actions.bos+=/plague_strike,if=!dot.blood_plague.ticking&runic_power>5
			//	actions.bos+=/icy_touch,if=!dot.frost_fever.ticking&runic_power>5
			//	actions.bos+=/death_strike,if=runic_power<16
			//	actions.bos+=/blood_tap,if=runic_power<16
			//	actions.bos+=/blood_boil,if=runic_power<16&runic_power>5&buff.crimson_scourge.down&(blood>=1&blood.death=0|blood=2&blood.death<2)
			//	actions.bos+=/arcane_torrent,if=runic_power<16
			//	actions.bos+=/chains_of_ice,if=runic_power<16&glyph.icy_runes.enabled
			//	actions.bos+=/blood_boil,if=runic_power<16&buff.crimson_scourge.down&(blood>=1&blood.death=0|blood=2&blood.death<2)
			//	actions.bos+=/icy_touch,if=runic_power<16
			//	actions.bos+=/plague_strike,if=runic_power<16
			//	actions.bos+=/rune_tap,if=runic_power<16&blood>=1&blood.death=0&frost=0&unholy=0&buff.crimson_scourge.up
			//	actions.bos+=/empower_rune_weapon,if=runic_power<16&blood=0&frost=0&unholy=0
			//	actions.bos+=/death_strike,if=(blood.frac>1.8&blood.death>=1|frost.frac>1.8|unholy.frac>1.8|buff.blood_charge.stack>=11)
			//	actions.bos+=/blood_tap,if=(blood.frac>1.8&blood.death>=1|frost.frac>1.8|unholy.frac>1.8)
			//	actions.bos+=/blood_boil,if=(blood>=1&blood.death=0&target.health.pct-3*(target.health.pct%target.time_to_die)>35|blood=2&blood.death<2)&buff.crimson_scourge.down
			//	actions.bos+=/antimagic_shell,if=runic_power<65
			//	actions.bos+=/plague_leech,if=runic_power<65
			//	actions.bos+=/outbreak,if=!dot.blood_plague.ticking
			//	actions.bos+=/outbreak,if=pet.dancing_rune_weapon.active&!pet.dancing_rune_weapon.dot.blood_plague.ticking
			//	actions.bos+=/death_and_decay,if=buff.crimson_scourge.up
			//	actions.bos+=/blood_boil,if=buff.crimson_scourge.up
		
			return false;
		}

		public bool Cdbos () {
			//	actions.cdbos=soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35
			//	actions.cdbos+=/blood_tap,if=buff.blood_charge.stack>=10
			//	actions.cdbos+=/death_coil,if=runic_power>65
			//	actions.cdbos+=/plague_strike,if=!dot.blood_plague.ticking&unholy=2
			//	actions.cdbos+=/icy_touch,if=!dot.frost_fever.ticking&frost=2
			//	actions.cdbos+=/death_strike,if=unholy=2|frost=2|blood=2&blood.death>=1
			//	actions.cdbos+=/blood_boil,if=blood=2&blood.death<2
			//	actions.cdbos+=/outbreak,if=!dot.blood_plague.ticking
			//	actions.cdbos+=/plague_strike,if=!dot.blood_plague.ticking
			//	actions.cdbos+=/icy_touch,if=!dot.frost_fever.ticking
			//	actions.cdbos+=/outbreak,if=pet.dancing_rune_weapon.active&!pet.dancing_rune_weapon.dot.blood_plague.ticking
			//	actions.cdbos+=/blood_boil,if=((dot.frost_fever.remains<4&dot.frost_fever.ticking)|(dot.blood_plague.remains<4&dot.blood_plague.ticking))
			//	actions.cdbos+=/death_and_decay,if=buff.crimson_scourge.up
			//	actions.cdbos+=/blood_boil,if=buff.crimson_scourge.up
			//	actions.cdbos+=/death_coil,if=runic_power>45
			//	actions.cdbos+=/blood_tap
			//	actions.cdbos+=/death_strike
			//	actions.cdbos+=/blood_boil,if=blood>=1&blood.death=0
			//	actions.cdbos+=/death_coil

			return false;
		}

		public bool Last ()
		{
			//	actions.last=antimagic_shell,if=runic_power<90
			//	actions.last+=/blood_tap
			BloodTap();
			//	actions.last+=/soul_reaper,if=target.time_to_die>7
			//	actions.last+=/death_coil,if=runic_power>80
			//	actions.last+=/death_strike
			//	actions.last+=/blood_boil,if=blood=2|target.time_to_die<=7
			//	actions.last+=/death_coil,if=runic_power>75|target.time_to_die<4|!dot.breath_of_sindragosa.ticking
			//	actions.last+=/plague_strike,if=target.time_to_die<2|cooldown.empower_rune_weapon.remains<2
			//	actions.last+=/icy_touch,if=target.time_to_die<2|cooldown.empower_rune_weapon.remains<2
			//	actions.last+=/empower_rune_weapon,if=!blood&!unholy&!frost&runic_power<76|target.time_to_die<5
			//	actions.last+=/plague_leech

			return true;
		}

		public bool Nbos ()
		{
			//	actions.nbos=breath_of_sindragosa,if=runic_power>=80
			if (RunicPower >= 80) {
				if (BreathofSindragosa ())
					return true;
			}
			//	actions.nbos+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35
			//	actions.nbos+=/chains_of_ice,if=!dot.frost_fever.ticking&glyph.icy_runes.enabled
			//	actions.nbos+=/icy_touch,if=!dot.frost_fever.ticking
			//	actions.nbos+=/plague_strike,if=!dot.blood_plague.ticking
			//	actions.nbos+=/death_strike,if=(blood.frac>1.8&blood.death>=1|frost.frac>1.8|unholy.frac>1.8)&runic_power<80
			//	actions.nbos+=/death_and_decay,if=buff.crimson_scourge.up
			//	actions.nbos+=/blood_boil,if=buff.crimson_scourge.up|(blood=2&runic_power<80&blood.death<2)

			return true;
		}
	}
}
