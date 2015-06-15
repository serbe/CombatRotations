using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbDeathKnight : SerbUtils
	{
		[JsonProperty ("Time run to use Death's Advance")]
		public double TDA = 2;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		// Check

		//		public bool HasBlood {
		//			get {
		//				return (Me.Runes (RuneType.Blood) > 0);
		//			}
		//		}
		//
		//		public bool HasUnholy {
		//			get {
		//				return (Me.Runes (RuneType.Unholy) > 0);
		//			}
		//		}
		//
		//		public bool HasFrost {
		//			get {
		//				return (Me.Runes (RuneType.Frost) > 0);
		//			}
		//		}
		//
		//		public bool HasDeath {
		//			get {
		//				return (Me.Runes (RuneType.Death) > 0);
		//			}
		//		}

		public bool HasBlood {
			get { 
				return Blood > 0 || Death > 0;
			}
		}

		public bool HasUnholy {
			get { 
				return Unholy > 0 || Death > 0;
			}
		}

		public bool HasFrost {
			get { 
				return Frost > 0 || Death > 0;
			}
		}

		public bool HasDeath {
			get { 
				return Death > 0;
			}
		}


		public bool HasFrostDisease (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HasAura ("Frost Fever", true);
		}

		public bool HasBloodDisease (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HasAura ("Blood Plague", true);
		}

		public bool HasNecroticDisease (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HasAura ("Necrotic Plague", true);
		}

		public bool HasDisease (UnitObject u = null)
		{
			u = u ?? Target;
			return HasBloodDisease (u) && HasFrostDisease (u);
		}


		// Get

		public int Blood {
			get { 
				return Me.Runes (RuneType.Blood);
			}
		}

		public int Frost {
			get { 
				return Me.Runes (RuneType.Frost);
			}
		}

		public int Unholy {
			get { 
				return Me.Runes (RuneType.Unholy);
			}
		}

		public int Death {
			get { 
				return Me.Runes (RuneType.Death);
			}
		}

		public double BloodFrac {
			get {
				var startTime1 = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(1); return start");
				var duration = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(1); return duration");
				var runeReady1 =
					API.ExecuteLua<bool> ("start, duration, runeReady = GetRuneCooldown(1); return runeReady");
				var startTime2 = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(2); return start");
				var runeReady2 =
					API.ExecuteLua<bool> ("start, duration, runeReady = GetRuneCooldown(2); return runeReady");
				var currentTime = API.ExecuteLua<double> ("return GetTime()");
				double result;
				if (!runeReady1) {
					result = (currentTime - startTime1) / duration;
				} else
					result = 1;
				if (!runeReady2) {
					result = result + (currentTime - startTime2) / duration;
				} else
					result = result + 1;
				return result;
			}
		}

		public double FrostFrac {
			get {
				var startTime1 = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(3); return start");
				var duration = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(3); return duration");
				var runeReady1 =
					API.ExecuteLua<bool> ("start, duration, runeReady = GetRuneCooldown(3); return runeReady");
				var startTime2 = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(4); return start");
				var runeReady2 =
					API.ExecuteLua<bool> ("start, duration, runeReady = GetRuneCooldown(4); return runeReady");
				var currentTime = API.ExecuteLua<double> ("return GetTime()");
				double result;
				if (!runeReady1) {
					result = (currentTime - startTime1) / duration;
				} else
					result = 1;
				if (!runeReady2) {
					result = result + (currentTime - startTime2) / duration;
				} else
					result = result + 1;
				return result;
			}
		}

		public double UnholyFrac {
			get {
				var startTime1 = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(5); return start");
				var duration = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(5); return duration");
				var runeReady1 =
					API.ExecuteLua<bool> ("start, duration, runeReady = GetRuneCooldown(5); return runeReady");
				var startTime2 = API.ExecuteLua<double> ("start, duration, runeReady = GetRuneCooldown(6); return start");
				var runeReady2 =
					API.ExecuteLua<bool> ("start, duration, runeReady = GetRuneCooldown(6); return runeReady");
				var currentTime = API.ExecuteLua<double> ("return GetTime()");
				double result;
				if (!runeReady1) {
					result = (currentTime - startTime1) / duration;
				} else
					result = 1;
				if (!runeReady2) {
					result = result + (currentTime - startTime2) / duration;
				} else
					result = result + 1;
				return result;
			}
		}

		public int BloodCharge {
			get { 
				return AuraStackCount ("Blood Charge");
			}
		}

		public int Disease (UnitObject u = null)
		{
			u = u ?? Target;
			var result = 0;
			if (HasBloodDisease (u))
				result = result + 1;
			if (HasFrostDisease (u))
				result = result + 1;
			return result;
		}

		public double FrostDiseaseRemaining (UnitObject u = null)
		{
			u = u ?? Target;
			return HasFrostDisease (u) ? u.AuraTimeRemaining ("Frost Fever", true) : 0;
		}

		public double BloodDiseaseRemaining (UnitObject u = null)
		{
			u = u ?? Target;
			return HasBloodDisease (u) ? u.AuraTimeRemaining ("Blood Plague", true) : 0;
		}

		public double DiseaseMinRemains (UnitObject u = null)
		{
			u = u ?? Target;
			return FrostDiseaseRemaining (u) < BloodDiseaseRemaining (u) ? FrostDiseaseRemaining (u) : BloodDiseaseRemaining (u);
		}

		// Combo

		public bool Aggro ()
		{ 
			if (InInstance && Time > 3) {
				Unit = Enemy.Where (u => (Range (30, u) || (HasGlyph (62259) && Range (35, u))) && u.Target != Me).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && DeathGrip (Unit))
					return true;
			}

			return false;
		}

		// Spell

		public bool DeathGrip (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Death Grip") && (Range (30, u) || (HasGlyph (62259) && Range (35, u))) && C ("Death Grip", u);
		}

		public bool HornofWinter ()
		{
			return Usable ("Horn of Winter") && !HasAura ("Horn of Winter") && !HasAura ("Battle Shout") && CS ("Horn of Winter");
		}

		public bool RaiseDead ()
		{
			return Usable ("Raise Dead") && !Me.HasAlivePet && CS ("Raise Dead");
		}

		public bool AntimagicShell ()
		{
			return Usable ("Anti-Magic Shell") && CS ("Anti-Magic Shell");
		}

		public bool UnholyBlight (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Unholy Blight") && Range (10, u) && CS ("Unholy Blight");
		}

		public bool Defile (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Defile") && (Me.HasAura ("Crimson Scourge") || HasUnholy) && Range (30, u) && COT ("Defile", u);
		}

		public bool BloodBoil (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Blood Boil") && (Me.HasAura ("Crimson Scourge") || HasBlood) && Range (10, u) && CS ("Blood Boil");
		}

		public bool SummonGargoyle (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Summon Gargoyle") && Range (30, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > 2) && C ("Summon Gargoyle");
		}

		public bool DarkTransformation ()
		{
			return Usable ("Dark Transformation") && !Me.Pet.HasAura ("Dark Transformation") && Me.HasAlivePet && Me.GetAura ("Shadow Infusion").StackCount == 5 && (HasSpell ("Enhanced Dark Transformation") || (HasDeath || HasUnholy)) && C ("Dark Transformation");
		}

		public bool BloodTap ()
		{
			return Usable ("Blood Tap") && BloodCharge >= 5 && (Blood == 0 || Unholy == 0 || Frost == 0) && CS ("Blood Tap");
		}

		public bool DeathandDecay (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Death and Decay", u.Position, () => Usable ("Death and Decay") && (Me.HasAura ("Crimson Scourge") || HasUnholy) && Range (30, u));
		}








		// --------------------------------------
		public bool SoulReaper (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Soul Reaper") && C ("Soul Reaper", u);
		}

		public bool ScourgeStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Scourge Strike") && (HasUnholy || HasDeath) && C ("Scourge Strike", u);
		}

		public bool DeathCoil (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Death Coil") && (Me.HasAura ("Sudden Doom") || RunicPower >= 30) && Range (40, u) && C ("Death Coil", u);
		}

		public bool IcyTouch (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Icy Touch") && (HasFrost || HasDeath) && Range (30, u) && C ("Icy Touch", u);
		}

		public bool PlagueLeech (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Plague Leech") && u.HasAura ("Frost Fever", true) && u.HasAura ("Blood Plague", true) && C ("Plague Leech");
		}

		public bool EmpowerRuneWeapon (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Empower Rune Weapon") && Danger (u, 10) && C ("Empower Rune Weapon");
		}

		public bool Outbreak (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Outbreak") && (!HasGlyph (59332) || RunicPower >= 30) && Range (30, u) && C ("Outbreak", u);
		}

		public bool PlagueStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Plague Strike") && (HasUnholy || HasDeath) && C ("Plague Strike", u);
		}

		public bool FesteringStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Festering Strike") && ((HasFrost && HasBlood) || (HasFrost && HasDeath) || (HasDeath && HasBlood) || Death == 2) && C ("Festering Strike", u);
		}

		public bool Interrupt ()
		{
			if (Usable ("Mind Freeze")) {
				if (InArena || InBg) {
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && Range (u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && MindFreeze (Unit))
						return true;
				} else {
					Unit = Enemy.Where (u => u.IsCastingAndInterruptible () && Range (u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && MindFreeze (Unit))
						return true;
				}
			}

			if (Usable ("Strangulate")) {
				if (InArena || InBg) {
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsHealer && u.IsCastingAndInterruptible () && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Strangulate (Unit))
						return true;
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Strangulate (Unit))
						return true;
				} else {
					Unit = Enemy.Where (u => u.IsCastingAndInterruptible () && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Strangulate (Unit))
						return true;
				}
			}

			if (Usable ("Asphyxiate")) {
				if (InArena || InBg) {
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsHealer && u.IsCastingAndInterruptible () && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Asphyxiate (Unit))
						return true;
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Asphyxiate (Unit))
						return true;
				} else {
					Unit = Enemy.Where (u => u.IsCastingAndInterruptible () && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Asphyxiate (Unit))
						return true;
				}
			}

			return false;
		}

		public bool Heal ()
		{
			if (Health (Me) < 0.45) {
				if (Healthstone ())
					return true;
			}

			if (Health (Me) <= 0.6) {
				if (DeathSiphon ())
					return true;
			}
			if (!InRaid && Health (Me) < 0.9) {
				if (DeathStrike ())
					return true;
			}
			if (Health (Me) < 0.7) {
				if (Lichborne ())
					return true;
			}
			if (Health (Me) < 0.5) {
				if (VampiricBlood ())
					return true;
			}
			if (Health (Me) < 0.3 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Vampiric Blood")) {
				if (IceboundFortitude ())
					return true;
			}
			if (Health (Me) < 0.8 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood")) {
				if (DancingRuneWeapon ())
					return true;
			}
			if (Health (Me) < 0.5) {
				if (DeathPact ())
					return true;
			}

			return false;
		}

		public bool MindFreeze (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("MindFreeze") && C ("Mind Freeze", u);
		}

		public bool Strangulate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Strangulate") && (HasBlood || HasDeath) && Range (30, u) && C ("Strangulate", u);
		}

		public bool Asphyxiate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Asphyxiate") && Range (30, u) && C ("Asphyxiate", u);
		}

		public bool DeathSiphon (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Death Siphon") && HasDeath && Range (30, u) && C ("Death Siphon", u);
		}

		public bool DeathStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Death Strike") && ((HasFrost && HasUnholy) || (HasFrost && HasDeath) || (HasDeath && HasUnholy) || Death == 2) && C ("Death Strike", u);
		}

		public bool BreathofSindragosa (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Breath of Sindragosa") && RunicPower > 0 && Danger (u, 10) && CS ("Breath of Sindragosa");
		}

		public bool Lichborne ()
		{
			return Usable ("Lichborne") && CS ("Lichborne");
		}

		public bool ArmyoftheDead (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Army of the Dead") && DangerBoss (u, 40) && ((HasBlood && HasFrost && HasUnholy) || (HasDeath && HasFrost && HasUnholy) || (HasBlood && HasDeath && HasUnholy) || (HasBlood && HasFrost && HasDeath) || ((HasBlood || HasFrost || HasUnholy) && Death >= 2) || Death >= 3) && CS ("Army of the Dead");
		}

		public bool VampiricBlood ()
		{
			return Usable ("Vampiric Blood") && CS ("Vampiric Blood");
		}

		public bool IceboundFortitude ()
		{
			return Usable ("Icebound Fortitude") && CS ("Icebound Fortitude");
		}

		public bool DancingRuneWeapon (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Dancing Rune Weapon") && Range (30, u) && CS ("Dancing Rune Weapon"); 
		}

		public bool DeathPact ()
		{
			return Usable ("Death Pact") && CS ("Death Pact");
		}

		public bool BoneShield ()
		{
			return Usable ("Bone Shield") && !Me.HasAura ("Bone Shield") && CS ("Bone Shield");
		}

		public bool ChainsofIce (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chains of Ice") && Range (30, u) && (HasFrost || HasDeath) && C ("Chains of Ice", u);
		}

		public bool RuneTap ()
		{
			return Usable ("Rune Tap") && (HasBlood || HasDeath) && CS ("Rune Tap");
		}

		public bool PillarofFrost ()
		{
			return Usable ("Pillar of Frost") && CS ("Pillar of Frost");
		}

		public bool Conversion ()
		{
			return Usable ("Conversion") && CS ("Conversion");
		}

		public bool DeathsAdvance ()
		{
			return Usable ("Death's Advance") && CS ("Death's Advance");
		}

		public bool HowlingBlast (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Howling Blast") && Range (30, u) && (Me.HasAura ("Freezing Fog") || HasFrost || HasDeath) && C ("Howling Blast", u);
		}

		public bool Obliterate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Obliterate") && Range (5, u) && HasFrost && HasUnholy && C ("Obliterate", u);
		}
	}
}