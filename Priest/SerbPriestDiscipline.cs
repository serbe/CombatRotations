using Newtonsoft.Json;
using ReBot.API;
using System.Linq;

namespace ReBot
{
	[Rotation ("Serb Priest Discipline", "Serb", WoWClass.Priest, Specialization.PriestDiscipline, 40, 25)]

	public class SerbPriestDisciplineSc : SerbPriest
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto target enemy or party membet")]
		public bool UseAutoTarget = true;
		[JsonProperty ("Fight in instance")]
		public bool FightInInstance = true;
		[JsonProperty ("Heal %/100")]
		public double HealPr = 0.8;
		[JsonProperty ("Heal tank %/100")]
		public double TankPr = 0.9;

		public SerbPriestDisciplineSc ()
		{
			GroupBuffs = new [] {
				"Power Word: Fortitude"
			};
			PullSpells = new [] {
				"Shadow Word: Pain",
				"Smite"
			};
		}

		//		public UnitObject DamageTarget {
		//			get {
		//				if (!Target.IsEnemy) {
		//					Unit = API.CollectUnits (40).Where (u => u.IsEnemy && !u.IsDead && u.IsInLoS && u.IsAttackable && u.InCombat && Range (40, u)).OrderBy (u => u.CombatRange).DefaultIfEmpty (null).FirstOrDefault ();
		//					if (Unit != null)
		//						return Unit;
		//				}
		//				return null;
		//			}
		//		}

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

//			if (HealTarget != null) {
//				if (HealTarget.HealthFraction < HealPr) {
//					if (Healing (Player))
//						return true;
//				}
//			}

			if (Health (Me) < 0.9 && !Me.IsMoving && !Me.IsMounted) {
				if (Heal (Me))
					return true;
			}

			if (DispelTarget != null && DispelMagic (DispelTarget))
				return true;
			if (PurifyTarget != null && Purify (PurifyTarget))
				return true;

			if (Me.FallingTime > 2) {
				if (Levitate ())
					return true;
			}

