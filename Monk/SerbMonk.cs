using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbMonk : SerbUtils
	{
		// Vars Consts

		[JsonProperty ("Time run to use Tiger Lust")]
		public double TTL = 1;


		Random Rnd = new Random ();

		// Get

		public int ElusiveBrewStacks {
			get {
				foreach (var a in Me.Auras) {
					if (a.SpellId == 128939)
						return a.StackCount;
				}
				return  0;
			}
		}

		public UnitObject ZenSphereTarget {
			get {
				if (Me.Focus != null) {
					if (Me.Focus.IsPlayer && !Me.Focus.IsDead && Range (40, Me.Focus) && !Me.Focus.HasAura ("Zen Sphere", true))
						return Me.Focus;
					return null;
				}
				if (Tank != null) {
					if (!Tank.IsDead && Range (40, Tank) && !Tank.HasAura ("Zen Sphere", true))
						return Tank;
					return null;
				}
				return null;
			}
		}

		// Check


		// Combo

		public bool MassDispel ()
		{
			if (MyGroup.Count > 0) {
				foreach (UnitObject p in MyGroup) {
					if (p.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType)) && Detox (p))
						return true;
				}
			}
			if (Me.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType)) && Detox (Me))
				return true;
			return false;
		}

		public bool MassResurect ()
		{
			if (CurrentBotName == "Combat" && MyGroup.Count > 0) {
				var Unit = MyGroup.FirstOrDefault (u => Range (40, u) && u.IsDead);
				if (Unit != null && Resuscitate (Unit))
					return true;
			}
			return false;
		}

		public bool Buff (UnitObject u = null)
		{
			u = u ?? Target;
			if (Usable ("Legacy of the Emperor")) {
				if (Range (40, u) && u.AuraTimeRemaining ("Legacy of the Emperor") < 300 && !u.HasAura ("Blessing of Kings") && !u.HasAura ("Mark of the Wild") && !u.HasAura ("Legacy of the White Tiger") && C ("Legacy of the Emperor", u))
					return true;
			}
			if (Usable ("Legacy of the White Tiger")) {
				if (Range (40, u) && u.AuraTimeRemaining ("Legacy of the White Tiger") < 300 && u.AuraTimeRemaining ("Blessing of Kings") < 300 && C ("Legacy of the White Tiger", u))
					return true;
			}	
			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Leg Sweep") || Usable ("Spear Hand Strike") || Usable ("Ring of Peace")) {
				var Unit = Enemy.Where (u => u.IsCastingAndInterruptible () && !IsBoss (u) && Range (5, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && (SpearHandStrike (Unit) || LegSweep (Unit) || RingofPeace (Unit)))
					return true;
			}
			return false;
		}

		public bool AggroDizzyingHaze ()
		{
			if (Usable ("Dizzying Haze") && Healer != null) {
				var Unit = Enemy.Where (u => u.InCombat && u.Target == Healer && Range (40, u, 8)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && DizzyingHaze (Unit))
					return true;
			}
			return false;
		}

		public bool HealStatue ()
		{
			if (!MyGroup.Any (p => p.InCombat))
				return false;
		
			const int StatueEntryID = 60849;
		
			var statue = API.Units.FirstOrDefault (u => u.EntryID == StatueEntryID && u.CreatedByMe);
			if (statue == null || statue.Distance > 35) {
				foreach (var u in MyGroup.Where(p => IsTank(p) || p == Me)) {
					if (u != null && u.Distance < 20) {
						var pos = u.Position;
						for (int i = 0; i < 8; i++) {
							var StatuePos = pos;
							StatuePos.X += (float)Rnd.NextDouble () * 10 - 5;
							StatuePos.Y += (float)Rnd.NextDouble () * 10 - 5;
		
							if (SummonJadeSerpentStatue (StatuePos))
								return true;
						}
					}
				}
			}
			return false;
		}

		// Spell

		public bool SummonJadeSerpentStatue (Vector3 p)
		{
			if (Usable ("Crackling Jade Lightning") && Vector3.Distance (Me.Position, p) <= 40) {
				if (CastOnTerrainPreventDouble ("Crackling Jade Lightning", p, null, 2000))
					return true;
				API.Print ("False CastOnTerrain Crackling Jade Lightning with " + Vector3.Distance (Me.Position, p) + " range");
			}
			return false;
		}

		public bool CracklingJadeLightning (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Crackling Jade Lightning") && Range (40, u) && C ("Crackling Jade Lightning", u);
		}

		public bool ManaTea ()
		{
			return Usable ("Mana Tea") && GetAuraStack ("Mana Tea", Me) > 0 && CS ("Mana Tea");
		}

		public bool RisingSunKick (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Rising Sun Kick") && Chi >= 2 && Range (5, u) && C ("Rising Sun Kick", u);
		}

		public bool RingofPeace (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Ring of Peace") && Range (40, u) && C ("Ring of Peace", u);
		}

		public bool Resuscitate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Resuscitate") && Range (40, u) && C ("Resuscitate", u);
		}

		public bool DizzyingHaze (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Dizzying Haze") && Range (40, u) && COTPD ("Dizzying Haze", u, 2000);
		}

		public bool ChiBrew ()
		{
			return Usable ("Chi Brew") && ChiMax - Chi >= 2 && CS ("Chi Brew");
		}

		public bool NimbleBrew ()
		{
			return Usable ("Nimble Brew") && CS ("Nimble Brew");
		}

		public bool OxStance ()
		{
			return Usable ("Stance of the Sturdy Ox") && !IsInShapeshiftForm ("Stance of the Sturdy Ox") && CS ("Stance of the Sturdy Ox");
		}

		public bool DampenHarm ()
		{
			return Usable ("Dampen Harm") && !Me.HasAura ("Dampen Harm") && CS ("Dampen Harm");
		}

		public bool FortifyingBrew ()
		{
			return Usable ("Fortifying Brew") && !Me.HasAura ("Fortifying Brew") && CS ("Fortifying Brew");
		}

		public bool ElusiveBrew ()
		{
			return Usable ("Elusive Brew") && CS ("Elusive Brew");
		}

		public bool InvokeXuentheWhiteTiger (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Invoke Xuen, the White Tiger") && Range (40, u) && DangerBoss () && C ("Invoke Xuen, the White Tiger", u);
		}

		public bool Serenity ()
		{
			return Usable ("Serenity") && CS ("Serenity");
		}

		public bool TouchofDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Touch of Death") && (Chi >= 3 || HasGlyph (123391)) && ((IsBoss (u) && Health (u) < 0.1) || u.Health < Me.MaxHealth) && Range (5, u) && Me.HasAura ("Death Note") && C ("Touch of Death", u);
		}

		public bool PurifyingBrew ()
		{
			return Usable ("Purifying Brew") && (Chi >= 1 || Me.HasAura ("Purifier")) && CS ("Purifying Brew");
		}

		public bool BlackoutKick (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Blackout Kick") && (Chi >= 2 || Me.HasAura ("Combo Breaker: Blackout Kick")) && Range (5, u) && C ("Blackout Kick", u);
		}

		public bool ChiExplosion (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chi Explosion") && (Chi >= 1 || Me.HasAura ("Combo Breaker: Chi Explosion")) && Range (40, u) && C ("Chi Explosion", u);
		}

		public bool Guard ()
		{
			return Usable ("Guard") && Chi >= 2 && SpellCharges ("Guard") > 0 && CS ("Guard");
		}

		public bool KegSmash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Keg Smash") && Energy >= 40 && (Range (5, u) || (HasGlyph (159495) && Range (10, u))) && C ("Keg Smash", u);
		}

		public bool ChiBurst (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chi Burst") && Range (40, u) && !Me.IsMoving && C ("Chi Burst", u);
		}

		public bool ChiWave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chi Wave") && Range (40, u) && C ("Chi Wave", u);
		}

		public bool SpearHandStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Spear Hand Strike") && Range (5, u) && C ("Spear Hand Strike", u);
		}

		public bool LegSweep (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Leg Sweep") && Range (5, u) && CS ("Leg Sweep");
		}

		public bool ZenSphere (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Zen Sphere") && Range (40, u) && u.IsFriendly && C ("Zen Sphere", u);
		}

		public bool ExpelHarm ()
		{
			return Usable ("Expel Harm") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && (Energy >= 40 || (HasGlyph (159487) && Health (Me) < 0.35 && Energy >= 35))) || Me.HasAura ("Stance of the Wise Serpent")) && CS ("Expel Harm");
		}

		public bool Jab (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Jab") && Range (5, u) && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && (Energy >= 40 || (Me.HasAura ("Heightened Senses") && Energy >= 10))) || Me.HasAura ("Stance of the Wise Serpent")) && C ("Jab", u);
		}

		public bool TigerPalm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Tiger Palm") && Range (5, u) && (Chi >= 1 || Me.HasAura ("Combo Breaker: Tiger Palm") || Me.Specialization == Specialization.MonkBrewmaster) && C ("Tiger Palm", u);
		}

		public bool RushingJadeWind ()
		{
			return Usable ("Rushing Jade Wind") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")) && CS ("Rushing Jade Wind");
		}

		public bool SurgingMist (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Surging Mist") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 30 && Range (40, u)) || (Me.HasAura ("Stance of the Wise Serpent") && (Range (40, u) || HasGlyph (120483)))) && C ("Surging Mist", u);
		}

		public bool Detox (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Detox") && Range (40, u) && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")) && C ("Detox", u);
		}

		public bool TigersLust ()
		{
			return Usable ("Tiger's Lust") && CS ("Tiger's Lust");
		}

		public bool SpinningCraneKick ()
		{
			return Usable ("Spinning Crane Kick") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")) && CS ("Spinning Crane Kick");
		}

		// Items

	}
}