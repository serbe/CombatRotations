﻿using System;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Priest Discipline SC", "ReBot", WoWClass.Priest, Specialization.PriestDiscipline, 40, 25)]

	public class SerbPriestDisciplineSC : SerbPriest
	{
		public SerbPriestDisciplineSC ()
		{
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/power_word_fortitude,if=!aura.stamina.up
			if (PowerWordFortitude ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect
			//	actions.precombat+=/smite

			return false;
		}

		public override void Combat ()
		{
			//	actions=potion,name=draenic_intellect,if=buff.bloodlust.react|target.time_to_die<=40
			if (IsBoss () && (Me.HasAura ("Bloodlust") || TimeToDie () <= 40)) {
				if (DraenicIntellect ())
					return;
			}
			//	actions+=/mindbender,if=talent.mindbender.enabled
			if (HasSpell ("Mindbender")) {
				if (Mindbender ())
					return;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return;
			}
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			if (HasSpell ("Power Infusion"))
				PowerInfusion ();
			//	actions+=/shadow_word_pain,if=!ticking
			if (!Target.HasAura ("Shadow Word: Pain", true)) {
				if (ShadowWordPain ())
					return;
			}
			//	actions+=/penance
			if (Penance ())
				return;
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (HasSpell ("Power Word: Solace")) {
				if (PowerWordSolace ())
					return;
			}
			//	actions+=/holy_fire,if=!talent.power_word_solace.enabled
			if (!HasSpell ("Power Word: Solace")) {
				if (HolyFire ())
					return;
			}
			//	actions+=/smite,if=glyph.smite.enabled&(dot.power_word_solace.remains+dot.holy_fire.remains)>cast_time
			if (HasGlyph (55692) && (Target.AuraTimeRemaining () + Target.AuraTimeRemaining ()) > 1.5) {
				if (Smite ())
					return;
			}
			//	actions+=/shadow_word_pain,if=remains<(duration*0.3)
			if (Target.AuraTimeRemaining ("Shadow Word: Pain", true) < 5.4) {
				if (ShadowWordPain ())
					return;
			}
			//	actions+=/smite
			if (Smite ())
				return;
			//	actions+=/shadow_word_pain
			if (ShadowWordPain ())
				return;
		}

		public bool Healing ()
		{
			//	actions=mana_potion,if=mana.pct<=75
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			if (HasSpell ("Power Infusion"))
				PowerInfusion ();
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (HasSpell ("Power Word: Solace")) {
				if (PowerWordSolace ())
					return true;
			}
			//	actions+=/mindbender,if=talent.mindbender.enabled&mana.pct<80
			if (HasSpell ("Mindbender") && Me.ManaFraction < 0.8) {
				if (Mindbender ())
					return true;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return true;
			}
			//	actions+=/power_word_shield
			if (PowerWordShield ())
				return true;
			//	actions+=/penance_heal,if=buff.borrowed_time.up
			if (Me.HasAura ("Borrowed Time")) {
				if (Penance ())
					return true;
			}
			//	actions+=/penance_heal
			if (Penance ())
				return true;
			//	actions+=/flash_heal,if=buff.surge_of_light.react
			if (Me.HasAura ("Surge of Light")) {
				if (FlashHeal ())
					return true;
			}
			//	actions+=/heal,if=buff.power_infusion.up|mana.pct>20
			if (Me.HasAura ("Power Infusion") || Me.ManaFraction > 0.2) {
				if (Heal ())
					return true;
			}
			//	actions+=/prayer_of_mending
			//	actions+=/clarity_of_will
			//	actions+=/heal
			return false;
		}
	}
}

