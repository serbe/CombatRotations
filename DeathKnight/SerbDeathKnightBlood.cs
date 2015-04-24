using ReBot.API;

namespace ReBot.DeathKnight
{
	[Rotation ("Serb Blood Death Knight SC", "Serb", WoWClass.DeathKnight, Specialization.DeathknightBlood, 5, 25)]
	public class SerbDeathKnightBloodSc : SerbDeathKnight
	{
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
			if (Interrupt ())
				return;

			if (Cast ("Remorseless Winter", () => HasSpell ("Remorseless Winter") && (EnemyInRange (8) >= 2 || (IsPlayer () && Target.CombatRange < 8))))
				return;

			if (Gcd && HasGlobalCooldown ())
				return;

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
			if (!Me.HasAura ("Army of the Dead") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Dancing Rune Weapon") &&
			    !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Rune Tap")) {
				if (BoneShield ())
					return;
			}
			//	actions+=/lichborne,if=health.pct<30
			if (Health (Me) < 0.3)
				Lichborne ();
			//	actions+=/vampiric_blood,if=health.pct<40
			if (Health (Me) < 0.4)
				VampiricBlood ();
			//	actions+=/icebound_fortitude,if=health.pct<30&buff.army_of_the_dead.down&buff.dancing_rune_weapon.down&buff.bone_shield.down&buff.rune_tap.down
			if (Health (Me) < 0.3 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") &&
			    !Me.HasAura ("Bone Shield") && !Me.HasAura ("Rune Tap"))
				IceboundFortitude ();
			//	actions+=/death_pact,if=health.pct<30
			if (Health (Me) < 0.3)
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
				Cdbos ();
			}
		}

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
			if (BloodCharge >= 9 && RunicPower > 85 &&
			    (Me.AuraTimeRemaining ("Convulsive Shadows") > 5 ||
			    Me.AuraTimeRemaining ("Convulsive Shadows") > 2 && Me.HasAura ("Bloodlust")))
				BloodTap ();
			//	actions.bos+=/death_coil,if=runic_power>85&(buff.convulsive_shadows.remains>5|buff.convulsive_shadows.remains>2&buff.bloodlust.up)
			if (RunicPower > 85 &&
			    (Me.AuraTimeRemaining ("Convulsive Shadows") > 5 ||
			    Me.AuraTimeRemaining ("Convulsive Shadows") > 2 && Me.HasAura ("Bloodlust"))) {
				if (DeathCoil ())
					return true;
			}
			//	actions.bos+=/outbreak,if=(!dot.blood_plague.ticking|!dot.frost_fever.ticking)&runic_power>21
			if ((!Target.HasAura ("Blood Plague", true) || !Target.HasAura ("Frost Fever", true)) && RunicPower > 21) {
				if (Outbreak ())
					return true;
			}
			//	actions.bos+=/chains_of_ice,if=!dot.frost_fever.ticking&glyph.icy_runes.enabled&runic_power<90
			if (!Target.HasAura ("Frost Fever", true) && HasGlyph (110802) && RunicPower < 90) {
				if (ChainsofIce ())
					return true;
			}
			//	actions.bos+=/plague_strike,if=!dot.blood_plague.ticking&runic_power>5
			if (!Target.HasAura ("Blood Plague", true) && RunicPower > 5) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.bos+=/icy_touch,if=!dot.frost_fever.ticking&runic_power>5
			if (!Target.HasAura ("Frost Fever", true) && RunicPower > 5) {
				if (IcyTouch ())
					return true;
			}
			//	actions.bos+=/death_strike,if=runic_power<16
			if (RunicPower < 16) {
				if (DeathStrike ())
					return true;
			}
			//	actions.bos+=/blood_tap,if=runic_power<16
			if (RunicPower < 16)
				BloodTap ();
			//	actions.bos+=/blood_boil,if=runic_power<16&runic_power>5&buff.crimson_scourge.down&(blood>=1&blood.death=0|blood=2&blood.death<2)
			if (RunicPower < 16 && RunicPower > 5 && !Me.HasAura ("Crimson Scourge") &&
			    (Blood >= 1 && Death == 0 || Blood == 2 && Death < 2)) {
				if (BloodBoil ())
					return true;
			}
			//	actions.bos+=/arcane_torrent,if=runic_power<16
			if (RunicPower < 16)
				ArcaneTorrent ();
			//	actions.bos+=/chains_of_ice,if=runic_power<16&glyph.icy_runes.enabled
			if (RunicPower < 16 && HasGlyph (110802)) {
				if (ChainsofIce ())
					return true;
			}
			//	actions.bos+=/blood_boil,if=runic_power<16&buff.crimson_scourge.down&(blood>=1&blood.death=0|blood=2&blood.death<2)
			if (RunicPower < 16 && !Me.HasAura ("Crimson Scourge") &&
			    (Blood >= 1 && Death == 0 || Blood == 2 && Death < 2)) {
				if (BloodBoil ())
					return true;
			}
			//	actions.bos+=/icy_touch,if=runic_power<16
			if (RunicPower < 16) {
				if (IcyTouch ())
					return true;
			}
			//	actions.bos+=/plague_strike,if=runic_power<16
			if (RunicPower < 16) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.bos+=/rune_tap,if=runic_power<16&blood>=1&blood.death=0&frost=0&unholy=0&buff.crimson_scourge.up
			if (RunicPower < 16 && Blood >= 1 && !HasDeath && !HasFrost && !HasUnholy && Me.HasAura ("Crimson Scourge"))
				RuneTap ();
			//	actions.bos+=/empower_rune_weapon,if=runic_power<16&blood=0&frost=0&unholy=0
			if (RunicPower < 16 && !HasBlood && !HasFrost && !HasUnholy) {
				if (EmpowerRuneWeapon ())
					return true;
			}
			//	actions.bos+=/death_strike,if=(blood.frac>1.8&blood.death>=1|frost.frac>1.8|unholy.frac>1.8|buff.blood_charge.stack>=11)
			if ((BloodFrac > 1.8 && Death >= 1 || FrostFrac > 1.8 || UnholyFrac > 1.8 || BloodCharge >= 11)) {
				if (DeathStrike ())
					return true;
			}
			//	actions.bos+=/blood_tap,if=(blood.frac>1.8&blood.death>=1|frost.frac>1.8|unholy.frac>1.8)
			if ((BloodFrac > 1.8 && Death >= 1 || FrostFrac > 1.8 || UnholyFrac > 1.8))
				BloodTap ();
			//	actions.bos+=/blood_boil,if=(blood>=1&blood.death=0&target.health.pct-3*(target.health.pct%target.time_to_die)>35|blood=2&blood.death<2)&buff.crimson_scourge.down
			if ((Blood >= 1 && Death == 0 &&
			    Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) < 35 ||
			    Blood == 2 && Death < 2) && !Me.HasAura ("Crimson Scourge")) {
				if (BloodBoil ())
					return true;
			}
			//	actions.bos+=/antimagic_shell,if=runic_power<65
			if (RunicPower < 65) {
				if (AntimagicShell ())
					return true;
			}
			//	actions.bos+=/plague_leech,if=runic_power<65
			if (RunicPower < 65) {
				if (PlagueLeech ())
					return true;
			}
			//	actions.bos+=/outbreak,if=!dot.blood_plague.ticking
			if (!Target.HasAura ("Blood Plague", true)) {
				if (Outbreak ())
					return true;
			}
			//	actions.bos+=/outbreak,if=pet.dancing_rune_weapon.active&!pet.dancing_rune_weapon.dot.blood_plague.ticking
			// +++++++++++++++++++++++++++++
			//	actions.bos+=/death_and_decay,if=buff.crimson_scourge.up
			if (Me.HasAura ("Crimson Scourge")) {
				if (DeathandDecay ())
					return true;
			}
			//	actions.bos+=/blood_boil,if=buff.crimson_scourge.up
			if (Me.HasAura ("Crimson Scourge")) {
				if (BloodBoil ())
					return true;
			}

			return false;
		}

		public bool Cdbos ()
		{
			//	actions.cdbos=soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35
			if (Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) < 35) {
				if (SoulReaper ())
					return true;
			}
			//	actions.cdbos+=/blood_tap,if=buff.blood_charge.stack>=10
			if (BloodCharge >= 10)
				BloodTap ();
			//	actions.cdbos+=/death_coil,if=runic_power>65
			if (RunicPower > 65) {
				if (DeathCoil ())
					return true;
			}
			//	actions.cdbos+=/plague_strike,if=!dot.blood_plague.ticking&unholy=2
			if (!Target.HasAura ("Blood Plague", true) && Frost == 2) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.cdbos+=/icy_touch,if=!dot.frost_fever.ticking&frost=2
			if (!Target.HasAura ("Frost Fever", true) && Frost == 2) {
				if (IcyTouch ())
					return true;
			}
			//	actions.cdbos+=/death_strike,if=unholy=2|frost=2|blood=2&blood.death>=1
			if (Unholy == 2 || Frost == 2 | Blood == 2 && Death >= 1) {
				if (DeathStrike ())
					return true;
			}
			//	actions.cdbos+=/blood_boil,if=blood=2&blood.death<2
			if (Blood == 2 && Death < 2) {
				if (BloodBoil ())
					return true;
			}
			//	actions.cdbos+=/outbreak,if=!dot.blood_plague.ticking
			if (!Target.HasAura ("Blood Plague", true)) {
				if (Outbreak ())
					return true;
			}
			//	actions.cdbos+=/plague_strike,if=!dot.blood_plague.ticking
			if (!Target.HasAura ("Blood Plague", true)) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.cdbos+=/icy_touch,if=!dot.frost_fever.ticking
			if (!Target.HasAura ("Frost Fever", true)) {
				if (IcyTouch ())
					return true;
			}
			//	actions.cdbos+=/outbreak,if=pet.dancing_rune_weapon.active&!pet.dancing_rune_weapon.dot.blood_plague.ticking
			// ++++++++++++++++++++++
			//	actions.cdbos+=/blood_boil,if=((dot.frost_fever.remains<4&dot.frost_fever.ticking)|(dot.blood_plague.remains<4&dot.blood_plague.ticking))
			if ((Target.AuraTimeRemaining ("Frost Fever", true) < 4 && Target.HasAura ("Frost Fever", true)) ||
			    (Target.AuraTimeRemaining ("Blood Plague", true) < 4 && Target.HasAura ("Blood Plague", true))) {
				if (BloodBoil ())
					return true;
			}
			//	actions.cdbos+=/death_and_decay,if=buff.crimson_scourge.up
			if (Me.HasAura ("Crimson Scourge")) {
				if (DeathandDecay ())
					return true;
			}
			//	actions.cdbos+=/blood_boil,if=buff.crimson_scourge.up
			if (Me.HasAura ("Crimson Scourge")) {
				if (BloodBoil ())
					return true;
			}
			//	actions.cdbos+=/death_coil,if=runic_power>45
			if (RunicPower > 45) {
				if (DeathCoil ())
					return true;
			}
			//	actions.cdbos+=/blood_tap
			BloodTap ();
			//	actions.cdbos+=/death_strike
			if (DeathStrike ())
				return true;
			//	actions.cdbos+=/blood_boil,if=blood>=1&blood.death=0
			if (Blood >= 1 && Death == 0) {
				if (BloodBoil ())
					return true;
			}
			//	actions.cdbos+=/death_coil
			if (DeathCoil ())
				return true;

			return false;
		}

		public bool Last ()
		{
			//	actions.last=antimagic_shell,if=runic_power<90
			if (RunicPower < 90) {
				if (AntimagicShell ())
					return true;
			}
			//	actions.last+=/blood_tap
			BloodTap ();
			//	actions.last+=/soul_reaper,if=target.time_to_die>7
			if (TimeToDie (Target) > 7) {
				if (SoulReaper ())
					return true;
			}
			//	actions.last+=/death_coil,if=runic_power>80
			if (RunicPower > 80) {
				if (DeathCoil ())
					return true;
			}
			//	actions.last+=/death_strike
			if (DeathStrike ())
				return true;
			//	actions.last+=/blood_boil,if=blood=2|target.time_to_die<=7
			if (Blood == 2 || TimeToDie (Target) <= 7) {
				if (BloodBoil ())
					return true;
			}
			//	actions.last+=/death_coil,if=runic_power>75|target.time_to_die<4|!dot.breath_of_sindragosa.ticking
			if (RunicPower > 75 || TimeToDie (Target) < 4 || !Me.HasAura ("Breath of Sindragosa")) {
				if (DeathCoil ())
					return true;
			}
			//	actions.last+=/plague_strike,if=target.time_to_die<2|cooldown.empower_rune_weapon.remains<2
			if (TimeToDie (Target) < 2 || Cooldown ("Empower Rune Weapon") < 2) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.last+=/icy_touch,if=target.time_to_die<2|cooldown.empower_rune_weapon.remains<2
			if (TimeToDie (Target) < 2 || Cooldown ("Empower Rune Weapon") < 2) {
				if (IcyTouch ())
					return true;
			}
			//	actions.last+=/empower_rune_weapon,if=!blood&!unholy&!frost&runic_power<76|target.time_to_die<5
			if (!HasBlood && !HasUnholy && !HasFrost && RunicPower < 76 || TimeToDie (Target) < 5) {
				if (EmpowerRuneWeapon ())
					return true;
			}
			//	actions.last+=/plague_leech
			if (PlagueLeech ())
				return true;

			return false;
		}

		public bool Nbos ()
		{
			//	actions.nbos=breath_of_sindragosa,if=runic_power>=80
			if (RunicPower >= 80) {
				if (BreathofSindragosa ())
					return true;
			}
			//	actions.nbos+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35
			if (Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) < 35) {
				if (SoulReaper ())
					return true;
			}
			//	actions.nbos+=/chains_of_ice,if=!dot.frost_fever.ticking&glyph.icy_runes.enabled
			if (!Target.HasAura ("Frost Fever", true) && HasGlyph (110802)) {
				if (ChainsofIce ())
					return true;
			}
			//	actions.nbos+=/icy_touch,if=!dot.frost_fever.ticking
			if (!Target.HasAura ("Frost Fever", true)) {
				if (IcyTouch ())
					return true;
			}
			//	actions.nbos+=/plague_strike,if=!dot.blood_plague.ticking
			if (!Target.HasAura ("Blood Plague", true)) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.nbos+=/death_strike,if=(blood.frac>1.8&blood.death>=1|frost.frac>1.8|unholy.frac>1.8)&runic_power<80
			if ((BloodFrac > 1.8 && Death >= 1 || FrostFrac > 1.8 || UnholyFrac > 1.8) && RunicPower < 80) {
				if (DeathStrike ())
					return true;
			}
			//	actions.nbos+=/death_and_decay,if=buff.crimson_scourge.up
			if (Me.HasAura ("Crimson Scourge")) {
				if (DeathandDecay ())
					return true;
			}
			//	actions.nbos+=/blood_boil,if=buff.crimson_scourge.up|(blood=2&runic_power<80&blood.death<2)
			if (Me.HasAura ("Crimson Scourge") || (Blood == 2 && RunicPower < 80 && Death < 2)) {
				if (BloodBoil ())
					return true;
			}

			return false;
		}
	}
}