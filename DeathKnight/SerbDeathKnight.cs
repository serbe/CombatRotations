using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.DeathKnight
{
	public abstract class SerbDeathKnight : CombatRotation
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public int CrystalOfInsanityId = 86569;
		public int OraliusWhisperingCrystalId = 118922;
		public UnitObject CycleTarget;

		public bool InRaid {
			get { return API.MapInfo.Type == MapType.Raid; }
		}

		public bool InInstance {
			get { return API.MapInfo.Type == MapType.Instance; }
		}

		public bool InArena {
			get { return API.MapInfo.Type == MapType.Arena; }
		}

		public bool InBg {
			get { return API.MapInfo.Type == MapType.PvP; }
		}

		public bool IsPlayer (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsPlayer;
		}

		public bool IsElite (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsElite (); 
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
		}

		public int RunicPower {
			get { return Me.GetPower (WoWPowerType.RunicPower); }
		}

		public bool HasBlood {
			get { return (Me.Runes (RuneType.Blood) > 0); }
		}

		public bool HasUnholy {
			get { return (Me.Runes (RuneType.Unholy) > 0); }
		}

		public bool HasFrost {
			get { return (Me.Runes (RuneType.Frost) > 0); }
		}

		public bool HasDeath {
			get { return (Me.Runes (RuneType.Death) > 0); }
		}

		public int Blood {
			get { return Me.Runes (RuneType.Blood); }
		}

		public int Frost {
			get { return Me.Runes (RuneType.Frost); }
		}

		public int Unholy {
			get { return Me.Runes (RuneType.Unholy); }
		}

		public int Death {
			get { return Me.Runes (RuneType.Death); }
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
			get { return AuraStackCount ("Blood Charge"); }
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return (u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable) {
					x++;
				}
			}
			return x;
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

		public bool HasDisease (UnitObject u = null)
		{
			u = u ?? Target;
			return HasBloodDisease (u) && HasFrostDisease (u);
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

		public double MinDisease (UnitObject u = null)
		{
			u = u ?? Target;
			return FrostDiseaseRemaining (u) < BloodDiseaseRemaining (u) ? FrostDiseaseRemaining (u) : BloodDiseaseRemaining (u);
		}

		public double Cooldown (string s)
		{
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public bool Usable (string s)
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public virtual bool HornofWinter ()
		{
			return CastSelf ("Horn of Winter",
				() => Usable ("Horn of Winter") && !HasAura ("Horn of Winter") && !HasAura ("Battle Shout"));
		}

		public virtual bool RaiseDead ()
		{
			return CastSelf ("Raise Dead", () => Usable ("Raise Dead") && !Me.HasAlivePet);
		}

		public virtual bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return API.HasItem (5512) && API.ItemCooldown (5512) == 0 && API.UseItem (5512);
		}

		public virtual bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityId) && !Me.HasAura ("Visions of Insanity") &&
			    API.ItemCooldown (CrystalOfInsanityId) == 0)
				return API.UseItem (CrystalOfInsanityId);
			return false;
		}

		public virtual bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalId) && !Me.HasAura ("Whispers of Insanity") &&
			    API.ItemCooldown (OraliusWhisperingCrystalId) == 0)
				return API.UseItem (OraliusWhisperingCrystalId);
			return false;
		}

		public virtual bool BloodFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public virtual bool Berserking (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Berserking", () => Usable ("Berserking") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public virtual bool ArcaneTorrent (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public virtual bool AntimagicShell ()
		{
			return CastSelf ("Anti-Magic Shell", () => Usable ("Anti-Magic Shell"));
		}

		public virtual bool UnholyBlight (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Unholy Blight", () => Usable ("Unholy Blight") && u.IsInLoS && u.CombatRange <= 10);
		}

		public virtual bool Defile (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Defile", u.Position, () => Usable ("Defile") && (Me.HasAura ("Crimson Scourge") || (HasUnholy || HasDeath)) && u.IsInLoS && u.CombatRange <= 30);
		}

		public virtual bool BloodBoil (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Blood Boil", () => Usable ("Blood Boil") && (Me.HasAura ("Crimson Scourge") || (HasBlood || HasDeath)) && u.IsInLoS && u.CombatRange <= 10);
		}

		public virtual bool SummonGargoyle (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Summon Gargoyle", () => Usable ("Summon Gargoyle") && u.IsInLoS && u.CombatRange <= 30 && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public virtual bool DarkTransformation ()
		{
			return Cast ("Dark Transformation",
				() =>
                    Usable ("Dark Transformation") && !Me.Pet.HasAura ("Dark Transformation") && Me.HasAlivePet &&
				Me.GetAura ("Shadow Infusion").StackCount == 5 &&
				(HasSpell ("Enhanced Dark Transformation") || (HasDeath || HasUnholy)));
		}

		public virtual bool BloodTap ()
		{
			return CastSelf ("Blood Tap", () => Usable ("Blood Tap") && BloodCharge >= 5);
		}

		public virtual bool DeathandDecay (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Death and Decay", u.Position, () => Usable ("Death and Decay") && (Me.HasAura ("Crimson Scourge") || (HasUnholy || HasDeath)) && u.IsInLoS && u.CombatRange <= 30);
		}

		public virtual bool SoulReaper (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper"), u);
		}

		public virtual bool ScourgeStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Scourge Strike", () => Usable ("Scourge Strike") && (HasUnholy || HasDeath), u);
		}

		public virtual bool DeathCoil (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Death Coil", () => Usable ("Death Coil") && (Me.HasAura ("Sudden Doom") || RunicPower >= 30) && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public virtual bool IcyTouch (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Icy Touch", () => Usable ("Icy Touch") && (HasFrost || HasDeath) && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public virtual bool PlagueLeech (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Plague Leech", () => Usable ("Plague Leech") && u.HasAura ("Frost Fever", true) && u.HasAura ("Blood Plague", true));
		}

		public virtual bool EmpowerRuneWeapon (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Empower Rune Weapon", () => Usable ("Empower Rune Weapon") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public virtual bool Outbreak (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Outbreak", () => Usable ("Outbreak") && (!HasGlyph (59332) || RunicPower >= 30) && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public virtual bool PlagueStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Plague Strike", () => Usable ("Plague Strike") && (HasUnholy || HasDeath), u);
		}

		public virtual bool FesteringStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Festering Strike", () => Usable ("Festering Strike") && ((HasFrost && HasBlood) || (HasFrost && HasDeath) || (HasDeath && HasBlood) || Death == 2), u);
		}

		public virtual bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Mind Freeze")) {
				if (InArena || InBg) {
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInCombatRangeAndLoS && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (MindFreeze (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInCombatRangeAndLoS && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (MindFreeze (CycleTarget))
							return true;
					}
				}
			}

			if (Usable ("Strangulate")) {
				if (InArena || InBg) {
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsHealer && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Strangulate (CycleTarget))
							return true;
					}
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Strangulate (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Strangulate (CycleTarget))
							return true;
					}
				}
			}

			if (Usable ("Asphyxiate")) {
				if (InArena || InBg) {
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsHealer && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Asphyxiate (CycleTarget))
							return true;
					}
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Asphyxiate (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Asphyxiate (CycleTarget))
							return true;
					}
				}
			}

			return false;
		}

		public virtual bool Heal ()
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
			if (Health (Me) < 0.3 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") &&
			    !Me.HasAura ("Vampiric Blood")) {
				if (IceboundFortitude ())
					return true;
			}
			if (Health (Me) < 0.8 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Icebound Fortitude") &&
			    !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood")) {
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
			return Cast ("Mind Freeze", () => Usable ("MindFreeze"), u);
		}

		public bool Strangulate (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Strangulate", () => Usable ("Strangulate") && (HasBlood || HasDeath) && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public virtual bool Asphyxiate (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Asphyxiate", () => Usable ("Asphyxiate") && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public virtual bool DeathSiphon (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Death Siphon", () => Usable ("Death Siphon") && HasDeath && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public virtual bool DeathStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Death Strike", () => Usable ("Death Strike") && ((HasFrost && HasUnholy) || (HasFrost && HasDeath) || (HasDeath && HasUnholy) || Death == 2), u);
		}

		public virtual bool BreathofSindragosa (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Breath of Sindragosa", () => Usable ("Breath of Sindragosa") && RunicPower > 0 && (IsElite (u) || IsPlayer (u)) && u.IsInLoS && u.CombatRange <= 10);
		}

		public virtual bool Lichborne ()
		{
			return CastSelf ("Lichborne", () => Usable ("Lichborne"));
		}

		public virtual bool ArmyoftheDead (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Army of the Dead", () => Usable ("Army of the Dead") && (IsPlayer (u) || IsBoss (u)) && ((HasBlood && HasFrost && HasUnholy) || (HasDeath && HasFrost && HasUnholy) || (HasBlood && HasDeath && HasUnholy) || (HasBlood && HasFrost && HasDeath) || ((HasBlood || HasFrost || HasUnholy) && Death >= 2) || Death >= 3));
		}

		public virtual bool VampiricBlood ()
		{
			return CastSelf ("Vampiric Blood", () => Usable ("Vampiric Blood"));
		}

		public virtual bool IceboundFortitude ()
		{
			return CastSelf ("Icebound Fortitude", () => Usable ("Icebound Fortitude"));
		}

		public virtual bool DancingRuneWeapon (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Dancing Rune Weapon", () => Usable ("Dancing Rune Weapon") && u.IsInLoS && u.CombatRange <= 30); 
		}

		public virtual bool DeathPact ()
		{
			return CastSelf ("Death Pact", () => Usable ("Death Pact"));
		}

		public virtual bool BoneShield ()
		{
			return CastSelf ("Bone Shield", () => Usable ("Bone Shield") && !Me.HasAura ("Bone Shield"));
		}

		public virtual bool ChainsofIce (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Chains of Ice", () => Usable ("Chains of Ice") && u.IsInLoS && u.CombatRange <= 30 && (HasFrost && HasDeath), u);
		}

		public bool RuneTap ()
		{
			return CastSelf ("Rune Tap", () => Usable ("Rune Tap") && (HasBlood || HasDeath));
		}

		public bool PillarofFrost ()
		{
			return CastSelf ("Pillar of Frost", () => Usable ("Pillar of Frost"));
		}
	}
}