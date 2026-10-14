using System;
using System.Linq;
using TCG.Foundation;
using TCG.Table;

public static class KeywordExpansionChecks
{
    static int checks, nextId = 81000;
    static void Assert(bool result, string label) { checks++; if (!result) throw new Exception(label); }
    static void Do(Match m, ActionKind kind, int target = -1, int unit = -1, string card = null)
    {
        if (!m.Try(new Command { player = m.Controller, revision = m.Revision, kind = kind,
            target = target, unit = unit, card = card, pile = 0 }, out var error)) throw new Exception(kind + ": " + error);
    }
    static Match Game(ContentCatalog catalog)
    {
        var main = new[] { Enumerable.Repeat("MED-085", 100).ToArray(), Enumerable.Repeat("MED-085", 100).ToArray() };
        var lands = new[] { Enumerable.Repeat("test-land-0", 50).ToArray(), Enumerable.Repeat("test-land-0", 50).ToArray() };
        var m = new Match(catalog, new EffectRegistry(), 2, false, 5, main, lands);
        Do(m, ActionKind.DrawMain); Do(m, ActionKind.Place, Match.Capital(m.Active, 2)); Do(m, ActionKind.NextPhase);
        for (int at = 0; at < 121; at++) { m.Board[at].Terrain = catalog.Get("test-land-0"); m.Board[at].Owner = m.Active; }
        for (int color = 0; color < 7; color++) m.Seats[m.Active].mana[color] = 100;
        return m;
    }
    static Piece Add(Match m, int owner, int at, bool resistant, bool anchored = false)
    {
        var data = new CardData { id = "keyword-test-piece", name = "Keyword fixture", kind = "Creature",
            attack = 2, defense = 8, actions = 2, movement = 2, range = 1,
            keywords = (resistant ? new[] { KeywordRules.MagicResistance } : Array.Empty<string>())
                .Concat(anchored ? new[] { KeywordRules.Anchored } : Array.Empty<string>()).ToArray() };
        var p = new Piece(nextId++, owner, new Definition(data, "isolated-keyword-fixtures")) { Game = m };
        m.Board[at].pieces.Add(p); return p;
    }
    static void Settle(Match m)
    {
        int guard = 0;
        while (m.Stack.Count > 0 || m.Choice != null)
        {
            if (++guard > 100) throw new Exception("Keyword fixture did not settle");
            if (m.Choice != null) Do(m, ActionKind.Choose, m.Choice.Options.First(x => x.Key >= 0).Key);
            else Do(m, ActionKind.Pass);
        }
    }
    [UnityEditor.MenuItem("TCG/Validar resistência à magia")]
    public static void Run() { if (Main() != 0) throw new Exception("Keyword expansion checks failed"); }
    public static int Main()
    {
        try
        {
            checks = 0; var catalog = ContentLoader.LoadFixtures(new EffectRegistry());
            Assert(MedievalRules.Valid("", new[] { KeywordRules.MagicResistance }), "schema accepts approved resistance");
            var m = Game(catalog); int own = m.Active, enemy = 1 - own;
            var resistant = Add(m, enemy, 61, true); var ordinary = Add(m, enemy, 62, false); var ally = Add(m, own, 60, true);
            Assert(!m.CanTargetByEffect(own, resistant), "opponent targeting forbidden");
            Assert(m.CanTargetByEffect(enemy, resistant), "controller targeting preserved");
            Assert(m.CanTargetByEffect(own, ordinary), "ordinary target preserved");
            var targetPredicate=typeof(Match).GetMethod("AbilityTarget",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert((bool)targetPredicate.Invoke(m,new object[]{new CardAbility{target="subject"},new Pending{Owner=own,Source=ally,Subject=resistant},resistant,false}),"automatic event subject is not a selected target");
            Assert(m.CanMove(ally.Id, 59), "resistance does not block movement");
            Assert(m.CanAttack(ally.Id, 61), "resistance does not block attack");
            m.Deal(resistant, 2); Assert(resistant.Damage == 2, "untargeted damage is not reduced");
            var spell = new Definition(new CardData { id = "keyword-test-damage", name = "Damage fixture", kind = "Spell",
                effects = new[] { new EffectData { operation = "damage", amount = 1 } } }, "isolated-keyword-fixtures");
            m.Seats[own].hand.Add(spell);
            Assert(!m.CanPlay(spell, 61, resistant.Id), "host rejects declared spell target");
            Assert(m.CanPlay(spell, 62, ordinary.Id), "host permits ordinary declared spell target");
            Do(m, ActionKind.Play, 62, ordinary.Id, spell.Id);
            ordinary.Modifiers.Add(new Modifier { Keyword = KeywordRules.MagicResistance });
            Settle(m); Assert(ordinary.Damage == 0, "resolution rechecks acquired resistance");
            var chosen = new Definition(new CardData { id = "keyword-test-chosen", name = "Chosen damage fixture", kind = "Spell", rule = "abilities",
                abilities = new[] { new CardAbility { trigger = "cast", target = "enemy", ops = new[] { new AbilityOp { kind = "damage", a = 1 } } } } }, "isolated-keyword-fixtures");
            var available = Add(m, enemy, 63, false); m.Seats[own].hand.Add(chosen); Do(m, ActionKind.Play, card: chosen.Id);
            int passes = 0; while (m.Choice == null && m.Stack.Count > 0 && passes++ < 10) Do(m, ActionKind.Pass);
            Assert(m.Choice != null && m.Choice.Options.Any(x => x.Key == available.Id), "effect exposes legal target");
            Assert(!m.Choice.Options.Any(x => x.Key == resistant.Id || x.Key == ordinary.Id), "effect choice excludes resistant targets");
            Settle(m); Assert(available.Damage == 1, "legal chosen effect resolves");
            var bounce=typeof(Match).GetMethod("PreconBounce",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            bounce.Invoke(m,new object[]{own,new AbilityOp{a=20,b=1},1,(Action)(()=>{})});
            typeof(Match).GetMethod("Drain",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(m,null);
            Assert(m.Choice!=null&&m.Choice.Options.Any(x=>x.Key==available.Id),"bounce exposes ordinary target");
            Assert(!m.Choice.Options.Any(x=>x.Key==resistant.Id||x.Key==ordinary.Id),"bounce excludes resistant opponent");
            Do(m,ActionKind.Choose,available.Id);Assert(m.Find(available.Id)==null,"legal bounce resolves");
            AnchorChecks(catalog);
            DefenseChecks(catalog);
            UnityEngine.Debug.Log("KeywordExpansionChecks: " + checks + " checks passed"); return 0;
        }
        catch (Exception error) { UnityEngine.Debug.LogException(error); return 1; }
    }

    static void AnchorChecks(ContentCatalog catalog)
    {
        var m = Game(catalog); int own = m.Active, enemy = 1 - own;
        var anchored = Add(m, own, 60, false, true);
        Assert(MedievalRules.Valid("", new[] { KeywordRules.Anchored }), "schema accepts anchored");
        Assert(!m.CanMoveByEffect(enemy, anchored), "hostile effect cannot move anchored piece");
        Assert(m.CanMoveByEffect(own, anchored), "own effect may move anchored piece");
        Assert(m.CanTargetByEffect(enemy, anchored), "anchored does not restrict targeting");
        Assert(m.CanMove(anchored.Id, 59), "anchored permits manual movement");
        var move = typeof(Match).GetMethod("ApplyDisplacement", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        move.Invoke(m, new object[] { enemy, anchored, 59, null });
        Assert(m.Position(anchored.Id) == 60, "enemy displacement leaves anchored piece in place");
        move.Invoke(m, new object[] { own, anchored, 59, null });
        Assert(m.Position(anchored.Id) == 59, "own displacement moves anchored piece");
        m.Deal(anchored, 2); Assert(anchored.Damage == 2, "anchored does not reduce damage");
        var passenger = Add(m, own, 60, false, true);
        var vehicle = new Piece(nextId++, own, new Definition(new CardData { id = "keyword-test-vehicle", name = "Anchored carrier fixture",
            kind = "Artifact", vehicleSeats = 2, vehicleCrew = 1, attack = 2, defense = 8, actions = 2, movement = 2 }, "isolated-keyword-fixtures")) { Game = m };
        m.Board[60].pieces.Add(vehicle); passenger.CarrierId = vehicle.Id;
        Assert(!m.CanMoveByEffect(enemy, vehicle), "anchored passenger blocks whole hostile displacement");
        move.Invoke(m, new object[] { enemy, vehicle, 61, null });
        Assert(m.Position(vehicle.Id) == 60 && m.Position(passenger.Id) == 60, "hostile displacement preserves vehicle and passenger");
        Assert(m.CanMove(vehicle.Id, 61), "anchored passenger permits manual carrier movement");
        move.Invoke(m, new object[] { own, vehicle, 61, null });
        Assert(m.Position(vehicle.Id) == 61 && m.Position(passenger.Id) == 61, "own displacement carries anchored passenger");
        var valeria = new Piece(nextId++, own, new Definition(new CardData { id = "keyword-test-valeria", name = "Valeria inheritance fixture",
            kind = "Creature", rule = "valeria", attack = 2, defense = 8 }, "isolated-keyword-fixtures")) { Game = m };
        m.Board[60].pieces.Add(valeria);
        Assert(m.EffectiveKeywords(vehicle.Id).Contains(KeywordRules.Anchored) && !m.CanMoveByEffect(enemy, vehicle), "Valeria inherits anchored without recursion");
        var hostile = Add(m, enemy, 63, false, true);
        var bounce = typeof(Match).GetMethod("PreconBounce", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        bounce.Invoke(m, new object[] { own, new AbilityOp { a = 20, b = 1 }, 1, (Action)(() => { }) });
        typeof(Match).GetMethod("Drain",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(m,null);
        Assert(m.Choice != null && m.Choice.Options.Any(x => x.Key == hostile.Id), "anchored remains eligible for bounce");
        Do(m, ActionKind.Choose, hostile.Id); Assert(m.Find(hostile.Id) == null, "anchored bounce resolves normally");
        var protectedEnemy=Add(m,enemy,63,false,true);
        var swap=new Definition(new CardData{id="keyword-terrain-swap",name="Terrain swap fixture",kind="Instant",rule="teletransport"},"isolated-keyword-fixtures");
        m.Seats[own].hand.Add(swap);Do(m,ActionKind.Play,card:swap.Id);
        int passes=0;while(m.Choice==null&&m.Stack.Count>0&&passes++<10)Do(m,ActionKind.Pass);
        Assert(m.Choice!=null&&!m.Choice.Options.Any(x=>x.Key==63),"host terrain options exclude anchored opponent");
        Assert(m.Choice.Options.Any(x=>x.Key==59),"own anchored terrain remains selectable");
        Assert(!m.Try(new Command{player=m.Controller,revision=m.Revision,kind=ActionKind.Choose,target=63},out _)&&m.Choice!=null&&m.Position(protectedEnemy.Id)==63,"forged terrain choice cannot bypass anchored");
        Do(m,ActionKind.Choose,59);Assert(!m.Choice.Options.Any(x=>x.Key==63),"second terrain choices also respect anchored");
        Do(m,ActionKind.Choose,58);Settle(m);Assert(m.Position(anchored.Id)==58,"own terrain swap remains functional");
    }

    static Piece DefensePiece(Match m, int owner, int at, params string[] keywords)
    {
        var p = new Piece(nextId++, owner, new Definition(new CardData { id = "keyword-test-defense", name = "Numeric defense fixture",
            kind = "Creature", attack = 5, defense = 30, actions = 2, movement = 2, range = 1, keywords = keywords }, "isolated-keyword-fixtures")) { Game = m };
        m.Board[at].pieces.Add(p); return p;
    }
    static void DefenseChecks(ContentCatalog catalog)
    {
        Assert(NumericKeywordRules.TryParse("armor:2", out var kind, out var n) && kind == "armor" && n == 2, "numeric keyword parses explicitly");
        foreach (var invalid in new[] { "armor:0", "armor:-1", "armor:+1", "armor: 2", "Armor:2", "armor:1001", "barrier:2x", "tenacity:999999999999" })
            Assert(!NumericKeywordRules.Valid(invalid), "numeric keyword rejects " + invalid);
        Assert(MedievalRules.Valid("", new[] { "armor:2", "tenacity:3", "barrier:4" }), "schema accepts explicit numeric defenses");
        var m = Game(catalog); int own = m.Active, enemy = 1 - own;
        var armor = DefensePiece(m, enemy, 61, "armor:2", "armor:1");
        m.Deal(armor, 5); Assert(armor.Damage == 5, "armor leaves effect damage unchanged"); armor.Damage = 0;
        var attacker = DefensePiece(m, own, 60);
        var combat = typeof(Match).GetMethod("ResolveCombat", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        combat.Invoke(m, new object[] { new Pending { Owner = own, Target = 61, Attackers = new[] { attacker.Id }, Blockers = new[] { armor.Id } } });
        Assert(armor.Damage == 3, "actual combat applies largest armor only");
        var tough = DefensePiece(m, enemy, 62, "tenacity:2", "tenacity:1");
        m.Deal(tough, 5); Assert(tough.Damage == 3, "tenacity reduces effect damage using largest value");
        tough.Damage = 0; m.Deal(tough, 1); Assert(tough.Damage == 0, "reduction floors damage at zero");
        var combo = DefensePiece(m, enemy, 61, "armor:2", "armor:1", "tenacity:1");
        combat.Invoke(m, new object[] { new Pending { Owner = own, Target = 61, Attackers = new[] { attacker.Id }, Blockers = new[] { combo.Id } } });
        Assert(combo.Damage == 2, "actual combat sums largest armor and largest tenacity");
        var barrier = DefensePiece(m, enemy, 64, "barrier:4", "tenacity:2");
        Assert(barrier.BarrierRemaining == 4, "printed barrier starts full");
        m.Deal(barrier, 0); Assert(barrier.BarrierRemaining == 4, "zero damage consumes no barrier");
        m.Deal(barrier, 2); Assert(barrier.BarrierRemaining == 4, "reduction precedes barrier consumption");
        m.Deal(barrier, 5); Assert(barrier.Damage == 0 && barrier.BarrierRemaining == 1, "barrier consumes reduced damage");
        m.Deal(barrier, 4); Assert(barrier.Damage == 1 && barrier.BarrierRemaining == 0, "excess damage passes exhausted barrier");
        m.Deal(barrier, 4); Assert(barrier.Damage == 3, "later event does not refresh barrier");
        var dynamic = DefensePiece(m, enemy, 65);
        var grant = new Modifier { Keyword = "barrier:4" }; dynamic.Modifiers.Add(grant);
        m.Deal(dynamic, 3); Assert(dynamic.BarrierRemaining == 1, "granted barrier tracks spent points");
        dynamic.Modifiers.Remove(grant); Assert(dynamic.BarrierRemaining == 0, "removed barrier unavailable");
        dynamic.Modifiers.Add(grant); Assert(dynamic.BarrierRemaining == 1, "regrant does not refill consumed barrier");
        dynamic.Modifiers.Add(new Modifier { Keyword = "barrier:6" }); Assert(dynamic.BarrierRemaining == 3, "increased maximum preserves cumulative spent points");
        var passenger = DefensePiece(m, own, 70, "barrier:4");
        var carrier = new Piece(nextId++, own, new Definition(new CardData { id = "keyword-test-barrier-carrier", name = "Barrier carrier fixture",
            kind = "Artifact", defense = 30, vehicleSeats = 2, vehicleCrew = 1 }, "isolated-keyword-fixtures")) { Game = m };
        m.Board[70].pieces.Add(carrier); passenger.CarrierId = carrier.Id;
        var valeria = new Piece(nextId++, own, new Definition(new CardData { id = "keyword-test-defense-valeria", name = "Defense inheritance fixture",
            kind = "Creature", rule = "valeria", defense = 30 }, "isolated-keyword-fixtures")) { Game = m };
        m.Board[70].pieces.Add(valeria);
        m.Deal(carrier, 2); Assert(carrier.BarrierRemaining == 2 && carrier.Damage == 0, "Valeria inherited barrier consumes on vehicle");
        passenger.CarrierId = -1; Assert(carrier.BarrierRemaining == 0, "removed passenger removes inherited barrier");
        passenger.CarrierId = carrier.Id; Assert(carrier.BarrierRemaining == 2, "passenger re-inheritance does not refill barrier");
        var view = m.ViewFor(own);
        var wire = UnityEngine.JsonUtility.ToJson(view); var decoded = UnityEngine.JsonUtility.FromJson<MatchView>(wire);
        var projected = Match.FromView(decoded, catalog, new EffectRegistry());
        Assert(projected.Find(dynamic.Id).BarrierRemaining == 3 && projected.Find(barrier.Id).BarrierRemaining == 0, "public projection preserves barrier remaining through serialization");
        Assert(projected.Find(barrier.Id).Damage == barrier.Damage, "projection preserves effective damage");
        int beforeTurn = dynamic.BarrierSpent; typeof(Match).GetProperty("Turn").SetValue(m, m.Turn + 1);
        Assert(dynamic.BarrierSpent == beforeTurn && dynamic.BarrierRemaining == 3, "turn change does not refresh barrier");
    }
}