			return false;
		}

		public override void Combat ()
		{
			if (MeIsBusy)
				return;

			if (SetShieldAll ())
				return;

			if (GroupMemberCount == 0)
				Solo ();
			if (GroupMemberCount < 6)
				Dungeon ();
//			if (GroupMemberCount > 2)
//				Raid ();

		}

		public void Solo ()
		{
			if (DispelTarget != null && DispelMagic (DispelTarget))
				return;
			if (PurifyTarget != null && Purify (PurifyTarget))
				return;
		
			if (GetAuraStack ("Evangelism", Me) >= 5) {
				if (Archangel ())
					return;
			}
			if (Health (Me) <= PainSuppressionHealth) {
				if (PainSuppression (Me))
					return;
			}

			//	actions=potion,name=draenic_intellect,if=buff.bloodlust.react|target.time_to_die<=40
			//	actions+=/mindbender,if=talent.mindbender.enabled
			if (Danger () && Mindbender ())
				return;
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (Danger () && Shadowfiend ())
				return;
			//	actions+=/blood_fury
			if (BloodFury ())
				return;
			//	actions+=/berserking
			if (Berserking ())
				return;
			//	actions+=/arcane_torrent
			if (ArcaneTorrent ())
				return;
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			if (Danger () && PowerInfusion ())
				return;

			if (Health (Me) <= 0.9 && PowerWordShield (Me))
				return;
			if (Health (Me) <= 0.35 && DesperatePrayer ())
				return;
			
			//	actions+=/shadow_word_pain,if=!ticking
			if (SWPTarget (0) != null && ShadowWordPain (SWPTarget (0)))
				return;
			//	actions+=/penance
			if (Penance ())
				return;
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (PowerWordSolace ())
				return;
			//	actions+=/holy_fire,if=!talent.power_word_solace.enabled
			if (HolyFire ())
				return;
			//	actions+=/smite,if=glyph.smite.enabled&(dot.power_word_solace.remains+dot.holy_fire.remains)>cast_time
			if (HasGlyph (55692) && (Target.AuraTimeRemaining ("Power Word: Solace", true) + Target.AuraTimeRemaining ("Holy  Ffire", true)) > CastTime (585)) {
				if (Smite ())
					return;
			}
			//	actions+=/shadow_word_pain,if=remains<(duration*0.3)
			if (SWPTarget (5.4) != null && ShadowWordPain (SWPTarget (5.4)))
				return;
			//	actions+=/smite
			if (Smite ())
				return;
			//	actions+=/shadow_word_pain
		
		}

		public void Dungeon ()
		{
			if (DispelTarget != null && DispelMagic (DispelTarget))
				return;
			if (PurifyTarget != null && Purify (PurifyTarget))
				return;

			if (GetAuraStack ("Evangelism", Me) >= 5 && Archangel ())
				return;
			if (UsePowerInfusion && PowerInfusion ())
				return;
			if (TankTarget != null && Shadowfiend (TankTarget))
				return;
			if (Mana (Me) <= MindbenderMana && MindbenderTarget != null && Mindbender (MindbenderTarget))
				return;
			if (PWSTarget != null && PowerWordShield (PWSTarget))
				return;
			if (ClarityofWillTarget != null && ClarityofWill (ClarityofWillTarget))
				return;
			if (Tank != null && Health (Tank) <= PainSuppressionHealth && PainSuppression (Tank))
				return;
			if (TankTarget != null && !Me.IsNotInFront (TankTarget) && PowerWordSolace (TankTarget))
				return;
			if (CascadeHealthTarget != null && Cascade (CascadeHealthTarget))
				return;
			if (HaloHealthTarget != null && Halo (HaloHealthTarget))
				return;
			if (PenanceTarget != null && Penance (PenanceTarget))
				return;
			if (PoMTarget != null && PrayerofMending (PoMTarget))
				return;
			if (PoHTarget != null && PrayerofHealing (PoHTarget))
				return;
			if (FlashHealTarget != null && FlashHeal (FlashHealTarget))
				return;
			if (HealTarget != null && Heal (HealTarget))
				return;

//			if (CastSelfPreventDouble ("Holy Nova", () => needHolyNova))
//				return;
//
//			if (Cast ("Holy Fire", () => HasSpell ("Holy Fire")
//			    && tankTarget != null && SpellCooldown ("Holy Fire") <= 0
//			    && tankTarget.Distance <= dpsRange
//			    && !Me.IsNotInFront (tankTarget), tankTarget))
//				return;
//
			if (Atonement && Mana (Me) > AtonementMana && TankTarget != null && !Me.IsNotInFront (TankTarget) && Smite (TankTarget))
				return;
		}

		public void Raid ()
		{
			//			if (useHealthstone ()) {
			//				Healthstone ();
			//				return;
			//			}
			//			if (useHealTonic ()) {
			//				HealTonic ();
			//				return;
			//			}
			//			if (useTrinket1) {
			//				Trinket1 ();
			//				return;
			//			}
			//			if (useTrinket2) {
			//				Trinket2 ();
			//				return;
			//			}
			//
			if (DispelTarget != null && DispelMagic (DispelTarget))
				return;
			if (PurifyTarget != null && Purify (PurifyTarget))
				return;
			if (GetAuraStack ("Evangelism", Me) >= 5 && Archangel ())
				return;
			if (UsePowerInfusion && PowerInfusion ())
				return;
			if (TankTarget != null && Shadowfiend (TankTarget))
				return;
			if (Mana (Me) <= MindbenderMana && MindbenderTarget != null && Mindbender (MindbenderTarget))
				return;
			if (PWSTarget != null && PowerWordShield (PWSTarget))
				return;
			if (ClarityofWillTarget != null && ClarityofWill (ClarityofWillTarget))
				return;
			if (Tank != null && Health (Tank) <= PainSuppressionHealth && PainSuppression (Tank))
				return;
			if (TankTarget != null && !Me.IsNotInFront (TankTarget) && PowerWordSolace (TankTarget))
				return;
			if (CascadeHealthTarget != null && Cascade (CascadeHealthTarget))
				return;
			if (HaloHealthTarget != null && Halo (HaloHealthTarget))
				return;
			if (PenanceTarget != null && Penance (PenanceTarget))
				return;
			if (PoMTarget != null && PrayerofMending (PoMTarget))
				return;
			if (PoHTarget != null && PrayerofHealing (PoHTarget))
				return;
			if (FlashHealTarget != null && FlashHeal (FlashHealTarget))
				return;
			if (HealTarget != null && Heal (HealTarget))
				return;
			//
			//			if (CastSelfPreventDouble ("Holy Nova", () => needHolyNova))
			//				return;
			//
			//			if (Cast ("Holy Fire", () => HasSpell ("Holy Fire")
			//			    && tankTarget != null && SpellCooldown ("Holy Fire") <= 0
			//			    && tankTarget.Distance <= dpsRange
			//			    && !Me.IsNotInFront (tankTarget), tankTarget))
			//				return;
			//
			if (Atonement && Mana (Me) > AtonementMana && TankTarget != null && !Me.IsNotInFront (TankTarget) && Smite (TankTarget))
				return;
		}
	}
}
