using System.Linq;
using ReBot.API;

namespace ReBot.DeathKnight
{
	[Rotation ("Serb Unholy DeathKnight SC", "Serb", WoWClass.DeathKnight, Specialization.DeathknightUnholy, 5, 25)]

	public class SerbDeathKnightUnholySc : SerbDeathKnight
	{
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
			if (Health (Me) < 0.9) {
				if (Heal ())
					return;
			}

			if (Gcd && HasGlobalCooldown ())
				return;

			if (CastOnTerrain ("Desecrated Ground", Target.Position, () => HasSpell ("Desecrated Ground") && Me.MovementSpeed < 1))
				return;

			if (!Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Vampiric Blood")) {
				if (ArmyoftheDead ())
					return;
			}
			if (Cast ("Rune Tap", () => HasSpell ("Rune Tap") && Me.HasRune (RuneType.Blood) && Health (Me) < 0.5 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood") && !Me.HasAura ("Icebound Fortitude")))
				return;

			var targets = Adds;
			targets.Add (Target);

			if (Interrupt ())
				return;

			if (HasSpell ("Anti-Magic Zone") && Cooldown ("Anti-Magic Zone") == 0 && !HasGlobalCooldown ()) {
				var antiMagicZoneTarget = targets.Where (u => u.IsCasting && u.Target == Me && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CastOnTerrain ("Anti-Magic Zone", Me.Position, () => Health (Me) <= 0.5 && !Me.HasAura ("Anti-Magic Shell") && antiMagicZoneTarget != null))
					return;
			}

			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (HasSpell ("Anti-Magic Shell") && Cooldown ("Anti-Magic Shell") == 0 && !HasGlobalCooldown ()) {
				var antiMagicShellTarget = targets.Where (u => u.IsCasting && u.Target == Me && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CastSelf ("Anti-Magic Shell", () => antiMagicShellTarget != null && Health (Me) <= 0.8))
					return;
			}

			if (Cast ("Chains of Ice", () => HasSpell ("Chains of Ice") && IsPlayer () && Target.IsFleeing && !Target.HasAura ("Chains of Ice") && Target.MovementSpeed >= 1))
				return;
			if (Cast ("Remorseless Winter", () => HasSpell ("Remorseless Winter") && (ActiveEnemies (8) >= 2 || (IsPlayer () && Range (8)))))
				return;

			// if (CastSelf("Dark Simulacrum", () => Target.IsPlayer && Target.IsCasting && Me.GetPower(WoWPowerType.RunicPower) >= 20)) return;
			// if (CastSelf("Dark Simulacrum", () => Target.HpGreaterThanOrElite(0.2) && Target.IsCasting && Me.GetPower(WoWPowerType.RunicPower) >= 20)) return;

			if (Me.HasAlivePet) {
				Me.PetAssist ();
			}


			//	actions=auto_attack
			//	actions+=/deaths_advance,if=movement.remains>2
			//	actions+=/antimagic_shell,damage=100000,if=((dot.breath_of_sindragosa.ticking&runic_power<25)|cooldown.breath_of_sindragosa.remains>40)|!talent.breath_of_sindragosa.enabled
			//	actions+=/blood_fury,if=!talent.breath_of_sindragosa.enabled
			if (!HasSpell ("Breath of Sindragosa"))
				BloodFury ();
			//	actions+=/berserking,if=!talent.breath_of_sindragosa.enabled
			if (!HasSpell ("Breath of Sindragosa"))
				Berserking ();
			//	actions+=/arcane_torrent,if=!talent.breath_of_sindragosa.enabled
			if (!HasSpell ("Breath of Sindragosa"))
				ArcaneTorrent ();
			//	actions+=/use_item,slot=trinket2,if=!talent.breath_of_sindragosa.enabled
			//	actions+=/potion,name=draenic_strength,if=(buff.convulsive_shadows.up&target.health.pct<45)&!talent.breath_of_sindragosa.enabled
			//	actions+=/potion,name=draenic_strength,if=(buff.dark_transformation.up&target.time_to_die<=60)&!talent.breath_of_sindragosa.enabled
			//	actions+=/run_action_list,name=unholy
			ActionUnholy ();

		}

		bool ActionUnholy ()
		{
			//	actions.unholy=plague_leech,if=((cooldown.outbreak.remains<1)|disease.min_remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
			if (((Cooldown ("Outbreak") < 1) || DiseaseMinRemains () < 1) && ((Blood < 1 && Frost < 1) || (Blood < 1 && Unholy < 1) || (Frost < 1 && Unholy < 1))) {
				if (PlagueLeech ())
					return true;
			}
			//	actions.unholy+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
			if (Health () * 100 - 3 * (Health () * 100 / TimeToDie (Target)) <= 45) {
				if (SoulReaper ())
					return true;
			}
			//	actions.unholy+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			if (((Health () * 100 - 3 * (Health () * 100 / TimeToDie (Target))) <= 45) && Cooldown ("Soul Reaper") == 0)
				BloodTap ();
			//	actions.unholy+=/summon_gargoyle
			if (SummonGargoyle ())
				return true;
			//	actions.unholy+=/breath_of_sindragosa,if=runic_power>75
			if (RunicPower > 75) {
				if (BreathofSindragosa ())
					return true;
			}
			//	actions.unholy+=/run_action_list,name=bos,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (ActionBos ())
					return true;
			}
			//	actions.unholy+=/unholy_blight,if=!disease.min_ticking
			if (Disease () == 0) {
				if (UnholyBlight ())
					return true;
			}
			//	actions.unholy+=/outbreak,cycle_targets=1,if=(active_enemies>=1&!talent.necrotic_plague.enabled)&(!(dot.blood_plague.ticking|dot.frost_fever.ticking))
			if (ActiveEnemies (30) >= 1 && !HasSpell ("Necrotic Plague")) {
				var Unit = Enemy.Where (u => Range (30, u) && !(HasBloodDisease (u) || HasFrostDisease (u))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Outbreak (Unit))
					return true;
			}
			//	actions.unholy+=/plague_strike,if=(!talent.necrotic_plague.enabled&!(dot.blood_plague.ticking|dot.frost_fever.ticking))|(talent.necrotic_plague.enabled&!dot.necrotic_plague.ticking)
			if ((!HasSpell ("Necrotic Plague") && !(HasBloodDisease () || HasFrostDisease ())) || (HasSpell ("Necrotic Plague") && !HasNecroticDisease ())) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.unholy+=/blood_boil,cycle_targets=1,if=(active_enemies>1&!talent.necrotic_plague.enabled)&(!(dot.blood_plague.ticking|dot.frost_fever.ticking))


			//	actions.unholy+=/death_and_decay,if=active_enemies>1&unholy>1
			if (ActiveEnemies (10) > 1 && Unholy > 1) {
				if (DeathandDecay ())
					return true;
			}
			//	actions.unholy+=/defile,if=unholy=2
			if (Unholy == 2) {
				if (Defile ())
					return true;
			}
			//	actions.unholy+=/blood_tap,if=talent.defile.enabled&cooldown.defile.remains=0
			if (HasSpell ("Defile") && Cooldown ("Defile") == 0)
				BloodTap ();
			//	actions.unholy+=/scourge_strike,if=unholy=2
			if (Unholy == 2) {
				if (ScourgeStrike ())
					return true;
			}
			//	actions.unholy+=/festering_strike,if=talent.necrotic_plague.enabled&talent.unholy_blight.enabled&dot.necrotic_plague.remains<cooldown.unholy_blight.remains%2
			if (HasSpell ("Necrotic Plague") && HasSpell ("Unholy Blight") && Target.AuraTimeRemaining ("Necrotic Plague", true) < Cooldown ("Unholy Blight") / 2) {
				if (FesteringStrike ())
					return true;
			}
			//	actions.unholy+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//	actions.unholy+=/festering_strike,if=blood=2&frost=2&(((Frost-death)>0)|((Blood-death)>0))
			if (Blood == 2 && Frost == 2 && (((Frost - Death) > 0) || ((Blood - Death) > 0))) {
				if (FesteringStrike ())
					return true;
			}
			//	actions.unholy+=/festering_strike,if=(blood=2|frost=2)&(((Frost-death)>0)&((Blood-death)>0))
			if ((Blood == 2 || Frost == 2) && (((Frost - Death) > 0) && ((Blood - Death) > 0))) {
				if (FesteringStrike ())
					return true;
			}
			//	actions.unholy+=/blood_boil,cycle_targets=1,if=(talent.necrotic_plague.enabled&!dot.necrotic_plague.ticking)&active_enemies>1


			//	actions.unholy+=/defile,if=blood=2|frost=2
			if (Blood == 2 || Frost == 2) {
				if (Defile ())
					return true;
			}
			//	actions.unholy+=/death_and_decay,if=active_enemies>1
			if (ActiveEnemies (10) > 1) {
				if (DeathandDecay ())
					return true;
			}
			//	actions.unholy+=/defile
			if (Defile ())
				return true;
			//	actions.unholy+=/blood_boil,if=talent.breath_of_sindragosa.enabled&((active_enemies>=4&(blood=2|(frost=2&death=2)))&(cooldown.breath_of_sindragosa.remains>6|runic_power<75))


			//	actions.unholy+=/blood_boil,if=!talent.breath_of_sindragosa.enabled&(active_enemies>=4&(blood=2|(frost=2&death=2)))


			//	actions.unholy+=/blood_tap,if=buff.blood_charge.stack>10
			if (GetAuraStack ("Blood Charge", Me) > 10)
				BloodTap ();
			//	actions.unholy+=/outbreak,if=talent.necrotic_plague.enabled&debuff.necrotic_plague.stack<=14


			//	actions.unholy+=/death_coil,if=(buff.sudden_doom.react|runic_power>80)&(buff.blood_charge.stack<=10)
			if ((Me.HasAura ("Sudden Doom") || RunicPower > 80) && (GetAuraStack ("Blood Charge", Me) <= 10)) {
				if (DeathCoil ())
					return true;
			}
			//	actions.unholy+=/blood_boil,if=(active_enemies>=4&(cooldown.breath_of_sindragosa.remains>6|runic_power<75))|(!talent.breath_of_sindragosa.enabled&active_enemies>=4)


			//	actions.unholy+=/scourge_strike,if=(cooldown.breath_of_sindragosa.remains>6|runic_power<75|unholy=2)|!talent.breath_of_sindragosa.enabled


			//	actions.unholy+=/festering_strike,if=(cooldown.breath_of_sindragosa.remains>6|runic_power<75)|!talent.breath_of_sindragosa.enabled


			//	actions.unholy+=/death_coil,if=(cooldown.breath_of_sindragosa.remains>20)|!talent.breath_of_sindragosa.enabled

			//	actions.unholy+=/plague_leech
			if (PlagueLeech ())
				return true;
			//	actions.unholy+=/empower_rune_weapon,if=!talent.breath_of_sindragosa.enabled

			return true;
		}

