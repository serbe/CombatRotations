using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace ReBot
{
	[Rotation ("SC Paladin Holy", "Serb", WoWClass.Paladin, Specialization.PaladinHoly, 40, 25)]

	public class SerbPaladinHolySC : SerbPaladin
	{

		[JsonProperty ("Select Mana Playstyle"), JsonConverter (typeof(StringEnumConverter))]
		public Menu ManaPlaystyle = Menu.Normal;
		[JsonProperty ("Ressurect all no combat")]
		public bool RessAll = true;
		[JsonProperty ("Heal Target")]
		public bool HealTarget;
		[JsonProperty ("Use Hand of Sactifice to focus")]
		public bool UseHoS;

		public SerbPaladinHolySC ()
		{
			BeerTimersInit ();
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };

			SetChoice (ManaPlaystyle);
		}

		public override bool OutOfCombat ()
		{
			if (HasGlobalCooldown ())
				return true;

			if (!IsInShapeshiftForm ("Seal of Insight") && GroupMemberCount > 0) {
				if (SealofInsight ())
					return true;
			}

			if (Buff (Me))
				return true;

			if (CleanseTarget != null && Cleanse (CleanseTarget))
				return true;

			if (RessAll && RessurectAll ())
				return true;

			if (BeaconofLightTarget != null && BeaconofLight (BeaconofLightTarget))
				return true;

//			if (UseSacredShield ())
//				return true;
//
//			if (UseEternalFlame ())
//				return true;

			if (Mana (Me) > 0.5) {
				if (HolyLightTarget != null && HolyLight (HolyLightTarget))
					return true;
				if (FlashofLightTarget != null && FlashofLight (FlashofLightTarget))
					return true;
			}

			if (HolyShockTarget != null && HolyShock (HolyShockTarget))
				return true;

			return false;
		}

		public override void Combat ()
		{
//			if (CurrentBotName == "Quest") {
//				OverrideCombatModus = CombatModus.Healer;
//				OverrideCombatRole = CombatRole.Healer;
//			} else {
//				OverrideCombatModus = CombatModus.Healer;
//				OverrideCombatRole = CombatRole.Healer;
//			}
				
			if (Me.CanNotParticipateInCombat ()) {
				if (PaladinFreedom ())
					return;
			}

			if (MeIsBusy)
				return;

			if (Health (Me) < 0.2) {
				if (DivineShield ())
					return;
			}
			if (Health (Me) <= 0.8 && UseDivineProtection) {
				if (DivineProtection ())
					return;
			}

			if (LowestPlayerCount (0.5) >= AOECount) {
				if (AvengingWrath ())
					return;
			}

			if (LayonHandsTarget != null && LayonHands (LayonHandsTarget))
				return;
			if (TankTarget != null && !Me.IsNotInFront (TankTarget) && HolyPrism (TankTarget))
				return;
			if (CleanseTarget != null && Cleanse (CleanseTarget))
				return;
			if (HandOfProtectionTarget != null && HandofProtection (HandOfProtectionTarget))
				return;
			if (BeaconofLightTarget != null && BeaconofLight (BeaconofLightTarget))
				return;
			if (EternalFlameTarget != null && EternalFlame (EternalFlameTarget))
				return;
			if (SacredShieldTarget != null && SacredShield (SacredShieldTarget))
				return;
			if (HolyShockTarget != null && HolyShock (HolyShockTarget))
				return;
			if (HandofSacrificeTarget != null && HandofSacrifice (HandofSacrificeTarget))
				return;

			if (Me.HasAura ("Divine Purpose")) {
				if (LightofDawnTarget != null && LightofDawn ())
					return;
			}

			if (LowestPlayerCount (0.7) >= AOECount && FocusTankorMe (0.2) == null) {

				if (LightofDawnTarget != null && LightofDawn ())
					return;
				if (GetHolyPower ())
					return;
				if (HolyRadianceTarget != null && HolyRadiance (HolyRadianceTarget))
					return;
			}


			if (FlashofLightTarget != null && FlashofLight (FlashofLightTarget))
				return;
			if (HolyLightTarget != null && HolyLight (HolyLightTarget))
				return;

			if (HealTarget && Target != null) {
				if (UseHealTarget ())
					return;
			}

//			if (UseWarningHeal ())
//				return;

			if (Health (LowestPlayer) > FlashofLightHealth) {
				if (GetHolyPower ())
					return;
			}

			if (Usable ("Hammer of Wrath")) {
				Unit = Enemy.Where (u => Range (30, u) && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && HammerofWrath (Unit))
					return;
			}

			if (Target != null && !Target.IsDead && Target.IsEnemy) {
				if (Health () < 0.2 && HammerofWrath ())
					return;
				if (Judgment ())
					return;
				if (HolyShock ())
					return;
				if (CrusaderStrike ())
					return;
				if (Denounce ())
					return;
			}
		}
	}
}

