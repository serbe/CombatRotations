using ReBot.API;
using System;
using Newtonsoft.Json;

namespace ReBot
{
	[Rotation ("SC DeathKnight Frost", "Serb", WoWClass.DeathKnight, Specialization.DeathknightFrost, 5, 25)]

	public class DeathKnightFrost: SerbDeathKnight
	{
		[JsonProperty ("Use 2H")]
		public bool Use2H = true;

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=buttered_sturgeon
			//	actions.precombat+=/horn_of_winter
			if (HornofWinter ())
				return true;
			//	actions.precombat+=/frost_presence
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/army_of_the_dead
			//	actions.precombat+=/potion,name=draenic_strength
			//	actions.precombat+=/pillar_of_frost

			return false;
		}

		public override void Combat ()
		{
			

			//	actions=auto_attack
			//	actions+=/deaths_advance,if=movement.remains>2
			if (Me.IsMoving) {
				if (!InRun) {
					StartRun = DateTime.Now;
					InRun = true;
					return;
				}
				if (InRun && TimeRun >= TDA) {
					DeathsAdvance ();
				}
			} else {
				InRun = false;
			}
			//	actions+=/antimagic_shell,damage=100000,if=((dot.breath_of_sindragosa.ticking&runic_power<25)|cooldown.breath_of_sindragosa.remains>40)|!talent.breath_of_sindragosa.enabled

			//	actions+=/pillar_of_frost
			if (PillarofFrost ())
				return;
			//	actions+=/potion,name=draenic_strength,if=target.time_to_die<=30|(target.time_to_die<=60&buff.pillar_of_frost.up)
			//	actions+=/empower_rune_weapon,if=target.time_to_die<=60&buff.potion.up
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/use_item,slot=trinket2
			//	actions+=/plague_leech,if=disease.min_remains<1
			if (DiseaseMinRemains () < 1) {
				if (PlagueLeech ())
					return;
			}
			//	actions+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35
			if (Health () - 3 * (Health () / TimeToDie ()) <= 0.35) {
				if (SoulReaper ())
					return;
			}
			//	actions+=/blood_tap,if=(target.health.pct-3*(target.health.pct%target.time_to_die)<=35&cooldown.soul_reaper.remains=0)
			if (Health () - 3 * (Health () / TimeToDie ()) <= 0.35 && Usable ("Soul Reaper")) {
				if (BloodTap ())
					return;
			}
			//	actions+=/run_action_list,name=single_target_2h,if=active_enemies<4&main_hand.2h
			if (Use2H && ActiveEnemies (8) < 4) {
				if (Single_target_2h ())
					return;
			}
			//	actions+=/run_action_list,name=single_target_1h,if=active_enemies<3&main_hand.1h
			if (!Use2H && ActiveEnemies (8) < 3) {
				if (Single_target_1h ())
					return;
			}
			//	actions+=/run_action_list,name=multi_target,if=active_enemies>=3+main_hand.2h
			if ((Use2H && ActiveEnemies (8) > 3) || (!Use2H && ActiveEnemies (8) > 2)) {
				if (Multi_target ())
					return;
			}
		}

