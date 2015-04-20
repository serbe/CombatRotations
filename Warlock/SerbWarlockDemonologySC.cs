using System;
using System.Linq;
using System.Net;

namespace ReBot.Warlock
{
	public class SerbWarlockDemonologySc : SerbWarlock
	{
		public SerbWarlockDemonologySc ()
		{
			GroupBuffs = new[] {
				"Dark Intent",
				(CurrentBotName == "PvP" ? "Create Soulwell" : null)
			};
			PullSpells = new[] {
				"Immolate",
				"Conflagrate",
				"Incinerate"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/dark_intent,if=!aura.spell_power_multiplier.up
			if (DarkIntent ())
				return true;
			//	actions.precombat+=/summon_pet,if=!talent.demonic_servitude.enabled&(!talent.grimoire_of_sacrifice.enabled|buff.grimoire_of_sacrifice.down)
			if (!HasSpell ("Demonic Servitude") && (!HasSpell ("Grimoire of Sacrifice") || !Me.HasAura ("Grimoire of Sacrifice"))) {
				if (SummonPet ())
					return true;
			}
			//	actions.precombat+=/summon_doomguard,if=talent.demonic_servitude.enabled&active_enemies<9
			//	actions.precombat+=/summon_infernal,if=talent.demonic_servitude.enabled&active_enemies>=9
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect
			//	actions.precombat+=/soul_fire

//			if (Cast ("Grimoire of Service", () => !Me.HasAura ("Grimoire of Service")))
//				return true;

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (Me.HasAura ("Metamorphosis"))
				CancelAura ("Metamorphosis");

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

			if (HandInFlight) {
				if (HandFlightTime >= HandRange * 2 / 40)
					HandInFlight = false;
			}

			var targets = Adds;
			targets.Add (Target);

			//	actions=potion,name=draenic_intellect,if=buff.bloodlust.remains>30|(((buff.dark_soul.up&(trinket.proc.any.react|trinket.stacking_proc.any.react>6)&!buff.demonbolt.remains)|target.health.pct<20)&(!talent.grimoire_of_service.enabled|!talent.demonic_servitude.enabled|pet.service_doomguard.active))
			//	actions+=/berserking
			if (Berserking ())
				return;
			//	actions+=/blood_fury
			if (BloodFury ())
				return;
			//	actions+=/arcane_torrent
			if (ArcaneTorrent ())
				return;
			//	actions+=/mannoroths_fury
			if (MannorothsFury ())
				return;
			//	actions+=/dark_soul,if=talent.demonbolt.enabled&((charges=2&((!glyph.imp_swarm.enabled&(dot.corruption.ticking|trinket.proc.haste.remains<=10))|cooldown.imp_swarm.remains))|target.time_to_die<buff.demonbolt.remains|(!buff.demonbolt.remains&demonic_fury>=790))
			if (HasSpell ("Demonbolt") && ((SpellCharges ("Dark Soul: Instability") == 2 && ((!HasGlyph (56242) && (Target.HasAura ("Corruption", true))) || Cooldown ("Imp Swarm") == 0)) || TimeToDie (Target) < Me.AuraTimeRemaining ("Demonbolt") || (!Me.HasAura ("Demonbolt") && Fury >= 790))) {
				if (DarkSoul ())
					return;
			}
			//	actions+=/dark_soul,if=!talent.demonbolt.enabled&((charges=2&(time>6|(debuff.shadowflame.stack=1&action.hand_of_guldan.in_flight)))|!talent.archimondes_darkness.enabled|(target.time_to_die<=20&!glyph.dark_soul.enabled)|target.time_to_die<=10|(target.time_to_die<=60&demonic_fury>400)|((trinket.proc.any.react|trinket.stacking_proc.any.react)&(demonic_fury>600|(glyph.dark_soul.enabled&demonic_fury>450))))
			if (!HasSpell ("Demonbolt") && ((SpellCharges ("Dark Soul: Instability") == 2 && (Time > 6 || (Target.GetAura ("Shadowflame", true).StackCount == 1 && HandInFlight))) || !HasSpell ("Archimonde's Darkness") || (TimeToDie (Target) <= 20 && !HasGlyph (159665) || TimeToDie (Target) <= 10) || (TimeToDie (Target) <= 60 && Fury > 400))) {
				if (DarkSoul ())
					return;
			}
			//	actions+=/imp_swarm,if=!talent.demonbolt.enabled&(buff.dark_soul.up|(cooldown.dark_soul.remains>(120%(1%spell_haste)))|time_to_die<32)&time>3
			if (!HasSpell ("Demonbolt") && (Me.HasAura ("Dark Soul: Instability") || (Cooldown ("Dark Soul: Instability") > (120 / (1 / SpellHaste))) || TimeToDie (Target) < 32) && Time > 3) {
				if (ImpSwarm ())
					return;
			}
			//	actions+=/felguard:felstorm
			if (Felstorm ())
				return;
			//	actions+=/wrathguard:wrathstorm
			if (Wrathstorm ())
				return;
			//	actions+=/wrathguard:mortal_cleave,if=pet.wrathguard.cooldown.wrathstorm.remains>5
			if (Cooldown ("Wrathstorm") > 5) {
				if (MortalCleave ())
					return;
			}
			//	actions+=/hand_of_guldan,if=!in_flight&dot.shadowflame.remains<travel_time+action.shadow_bolt.cast_time&(((set_bonus.tier17_4pc=0&((charges=1&recharge_time<4)|charges=2))|(charges=3|(charges=2&recharge_time<13.8-travel_time*2))&((cooldown.cataclysm.remains>dot.shadowflame.duration)|!talent.cataclysm.enabled))|dot.shadowflame.remains>travel_time)
			if (!HandInFlight && Target.AuraTimeRemaining ("Shadowflame", true) < HandTravelTime + CastTimeSB && (((!HasSpell (165451) && ((SpellCharges ("Hand of Gul'dan") == 1 && Cooldown ("Hand of Gul'dan") < 4) || SpellCharges ("Hand of Gul'dan") == 2)) || (SpellCharges ("Hand of Gul'dan") == 3 || (SpellCharges ("Hand of Gul'dan") == 2 && Cooldown ("Hand of Gul'dan") < 13.8 - HandTravelTime * 2)) && ((Cooldown ("Cataclysm") > 6) || !HasSpell ("Cataclysm"))) || Target.AuraTimeRemaining ("Shadowflame", true) > HandTravelTime)) {
				if (HandofGuldan ()) {
					HandInFlight = true;
					HandRange = Target.CombatRange;
					StartHandTime = DateTime.Now;
					return;
				}
			}
			//	actions+=/hand_of_guldan,if=!in_flight&dot.shadowflame.remains<travel_time+action.shadow_bolt.cast_time&talent.demonbolt.enabled&((set_bonus.tier17_4pc=0&((charges=1&recharge_time<4)|charges=2))|(charges=3|(charges=2&recharge_time<13.8-travel_time*2))|dot.shadowflame.remains>travel_time)
			if (!HandInFlight && Target.AuraTimeRemaining ("Shadowflame", true) < HandTravelTime + CastTimeSB && HasSpell ("Demonbolt") && ((!HasSpell (165451) && ((SpellCharges ("Hand of Gul'dan") == 1 && Cooldown ("Hand of Gul'dan") < 4) || SpellCharges ("Hand of Gul'dan") == 2)) || (SpellCharges ("Hand of Gul'dan") == 3 || (SpellCharges ("Hand of Gul'dan") == 2 && Cooldown ("Hand of Gul'dan") < 13.8 - HandTravelTime * 2)) || Target.AuraTimeRemaining ("Shadowflame", true) > HandTravelTime)) {
				if (HandofGuldan ()) {
					HandInFlight = true;
					HandRange = Target.CombatRange;
					StartHandTime = DateTime.Now;
					return;
				}
			}
			//	actions+=/hand_of_guldan,if=!in_flight&dot.shadowflame.remains<3.7&time<5&buff.demonbolt.remains<gcd*2&(charges>=2|set_bonus.tier17_4pc=0)&action.dark_soul.charges>=1
			if (!HandInFlight && Target.AuraTimeRemaining ("Shadowflame", true) < 3.7 && Time < 5 && Me.AuraTimeRemaining ("Demonbolt") < 1.5 * 2 && (SpellCharges ("Hand of Gul'dan") >= 1 || !HasSpell (165451)) && SpellCharges ("Dark Soul: Instability") >= 1) {
				if (HandofGuldan ()) {
					HandInFlight = true;
					HandRange = Target.CombatRange;
					StartHandTime = DateTime.Now;
					return;
				}
			}
			//	actions+=/service_pet,if=talent.grimoire_of_service.enabled&(target.time_to_die>120|target.time_to_die<=25|(buff.dark_soul.remains&target.health.pct<20))
			if (HasSpell ("Grimoire of Service") && (TimeToDie () > 120 || TimeToDie () <= 25 || (Me.HasAura ("Dark Soul: Instability") && Health (Target) < 0.2))) {
				if (GrimoireofService ())
					return;
			}
			//	actions+=/summon_doomguard,if=!talent.demonic_servitude.enabled&active_enemies<9
			if (!HasSpell ("Demonic Servitude") && EnemyInRange (40) < 9)
				SummonDoomguard ();
			//	actions+=/summon_infernal,if=!talent.demonic_servitude.enabled&active_enemies>=9
			if (!HasSpell ("Demonic Servitude") && EnemyInRange (40) >= 9)
				SummonInfernal ();
			//	actions+=/call_action_list,name=db,if=talent.demonbolt.enabled
			if (HasSpell ("Demonbolt")) {
				if (DB_Action ())
					return;
			}
			//	actions+=/kiljaedens_cunning,if=!cooldown.cataclysm.remains&buff.metamorphosis.up
			if (Cooldown ("Cataclysm") == 0 && Me.HasAura ("Metamorphosis"))
				KiljaedensCunning ();
			//	actions+=/cataclysm,if=buff.metamorphosis.up
			if (Me.HasAura ("Metamorphosis")) {
				if (Cataclysm ())
					return;
			}
			//	actions+=/immolation_aura,if=demonic_fury>450&active_enemies>=3&buff.immolation_aura.down
			if (Fury > 450 && EnemyInRange (10) >= 3 && !Me.HasAura ("Immolation Aura")) {
				if (ImmolationAura ())
					return;
			}
			//	actions+=/doom,if=buff.metamorphosis.up&target.time_to_die>=30*spell_haste&remains<=(duration*0.3)&(remains<cooldown.cataclysm.remains|!talent.cataclysm.enabled)&trinket.stacking_proc.multistrike.react<10
			if (Me.HasAura ("Metamorphosis") && TimeToDie () >= 30 * SpellHaste && Target.AuraTimeRemaining ("Doom", true) <= (60 * 0.3) && (Target.AuraTimeRemaining ("Doom", true) < Cooldown ("Cataclysm") || !HasSpell ("Cataclysm"))) {
				if (Doom ())
					return;
			}
			//	actions+=/corruption,cycle_targets=1,if=target.time_to_die>=6&remains<=(0.3*duration)&buff.metamorphosis.down
			if (Usable ("Corruption") && !Me.HasAura ("Metamorphosis")) {
				CycleTarget = targets.Where (u => u.IsInLoS && u.CombatRange <= 40 && u.AuraTimeRemaining ("Corruption", true) < (18 * 0.3) && TimeToDie (u) >= 6).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Corruption (CycleTarget))
						return;
				}
			}
			//	actions+=/cancel_metamorphosis,if=buff.metamorphosis.up&((demonic_fury<650&!glyph.dark_soul.enabled)|demonic_fury<450)&buff.dark_soul.down&(trinket.stacking_proc.multistrike.down&trinket.proc.any.down|demonic_fury<(800-cooldown.dark_soul.remains*(10%spell_haste)))&target.time_to_die>20
			if (Me.HasAura ("Metamorphosis") && ((Fury < 650 && !HasGlyph (159665)) || Fury < 450) && !Me.HasAura ("Dark Soul: Instability") && (Fury < (800 - Cooldown ("Dark Soul: Instability") * (10 / SpellHaste))) && TimeToDie () > 20)
				CancelAura ("Metamorphosis");
			//	actions+=/cancel_metamorphosis,if=buff.metamorphosis.up&action.hand_of_guldan.charges>0&dot.shadowflame.remains<action.hand_of_guldan.travel_time+action.shadow_bolt.cast_time&((demonic_fury<100&buff.dark_soul.remains>10)|time<15)&!glyph.dark_soul.enabled
			if (Me.HasAura ("Metamorphosis") && SpellCharges ("Hand of Gul'dan") > 0 && Target.AuraTimeRemaining ("Shadowflame", true) < HandTravelTime + CastTimeSB && ((Fury < 100 && Me.AuraTimeRemaining ("Dark Soul: Instability") > 10) || Time < 15) && !HasGlyph (159665)) {
				CancelAura ("Metamorphosis");
			}
			//	actions+=/cancel_metamorphosis,if=buff.metamorphosis.up&action.hand_of_guldan.charges=3&(!buff.dark_soul.remains>gcd|action.metamorphosis.cooldown<gcd)
			if (Me.HasAura ("Metamorphosis") && SpellCharges ("Hand of Gul'dan") == 3 && (Me.AuraTimeRemaining ("Dark Soul: Instability") > 1.5 || Cooldown ("Metamorphosis") < 1.5)) {
				CancelAura ("Metamorphosis");
			}
			//	actions+=/chaos_wave,if=buff.metamorphosis.up&(buff.dark_soul.up&active_enemies>=2|(charges=3|set_bonus.tier17_4pc=0&charges=2))
			if (Me.HasAura ("Metamorphosis") && (Me.HasAura ("Dark Soul: Instability") && EnemyInRange (40) >= 2 || (SpellCharges ("Chaos Wave") == 3 || !HasSpell (165451) && SpellCharges ("Chaos Wave") == 2))) {
				if (ChaosWave ())
					return;
			}
			//	actions+=/soul_fire,if=buff.metamorphosis.up&buff.molten_core.react&(buff.dark_soul.remains>execute_time|target.health.pct<=25)&(((buff.molten_core.stack*execute_time>=trinket.stacking_proc.multistrike.remains-1|demonic_fury<=ceil((trinket.stacking_proc.multistrike.remains-buff.molten_core.stack*execute_time)*40)+80*buff.molten_core.stack)|target.health.pct<=25)&trinket.stacking_proc.multistrike.remains>=execute_time|trinket.stacking_proc.multistrike.down|!trinket.has_stacking_proc.multistrike)
			//	actions+=/touch_of_chaos,cycle_targets=1,if=buff.metamorphosis.up&dot.corruption.remains<17.4&demonic_fury>750
			if (Me.HasAura ("Metamorphosis") && Fury > 750) {
				CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 40 && x.AuraTimeRemaining ("Corruption", true) < 17.4).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (TouchofChaos (CycleTarget))
						return;
				}
			}
			//	actions+=/touch_of_chaos,if=buff.metamorphosis.up
			if (Me.HasAura ("Metamorphosis")) {
				if (TouchofChaos ())
					return;
			}
			//	actions+=/metamorphosis,if=buff.dark_soul.remains>gcd&(time>6|debuff.shadowflame.stack=2)&(demonic_fury>300|!glyph.dark_soul.enabled)&(demonic_fury>=80&buff.molten_core.stack>=1|demonic_fury>=40)
			if (Me.AuraTimeRemaining ("Dark Soul: Instability") > 1.5 && (Time > 6 || Target.GetAura ("Shadowflame", true).StackCount == 2) && (Fury > 300 || !HasGlyph (159665)) && (Fury >= 80 && AuraStackCount ("Molten Core") >= 1 || Fury >= 40)) {
				if (Metamorphosis ())
					return;
			}
			//	actions+=/metamorphosis,if=(trinket.stacking_proc.multistrike.react|trinket.proc.any.react)&((demonic_fury>450&action.dark_soul.recharge_time>=10&glyph.dark_soul.enabled)|(demonic_fury>650&cooldown.dark_soul.remains>=10))
			//	actions+=/metamorphosis,if=!cooldown.cataclysm.remains&talent.cataclysm.enabled
			if (Cooldown ("Cataclysm") == 0 && HasSpell ("Cataclysm")) {
				if (Metamorphosis ())
					return;
			}
			//	actions+=/metamorphosis,if=!dot.doom.ticking&target.time_to_die>=30%(1%spell_haste)&demonic_fury>300
			if (!Target.HasAura ("Doom", true) && TimeToDie () >= 30 / (1 / SpellHaste) && Fury > 300) {
				if (Metamorphosis ())
					return;
			}
			//	actions+=/metamorphosis,if=(demonic_fury>750&(action.hand_of_guldan.charges=0|(!dot.shadowflame.ticking&!action.hand_of_guldan.in_flight_to_target)))|floor(demonic_fury%80)*action.soul_fire.execute_time>=target.time_to_die
			if ((Fury > 750 && (SpellCharges ("Hand of Gul'dan") == 0 || (!Target.HasAura ("Shadowflame") && !HandInFlight))) || (Fury / 80) * 4 >= TimeToDie ()) {
				if (Metamorphosis ())
					return;
			}
			//	actions+=/metamorphosis,if=demonic_fury>=950
			if (Fury >= 950) {
				if (Metamorphosis ())
					return;
			}
			//	actions+=/cancel_metamorphosis
			if (Me.HasAura ("Metamorphosis"))
				CancelAura ("Metamorphosis");
			//	actions+=/imp_swarm
			if (ImpSwarm ())
				return;
			//	actions+=/hellfire,interrupt=1,if=active_enemies>=5
			//	actions+=/soul_fire,if=buff.molten_core.react&(buff.molten_core.stack>=7|target.health.pct<=25|(buff.dark_soul.remains&cooldown.metamorphosis.remains>buff.dark_soul.remains)|trinket.proc.any.remains>execute_time|trinket.stacking_proc.multistrike.remains>execute_time)&(buff.dark_soul.remains<action.shadow_bolt.cast_time|buff.dark_soul.remains>execute_time)
			//	actions+=/soul_fire,if=buff.molten_core.react&target.time_to_die<(time+target.time_to_die)*0.25+cooldown.dark_soul.remains
			if (Me.HasAura ("Molten Core") && TimeToDie () < (Time + TimeToDie ()) * 0.25 + Cooldown ("Dark Soul: Instability")) {
				if (SoulFire ())
					return;
			}
			//	actions+=/life_tap,if=mana.pct<40&buff.dark_soul.down
			if (Mana () < 0.4 && !Me.HasAura ("Dark Soul: Instability")) {
				if (LifeTap ())
					return;
			}
			//	actions+=/hellfire,interrupt=1,if=active_enemies>=4
			//	actions+=/shadow_bolt
			if (ShadowBolt ())
				return;
			//	actions+=/hellfire,moving=1,interrupt=1
			//	actions+=/life_tap
			if (LifeTap ())
				return;
			
			InCombat = true;
		}

		public bool DB_Action ()
		{
			//	actions.db=immolation_aura,if=demonic_fury>450&active_enemies>=5&buff.immolation_aura.down
			//	actions.db+=/doom,cycle_targets=1,if=buff.metamorphosis.up&active_enemies>=6&target.time_to_die>=30*spell_haste&remains<=(duration*0.3)&(buff.dark_soul.down|!glyph.dark_soul.enabled)
			//	actions.db+=/kiljaedens_cunning,moving=1,if=buff.demonbolt.stack=0|(buff.demonbolt.stack<4&buff.demonbolt.remains>=(40*spell_haste-execute_time))
			//	actions.db+=/demonbolt,if=buff.demonbolt.stack=0|(buff.demonbolt.stack<4&buff.demonbolt.remains>=(40*spell_haste-execute_time))
			//	actions.db+=/doom,cycle_targets=1,if=buff.metamorphosis.up&target.time_to_die>=30*spell_haste&remains<=(duration*0.3)&(buff.dark_soul.down|!glyph.dark_soul.enabled)
			//	actions.db+=/corruption,cycle_targets=1,if=target.time_to_die>=6&remains<=(0.3*duration)&buff.metamorphosis.down
			//	actions.db+=/cancel_metamorphosis,if=buff.metamorphosis.up&buff.demonbolt.stack>3&demonic_fury<=600&target.time_to_die>buff.demonbolt.remains&buff.dark_soul.down
			//	actions.db+=/chaos_wave,if=buff.metamorphosis.up&buff.dark_soul.up&active_enemies>=2&demonic_fury>450
			//	actions.db+=/soul_fire,if=buff.metamorphosis.up&buff.molten_core.react&(((buff.dark_soul.remains>execute_time)&demonic_fury>=175)|(target.time_to_die<buff.demonbolt.remains))
			//	actions.db+=/soul_fire,if=buff.metamorphosis.up&buff.molten_core.react&target.health.pct<=25&(((demonic_fury-80)%800)>(buff.demonbolt.remains%(40*spell_haste)))&demonic_fury>=750
			//	actions.db+=/touch_of_chaos,cycle_targets=1,if=buff.metamorphosis.up&dot.corruption.remains<17.4&demonic_fury>750
			//	actions.db+=/touch_of_chaos,if=buff.metamorphosis.up&(target.time_to_die<buff.demonbolt.remains|(demonic_fury>=750&buff.demonbolt.remains)|buff.dark_soul.up)
			//	actions.db+=/touch_of_chaos,if=buff.metamorphosis.up&(((demonic_fury-40)%800)>(buff.demonbolt.remains%(40*spell_haste)))&demonic_fury>=750
			//	actions.db+=/metamorphosis,if=buff.dark_soul.remains>gcd&(demonic_fury>=470|buff.dark_soul.remains<=action.demonbolt.execute_time*3)&(buff.demonbolt.down|target.time_to_die<buff.demonbolt.remains|(buff.dark_soul.remains>execute_time&demonic_fury>=175))
			//	actions.db+=/metamorphosis,if=buff.demonbolt.down&demonic_fury>=480&(action.dark_soul.charges=0|!talent.archimondes_darkness.enabled&cooldown.dark_soul.remains)
			//	actions.db+=/metamorphosis,if=(demonic_fury%80)*2*spell_haste>=target.time_to_die&target.time_to_die<buff.demonbolt.remains
			//	actions.db+=/metamorphosis,if=target.time_to_die>=30*spell_haste&!dot.doom.ticking&buff.dark_soul.down&time>10
			//	actions.db+=/metamorphosis,if=demonic_fury>750&buff.demonbolt.remains>=action.metamorphosis.cooldown
			//	actions.db+=/metamorphosis,if=(((demonic_fury-120)%800)>(buff.demonbolt.remains%(40*spell_haste)))&buff.demonbolt.remains>=10&dot.doom.remains<=dot.doom.duration*0.3
			//	actions.db+=/cancel_metamorphosis
			//	actions.db+=/imp_swarm
			//	actions.db+=/hellfire,interrupt=1,if=active_enemies>=5
			//	actions.db+=/soul_fire,if=buff.molten_core.react&(buff.dark_soul.remains<action.shadow_bolt.cast_time|buff.dark_soul.remains>cast_time)
			//	actions.db+=/life_tap,if=mana.pct<40&buff.dark_soul.down
			//	actions.db+=/hellfire,interrupt=1,if=active_enemies>=4
			//	actions.db+=/shadow_bolt
			if (ShadowBolt ())
				return true;
			//	actions.db+=/hellfire,moving=1,interrupt=1
			//	actions.db+=/life_tap
			if (LifeTap ())
				return true;

			return false;
		}
	}
}