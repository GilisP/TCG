using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace TCG
{
    public static class CardAuthoring
    {
        public static List<string> Validate(CardDefinition c,Func<string,CardDefinition> resolve = null)
        {
            var errors = new List<string>();
            if (c == null) { errors.Add("Carta ausente."); return errors; }
            if (string.IsNullOrWhiteSpace(c.Id) || c.Id.Length > 100) errors.Add("ID inválido.");
            if (string.IsNullOrWhiteSpace(c.Name) || c.Name.Length > 64) errors.Add("Nome obrigatório: até 64 caracteres.");
            if (c.Text == null || c.Text.Length > 1500) errors.Add("Descrição: até 1500 caracteres.");
            if (!Enum.IsDefined(typeof(CardKind),c.Kind) || !Enum.IsDefined(typeof(Trait),c.Trait)) errors.Add("Tipo ou característica inválida.");
            if (c.Element < 0 || c.Element > 6 || c.Cost < 0 || c.Cost > 20) errors.Add("Elemento ou custo genérico inválido.");
            if (c.ColoredCost == null || c.ColoredCost.Length != 6 || c.ColoredCost.Any(v => v < 0 || v > 20)) errors.Add("Informe seis custos elementais entre 0 e 20.");
            if (c.IsUnit && (c.Life < 1 || c.Life > 50 || c.Attack < 0 || c.Attack > 30 || c.Movement < 0 || c.Movement > 10 || c.Range < 1 || c.Range > 10)) errors.Add("Atributos da criatura fora dos limites.");
            if (c.Housing < 0 || c.Housing > 6 || c.HousingUse < 1 || c.HousingUse > 6 || c.ActivationCost < 0 || c.ActivationCost > 20 || c.ExtraMana < 0 || c.ExtraMana > 5) errors.Add("Habitação, geração ou custo de ativação inválidos.");
            if (c.EquipmentAttack < 0 || c.EquipmentAttack > 10 || c.EquipmentArmor < 0 || c.EquipmentArmor > 5 || c.EquipmentMovement < 0 || c.EquipmentMovement > 5) errors.Add("Bônus de equipamento fora dos limites.");
            if (c.Steps.Length > 8) errors.Add("Máximo de 8 efeitos por carta.");
            if (c.BuildingLife < 1 || c.BuildingLife > 50 || c.Seats < 1 || c.Seats > 6 || c.CrewRequired < 1 || c.CrewRequired > c.Seats) errors.Add("Vida da construção ou assentos/tripulação inválidos.");
            if (!Enum.IsDefined(typeof(TerrainTrigger),c.AutomaticTrigger)) errors.Add("Gatilho automático inválido.");
            if (c.AutomaticEffects == null || c.AutomaticEffects.Length > 8) errors.Add("Máximo de oito efeitos automáticos.");
            if (c.Kind != CardKind.Terrain && (c.AutomaticTrigger != TerrainTrigger.None || (c.AutomaticEffects?.Length ?? 0) > 0)) errors.Add("Somente terrenos têm gatilhos automáticos.");
            if (c.AutomaticTrigger == TerrainTrigger.None && (c.AutomaticEffects?.Length ?? 0) > 0) errors.Add("Escolha quando ativar os efeitos automáticos.");
            if(c.ManaColors == null || c.ManaColors.Length > 6 || c.ManaColors.Any(e=>e<0||e>6) || c.ManaColors.Distinct().Count()!=c.ManaColors.Length) errors.Add("Cores de mana do terreno inválidas.");
            if(c.Kind != CardKind.Terrain && (c.ManaColors?.Length ?? 0) > 0) errors.Add("Somente terrenos possuem cores de geração.");
            if(!string.IsNullOrEmpty(c.Illustration) && !GeneratedArt.Keys.Contains(c.Illustration)) errors.Add("Ilustração desconhecida: escolha uma arte disponível.");
            var chosen = new HashSet<EffectTarget>();
            foreach (var s in c.Steps.Concat(c.AutomaticEffects ?? Array.Empty<EffectStep>()))
            {
                if (s == null) { errors.Add("Efeito ausente."); continue; }
                if (!Enum.IsDefined(typeof(Effect),s.Operation) || s.Operation == Effect.None || !AllowedTargets(s.Operation).Contains(s.Target)) errors.Add("Alvo incompatível com "+s.Operation+".");
                if (!AllowedDurations(s.Operation).Contains(s.Duration)) errors.Add("Duração incompatível com "+s.Operation+".");
                if (s.Value < 0 || s.Value > 30 || s.Element < 0 || s.Element > 6) errors.Add("Valor ou elemento do efeito inválido.");
                bool pick = s.Target == EffectTarget.AllyUnit || s.Target == EffectTarget.EnemyUnit || s.Target == EffectTarget.AnyUnit || s.Target == EffectTarget.EmptyOwnedTile;
                if (pick) chosen.Add(s.Target);
                if ((c.IsUnit || c.Kind == CardKind.Building || c.Kind == CardKind.Terrain) && (pick || s.Target == EffectTarget.Stack)) errors.Add("Permanentes usam efeitos no jogador, ocupante/fonte ou todos os aliados.");
                if (!c.IsUnit && c.Kind != CardKind.Building && c.Kind != CardKind.Terrain && s.Target == EffectTarget.Source) errors.Add("Este tipo de carta não possui criatura fonte.");
                if (s.Operation == Effect.SummonToken && (resolve ?? Catalog.Get)(s.TokenId)?.Kind != CardKind.Token) errors.Add("Escolha uma ficha existente.");
            }
            if (chosen.Count > 1 || (chosen.Count > 0 && c.Steps.Any(s => s != null && s.Target == EffectTarget.Stack))) errors.Add("Cada carta pode escolher apenas um tipo de alvo.");
            if (c.Kind == CardKind.Token && c.Steps.Length > 0) errors.Add("Fichas criadas por efeitos não disparam habilidades de entrada nesta versão.");
            if (c.Kind == CardKind.Equipment && c.Steps.Length > 0) errors.Add("Equipamentos usam os três campos de bônus; remova os efeitos adicionais.");
            if (c.Kind == CardKind.Terrain && c.TotalCost != 0) errors.Add("Terrenos não têm custo para colocação; use custo de ativação.");
            return errors.Distinct().ToList();
        }
        public static EffectTarget[] AllowedTargets(Effect op)
        {
            if (op == Effect.Draw || op == Effect.GainMana || op == Effect.Recover || op == Effect.GainPopularity) return new[]{EffectTarget.Owner};
            if (op == Effect.Discard) return new[]{EffectTarget.EnemyPlayer,EffectTarget.Owner};
            if (op == Effect.Counter) return new[]{EffectTarget.Stack};
            if (op == Effect.SummonToken) return new[]{EffectTarget.EmptyOwnedTile};
            var list = new List<EffectTarget>{EffectTarget.EnemyUnit,EffectTarget.AllyUnit,EffectTarget.AnyUnit,EffectTarget.Source,EffectTarget.AllAllies,EffectTarget.AllEnemies};
            if(op == Effect.Damage) list.Add(EffectTarget.EnemyPlayer);
            if (op == Effect.Heal) list.Add(EffectTarget.Owner);
            return list.ToArray();
        }
        public static EffectDuration[] AllowedDurations(Effect op) => op == Effect.Buff || op == Effect.Weaken ? new[]{EffectDuration.EndOfTurn} :
            op == Effect.Shield ? new[]{EffectDuration.EndOfTurn,EffectDuration.Permanent} : op == Effect.Stun ? new[]{EffectDuration.ThroughNextControllerTurn} : new[]{EffectDuration.Instant};
        public static CardDefinition Copy(CardDefinition c)
        {
            // Materialize legacy effects before serializing so editing never changes an original definition.
            var clone = JsonUtility.FromJson<CardDefinition>(JsonUtility.ToJson(c));
            clone.Effect = Effect.None;
            clone.Effects = c.Steps.Select(s => new EffectStep(s.Operation,s.Target,s.Value,s.Duration){Element=s.Element,TokenId=s.TokenId}).ToArray();
            return clone;
        }
    }
    [Serializable] public sealed class CustomCardFile { public int version = 1; public CardDefinition card; }
    public static class CustomCardStore
    {
        public static string Serialize(CardDefinition card,Func<string,CardDefinition> resolve = null)
        {
            var errors = CardAuthoring.Validate(card,resolve);
            if (card == null || string.IsNullOrEmpty(card.Id) || !card.Id.StartsWith("custom-") || card.Id.Any(c => !char.IsLetterOrDigit(c) && c != '-')) errors.Add("ID personalizado inválido.");
            if (errors.Count > 0) throw new InvalidDataException(string.Join("\n",errors));
            return JsonUtility.ToJson(new CustomCardFile { card=CardAuthoring.Copy(card) },true);
        }
        public static CardDefinition Parse(string json)
        {
            if (json == null || json.Length > 100000) throw new InvalidDataException("Arquivo de carta inválido ou muito grande.");
            var file = JsonUtility.FromJson<CustomCardFile>(json);
            if (file == null || file.version != 1 || file.card == null) throw new InvalidDataException("Versão de carta inválida.");
            Serialize(file.card); return file.card;
        }
        public static void Save(string directory,CardDefinition card)
        {
            string json = Serialize(card); Directory.CreateDirectory(directory);
            string path = Path.Combine(directory,card.Id+".json"), temp = path+".tmp";
            File.WriteAllText(temp,json);
            if (File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
        }
        static bool IsToken(string json)
        { try { return JsonUtility.FromJson<CustomCardFile>(json)?.card?.Kind == CardKind.Token; } catch { return false; } }
        public static void LoadResources()
        {
            var assets = Resources.LoadAll<TextAsset>("TCGCards");
            // Load custom tokens first so dependent summon effects can validate deterministically.
            foreach (var asset in assets.OrderBy(a => IsToken(a.text) ? 0 : 1))
                try { Catalog.RegisterCustom(Parse(asset.text)); }
                catch (Exception e) { Debug.LogError("Carta personalizada ignorada: "+asset.name+" / "+e.Message); }
            DeckPackages.LoadInstalled();
        }
    }
}
