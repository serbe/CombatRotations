using System;
using System.Collections.Generic;
using System.Linq;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Unholy DeathKnight SC", "Serb", WoWClass.DeathKnight, Specialization.DeathknightUnholy, 5, 25)]

	public class SerbDeathKnightUnholySC : DeathKnight
	{
		public SerbDeathKnightUnholySC ()
		{
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/horn_of_winter
			if (HornofWinter ())
				return true;
			//	actions.precombat+=/unholy_presence
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/army_of_the_dead
			//	actions.precombat+=/potion,name=draenic_strength
			//	actions.precombat+=/raise_dead
			if (RaiseDead ())
				return true;

			return false;
		}

		public override void Combat ()
		{
			if (Health < 0.9) {
				if (Heal ())
					return;}

			if (CastOnTerrain ("Desecrated Ground", Target.Position, () => HasSpell ("Desecrated Ground") && Me.MovementSpeed < 1))
				return;

			if (!Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Vampiric Blood")) {
				if (ArmyoftheDead ())
					return;
			}
			if (Cast ("Rune Tap", () => HasSpell ("Rune Tap") && Me.HasRune (RuneType.Blood) && Health < 0.5 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood") && !Me.HasAura ("Icebound Fortitude")))
				return;

			var targets = Adds;
			targets.Add (Target);

			if (Interrupt ())
				return;

			if (HasSpell ("Anti-Magic Zone") && Cooldown ("Anti-Magic Zone") == 0 && !HasGlobalCooldown ()) {
				var AntiMagicZoneTarget = targets.Where (u => u.IsCasting && u.Target == Me && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CastOnTerrain ("Anti-Magic Zone", Me.Position, () => Health <= 0.5 && !Me.HasAura ("Anti-Magic Shell") && AntiMagicZoneTarget != null))
					return;
			}

			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (HasSpell ("Anti-Magic Shell") && Cooldown ("Anti-Magic Shell") == 0 && !HasGlobalCooldown ()) {
				var AntiMagicShellTarget = targets.Where (u => u.IsCasting && u.Target == (UnitObject)Me && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CastSelf ("Anti-Magic Shell", () => AntiMagicShellTarget != null && Health <= 0.8))
					return;
			}

			if (Cast ("Chains of Ice", () => HasSpell ("Chains of Ice") && IsPlayer && Target.IsFleeing && !Target.HasAura ("Chains of Ice") && Target.MovementSpeed >= 1))
				return;
			if (Cast ("Remorseless Winter", () => HasSpell ("Remorseless Winter") && (EnemyInRange (8) >= 2 || (IsPlayer && Target.CombatRange < 8))))
				return;

			// if (CastSelf("Dark Simulacrum", () => Target.IsPlayer && Target.IsCasting && Me.GetPower(WoWPowerType.RunicPower) >= 20)) return;
			// if (CastSelf("Dark Simulacrum", () => Target.HpGreaterThanOrElite(0.2) && Target.IsCasting && Me.GetPower(WoWPowerType.RunicPower) >= 20)) return;

			if (Me.HasAlivePet) {
				Me.PetAssist ();
			}


			//actions=auto_attack
			//actions+=/deaths_advance,if=movement.remains>2
			//actions+=/run_action_list,name=bos,if=talent.breath_of_sindragosa.enabled
			if (HasSpell ("Breath of Sindragosa")) {
				if (Bos ())
					return;
			}
			//actions+=/antimagic_shell,damage=100000
			if (Me.HealthFraction <= 0.75)
				AntimagicShell ();
			//actions+=/blood_fury
			BloodFury ();
			//actions+=/berserking
			Berserking ();
			//actions+=/arcane_torrent
			ArcaneTorrent ();
			//actions+=/use_item,slot=trinket2
			//actions+=/potion,name=draenic_strength,if=buff.dark_transformation.up&target.time_to_die<=60
			//actions+=/run_action_list,name=aoe,if=(!talent.necrotic_plague.enabled&active_enemies>=2)|active_enemies>=4
			if ((!HasSpell ("Necrotic Plague") && EnemyInRange (10) >= 2) || EnemyInRange (10) >= 4) {
				if (Aoe ())
					return;
			}
			//actions+=/run_action_list,name=single_target,if=(!talent.necrotic_plague.enabled&active_enemies<2)|active_enemies<4
			if ((!HasSpell ("Necrotic Plague") && EnemyInRange (10) < 2) || EnemyInRange (10) < 4) {
				if (Single_target ())
					return;
			}
		}

		public bool Bos ()
		{
			//actions.bos=antimagic_shell,damage=100000,if=(dot.breath_of_sindragosa.ticking&runic_power<25)|cooldown.breath_of_sindragosa.remains>40
			if (Me.HealthFraction <= 0.75 && ((Me.HasAura ("Breath of Sindragosa") && RunicPower < 25) || Cooldown ("Breath of Sindragosa") > 40))
				AntimagicShell ();
			//actions.bos+=/blood_fury,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa"))
				BloodFury ();
			//actions.bos+=/berserking
			Berserking ();
			//actions.bos+=/use_item,slot=trinket2,if=dot.breath_of_sindragosa.ticking
			//actions.bos+=/potion,name=draenic_strength,if=dot.breath_of_sindragosa.ticking
			//actions.bos+=/run_action_list,name=bos_st
			if (Bos_st ())
				return true;

			return false;
		}

		public bool Aoe ()
		{
			//actions.aoe=unholy_blight
			if (UnholyBlight ())
				return true;
			//actions.aoe+=/call_action_list,name=spread,if=!dot.blood_plague.ticking|!dot.frost_fever.ticking|(!dot.necrotic_plague.ticking&talent.necrotic_plague.enabled)
			if (!Target.HasAura ("Blood Plague", true) || !Target.HasAura ("Frost Fever", true) || (!Target.HasAura ("Necrotic Plague", true) && HasSpell ("Necrotic Plague"))) {
				if (Spread ())
					return true;
			}
			//actions.aoe+=/defile
			if (Defile ())
				return true;
			//actions.aoe+=/blood_boil,if=blood=2|(frost=2&death=2)
			if (Blood == 2 || (Frost == 2 && Death == 2)) {
				if (BloodBoil ())
					return true;
			}
			//actions.aoe+=/summon_gargoyle
			if (SummonGargoyle ())
				return true;
			//actions.aoe+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//actions.aoe+=/blood_tap,if=level<=90&buff.shadow_infusion.stack=5
			if (Me.Level <= 90 && Me.GetAura ("Shadow Infusion").StackCount == 5)
				BloodTap ();
			//actions.aoe+=/defile
			if (Defile ())
				return true;
			//actions.aoe+=/death_and_decay,if=unholy=1
			if (Unholy == 1) {
				if (DeathandDecay ())
					return true;
			}
			//actions.aoe+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=45
			if (Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) {
				if (SoulReaper ())
					return true;
			}
			//actions.aoe+=/scourge_strike,if=unholy=2
			if (Unholy == 2) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.aoe+=/blood_tap,if=buff.blood_charge.stack>10
			if (BloodCharge > 10)
				BloodTap ();
			//actions.aoe+=/death_coil,if=runic_power>90|buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
			if (RunicPower > 90 || Me.HasAura ("Sudden Doom") || (!Me.Pet.HasAura("Dark Transformation") && Unholy <= 1)) {
				if (DeathCoil ())
					return true;
			}
			//actions.aoe+=/blood_boil
			if (BloodBoil ())
				return true;
			//actions.aoe+=/icy_touch
			if (IcyTouch ())
				return true;
			//actions.aoe+=/scourge_strike,if=unholy=1
			if (Unholy == 1) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.aoe+=/death_coil
			if (DeathCoil ())
				return true;
			//actions.aoe+=/blood_tap
			BloodTap ();
			//actions.aoe+=/plague_leech
			if (PlagueLeech ())
				return true;
			//actions.aoe+=/empower_rune_weapon
			EmpowerRuneWeapon ();

			return false;
		}

		public bool Single_target ()
		{
			//actions.single_target=plague_leech,if=(cooldown.outbreak.remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
			if ((Cooldown ("Outbreak") < 1) && ((Blood < 1 && Frost < 1) || (Blood < 1 && Unholy < 1) || (Frost < 1 && Unholy < 1))) {
				if (PlagueLeech ())
					return true;
			}
			//actions.single_target+=/plague_leech,if=((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))&disease.min_remains<3
			if (((Blood < 1 && Frost < 1) || (Blood < 1 && Unholy < 1) || (Frost < 1 && Unholy < 1)) && MinDisease (Target) < 3) {
				if (PlagueLeech ())
					return true;
			}
			//actions.single_target+=/plague_leech,if=disease.min_remains<1
			if (MinDisease (Target) < 1) {
				if (PlagueLeech ())
					return true;
			}
			//actions.single_target+=/outbreak,if=!disease.min_ticking
			if (!HasDisease (Target)) {
				if (Outbreak ())
					return true;
			}
			//actions.single_target+=/unholy_blight,if=!talent.necrotic_plague.enabled&disease.min_remains<3
			if (!HasSpell ("Necrotic Plague") && MinDisease (Target) < 3) {
				if (UnholyBlight ())
					return true;
			}
			//actions.single_target+=/unholy_blight,if=talent.necrotic_plague.enabled&dot.necrotic_plague.remains<1
			if (HasSpell ("Necrotic Plague") && Target.AuraTimeRemaining ("Necrotic Plague") < 1) {
				if (UnholyBlight ())
					return true;
			}			
			//actions.single_target+=/death_coil,if=runic_power>90
			if (RunicPower > 90) {
				if (DeathCoil ())
					return true;
			}
			//actions.single_target+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
			if (Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) {
				if (SoulReaper ())
					return true;
			}
			//actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if ((Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) && Cooldown ("Soul Reaper") == 0)
				BloodTap ();
			//actions.single_target+=/death_and_decay,if=(!talent.unholy_blight.enabled|!talent.necrotic_plague.enabled)&unholy=2
			if ((!HasSpell ("Unholy Blight") || !HasSpell ("Necrotic Plague")) && Unholy == 2) {
				if (DeathandDecay ())
					return true;
			}
			//actions.single_target+=/defile,if=unholy=2
			if (Unholy == 2) {
				if (Defile ())
					return true;
			}
			//actions.single_target+=/plague_strike,if=!disease.min_ticking&unholy=2
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0 && Unholy == 2) {
				if (PlagueStrike ())
					return true;
			}
			//actions.single_target+=/scourge_strike,if=unholy=2
			if (Unholy == 2) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.single_target+=/death_coil,if=runic_power>80
			if (RunicPower > 80) {
				if (DeathCoil ())
					return true;
			}
			//actions.single_target+=/festering_strike,if=talent.necrotic_plague.enabled&talent.unholy_blight.enabled&dot.necrotic_plague.remains<cooldown.unholy_blight.remains%2
			if (HasSpell ("Necrotic Plague") && HasSpell ("Unholy Blight") && Target.AuraTimeRemaining ("Necrotic Plague", true) < Cooldown ("Unholy Blight") / 2) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/festering_strike,if=blood=2&frost=2&(((Frost-death)>0)|((Blood-death)>0))
			if (Blood == 2 && Frost == 2 && (((Frost - Death) > 0) || ((Blood - Death) > 0))) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/festering_strike,if=(blood=2|frost=2)&(((Frost-death)>0)&((Blood-death)>0))
			if ((Blood == 2 || Frost == 2) && (((Frost - Death) > 0) && ((Blood - Death) > 0))) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/defile,if=blood=2|frost=2
			if (Blood == 2 || Frost == 2) {
				if (Defile ())
					return true;
			}
			//actions.single_target+=/plague_strike,if=!disease.min_ticking&(blood=2|frost=2)
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0 && (Blood == 2 || Frost == 2)) {
				if (PlagueStrike ())
					return true;
			}
			//actions.single_target+=/scourge_strike,if=blood=2|frost=2
			if (Blood == 2 || Frost == 2) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.single_target+=/festering_strike,if=((Blood-death)>1)
			if ((Blood - Death) > 1) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/blood_boil,if=((Blood-death)>1)
			if ((Blood - Death) > 1) {
				if (BloodBoil ())
					return true;
			}
			//actions.single_target+=/festering_strike,if=((Frost-death)>1)
			if ((Frost - Death) > 1) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if ((Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) && Cooldown ("Soul Reaper") == 0)
				BloodTap ();
			//actions.single_target+=/summon_gargoyle
			if (SummonGargoyle ())
				return true;
			//actions.single_target+=/death_and_decay,if=(!talent.unholy_blight.enabled|!talent.necrotic_plague.enabled)
			if (!HasSpell ("Unholy Blight") || !HasSpell ("Necrotic Plague")) {
				if (DeathandDecay ())
					return true;
			}
			//actions.single_target+=/defile
			if (Defile ())
				return true;
			//actions.single_target+=/blood_tap,if=talent.defile.enabled&cooldown.defile.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (HasSpell ("Defile") && Cooldown ("Defile") == 0)
				BloodTap ();
			//actions.single_target+=/plague_strike,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0) {
				if (PlagueStrike ())
					return true;
			}
			//actions.single_target+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//actions.single_target+=/blood_tap,if=buff.blood_charge.stack>10&(buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1))
			if (BloodCharge > 10 && (Me.HasAura ("Sudden Doom") || (!Me.Pet.HasAura("Dark Transformation") && Unholy <= 1)))
				BloodTap ();
			//actions.single_target+=/death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
			if (Me.HasAura ("Sudden Doom") || (Me.Pet.HasAura("Dark Transformation") && Unholy <= 1)) {
				if (DeathCoil ())
					return true;
			}
			//actions.single_target+=/scourge_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(Unholy>=2)
			if (!((Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target))) <= 45) || (Unholy >= 2)) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.single_target+=/blood_tap
			BloodTap ();
			//actions.single_target+=/festering_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(((Frost-death)>0)&((Blood-death)>0))
			if (!((Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target))) <= 45) || (((Frost - Death) > 0) && ((Blood - Death) > 0))) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/death_coil
			if (DeathCoil ())
				return true;
			//actions.single_target+=/plague_leech
			if (PlagueLeech ())
				return true;
			//actions.single_target+=/scourge_strike,if=cooldown.empower_rune_weapon.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (Cooldown ("Empower Rune Weapon") == 0) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.single_target+=/festering_strike,if=cooldown.empower_rune_weapon.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (Cooldown ("Empower Rune Weapon") == 0) {
				if (FesteringStrike ())
					return true;
			}
			//actions.single_target+=/blood_boil,if=cooldown.empower_rune_weapon.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (Cooldown ("Empower Rune Weapon") == 0) {
				if (BloodBoil ())
					return true;
			}
			//actions.single_target+=/icy_touch,if=cooldown.empower_rune_weapon.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (Cooldown ("Empower Rune Weapon") == 0) {
				if (IcyTouch ())
					return true;
			}
			//actions.single_target+=/empower_rune_weapon,if=blood<1&unholy<1&frost<1
			if (Blood < 1 && Unholy < 1 && Frost < 1)
				EmpowerRuneWeapon ();

			return false;
		}

		public bool Bos_st ()
		{
			var targets = Adds;
			targets.Add (Target);

			//actions.bos_st=plague_leech,if=((cooldown.outbreak.remains<1)|disease.min_remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
			if (((Cooldown ("Outbreak") < 1) || MinDisease (Target) < 1) && ((Blood < 1 && Frost < 1) || (Blood < 1 && Unholy < 1) || (Frost < 1 && Unholy < 1))) {
				if (PlagueLeech ())
					return true;
			}
			//actions.bos_st+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
			if (Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) {
				if (SoulReaper ())
					return true;
			}
			//actions.bos_st+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if ((Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) && Cooldown ("Soul Reaper") == 0)
				BloodTap ();
			//actions.bos_st+=/breath_of_sindragosa,if=runic_power>75
			if (RunicPower > 75) {
				if (BreathofSindragosa ())
					return true;
			}
			//actions.bos_st+=/run_action_list,name=bos_active,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (Bos_active ())
					return true;
			}
			//actions.bos_st+=/summon_gargoyle
			if (SummonGargoyle ())
				return true;
			//actions.bos_st+=/unholy_blight,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			if (!(Target.HasAura ("Blood Plague") || Target.HasAura ("Frost Fever"))) {
				if (UnholyBlight ())
					return true;
			}
			//actions.bos_st+=/outbreak,cycle_targets=1,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			if (Usable ("Outbreak")) {
				CycleTarget = targets.Where (x => !(x.HasAura ("Blood Plague") || x.HasAura ("Frost Fever"))).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Outbreak (CycleTarget))
						return true;
				}
			}
			//actions.bos_st+=/plague_strike,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			if (!(Target.HasAura ("Blood Plague") || Target.HasAura ("Frost Fever"))) {
				if (PlagueStrike ())
					return true;
			}
			//actions.bos_st+=/blood_boil,cycle_targets=1,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			if (Usable ("Blood Boil")) {
				CycleTarget = targets.Where (x => !(x.HasAura ("Blood Plague") || x.HasAura ("Frost Fever"))).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (BloodBoil ())
						return true;
				}
			}
			//actions.bos_st+=/death_and_decay,if=active_enemies>1&unholy>1
			if (EnemyInRange (10) > 1 && Unholy > 1) {
				if (DeathandDecay ())
					return true;
			}
			//actions.bos_st+=/festering_strike,if=blood>1&frost>1
			if (Blood > 1 && Frost > 1) {
				if (FesteringStrike ())
					return true;
			}
			//actions.bos_st+=/scourge_strike,if=((unholy>1|death>1)&active_enemies<=3)|(unholy>1&active_enemies>=4)
			if (((Unholy > 1 || Death > 1) && EnemyInRange (10) <= 3) || (Unholy > 1 && EnemyInRange (10) >= 4)) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.bos_st+=/death_and_decay,if=active_enemies>1
			if (EnemyInRange (10) > 1) {
				if (DeathandDecay ())
					return true;
			}
			//actions.bos_st+=/blood_boil,if=active_enemies>=4&(blood=2|(frost=2&death=2))
			if (EnemyInRange (10) >= 4 && (Blood == 2 || (Frost == 2 && Death == 2))) {
				if (BloodBoil ())
					return true;
			}
			//actions.bos_st+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//actions.bos_st+=/blood_tap,if=buff.blood_charge.stack>10
			if (BloodCharge > 10)
				BloodTap ();
			//actions.bos_st+=/blood_boil,if=active_enemies>=4
			if (EnemyInRange (10) >= 4) {
				if (BloodBoil ())
					return true;
			}
			//actions.bos_st+=/death_coil,if=(buff.sudden_doom.react|runic_power>80)&(buff.blood_charge.stack<=10)
			if ((Me.HasAura ("Sudden Doom") || RunicPower > 80) && (BloodCharge <= 10)) {
				if (DeathCoil ())
					return true;
			}
			//actions.bos_st+=/scourge_strike,if=cooldown.breath_of_sindragosa.remains>6|runic_power<75
			if (Cooldown ("Breath of Sindragosa") > 6 || RunicPower < 75) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.bos_st+=/festering_strike,if=cooldown.breath_of_sindragosa.remains>6|runic_power<75
			if (Cooldown ("Breath of Sindragosa") > 6 || RunicPower < 75) {
				if (FesteringStrike ())
					return true;
			}
			//actions.bos_st+=/death_coil,if=cooldown.breath_of_sindragosa.remains>20
			if (Cooldown ("Breath of Sindragosa") > 20) {
				if (DeathCoil ())
					return true;
			}
			//actions.bos_st+=/plague_leech
			if (PlagueLeech ())
				return true;

			return false;
		}

		public bool Bos_active ()
		{
			var targets = Adds;
			targets.Add (Target);

			//actions.bos_active=plague_strike,if=!disease.ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0) {
				if (PlagueStrike ())
					return true;
			}
			//actions.bos_active+=/blood_boil,cycle_targets=1,if=(active_enemies>=2&!(dot.blood_plague.ticking|dot.frost_fever.ticking))|active_enemies>=4&(runic_power<88&runic_power>30)
			if (Usable ("Blood Boil")) {
				CycleTarget = targets.Where (x => !(EnemyInRange (10) >= 2 && x.HasAura ("Blood Plague") || x.HasAura ("Frost Fever")) || EnemyInRange (10) >= 4 && x.IsInLoS && (RunicPower < 88 && RunicPower > 30)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (BloodBoil ())
						return true;
				}
			}
			//actions.bos_active+=/scourge_strike,if=active_enemies<=3&(runic_power<88&runic_power>30)
			if (EnemyInRange (10) <= 3 && (RunicPower < 88 && RunicPower > 30)) {
				if (ScourgeStrike ())
					return true;
			}

			//actions.bos_active+=/festering_strike,if=runic_power<77
			if (RunicPower < 77) {
				if (FesteringStrike ())
					return true;
			}
			//actions.bos_active+=/blood_boil,if=active_enemies>=4
			if (EnemyInRange (10) >= 4) {
				if (BloodBoil ())
					return true;
			}
			//actions.bos_active+=/scourge_strike,if=active_enemies<=3
			if (EnemyInRange (10) <= 3) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.bos_active+=/blood_tap,if=buff.blood_charge.stack>=5
			if (BloodCharge >= 5)
				BloodTap ();
			//actions.bos_active+=/arcane_torrent,if=runic_power<70
			if (RunicPower < 70)
				ArcaneTorrent ();
			//actions.bos_active+=/plague_leech
			if (PlagueLeech ())
				return true;
			//actions.bos_active+=/empower_rune_weapon,if=runic_power<60
			if (RunicPower < 60)
				EmpowerRuneWeapon ();
			//actions.bos_active+=/death_coil,if=buff.sudden_doom.react
			if (Me.HasAura ("Sudden Doom")) {
				if (DeathCoil ())
					return true;
			}

			return false;
		}

		public bool Spread ()
		{
			//actions.spread=blood_boil,cycle_targets=1,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0) {
				if (BloodBoil ())
					return true;
			}
			//actions.spread+=/outbreak,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0) {
				if (Outbreak ())
					return true;
			}
			//actions.spread+=/plague_strike,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (MinDisease (Target) == 0) {
				if (PlagueStrike ())
					return true;
			}

			return false;
		}
	}
}