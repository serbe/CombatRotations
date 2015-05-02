﻿using System;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Paladin Protiction SC", "Serb", WoWClass.Paladin, Specialization.PaladinProtection, 5, 25)]

	public class SerbPaladinProtictionSC : SerbPaladin
	{
		public SerbPaladinProtictionSC ()
		{
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_stamina_flask
			//	actions.precombat+=/flask,type=greater_draenic_strength_flask,if=role.attack|using_apl.max_dps
			//	actions.precombat+=/food,type=whiptail_fillet
			//	actions.precombat+=/food,type=pickled_eel,if=role.attack|using_apl.max_dps
			//	actions.precombat+=/blessing_of_kings,if=(!aura.str_agi_int.up)&(aura.mastery.up)
			//	actions.precombat+=/blessing_of_might,if=!aura.mastery.up
			//	actions.precombat+=/seal_of_insight
			//	actions.precombat+=/seal_of_righteousness,if=role.attack|using_apl.max_dps
			//	actions.precombat+=/sacred_shield
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_armor
			if (InCombat) {
				InCombat = false;
				return true;
			}

			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			//	actions=auto_attack
			//	actions+=/speed_of_light,if=movement.remains>1
			if (Range () > 15)
				SpeedofLight ();
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	# Off-GCD spells.

			//	actions+=/holy_avenger
			HolyAvenger ();
			//	actions+=/potion,name=draenic_armor,if=buff.shield_of_the_righteous.down&buff.seraphim.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down&buff.ardent_defender.down
			//	actions+=/seraphim
			Seraphim ();
			//	actions+=/divine_protection,if=time<5|!talent.seraphim.enabled|(buff.seraphim.down&cooldown.seraphim.remains>5&cooldown.seraphim.remains<9)
			if (Time < 5 || !HasSpell ("Seraphim") || (!Me.HasAura ("Seraphim") && Cooldown ("Seraphim") > 5 && Cooldown ("Seraphim") < 9))
				DivineProtection ();
			//	actions+=/guardian_of_ancient_kings,if=time<5|(buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down)
			if (Time < 5 || (!Me.HasAura ("Holy Avenger") && !Me.HasAura ("Shield of the Righteous") && !Me.HasAura ("Divine Protection")))
				GuardianofAncientKings ();
			//	actions+=/ardent_defender,if=time<5|(buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down)
			if (Time < 5 || (!Me.HasAura ("Holy Avenger") && !Me.HasAura ("Shield of the Righteous") && !Me.HasAura ("Divine Protection") && !Me.HasAura ("Guardian of Ancient Kings")))
				ArdentDefender ();
			//	actions+=/eternal_flame,if=buff.eternal_flame.remains<2&buff.bastion_of_glory.react>2&(holy_power>=3|buff.divine_purpose.react|buff.bastion_of_power.react)
			if (Me.AuraTimeRemaining ("Eternal Flame") < 2 && Me.GetAura ("Bastion of Glory").StackCount > 2 && (HolyPower >= 3 || Me.HasAura ("Divine Purpose") || Me.HasAura ("Bastion of Power"))) {
				if (EternalFlame ())
					return;
			}
			//	actions+=/eternal_flame,if=buff.bastion_of_power.react&buff.bastion_of_glory.react>=5
			if (Me.HasAura ("Bastion of Power") && Me.GetAura ("Bastion of Glory").StackCount >= 5) {
				if (EternalFlame ())
					return;
			}
			//	actions+=/shield_of_the_righteous,if=buff.divine_purpose.react
			if (Me.HasAura ("Divine Purpose"))
				ShieldoftheRighteous ();
			//	actions+=/shield_of_the_righteous,if=(holy_power>=5|incoming_damage_1500ms>=health.max*0.3)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5)
//			if (HolyPower >= 5 || DamageTaken(1500) >= Me.MaxHealth * 0.3)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5))
			ShieldoftheRighteous ();
			//	actions+=/shield_of_the_righteous,if=buff.holy_avenger.remains>time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)
//			if (Me.AuraTimeRemaining ("Holy Avenger") > time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)

			//	# GCD-bound spells
			if (HasGlobalCooldown () && Gcd)
				return;
			//	actions+=/seal_of_insight,if=talent.empowered_seals.enabled&!seal.insight&buff.uthers_insight.remains<cooldown.judgment.remains
			//	actions+=/seal_of_righteousness,if=talent.empowered_seals.enabled&!seal.righteousness&buff.uthers_insight.remains>cooldown.judgment.remains&buff.liadrins_righteousness.down
			//	actions+=/avengers_shield,if=buff.grand_crusader.react&active_enemies>1&!glyph.focused_shield.enabled
			//	actions+=/hammer_of_the_righteous,if=active_enemies>=3
			//	actions+=/crusader_strike
			//	actions+=/wait,sec=cooldown.crusader_strike.remains,if=cooldown.crusader_strike.remains>0&cooldown.crusader_strike.remains<=0.35
			//	actions+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&last_judgment_target!=target
			//	actions+=/judgment
			//	actions+=/wait,sec=cooldown.judgment.remains,if=cooldown.judgment.remains>0&cooldown.judgment.remains<=0.35
			//	actions+=/avengers_shield,if=active_enemies>1&!glyph.focused_shield.enabled
			//	actions+=/holy_wrath,if=talent.sanctified_wrath.enabled
			//	actions+=/avengers_shield,if=buff.grand_crusader.react
			//	actions+=/sacred_shield,if=target.dot.sacred_shield.remains<2
			//	actions+=/holy_wrath,if=glyph.final_wrath.enabled&target.health.pct<=20
			//	actions+=/avengers_shield
			//	actions+=/lights_hammer,if=!talent.seraphim.enabled|buff.seraphim.remains>10|cooldown.seraphim.remains<6
			//	actions+=/holy_prism,if=!talent.seraphim.enabled|buff.seraphim.up|cooldown.seraphim.remains>5|time<5
			//	actions+=/consecration,if=target.debuff.flying.down&active_enemies>=3
			//	actions+=/execution_sentence,if=!talent.seraphim.enabled|buff.seraphim.up|time<12
			//	actions+=/hammer_of_wrath
			//	actions+=/sacred_shield,if=target.dot.sacred_shield.remains<8
			//	actions+=/consecration,if=target.debuff.flying.down
			//	actions+=/holy_wrath
			//	actions+=/seal_of_insight,if=talent.empowered_seals.enabled&!seal.insight&buff.uthers_insight.remains<=buff.liadrins_righteousness.remains
			//	actions+=/seal_of_righteousness,if=talent.empowered_seals.enabled&!seal.righteousness&buff.liadrins_righteousness.remains<=buff.uthers_insight.remains
			//	actions+=/sacred_shield
			//	actions+=/flash_of_light,if=talent.selfless_healer.enabled&buff.selfless_healer.stack>=3
		}
	}
}