		bool ActionBos ()
		{
			//	actions.bos=blood_fury,if=dot.breath_of_sindragosa.ticking
			//	actions.bos+=/berserking,if=dot.breath_of_sindragosa.ticking
			//	actions.bos+=/use_item,slot=trinket2,if=dot.breath_of_sindragosa.ticking
			//	actions.bos+=/potion,name=draenic_strength,if=dot.breath_of_sindragosa.ticking
			//	actions.bos+=/unholy_blight,if=!disease.ticking
			//	actions.bos+=/plague_strike,if=!disease.ticking
			if (Disease () == 0) {
				if (PlagueStrike ())
					return true;
			}
			//	actions.bos+=/blood_boil,cycle_targets=1,if=(active_enemies>=2&!(dot.blood_plague.ticking|dot.frost_fever.ticking))|active_enemies>=4&(runic_power<88&runic_power>30)
			if (ActiveEnemies (10) >= 2) {
				var Unit = Enemy.Where (u => Range (10, u) && ((ActiveEnemies (10) >= 2 && !(HasBloodDisease (u) || HasFrostDisease (u))) || ActiveEnemies (10) >= 4 && (RunicPower < 88 && RunicPower > 30))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && BloodBoil ())
					return true;
			}
			//	actions.bos+=/death_and_decay,if=active_enemies>=2&(runic_power<88&runic_power>30)
			//	actions.bos+=/festering_strike,if=(blood=2&frost=2&(((Frost-death)>0)|((Blood-death)>0)))&runic_power<80
			//	actions.bos+=/festering_strike,if=((blood=2|frost=2)&(((Frost-death)>0)&((Blood-death)>0)))&runic_power<80
			//	actions.bos+=/arcane_torrent,if=runic_power<70
			if (RunicPower < 70)
				ArcaneTorrent ();
			//	actions.bos+=/scourge_strike,if=active_enemies<=3&(runic_power<88&runic_power>30)
			if (ActiveEnemies (10) <= 3 && (RunicPower < 88 && RunicPower > 30)) {
				if (ScourgeStrike ())
					return true;
			}
			//	actions.bos+=/blood_boil,if=active_enemies>=4&(runic_power<88&runic_power>30)
			//	actions.bos+=/festering_strike,if=runic_power<77
			if (RunicPower < 77) {
				if (FesteringStrike ())
					return true;
			}
			//	actions.bos+=/scourge_strike,if=(active_enemies>=4&(runic_power<88&runic_power>30))|active_enemies<=3
			//	actions.bos+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//	actions.bos+=/blood_tap,if=buff.blood_charge.stack>=5
			if (GetAuraStack ("Blood Charge", Me) >= 5)
				BloodTap ();
			//	actions.bos+=/plague_leech
			if (PlagueLeech ())
				return true;
			//	actions.bos+=/empower_rune_weapon,if=runic_power<60
			if (RunicPower < 60)
				EmpowerRuneWeapon ();
			//	actions.bos+=/death_coil,if=buff.sudden_doom.react
			if (Me.HasAura ("Sudden Doom")) {
				if (DeathCoil ())
					return true;
			}

			return true;
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
			if (BosSt ())
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

			//actions.aoe+=/blood_boil,if=blood=2|(frost=2&death=2)
			if (Blood == 2 || (Frost == 2 && Death == 2)) {
				if (BloodBoil ())
					return true;
			}


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
			if ((HasUnholy || HasDeath) && Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) {
				if (SoulReaper ())
					return true;
			}


			//actions.aoe+=/death_coil,if=runic_power>90|buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
			if (RunicPower > 90 || Me.HasAura ("Sudden Doom") || (!Me.Pet.HasAura ("Dark Transformation") && Unholy <= 1)) {
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

			//actions.aoe+=/empower_rune_weapon
			EmpowerRuneWeapon ();

			return false;
		}

		public bool SingleTarget ()
		{
			//actions.single_target=plague_leech,if=(cooldown.outbreak.remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
			if ((Cooldown ("Outbreak") < 1) && ((Blood < 1 && Frost < 1) || (Blood < 1 && Unholy < 1) || (Frost < 1 && Unholy < 1))) {
				if (PlagueLeech ())
					return true;
			}
			//actions.single_target+=/plague_leech,if=((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))&disease.min_remains<3
			if (((Blood < 1 && Frost < 1) || (Blood < 1 && Unholy < 1) || (Frost < 1 && Unholy < 1)) && DiseaseMinRemains (Target) < 3) {
				if (PlagueLeech ())
					return true;
			}
			//actions.single_target+=/plague_leech,if=disease.min_remains<1
			if (DiseaseMinRemains (Target) < 1) {
				if (PlagueLeech ())
					return true;
			}
			//actions.single_target+=/outbreak,if=!disease.min_ticking
			if (!HasDisease (Target)) {
				if (Outbreak ())
					return true;
			}
			//actions.single_target+=/unholy_blight,if=!talent.necrotic_plague.enabled&disease.min_remains<3
			if (!HasSpell ("Necrotic Plague") && DiseaseMinRemains (Target) < 3) {
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


			//actions.single_target+=/death_and_decay,if=(!talent.unholy_blight.enabled|!talent.necrotic_plague.enabled)&unholy=2
			if ((!HasSpell ("Unholy Blight") || !HasSpell ("Necrotic Plague")) && Unholy == 2) {
				if (DeathandDecay ())
					return true;
			}

			//actions.single_target+=/plague_strike,if=!disease.min_ticking&unholy=2
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (DiseaseMinRemains (Target) == 0 && Unholy == 2) {
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


			//actions.single_target+=/defile,if=blood=2|frost=2
			if (Blood == 2 || Frost == 2) {
				if (Defile ())
					return true;
			}
			//actions.single_target+=/plague_strike,if=!disease.min_ticking&(blood=2|frost=2)
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (DiseaseMinRemains (Target) == 0 && (Blood == 2 || Frost == 2)) {
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
			
			//actions.single_target+=/plague_strike,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (DiseaseMinRemains (Target) == 0) {
				if (PlagueStrike ())
					return true;
			}
			//actions.single_target+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//actions.single_target+=/blood_tap,if=buff.blood_charge.stack>10&(buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1))
			if (GetAuraStack ("Blood Charge", Me) > 10 && (Me.HasAura ("Sudden Doom") || (!Me.Pet.HasAura ("Dark Transformation") && Unholy <= 1)))
				BloodTap ();
			//actions.single_target+=/death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
			if (Me.HasAura ("Sudden Doom") || (Me.Pet.HasAura ("Dark Transformation") && Unholy <= 1)) {
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

		public bool BosSt ()
		{
			var targets = Adds;
			targets.Add (Target);

			//actions.bos_st+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
			if ((HasUnholy || HasDeath) && Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) {
				if (SoulReaper ())
					return true;
			}
			//actions.bos_st+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if ((Target.HealthFraction * 100 - 3 * (Target.HealthFraction * 100 / TimeToDie (Target)) <= 45) && Cooldown ("Soul Reaper") == 0)
				BloodTap ();
			//actions.bos_st+=/run_action_list,name=bos_active,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (BosActive ())
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
				var Unit = targets.Where (x => !(x.HasAura ("Blood Plague") || x.HasAura ("Frost Fever"))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (Outbreak (Unit))
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
				var Unit = targets.Where (x => !(x.HasAura ("Blood Plague") || x.HasAura ("Frost Fever"))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (BloodBoil ())
						return true;
				}
			}

			//actions.bos_st+=/festering_strike,if=blood>1&frost>1
			if (Blood > 1 && Frost > 1) {
				if (FesteringStrike ())
					return true;
			}
			//actions.bos_st+=/scourge_strike,if=((unholy>1|death>1)&active_enemies<=3)|(unholy>1&active_enemies>=4)
			if (((Unholy > 1 || Death > 1) && ActiveEnemies (10) <= 3) || (Unholy > 1 && ActiveEnemies (10) >= 4)) {
				if (ScourgeStrike ())
					return true;
			}

			//actions.bos_st+=/blood_boil,if=active_enemies>=4&(blood=2|(frost=2&death=2))
			if (ActiveEnemies (10) >= 4 && (Blood == 2 || (Frost == 2 && Death == 2))) {
				if (BloodBoil ())
					return true;
			}
			//actions.bos_st+=/dark_transformation
			if (DarkTransformation ())
				return true;
			//actions.bos_st+=/blood_tap,if=buff.blood_charge.stack>10
			if (GetAuraStack ("Blood Charge", Me) > 10)
				BloodTap ();
			//actions.bos_st+=/blood_boil,if=active_enemies>=4
			if (ActiveEnemies (10) >= 4) {
				if (BloodBoil ())
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

		public bool BosActive ()
		{


			//actions.bos_active+=/blood_boil,if=active_enemies>=4
			if (ActiveEnemies (10) >= 4) {
				if (BloodBoil ())
					return true;
			}
			//actions.bos_active+=/scourge_strike,if=active_enemies<=3
			if (ActiveEnemies (10) <= 3) {
				if (ScourgeStrike ())
					return true;
			}
			//actions.bos_active+=/plague_leech
			if (PlagueLeech ())
				return true;
			


			return false;
		}

		public bool Spread ()
		{
			//actions.spread=blood_boil,cycle_targets=1,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (DiseaseMinRemains (Target) == 0) {
				if (BloodBoil ())
					return true;
			}
			//actions.spread+=/outbreak,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (DiseaseMinRemains (Target) == 0) {
				if (Outbreak ())
					return true;
			}
			//actions.spread+=/plague_strike,if=!disease.min_ticking
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (DiseaseMinRemains (Target) == 0) {
				if (PlagueStrike ())
					return true;
			}

			return false;
		}
	}
}
