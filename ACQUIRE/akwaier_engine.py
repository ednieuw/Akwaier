"""
Spel-engine voor AKWAIER  --  Windows-versie van het BBC BASIC-spel "aquire 23/05/87".

Deze module bevat uitsluitend de spelregels en de spelstand. Er zit geen enkele
verwijzing naar een schermweergave in; de UI-laag (akwaier.py) praat via de
methodes hieronder met de engine.

Herkomst van de regels: AQUIRE_detokenized.bas (BBC Micro, 1987).
De regels zijn 1-op-1 overgenomen, met de aantoonbare programmeerfouten uit het
origineel hersteld. Elke correctie staat gemarkeerd met "FIX:".
"""

from __future__ import annotations

import json
import os
import random
from dataclasses import dataclass, field

# ---------------------------------------------------------------- constanten

SIZE = 15                       # bord is 15 x 15
EMPTY = -1                      # leeg vakje            (in het origineel 99/-1)
LOOSE = 0                       # losse steen, nog geen keten
CHAINS = tuple(range(1, 9))     # acht ketens, 1..8
CHAIN_LETTERS = "ABCDEFGH"
NEIGHBOURS = ((1, 0), (-1, 0), (0, 1), (0, -1))   # DATA 1300 uit het origineel

BASE_PRICE = {1: 900, 2: 900, 3: 800, 4: 700, 5: 700, 6: 600, 7: 500, 8: 400}
PRICE_STEPS = (0, 1, 2, 3, 4, 9, 14, 19, 24, 35)  # DATA 1980

START_MONEY = 25_000
HAND_SIZE = 8                   # speelsteen%(8,6): acht stenen in de hand
NPLAYERS = 6
MAX_TURNS = 210                 # W% >= 210 -> einde
END_CHAIN_SIZE = 100            # keten groter dan 100 tegels -> einde

DEFAULT_NAMES = ("OPA", "OMA", "BROER", "ZUS", "PA", "MA")

SCORE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "akwaier_scores.json")


# ------------------------------------------------------------- hulpfuncties

def tile_pos(n: int) -> tuple[int, int]:
    """Steennummer 1..225 -> (kolom 1..15, rij 1..15), net als regel 2240/2250."""
    y = n % SIZE or SIZE
    x = n // SIZE or SIZE
    return x, y


def label(x: int, y: int) -> str:
    """(kolom, rij) -> "B7"-notatie: rijletter gevolgd door kolomnummer."""
    return f"{chr(64 + y)}{x}"


def chain_letter(c: int) -> str:
    return CHAIN_LETTERS[c - 1] if 1 <= c <= 8 else "?"


def bonus_factors(counts: list[int]) -> list[float]:
    """
    Verdeelsleutel van de fusiebonus over de zes spelers, gesorteerd op
    aandeelbezit (aflopend). Tabel uit regels 2810-2819.

    FIX: in het origineel stond de test B=C (2814) voor de tests B=C=D (2816),
    waardoor die laatste onbereikbaar was. Hier staan de tests van specifiek
    naar algemeen. FIX: de breuken .66/.33/.17/.11/.085/.061 waren afgeronde
    benaderingen die niet op 1 uitkwamen; hier staan de exacte breuken.
    """
    a, b, c, d, e, f = counts
    if b == 0:                                   # maar een aandeelhouder
        return [1.0, 0, 0, 0, 0, 0]
    if a == b == c:                              # drie gelijk aan kop
        return [1 / 3, 1 / 3, 1 / 3, 0, 0, 0]
    if a == b:                                   # twee gelijk aan kop
        return [0.5, 0.5, 0, 0, 0, 0]
    if b == c == d == e == f:                    # nummers 2 t/m 6 gelijk
        return [2 / 3] + [1 / 15] * 5
    if b == c == d == e:                         # nummers 2 t/m 5 gelijk
        return [2 / 3] + [1 / 12] * 4 + [0.0]
    if b == c == d:                              # nummers 2 t/m 4 gelijk
        return [2 / 3] + [1 / 9] * 3 + [0.0, 0.0]
    if b == c:                                   # nummers 2 en 3 gelijk
        return [2 / 3, 1 / 6, 1 / 6, 0, 0, 0]
    return [2 / 3, 1 / 3, 0, 0, 0, 0]            # gewone verdeling


# ----------------------------------------------------------------- datatypes

class IllegalMove(Exception):
    pass


@dataclass
class Player:
    number: int
    name: str
    human: bool = False
    money: int = START_MONEY
    shares: list[int] = field(default_factory=lambda: [0] * 9)   # index 1..8


