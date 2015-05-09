using ReBot.API;
using System;

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

			if (Cast ("Remorseless Winter", () => HasSpell ("Remorseless Winter") && (ActiveEnemies (8) >= 2 || (IsPlayer () && Range (8)))))
				return;

			if (Gcd && HasGlobalCooldown ())
				return;


			//	actions=auto_attack
			//	actions+=/potion,name=draenic_armor,if=buff.potion.down&buff.blood_shield.down&!unholy&!frost
			//	# if=time>10
			//	actions+=/blood_fury
			if (Time > 10)
				BloodFury ();
			//	# if=time>10
			//	actions+=/berserking
			if (Time > 10)
				Berserking ();
			//	# if=time>10
			//	actions+=/arcane_torrent
			if (Time > 10)
				ArcaneTorrent ();
			//	actions+=/antimagic_shell
			if (AntimagicShell ())
				return;
			//	actions+=/conversion,if=!buff.conversion.up&runic_power>50&health.pct<90
			if (!Me.HasAura ("Conversion") && RunicPower > 50 && Health (Me) < 0.9) {
				if (Conversion ())
					return;
			}
			//	actions+=/lichborne,if=health.pct<90
			if (Health (Me) < 0.9) {
				if (Lichborne ())
					return;
			}
			//	actions+=/death_strike,if=incoming_damage_5s>=health.max*0.65
			if (DamageTaken (5000) >= Me.MaxHealth * 0.65) {
				if (DeathStrike ())
					return;
			}
			//	actions+=/army_of_the_dead,if=buff.bone_shield.down&buff.dancing_rune_weapon.down&buff.icebound_fortitude.down&buff.vampiric_blood.down
			if (!Me.HasAura ("Bone Shield") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Vampiric Blood")) {
				if (ArmyoftheDead ())
					return;
			}
			//	actions+=/bone_shield,if=buff.army_of_the_dead.down&buff.bone_shield.down&buff.dancing_rune_weapon.down&buff.icebound_fortitude.down&buff.vampiric_blood.down
			if (!Me.HasAura ("Army of the Dead") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Vampiric Blood")) {
				if (BoneShield ())
					return;
			}
			//	actions+=/vampiric_blood,if=health.pct<50
			if (Health (Me) < 0.5) {
				if (VampiricBlood ())
					return;
			}
			//	actions+=/icebound_fortitude,if=health.pct<30&buff.army_of_the_dead.down&buff.dancing_rune_weapon.down&buff.bone_shield.down&buff.vampiric_blood.down
			if (Health (Me) < 0.3 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood")) {
				if (IceboundFortitude ())
					return;
			}
			//	actions+=/rune_tap,if=health.pct<50&buff.army_of_the_dead.down&buff.dancing_rune_weapon.down&buff.bone_shield.down&buff.vampiric_blood.down&buff.icebound_fortitude.down
			if (Health (Me) < 0.5 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood") && !Me.HasAura ("Icebound Fortitude")) {
				if (RuneTap ())
					return;
			}
			//	actions+=/dancing_rune_weapon,if=health.pct<80&buff.army_of_the_dead.down&buff.icebound_fortitude.down&buff.bone_shield.down&buff.vampiric_blood.down
			if (Health (Me) < 0.8 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood")) {
				if (DancingRuneWeapon ())
					return;
			}
			//	actions+=/death_pact,if=health.pct<50
			if (Health (Me) < 0.5) {
				if (DeathPact ())
					return;
			}
			//	actions+=/outbreak,if=(!talent.necrotic_plague.enabled&disease.min_remains<8)|!disease.ticking
			if ((!HasSpell ("Necrotic Plague") && MinDisease () < 8) || Disease () == 0) {
				if (Outbreak ())
					return;
			}
			//	actions+=/death_coil,if=runic_power>90
			if (RunicPower > 90) {
				if (DeathCoil ())
					return;
			}
			//	actions+=/plague_strike,if=(!talent.necrotic_plague.enabled&!dot.blood_plague.ticking)|(talent.necrotic_plague.enabled&!dot.necrotic_plague.ticking)
			if ((!HasSpell ("Necrotic Plague") && !HasBloodDisease ()) || (HasSpell ("Necrotic Plague") && !HasNecroticDisease ())) {
				if (PlagueStrike ())
					return;
			}
			//	actions+=/icy_touch,if=(!talent.necrotic_plague.enabled&!dot.frost_fever.ticking)|(talent.necrotic_plague.enabled&!dot.necrotic_plague.ticking)
			if ((!HasSpell ("Necrotic Plague") && !HasFrostDisease ()) || (HasSpell ("Necrotic Plague") && !HasNecroticDisease ())) {
				if (IcyTouch ())
					return;
			}
			//	actions+=/defile
			if (Defile ())
				return;
			//	actions+=/plague_leech,if=((!blood&!unholy)|(!blood&!frost)|(!unholy&!frost))&cooldown.outbreak.remains<=gcd
			if (((!HasBlood && !HasUnholy) || (!HasBlood && !HasFrost) || (!HasUnholy && !HasFrost)) && Cooldown ("Outbreak") <= 1) {
				if (PlagueLeech ())
					return;
			}
			//	actions+=/call_action_list,name=bt,if=talent.blood_tap.enabled
			if (HasSpell ("Blood Tap")) {
				if (ActionsBt ())
					return;
			}
			//	actions+=/call_action_list,name=re,if=talent.runic_empowerment.enabled
			if (HasSpell ("Runic Empowerment")) {
				if (ActionsRe ())
					return;
			}
			//	actions+=/call_action_list,name=rc,if=talent.runic_corruption.enabled
			if (HasSpell ("Runic Corruption")) {
				if (ActionsRc ())
					return;
			}
			//	actions+=/call_action_list,name=nrt,if=!talent.blood_tap.enabled&!talent.runic_empowerment.enabled&!talent.runic_corruption.enabled
			if (!HasSpell ("Blood Tap") && !HasSpell ("Runic Empowerment") && !HasSpell ("Runic Corruption")) {
				if (ActionsNrt ())
					return;
			}
			//	actions+=/defile,if=buff.crimson_scourge.react
			if (Me.HasAura ("Crimson Scourge")) {
				if (Defile ())
					return;
			}
			//	actions+=/death_and_decay,if=buff.crimson_scourge.react
			if (Me.HasAura ("Crimson Scourge")) {
				if (DeathandDecay ())
					return;
			}
			//	actions+=/blood_boil,if=buff.crimson_scourge.react
			if (Me.HasAura ("Crimson Scourge")) {
				if (BloodBoil ())
					return;
			}
			//	actions+=/death_coil
			if (DeathCoil ())
				return;
			//	actions+=/empower_rune_weapon,if=!blood&!unholy&!frost
			if (!HasBlood && !HasUnholy && !HasFrost) {
				if (EmpowerRuneWeapon ())
					return;
			}

		}

		bool ActionsBt ()
		{
			//	actions.bt=death_strike,if=unholy=2|frost=2
			if (Unholy == 2 || Frost == 2) {
				if (DeathStrike ())
					return true;
			}
			//	actions.bt+=/blood_tap,if=buff.blood_charge.stack>=5&!blood
			if (BloodCharge >= 5 && !HasBlood)
				BloodTap ();
			//	actions.bt+=/death_strike,if=buff.blood_charge.stack>=10&unholy&frost
			if (BloodCharge >= 10 && HasUnholy && HasFrost) {
				if (DeathStrike ())
					return true;
			}
			//	actions.bt+=/blood_tap,if=buff.blood_charge.stack>=10&!unholy&!frost
			if (BloodCharge >= 10 && !HasUnholy && !HasFrost)
				BloodTap ();
			//	actions.bt+=/blood_tap,if=buff.blood_charge.stack>=5&(!unholy|!frost)
			if (BloodCharge >= 5 && (!HasUnholy || !HasFrost))
				BloodTap ();
			//	actions.bt+=/blood_tap,if=buff.blood_charge.stack>=5&blood.death&!unholy&!frost
			if (BloodCharge >= 5 && HasDeath && !HasUnholy && !HasFrost)
				BloodTap ();
			//	actions.bt+=/death_coil,if=runic_power>70
			if (RunicPower > 70) {
				if (DeathCoil ())
					return true;
			}
			//	actions.bt+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35&(blood=2|(blood&!blood.death))
			if (Health () * 100 - 3 * (Health () * 100 / TimeToDie ()) <= 35 && (Blood == 2 || (HasBlood && !HasDeath))) {
				if (SoulReaper ())
					return true;
			}
			//	actions.bt+=/blood_boil,if=blood=2|(blood&!blood.death)
			if (Blood == 2 || (HasBlood && !HasDeath)) {
				if (BloodBoil ())
					return true;
			}


			return false;
		}

		bool ActionsRe ()
		{
			//	actions.re=death_strike,if=unholy&frost
			if (HasUnholy && HasFrost) {
				if (DeathStrike ())
					return true;
			}
			//	actions.re+=/death_coil,if=runic_power>70
			if (RunicPower > 70) {
				if (DeathCoil ())
					return true;
			}
			//	actions.re+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35&blood=2
			if (Health () * 100 - 3 * (Health () * 100 / TimeToDie ()) <= 35 && Blood == 2) {
				if (SoulReaper ())
					return true;
			}
			//	actions.re+=/blood_boil,if=blood=2
			if (Blood == 2) {
				if (BloodBoil ())
					return true;
			}
				
			return false;
		}

		bool ActionsRc ()
		{
			//	actions.rc=death_strike,if=unholy=2|frost=2
			if (Unholy == 2 || Frost == 2) {
				if (DeathStrike ())
					return true;
			}
			//	actions.rc+=/death_coil,if=runic_power>70
			if (RunicPower > 70) {
				if (DeathCoil ())
					return true;
			}
			//	actions.rc+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35&blood>=1
			if (Health () * 100 - 3 * (Health () * 100 / TimeToDie ()) <= 35 && Blood > 1) {
				if (SoulReaper ())
					return true;
			}
			//	actions.rc+=/blood_boil,if=blood=2
			if (Blood == 2) {
				if (BloodBoil ())
					return true;
			}

			return false;
		}

		bool ActionsNrt ()
		{
			//	actions.nrt=death_strike,if=unholy=2|frost=2
			if (Unholy == 2 || Frost == 2) {
				if (DeathStrike ())
					return true;
			}
			//	actions.nrt+=/death_coil,if=runic_power>70
			if (RunicPower > 70) {
				if (DeathCoil ())
					return true;
			}
			//	actions.nrt+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35&blood>=1
			if (Health () * 100 - 3 * (Health () * 100 / TimeToDie ()) <= 35 && Blood > 1) {
				if (SoulReaper ())
					return true;
			}
			//	actions.nrt+=/blood_boil,if=blood>=1
			if (Blood >= 1) {
				if (BloodBoil ())
					return true;
			}

			return false;
		}
	}
}