//
//	# This is a high-DPS (but low-survivability) configuration.
//	# Invoke by adding "actions+=/run_action_list,name=max_dps" to the beginning of the default APL.
//
//	actions.max_dps=auto_attack
//	actions.max_dps+=/speed_of_light,if=movement.remains>1
//	actions.max_dps+=/blood_fury
//	actions.max_dps+=/berserking
//	actions.max_dps+=/arcane_torrent
//	# Off-GCD spells.
//	actions.max_dps+=/holy_avenger
//	actions.max_dps+=/potion,name=draenic_armor,if=buff.holy_avenger.up|(!talent.holy_avenger.enabled&(buff.seraphim.up|(!talent.seraphim.enabled&buff.bloodlust.react)))|target.time_to_die<=20
//	actions.max_dps+=/seraphim
//	actions.max_dps+=/shield_of_the_righteous,if=buff.divine_purpose.react
//	actions.max_dps+=/shield_of_the_righteous,if=(holy_power>=5|talent.holy_avenger.enabled)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5)
//	actions.max_dps+=/shield_of_the_righteous,if=buff.holy_avenger.remains>time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)
//	# GCD-bound spells
//	actions.max_dps+=/avengers_shield,if=buff.grand_crusader.react&active_enemies>1&!glyph.focused_shield.enabled
//	actions.max_dps+=/holy_wrath,if=talent.sanctified_wrath.enabled&(buff.seraphim.react|(glyph.final_wrath.enabled&target.health.pct<=20))
//	actions.max_dps+=/hammer_of_the_righteous,if=active_enemies>=3
//	actions.max_dps+=/judgment,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.down
//	actions.max_dps+=/crusader_strike
//	actions.max_dps+=/wait,sec=cooldown.crusader_strike.remains,if=cooldown.crusader_strike.remains>0&cooldown.crusader_strike.remains<=0.35
//	actions.max_dps+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&last_judgment_target!=target
//	actions.max_dps+=/judgment
//	actions.max_dps+=/wait,sec=cooldown.judgment.remains,if=cooldown.judgment.remains>0&cooldown.judgment.remains<=0.35
//	actions.max_dps+=/avengers_shield,if=active_enemies>1&!glyph.focused_shield.enabled
//	actions.max_dps+=/holy_wrath,if=talent.sanctified_wrath.enabled
//	actions.max_dps+=/avengers_shield,if=buff.grand_crusader.react
//	actions.max_dps+=/execution_sentence,if=active_enemies<3
//	actions.max_dps+=/holy_wrath,if=glyph.final_wrath.enabled&target.health.pct<=20
//	actions.max_dps+=/avengers_shield
//	actions.max_dps+=/seal_of_righteousness,if=talent.empowered_seals.enabled&!seal.righteousness
//	actions.max_dps+=/lights_hammer
//	actions.max_dps+=/holy_prism
//	actions.max_dps+=/consecration,if=target.debuff.flying.down&active_enemies>=3
//	actions.max_dps+=/execution_sentence
//	actions.max_dps+=/hammer_of_wrath
//	actions.max_dps+=/consecration,if=target.debuff.flying.down
//	actions.max_dps+=/holy_wrath
//	actions.max_dps+=/sacred_shield
//	actions.max_dps+=/flash_of_light,if=talent.selfless_healer.enabled&buff.selfless_healer.stack>=3
//
//	# This is a high-survivability (but low-DPS) configuration.
//	# Invoke by adding "actions+=/run_action_list,name=max_survival" to the beginning of the default APL.
//
//	actions.max_survival=auto_attack
//	actions.max_survival+=/speed_of_light,if=movement.remains>1
//	actions.max_survival+=/blood_fury
//	actions.max_survival+=/berserking
//	actions.max_survival+=/arcane_torrent
//	# Off-GCD spells.
//	actions.max_survival+=/holy_avenger
//	actions.max_survival+=/potion,name=draenic_armor,if=buff.shield_of_the_righteous.down&buff.seraphim.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down&buff.ardent_defender.down
//	actions.max_survival+=/divine_protection,if=time<5|!talent.seraphim.enabled|(buff.seraphim.down&cooldown.seraphim.remains>5&cooldown.seraphim.remains<9)
//	actions.max_survival+=/seraphim,if=buff.divine_protection.down&cooldown.divine_protection.remains>0
//	actions.max_survival+=/guardian_of_ancient_kings,if=buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down
//	actions.max_survival+=/ardent_defender,if=buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down
//	actions.max_survival+=/eternal_flame,if=buff.eternal_flame.remains<2&buff.bastion_of_glory.react>2&(holy_power>=3|buff.divine_purpose.react|buff.bastion_of_power.react)
//	actions.max_survival+=/eternal_flame,if=buff.bastion_of_power.react&buff.bastion_of_glory.react>=5
//	actions.max_survival+=/shield_of_the_righteous,if=buff.divine_purpose.react
//	actions.max_survival+=/shield_of_the_righteous,if=(holy_power>=5|incoming_damage_1500ms>=health.max*0.3)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5)
//	actions.max_survival+=/shield_of_the_righteous,if=buff.holy_avenger.remains>time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)
//	# GCD-bound spells
//	actions.max_survival+=/hammer_of_the_righteous,if=active_enemies>=3
//	actions.max_survival+=/crusader_strike
//	actions.max_survival+=/wait,sec=cooldown.crusader_strike.remains,if=cooldown.crusader_strike.remains>0&cooldown.crusader_strike.remains<=0.35
//	actions.max_survival+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&last_judgment_target!=target
//	actions.max_survival+=/judgment
//	actions.max_survival+=/wait,sec=cooldown.judgment.remains,if=cooldown.judgment.remains>0&cooldown.judgment.remains<=0.35
//	actions.max_survival+=/avengers_shield,if=buff.grand_crusader.react&active_enemies>1
//	actions.max_survival+=/holy_wrath,if=talent.sanctified_wrath.enabled
//	actions.max_survival+=/avengers_shield,if=buff.grand_crusader.react
//	actions.max_survival+=/sacred_shield,if=target.dot.sacred_shield.remains<2
//	actions.max_survival+=/avengers_shield
//	actions.max_survival+=/lights_hammer
//	actions.max_survival+=/holy_prism
//	actions.max_survival+=/consecration,if=target.debuff.flying.down&active_enemies>=3
//	actions.max_survival+=/execution_sentence
//	actions.max_survival+=/flash_of_light,if=talent.selfless_healer.enabled&buff.selfless_healer.stack>=3
//	actions.max_survival+=/hammer_of_wrath
//	actions.max_survival+=/sacred_shield,if=target.dot.sacred_shield.remains<8
//	actions.max_survival+=/holy_wrath,if=glyph.final_wrath.enabled&target.health.pct<=20
//	actions.max_survival+=/consecration,if=target.debuff.flying.down&!ticking
//	actions.max_survival+=/holy_wrath
//	actions.max_survival+=/sacred_shield