@dataclass
class Move:
    """Uitkomst van het bekijken van een vakje, zonder iets te wijzigen."""
    x: int
    y: int
    legal: bool
    adj: list[int]                 # aanliggende ketens, ontdubbeld, in scanvolgorde
    loose: list[tuple[int, int]]   # aanliggende losse stenen
    winner: int                    # grootste aanliggende keten (0 = geen)
    tie: bool                      # meerdere even grote ketens -> speler kiest
    tied: list[int]

    @property
    def merges(self) -> bool:
        return len(self.adj) > 1


# --------------------------------------------------------------------- spel

class Game:
    """Volledige spelstand van een partij Akwaier."""

    def __init__(self, human_seat: int = 0, human_name: str = "", seed=None):
        self.rng = random.Random(seed)
        self.board = [[EMPTY] * (SIZE + 1) for _ in range(SIZE + 1)]
        self.sizes = {c: 0 for c in CHAINS}

        self.players = [Player(i, DEFAULT_NAMES[i - 1]) for i in range(1, NPLAYERS + 1)]
        self.human_seat = human_seat
        if human_seat:
            p = self.players[human_seat - 1]
            p.human = True
            p.name = (human_name.strip()[:8].upper() or "MENS")

        self.bag = list(range(1, SIZE * SIZE + 1))
        self.rng.shuffle(self.bag)

        # PROCsteen_pakken stopte zodra er nog 1 steen over was; die steen bleef
        # ongebruikt liggen. FIX: de zak wordt nu netjes leeggespeeld.
        self.hands: dict[int, list[int | None]] = {
            p.number: [self.draw() for _ in range(HAND_SIZE)] for p in self.players
        }

        self.turn = 0            # W%
        self.current = 0         # nog niemand aan de beurt
        self.round_passes = 0    # kanniet%: aantal spelers dat deze ronde paste
        self.finished = False
        self.end_reason = ""

    # ------------------------------------------------------------ bord/zak

    def draw(self) -> int | None:
        return self.bag.pop() if self.bag else None

    def _set(self, x: int, y: int, v: int) -> None:
        old = self.board[x][y]
        if old == v:
            return
        if old >= 1:
            self.sizes[old] -= 1
        self.board[x][y] = v
        if v >= 1:
            self.sizes[v] += 1

    def cell(self, x: int, y: int) -> int:
        if 1 <= x <= SIZE and 1 <= y <= SIZE:
            return self.board[x][y]
        return EMPTY

    def chain_size(self, c: int) -> int:
        """FNmax(): aantal tegels van een keten."""
        return self.sizes.get(c, 0)

    def available_chains(self) -> list[int]:
        """sticht%: de ketens die nog gesticht kunnen worden."""
        return [c for c in CHAINS if self.sizes[c] == 0]

    def existing_chains(self) -> list[int]:
        return [c for c in CHAINS if self.sizes[c] > 0]

    # ----------------------------------------------------------- geldzaken

    def price(self, c: int) -> int:
        """PROCbereken_prijs_keten (regel 1900)."""
        n = self.sizes.get(c, 0)
        if n == 0:
            return 0
        p = BASE_PRICE[c] + 100 * sum(1 for step in PRICE_STEPS if n > step)
        p += 10 * sum(pl.shares[c] for pl in self.players)
        return p

    def net_worth(self, pno: int) -> int:
        """PROCvermogen: banksaldo plus de waarde van alle aandelen."""
        pl = self.players[pno - 1]
        return pl.money + sum(self.price(c) * pl.shares[c] for c in CHAINS)

    def rank(self, c: int) -> list[int]:
        """
        PROCsort_aandelen: spelers gesorteerd op aandeelbezit in keten c,
        aflopend; bij gelijk bezit wint het hoogste spelernummer (10*bezit+nr).
        """
        return sorted(range(1, NPLAYERS + 1),
                      key=lambda q: (self.players[q - 1].shares[c], q),
                      reverse=True)

    # -------------------------------------------------------- zetten kijken

    def analyse(self, x: int, y: int) -> Move:
        """
        PROCcontroleer_aanliggen (regel 1200), maar zonder het bord aan te raken.

        FIX: het origineel las eerst de buur uit en controleerde daarna pas of
        die buiten het bord lag, en het las dan geheugen buiten de bordarray.
        FIX: kiezen% werd nooit teruggezet en bleef tussen beurten staan.
        FIX: aanliggende ketens werden dubbel geteld als twee buren tot dezelfde
        keten hoorden.
        """
        adj: list[int] = []
        loose: list[tuple[int, int]] = []
        for dx, dy in NEIGHBOURS:
            nx, ny = x + dx, y + dy
            if not (1 <= nx <= SIZE and 1 <= ny <= SIZE):
                continue
            v = self.board[nx][ny]
            if 1 <= v <= 8:
                if v not in adj:
                    adj.append(v)
            elif v == LOOSE:
                loose.append((nx, ny))

        best, winner, tied = 0, 0, []
        for c in adj:
            s = self.sizes[c]
            if s > best:
                best, winner, tied = s, c, [c]
            elif s == best:
                tied.append(c)

        # kanniet%: naast een losse steen leggen mag alleen als je ook een
        # bestaande keten raakt; anders zou er ongemerkt een keten ontstaan.
        legal = bool(adj) or not loose
        return Move(x, y, legal, adj, loose, winner, len(tied) > 1, tied)

    def can_found(self, x: int, y: int) -> bool:
        """PROCcontroleer_vrij (regel 2500): stichten mag op een vrij liggend vakje."""
        if self.board[x][y] != EMPTY:
            return False
        return all(self.cell(x + dx, y + dy) == EMPTY for dx, dy in NEIGHBOURS)

    def legal_slots(self, pno: int) -> list[int]:
        """Handposities (0..7) waarmee deze speler een geldige zet kan doen."""
        out = []
        for slot, t in enumerate(self.hands[pno]):
            if t is None:
                continue
            x, y = tile_pos(t)
            if self.analyse(x, y).legal:
                out.append(slot)
        return out

    def foundable_slots(self, pno: int) -> list[int]:
        if not self.available_chains():
            return []
        return [slot for slot, t in enumerate(self.hands[pno])
                if t is not None and self.can_found(*tile_pos(t))]

    # -------------------------------------------------------- zetten doen

    def _refill(self, pno: int, slot: int) -> None:
        self.hands[pno][slot] = self.draw()

    def found(self, pno: int, slot: int, chain: int) -> dict:
        """PROCstichten: losse steen wordt meteen een keten van 1, plus 1 gratis aandeel."""
        tile = self.hands[pno][slot]
        if tile is None:
            raise IllegalMove("geen steen op die plaats in de hand")
        x, y = tile_pos(tile)
        if not self.can_found(x, y):
            raise IllegalMove("dit vakje ligt niet vrij")
        if self.sizes.get(chain, 0) != 0:
            raise IllegalMove("die keten bestaat al")
        self._set(x, y, chain)
        self.players[pno - 1].shares[chain] += 1     # oprichtersaandeel
        self._refill(pno, slot)
        return {"kind": "found", "x": x, "y": y, "chain": chain}

    def place(self, pno: int, slot: int, winner: int | None = None) -> dict:
        """
        Een steen neerleggen. Levert een verslag op van wat er gebeurde.
        Combineert regels 3174-3210 (mens) en 3680-3710 (computer).
        """
        tile = self.hands[pno][slot]
        if tile is None:
            raise IllegalMove("geen steen op die plaats in de hand")
        x, y = tile_pos(tile)
        m = self.analyse(x, y)
        if not m.legal:
            raise IllegalMove("daar mag je niet leggen")

        info = {"kind": "place", "x": x, "y": y, "chain": 0,
                "absorbed": [], "payouts": [], "loose": len(m.loose)}

        if not m.adj:
            self._set(x, y, LOOSE)                   # losse steen, nog geen keten
        else:
            # FIX: in het origineel overschreef PROCcontroleer_aanliggen de door
            # de speler gekozen keten weer met de eigen berekening (regel 3202).
            w = winner if (winner in m.tied) else m.winner
            losers = [c for c in m.adj if c != w]

            # PROCverdeel_geld draait voor het omkleuren, zodat de bonus met de
            # ketengroottes van voor de fusie gerekend wordt.
            for lo in losers:
                info["payouts"].append(self._dissolve(lo, w))

            self._set(x, y, w)
            for lx, ly in m.loose:                   # aanliggende losse stenen mee
                self._set(lx, ly, w)
            if losers:
                for xx in range(1, SIZE + 1):
                    for yy in range(1, SIZE + 1):
                        if self.board[xx][yy] in losers:
                            self._set(xx, yy, w)
            info["chain"] = w
            info["absorbed"] = losers

        self._refill(pno, slot)
        return info

    def _dissolve(self, loser: int, winner: int) -> dict:
        """
        PROCverdeel (regel 2800): bonus uitkeren en aandelen half omruilen.

        FIX: de rangschikking werd uit een tabel gelezen die pas na de koopfase
        van de vorige speler was bijgewerkt; hier wordt hij vers berekend.
        """
        size = self.sizes[loser]
        pr = self.price(loser)
        ranked = self.rank(loser)
        counts = [self.players[q - 1].shares[loser] for q in ranked]
        outstanding = sum(counts)
        pot = (size + outstanding) * pr

        payout: dict[int, int] = {}
        for f, q in zip(bonus_factors(counts), ranked):
            amount = int(round(f * pot))
            if amount:
                self.players[q - 1].money += amount
                payout[q] = amount

        for pl in self.players:                      # helft omruilen in de winnaar
            pl.shares[winner] += pl.shares[loser] // 2
            pl.shares[loser] = 0

        return {"chain": loser, "into": winner, "size": size,
                "price": pr, "pot": pot, "payout": payout}

    def buy(self, pno: int, chain: int | None, count: int) -> int:
        """
        PROCkoopt (regel 9390): maximaal drie aandelen van een bestaande keten.

        FIX: de lus in het origineel kon eindeloos doordraaien bij een saldo van
        precies 0, en gebruikte > waar >= bedoeld was.
        """
        if not chain or count <= 0 or self.sizes.get(chain, 0) == 0:
            return 0
        pl = self.players[pno - 1]
        pr = self.price(chain)
        count = min(count, 3)
        while count > 0 and pl.money < pr * count:
            count -= 1
        if count <= 0:
            return 0
        pl.shares[chain] += count
        pl.money -= pr * count
        return count

    def passes(self, pno: int) -> None:
        self.round_passes += 1

    # ---------------------------------------------------------------- de AI

    def _fusion_value(self, m: Move, pno: int) -> int:
        """
        PROCbekijk_fusie (regel 7000): hoe aantrekkelijk is deze fusie voor mij?

        FIX: het origineel vergeleek een ketennummer met een ketengrootte
        (kleinere(N%) <> MAX%) en gaf ook punten aan spelers zonder aandelen.
        """
        best = 0
        for c in m.adj:
            ranked = self.rank(c)
            mine = self.players[pno - 1].shares[c]
            if mine > 0 and pno in ranked[:2]:
                v = 5 if c == m.winner else 10       # opgeslokte keten = bonus
            elif mine > 0:
                v = 2                                # klein belang, weinig winst
            else:
                v = 4
            best = max(best, v)
        return best

    def ai_choose_slot(self, pno: int) -> int | None:
        """PROCbepaal_gunstigste_legsteen (regel 5200)."""
        scored: list[tuple[int, int]] = []
        for slot, t in enumerate(self.hands[pno]):
            if t is None:
                scored.append((0, slot))
                continue
            m = self.analyse(*tile_pos(t))
            if not m.legal:
                v = 0
            elif not m.adj:
                v = 2
            else:
                v = self._fusion_value(m, pno)
            scored.append((v, slot))

        scored.sort(key=lambda s: (-s[0], s[1]))
        if scored[0][0] == 0:
            return None

        # FIX: regel 5332 wilde voorkomen dat een al erg grote keten nog groter
        # gemaakt wordt, maar gebruikte het handnummer als ketennummer.
        for i, (v, slot) in enumerate(scored):
            if v == 0:
                break
            m = self.analyse(*tile_pos(self.hands[pno][slot]))
            if (m.winner and self.sizes[m.winner] > 15 and v < 9
                    and any(vv > 0 for vv, _ in scored[i + 1:])):
                continue
            return slot
        return scored[0][1]

    def ai_choose_purchase(self, pno: int) -> int | None:
        """
        PROCkopen (regel 9180): koop daar waar je positie bedreigd wordt.

        FIX: de rangordebewerking zat in het origineel in een decimaal
        ingepakt getal en regel 9275 had een operatorvolgorde-fout waardoor
        niet-bestaande ketens gekocht konden worden.
        """
        existing = self.existing_chains()
        if not existing:
            return None

        candidates: list[int] = []
        for c in existing:
            ranked = self.rank(c)
            counts = [self.players[q - 1].shares[c] for q in ranked]
            for n in range(3):                       # alleen plek 1, 2 of 3
                if ranked[n] != pno:
                    continue
                if counts[n + 1] + 4 > counts[n]:    # iemand zit vlak achter me
                    candidates.append(c)
                elif n + 2 < NPLAYERS and counts[n + 1] == counts[n + 2]:
                    candidates.append(c)
                break
            if len(candidates) >= 4:
                break

        for c in candidates:
            ranked = self.rank(c)
            counts = [self.players[q - 1].shares[c] for q in ranked]
            if ranked[0] == pno and counts[0] - counts[1] > 4:
                continue                             # onbedreigd eerste, sla over
            return c
        return self.rng.choice(existing)

    def ai_turn(self, pno: int) -> list[str]:
        """PROCcomp (regel 3582): volledige beurt van een computerspeler."""
        pl = self.players[pno - 1]
        lines: list[str] = []

        founded = False
        if self.available_chains():
            for slot in self.foundable_slots(pno):
                chain = self.rng.choice(self.available_chains())
                r = self.found(pno, slot, chain)
                lines.append(f"{pl.name} sticht keten {chain_letter(chain)} "
                             f"op {label(r['x'], r['y'])}")
                founded = True
                break

        if not founded:
            slot = self.ai_choose_slot(pno)
            if slot is None:
                self.passes(pno)
                lines.append(f"{pl.name} kan niet")
                return lines
            r = self.place(pno, slot)
            lines.extend(self._describe(pl.name, r))

        chain = self.ai_choose_purchase(pno)
        n = self.buy(pno, chain, 3)
        if n:
            lines.append(f"{pl.name} koopt {n} x {chain_letter(chain)} "
                         f"a {self.price(chain)}")
        else:
            lines.append(f"{pl.name} koopt niets")
        return lines

    def _describe(self, name: str, r: dict) -> list[str]:
        """Zetverslag omzetten naar leesbare regels."""
        pos = label(r["x"], r["y"])
        lines = []
        if r["chain"] == 0:
            lines.append(f"{name} legt {pos} (losse steen)")
        else:
            lines.append(f"{name} legt {pos} bij keten {chain_letter(r['chain'])}")
        for p in r["payouts"]:
            weg = chain_letter(p["chain"])
            heen = chain_letter(p["into"])
            lines.append(f"  keten {weg} ({p['size']} tegels) gaat op in {heen}, "
                         f"bonus {p['pot']}")
            for q, amount in sorted(p["payout"].items()):
                lines.append(f"    {self.players[q - 1].name}: +{amount}")
        return lines

    # ------------------------------------------------------------ beurtloop

    def next_player(self) -> int | None:
        """
        PROCspelen (regel 4000). Levert het nummer van de speler die aan de beurt
        is, of None als het spel afgelopen is.

        FIX: de test 'iedereen kon niet' stond in het origineel boven in de lus
        en werd elke ronde teruggezet voordat hij ooit 6 kon bereiken.
        """
        if self.finished:
            return None

        if self.current == 0:
            self.current, self.round_passes = 1, 0
        elif self.current >= NPLAYERS:
            if self.round_passes >= NPLAYERS:
                return self._finish("niemand kan nog een steen kwijt")
            self.current, self.round_passes = 1, 0
        else:
            self.current += 1

        self.turn += 1
        if self.turn >= MAX_TURNS:
            return self._finish(f"{MAX_TURNS} beurten gespeeld")
        for c in CHAINS:
            if self.sizes[c] > END_CHAIN_SIZE:
                return self._finish(f"keten {chain_letter(c)} is groter dan "
                                    f"{END_CHAIN_SIZE} tegels")
        return self.current

    def _finish(self, reason: str) -> None:
        self.finished = True
        self.end_reason = reason
        return None

    def standings(self) -> list[tuple[int, str, int]]:
        rows = [(p.number, p.name, self.net_worth(p.number)) for p in self.players]
        rows.sort(key=lambda r: -r[2])
        return rows


# ----------------------------------------------------------- highscorelijst

def load_scores(path: str = SCORE_FILE) -> list[dict]:
    """Vervangt het originele bestand "SCORE" door een leesbaar JSON-bestand."""
    try:
        with open(path, "r", encoding="utf-8") as fh:
            data = json.load(fh)
        return [d for d in data if isinstance(d, dict) and "naam" in d and "vermogen" in d]
    except (OSError, ValueError):
        return []


def save_scores(new_rows: list[tuple[str, int]], path: str = SCORE_FILE,
                keep: int = 30) -> list[dict]:
    rows = load_scores(path)
    rows.extend({"naam": n, "vermogen": int(v)} for n, v in new_rows)
    rows.sort(key=lambda d: -d["vermogen"])
    rows = rows[:keep]
    try:
        with open(path, "w", encoding="utf-8") as fh:
            json.dump(rows, fh, ensure_ascii=False, indent=1)
    except OSError:
        pass
    return rows