		bool Single_target_2h ()
		{
			//	actions.single_target_2h=defile
			if (Defile ())
				return true;
			//	actions.single_target_2h+=/blood_tap,if=talent.defile.enabled&cooldown.defile.remains=0
			if (Usable ("Defile")) {
				if (BloodTap ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=buff.rime.react&disease.min_remains>5&buff.killing_machine.react
			if (Me.HasAura ("Rime") && DiseaseMinRemains () > 5 && Me.HasAura ("Killing Machine")) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/obliterate,if=buff.killing_machine.react
			if (Me.HasAura ("Killing Machine")) {
				if (Obliterate ())
					return true;
			}
			//	actions.single_target_2h+=/blood_tap,if=buff.killing_machine.react
			if (Me.HasAura ("Killing Machine")) {
				if (BloodTap ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=!talent.necrotic_plague.enabled&!dot.frost_fever.ticking&buff.rime.react
			if (!HasSpell ("Necrotic Plague") && !HasFrostDisease () && Me.HasAura ("Rime")) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/outbreak,if=!disease.max_ticking
			if (!HasDisease) {
				if (Outbreak ())
					return true;
			}
			//	actions.single_target_2h+=/unholy_blight,if=!disease.min_ticking
			if (!HasDisease) {
				if (UnholyBlight ())
					return true;
			}
			//	actions.single_target_2h+=/breath_of_sindragosa,if=runic_power>75
			if (RunicPower > 75) {
				if (BreathofSindragosa ())
					return true;
			}
			//	actions.single_target_2h+=/run_action_list,name=single_target_bos,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (Single_target_bos ())
					return true;
			}
			//	actions.single_target_2h+=/obliterate,if=talent.breath_of_sindragosa.enabled&cooldown.breath_of_sindragosa.remains<7&runic_power<76
			if (HasSpell ("Breath of Sindragosa") && Cooldown ("Breath of Sindragosa") < 7 && RunicPower < 76) {
				if (Obliterate ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=talent.breath_of_sindragosa.enabled&cooldown.breath_of_sindragosa.remains<3&runic_power<88
			if (HasSpell ("Breath of Sindragosa") && Cooldown ("Breath of Sindragosa") < 3 && RunicPower < 88) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=!talent.necrotic_plague.enabled&!dot.frost_fever.ticking
			if (!HasSpell ("Necrotic Plague") && !HasFrostDisease ()) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=talent.necrotic_plague.enabled&!dot.necrotic_plague.ticking
			if (HasSpell ("Necrotic Plague") && !HasNecroticDisease ()) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/plague_strike,if=!talent.necrotic_plague.enabled&!dot.blood_plague.ticking
			if (!HasSpell ("Necrotic Plague") && !HasBloodDisease ()) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.single_target_2h+=/blood_tap,if=buff.blood_charge.stack>10&runic_power>76
			if (BloodCharge > 10 && RunicPower > 76) {
				if (BloodTap ())
					return true;
			}
			//	actions.single_target_2h+=/frost_strike,if=runic_power>76
			if (RunicPower > 76) {
				if (FrostStrike ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=buff.rime.react&disease.min_remains>5&(blood.frac>=1.8|unholy.frac>=1.8|frost.frac>=1.8)
			if (Me.HasAura ("Rime") && DiseaseMinRemains > 5 && (BloodFrac >= 1.8 || UnholyFrac >= 1.8 || FrostFrac >= 1.8)) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/obliterate,if=blood.frac>=1.8|unholy.frac>=1.8|frost.frac>=1.8
			if (BloodFrac >= 1.8 || UnholyFrac >= 1.8 || FrostFrac >= 1.8) {
				if (Obliterate ())
					return true;
			}
			//	actions.single_target_2h+=/plague_leech,if=disease.min_remains<3&((blood.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&blood.frac<=0.95))
			if (DiseaseMinRemains < 3 && ((BloodFrac <= 0.95 && UnholyFrac <= 0.95) || (FrostFrac <= 0.95 && UnholyFrac <= 0.95) || (FrostFrac <= 0.95 && BloodFrac <= 0.95))) {
				if (PlagueLeech ())
					return true;
			}
			//	actions.single_target_2h+=/frost_strike,if=talent.runic_empowerment.enabled&(frost=0|unholy=0|blood=0)&(!buff.killing_machine.react|!obliterate.ready_in<=1)
			if (HasSpell ("Runic Empowerment") && (Frost == 0 || Unholy == 0 || Blood == 0) && (!Me.HasAura ("Killing Machine") || Cooldown ("Obliterate") > 1)) {
				if (FrostStrike ())
					return true;
			}
			//	actions.single_target_2h+=/frost_strike,if=talent.blood_tap.enabled&buff.blood_charge.stack<=10&(!buff.killing_machine.react|!obliterate.ready_in<=1)
			if (HasSpell ("Blood Tap") && BloodCharge <= 10 && (!Me.HasAura ("Killing Machine") || Cooldown ("Obliterate") > 1)) {
				if (FrostStrike ())
					return true;
			}
			//	actions.single_target_2h+=/howling_blast,if=buff.rime.react&disease.min_remains>5
			if (Me.HasAura ("Rime") && DiseaseMinRemains > 5) {
				if (HowlingBlast ())
					return true;
			}
			//	actions.single_target_2h+=/obliterate,if=blood.frac>=1.5|unholy.frac>=1.6|frost.frac>=1.6|buff.bloodlust.up|cooldown.plague_leech.remains<=4
			if (BloodFrac >= 1.5 || UnholyFrac >= 1.6 || FrostFrac >= 1.6 || Me.HasAura ("Bloodlust") || (HasSpell ("Plague Leech") && Cooldown ("Plague Leech") <= 4)) {
				if (Obliterate ())
					return true;
			}
			//	actions.single_target_2h+=/blood_tap,if=(buff.blood_charge.stack>10&runic_power>=20)|(blood.frac>=1.4|unholy.frac>=1.6|frost.frac>=1.6)
			if ((BloodCharge > 10 && RunicPower >= 20) || (BloodFrac >= 1.4 || UnholyFrac >= 1.6 || FrostFrac >= 1.6)) {
				if (BloodTap ())
					return true;
			}
			//	actions.single_target_2h+=/frost_strike,if=!buff.killing_machine.react
			if (!Me.HasAura ("Killing Machine")) {
				if (FrostStrike ())
					return true;
			}
			//	actions.single_target_2h+=/plague_leech,if=(blood.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&blood.frac<=0.95)
			if ((BloodFrac <= 0.95 && UnholyFrac <= 0.95) || (FrostFrac <= 0.95 && UnholyFrac <= 0.95) || (FrostFrac <= 0.95 && BloodFrac <= 0.95)) {
				if (PlagueLeech ())
					return true;
			}
			//	actions.single_target_2h+=/empower_rune_weapon
			if (EmpowerRuneWeapon ())
				return true;

			return false;
		}

		bool Single_target_1h ()
		{
			//	actions.single_target_1h=breath_of_sindragosa,if=runic_power>75
			//	actions.single_target_1h+=/run_action_list,name=single_target_bos,if=dot.breath_of_sindragosa.ticking
			//	actions.single_target_1h+=/frost_strike,if=buff.killing_machine.react
			//	actions.single_target_1h+=/obliterate,if=unholy>1|buff.killing_machine.react
			//	actions.single_target_1h+=/defile
			//	actions.single_target_1h+=/blood_tap,if=talent.defile.enabled&cooldown.defile.remains=0
			//	actions.single_target_1h+=/frost_strike,if=runic_power>88
			//	actions.single_target_1h+=/howling_blast,if=buff.rime.react|death>1|frost>1
			//	actions.single_target_1h+=/blood_tap,if=buff.blood_charge.stack>10
			//	actions.single_target_1h+=/frost_strike,if=runic_power>76
			//	actions.single_target_1h+=/unholy_blight,if=!disease.ticking
			//	actions.single_target_1h+=/outbreak,if=!dot.blood_plague.ticking
			//	actions.single_target_1h+=/plague_strike,if=!talent.necrotic_plague.enabled&!dot.blood_plague.ticking
			//	actions.single_target_1h+=/howling_blast,if=!(target.health.pct-3*(target.health.pct%target.time_to_die)<=35&cooldown.soul_reaper.remains<3)|death+frost>=2
			//	actions.single_target_1h+=/outbreak,if=talent.necrotic_plague.enabled&debuff.necrotic_plague.stack<=14
			//	actions.single_target_1h+=/blood_tap
			//	actions.single_target_1h+=/plague_leech
			//	actions.single_target_1h+=/empower_rune_weapon

			return false;
		}


		bool Multi_target ()
		{
			//	actions.multi_target=unholy_blight
			//	actions.multi_target+=/frost_strike,if=buff.killing_machine.react&main_hand.1h
			//	actions.multi_target+=/obliterate,if=unholy>1
			//	actions.multi_target+=/blood_boil,if=dot.blood_plague.ticking&(!talent.unholy_blight.enabled|cooldown.unholy_blight.remains<49),line_cd=28
			//	actions.multi_target+=/defile
			//	actions.multi_target+=/breath_of_sindragosa,if=runic_power>75
			//	actions.multi_target+=/run_action_list,name=multi_target_bos,if=dot.breath_of_sindragosa.ticking
			//	actions.multi_target+=/howling_blast
			//	actions.multi_target+=/blood_tap,if=buff.blood_charge.stack>10
			//	actions.multi_target+=/frost_strike,if=runic_power>88
			//	actions.multi_target+=/death_and_decay,if=unholy=1
			//	actions.multi_target+=/plague_strike,if=unholy=2&!dot.blood_plague.ticking&!talent.necrotic_plague.enabled
			//	actions.multi_target+=/blood_tap
			//	actions.multi_target+=/frost_strike,if=!talent.breath_of_sindragosa.enabled|cooldown.breath_of_sindragosa.remains>=10
			//	actions.multi_target+=/plague_leech
			//	actions.multi_target+=/plague_strike,if=unholy=1
			//	actions.multi_target+=/empower_rune_weapon

			return false;
		}


		bool Single_target_bos ()
		{
			//	actions.single_target_bos=obliterate,if=buff.killing_machine.react
			//	actions.single_target_bos+=/blood_tap,if=buff.killing_machine.react&buff.blood_charge.stack>=5
			//	actions.single_target_bos+=/plague_leech,if=buff.killing_machine.react
			//	actions.single_target_bos+=/blood_tap,if=buff.blood_charge.stack>=5
			//	actions.single_target_bos+=/plague_leech
			//	actions.single_target_bos+=/obliterate,if=runic_power<76
			//	actions.single_target_bos+=/howling_blast,if=((death=1&frost=0&unholy=0)|death=0&frost=1&unholy=0)&runic_power<88
			if (((Death == 1 && Frost == 0 && Unholy == 0) || Death == 0 && Frost == 1 && Unholy == 0) && RunicPower < 88) {
				if (HowlingBlast ())
					return true;
			}

			return false;
		}

		bool Multi_target_bos ()
		{
			//	actions.multi_target_bos=howling_blast
			if (HowlingBlast ())
				return true;
			//	actions.multi_target_bos+=/blood_tap,if=buff.blood_charge.stack>10
			if (BloodCharge > 10) {
				if (BloodTap ())
					return true;
			}
			//	actions.multi_target_bos+=/death_and_decay,if=unholy=1
			if (Unholy == 1) {
				if (DeathandDecay ())
					return true;
			}
			//	actions.multi_target_bos+=/plague_strike,if=unholy=2
			if (Unholy == 2) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.multi_target_bos+=/blood_tap
			if (BloodTap ())
				return true;
			//	actions.multi_target_bos+=/plague_leech
			if (PlagueLeech ())
				return true;
			//	actions.multi_target_bos+=/plague_strike,if=unholy=1
			if (Unholy == 1) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.multi_target_bos+=/empower_rune_weapon
			if (EmpowerRuneWeapon ())
				return true;

			return false;
		}
	}
}
	
