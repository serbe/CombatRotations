using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Feral Druid SC", "Serb", WoWClass.Druid, Specialization.DruidFeral, 5, 25)]

	public class SerbWarriorProtectionSC : SerbWarrior
	{
		public 	SerbWarriorProtectionSC ()
		{
			GroupBuffs = new[] {
				"Mark of the Wild"
			};
			PullSpells = new[] {
				"Charge",
				"Shred",
				"Faerie Swarm",
				"Faerie Fire",
				"Moonfire"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_stamina_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/stance,choose=defensive
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	# Generic on-use trinket line if needed when swapping trinkets out. 
			//	#actions+=/use_item,slot=trinket1,if=buff.bloodbath.up|buff.avatar.up|target.time_to_die<10
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/shield_wall
			//	actions.precombat+=/potion,name=draenic_armor

			return false;
		}

		public override void Combat ()
		{
			//	actions=charge
			if (Range () > 10) {
				if (Charge ())
					return;
			}
			//	actions+=/auto_attack
			//	actions+=/use_item,name=tablet_of_turnbuckle_teamwork,if=active_enemies=1&(buff.bloodbath.up|!talent.bloodbath.enabled)|(active_enemies>=2&buff.ravager_protection.up)
			//	actions+=/blood_fury,if=buff.bloodbath.up|buff.avatar.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				BloodFury ();
			//	actions+=/berserking,if=buff.bloodbath.up|buff.avatar.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				Berserking ();
			//	actions+=/arcane_torrent,if=buff.bloodbath.up|buff.avatar.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				ArcaneTorrent ();
			//	actions+=/berserker_rage,if=buff.enrage.down
			if (!Me.HasAura ("Enrage"))
				BerserkerRage ();
			//	actions+=/call_action_list,name=prot
			if (Prot ())
				return;
		}

		bool Prot ()
		{
			//	actions.prot=shield_block,if=!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up)
			if (!(Target.HasAura ("Demoralizing Shout") || Me.HasAura ("Ravager Protection") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block")))
				ShieldBlock ();
			//	actions.prot+=/shield_barrier,if=buff.shield_barrier.down&((buff.shield_block.down&action.shield_block.charges_fractional<0.75)|rage>=85)
			//	actions.prot+=/demoralizing_shout,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			//	actions.prot+=/enraged_regeneration,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			//	actions.prot+=/shield_wall,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			//	actions.prot+=/last_stand,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			//	actions.prot+=/potion,name=draenic_armor,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)|target.time_to_die<=25
			//	actions.prot+=/stoneform,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			//	actions.prot+=/call_action_list,name=prot_aoe,if=active_enemies>3
			if (EnemyInRange (10) > 3) {
				if (Prot_aoe ())
					return true;
			}
			//	actions.prot+=/heroic_strike,if=buff.ultimatum.up|(talent.unyielding_strikes.enabled&buff.unyielding_strikes.stack>=6)
			//	actions.prot+=/bloodbath,if=talent.bloodbath.enabled&((cooldown.dragon_roar.remains=0&talent.dragon_roar.enabled)|(cooldown.storm_bolt.remains=0&talent.storm_bolt.enabled)|talent.shockwave.enabled)
			//	actions.prot+=/avatar,if=talent.avatar.enabled&((cooldown.ravager.remains=0&talent.ravager.enabled)|(cooldown.dragon_roar.remains=0&talent.dragon_roar.enabled)|(talent.storm_bolt.enabled&cooldown.storm_bolt.remains=0)|(!(talent.dragon_roar.enabled|talent.ravager.enabled|talent.storm_bolt.enabled)))
			//	actions.prot+=/shield_slam
			//	actions.prot+=/revenge
			//	actions.prot+=/ravager
			//	actions.prot+=/storm_bolt
			//	actions.prot+=/dragon_roar
			//	actions.prot+=/impending_victory,if=talent.impending_victory.enabled&cooldown.shield_slam.remains<=execute_time
			//	actions.prot+=/victory_rush,if=!talent.impending_victory.enabled&cooldown.shield_slam.remains<=execute_time
			//	actions.prot+=/execute,if=buff.sudden_death.react
			//	actions.prot+=/devastate

			return false;
		}

		bool Prot_aoe ()
		{
			//	actions.prot_aoe=bloodbath
			//	actions.prot_aoe+=/avatar
			//	actions.prot_aoe+=/thunder_clap,if=!dot.deep_wounds.ticking
			//	actions.prot_aoe+=/heroic_strike,if=buff.ultimatum.up|rage>110|(talent.unyielding_strikes.enabled&buff.unyielding_strikes.stack>=6)
			//	actions.prot_aoe+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
			//	actions.prot_aoe+=/shield_slam,if=buff.shield_block.up
			//	actions.prot_aoe+=/ravager,if=(buff.avatar.up|cooldown.avatar.remains>10)|!talent.avatar.enabled
			//	actions.prot_aoe+=/dragon_roar,if=(buff.bloodbath.up|cooldown.bloodbath.remains>10)|!talent.bloodbath.enabled
			//	actions.prot_aoe+=/shockwave
			//	actions.prot_aoe+=/revenge
			//	actions.prot_aoe+=/thunder_clap
			//	actions.prot_aoe+=/bladestorm
			//	actions.prot_aoe+=/shield_slam
			//	actions.prot_aoe+=/storm_bolt
			//	actions.prot_aoe+=/shield_slam
			//	actions.prot_aoe+=/execute,if=buff.sudden_death.react
			//	actions.prot_aoe+=/devastate

			return false;
		}
	}
}
	