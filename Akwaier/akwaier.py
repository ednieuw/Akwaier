"""
AKWAIER  --  Windows 11-versie van het BBC BASIC-spel "aquire 23/05/87".

Starten:  dubbelklik op START_AKWAIER.bat, of vanaf de opdrachtregel:

    python akwaier.py

Alleen tkinter uit de standaardbibliotheek is nodig; er hoeft niets
geinstalleerd te worden.

De spelregels zitten volledig in akwaier_engine.py. Dit bestand tekent alleen
het scherm en verwerkt de muisklikken. Er zijn twee weergaven, om te wisselen
met de knop rechtsboven:

    Modern     -- rustige kleuren, dikke vakjes, muisbediening
    Retro BBC  -- de kleuren en indeling van het originele BBC Micro-scherm
"""

from __future__ import annotations

import tkinter as tk
from tkinter import font as tkfont

from akwaier_engine import (
    CHAINS, EMPTY, LOOSE, NPLAYERS, SIZE, Game,
    chain_letter, label, load_scores, save_scores, tile_pos,
)

CELL = 33
MARGIN = 26
BOARD_PX = MARGIN * 2 + CELL * SIZE

# ------------------------------------------------------------------- skins

THEMES = {
    "modern": {
        "naam": "Modern",
        "bg": "#eceff1", "panel": "#ffffff", "fg": "#212121", "muted": "#78909c",
        "grid": "#cfd8dc", "empty": "#f5f7f8", "dot": "#dbe2e6",
        "loose": "#90a4ae", "loose_fg": "#ffffff",
        "head_bg": "#37474f", "head_fg": "#ffffff",
        "band": "#e3e8ea", "sel": "#ff6f00", "hint": "#b0bec5",
        "log_bg": "#ffffff", "log_fg": "#37474f",
        "btn": "#37474f", "btn_fg": "#ffffff",
        "font": ("Segoe UI", 10), "bold": ("Segoe UI Semibold", 10),
        "small": ("Segoe UI", 8), "cellfont": ("Segoe UI Semibold", 15),
        "title": ("Segoe UI Semibold", 13),
        "chain": {
            1: ("#455a64", "#ffffff"), 2: ("#2e7d32", "#ffffff"),
            3: ("#f9a825", "#3e2723"), 4: ("#1565c0", "#ffffff"),
            5: ("#8e24aa", "#ffffff"), 6: ("#00838f", "#ffffff"),
            7: ("#5d4037", "#ffffff"), 8: ("#c62828", "#ffffff"),
        },
    },
    "retro": {
        "naam": "Retro BBC",
        "bg": "#000000", "panel": "#000000", "fg": "#ffffff", "muted": "#00ffff",
        "grid": "#303030", "empty": "#000000", "dot": "#404040",
        "loose": "#00ff00", "loose_fg": "#000000",
        "head_bg": "#00ffff", "head_fg": "#000000",
        "band": "#101010", "sel": "#ffff00", "hint": "#005000",
        "log_bg": "#000000", "log_fg": "#00ff00",
        "btn": "#0000ff", "btn_fg": "#ffffff",
        "font": ("Consolas", 10), "bold": ("Consolas", 10, "bold"),
        "small": ("Consolas", 8), "cellfont": ("Consolas", 15, "bold"),
        "title": ("Consolas", 13, "bold"),
        # Kleurparen uit DATA 29000-29007 van het origineel, met twee
        # aanpassingen voor de leesbaarheid: keten A staat op 10,10,10 in plaats
        # van zuiver zwart (anders valt het vlak weg tegen de achtergrond) en de
        # F krijgt een zwarte letter op het lichtblauwe vlak in plaats van wit.
        "chain": {
            1: ("#0a0a0a", "#ffffff"), 2: ("#00ff00", "#ffffff"),
            3: ("#ffff00", "#0000ff"), 4: ("#0000ff", "#ffffff"),
            5: ("#ff00ff", "#ffffff"), 6: ("#00ffff", "#000000"),
            7: ("#ffffff", "#0000ff"), 8: ("#ff0000", "#ffffff"),
        },
    },
}


# ------------------------------------------------------------ startdialoog

class NewGameDialog(tk.Toplevel):
    """Vervangt "DOE JE MEE MENS ?" uit regel 2009 van het origineel."""

    def __init__(self, master, theme):
        super().__init__(master)
        self.title("Nieuw spel")
        self.resizable(False, False)
        self.configure(bg=theme["bg"])
        self.result = None
        self.transient(master)

        tk.Label(self, text="DOE JE MEE, MENS?", bg=theme["bg"], fg=theme["fg"],
                 font=theme["title"]).grid(row=0, column=0, columnspan=2,
                                           padx=16, pady=(14, 4))
        tk.Label(self, text="OPA, OMA, BROER, ZUS, PA en MA spelen mee.",
                 bg=theme["bg"], fg=theme["muted"], font=theme["small"]
                 ).grid(row=1, column=0, columnspan=2, padx=16, pady=(0, 10))

        tk.Label(self, text="Naam:", bg=theme["bg"], fg=theme["fg"],
                 font=theme["font"]).grid(row=2, column=0, sticky="e", padx=(16, 4))
        self.naam = tk.Entry(self, width=14, font=theme["font"])
        self.naam.insert(0, "ED")
        self.naam.grid(row=2, column=1, sticky="w", padx=(0, 16), pady=2)

        tk.Label(self, text="Plaats:", bg=theme["bg"], fg=theme["fg"],
                 font=theme["font"]).grid(row=3, column=0, sticky="e", padx=(16, 4))
        self.seat = tk.StringVar(value="willekeurig")
        opts = ["willekeurig"] + [str(i) for i in range(1, NPLAYERS + 1)]
        tk.OptionMenu(self, self.seat, *opts).grid(row=3, column=1, sticky="w",
                                                   padx=(0, 16), pady=2)

        knoppen = tk.Frame(self, bg=theme["bg"])
        knoppen.grid(row=4, column=0, columnspan=2, pady=(12, 14))
        tk.Button(knoppen, text="Ik doe mee", width=12, font=theme["font"],
                  command=self._play).pack(side="left", padx=4)
        tk.Button(knoppen, text="Alleen kijken", width=12, font=theme["font"],
                  command=self._watch).pack(side="left", padx=4)

        self.naam.focus_set()
        self.bind("<Return>", lambda e: self._play())

        self.update_idletasks()          # midden boven het hoofdvenster zetten
        master.update_idletasks()
        x = master.winfo_rootx() + (master.winfo_width() - self.winfo_width()) // 2
        y = master.winfo_rooty() + (master.winfo_height() - self.winfo_height()) // 3
        self.geometry(f"+{max(x, 0)}+{max(y, 0)}")
        self.grab_set()

    def _play(self):
        import random
        seat = self.seat.get()
        seat = random.randint(1, NPLAYERS) if seat == "willekeurig" else int(seat)
        self.result = (seat, self.naam.get())
        self.destroy()

    def _watch(self):
        self.result = (0, "")
        self.destroy()


# ------------------------------------------------------------------- app

class AkwaierApp:

    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("AKWAIER  -  naar het BBC BASIC-spel uit 1987")
        self.skin = tk.StringVar(value="modern")
        self.snel = tk.BooleanVar(value=False)

        self.game: Game | None = None
        self.phase = "idle"
        self.sel_slot: int | None = None
        self.pending: dict = {}

        self._build()
        self.apply_theme()
        self.root.after(120, self.new_game)

    # ------------------------------------------------------------- opbouw

    @property
    def th(self):
        return THEMES[self.skin.get()]

    def _build(self):
        r = self.root
        self.top = tk.Frame(r)
        self.top.grid(row=0, column=0, columnspan=2, sticky="ew", padx=8, pady=(8, 4))

        self.b_new = tk.Button(self.top, text="Nieuw spel", command=self.new_game)
        self.b_new.pack(side="left")
        self.b_scores = tk.Button(self.top, text="Scorelijst", command=self.show_scores)
        self.b_scores.pack(side="left", padx=(6, 0))
        self.c_snel = tk.Checkbutton(self.top, text="snel spelen", variable=self.snel)
        self.c_snel.pack(side="left", padx=(14, 0))

        self.b_skin = tk.Button(self.top, text="Weergave: Modern", command=self.toggle_skin)
        self.b_skin.pack(side="right")
        self.l_turn = tk.Label(self.top, text="")
        self.l_turn.pack(side="right", padx=(0, 18))

        self.board = tk.Canvas(r, width=BOARD_PX, height=BOARD_PX,
                               highlightthickness=0)
        self.board.grid(row=1, column=0, padx=(8, 4), pady=4)
        self.board.bind("<Button-1>", self.on_board_click)

        self.info = tk.Canvas(r, width=372, height=BOARD_PX, highlightthickness=0)
        self.info.grid(row=1, column=1, padx=(4, 8), pady=4, sticky="n")

        self.hand = tk.Canvas(r, width=BOARD_PX, height=64, highlightthickness=0)
        self.hand.grid(row=2, column=0, padx=(8, 4), sticky="ew")
        self.hand.bind("<Button-1>", self.on_hand_click)

        self.actions = tk.Frame(r)
        self.actions.grid(row=3, column=0, sticky="ew", padx=8, pady=(4, 2))
        self.l_msg = tk.Label(r, text="", anchor="w")
        self.l_msg.grid(row=4, column=0, sticky="ew", padx=10, pady=(0, 6))

        logbox = tk.Frame(r)
        logbox.grid(row=2, column=1, rowspan=3, padx=(4, 8), pady=4, sticky="nsew")
        self.log = tk.Text(logbox, width=44, height=13, wrap="word",
                           state="disabled", highlightthickness=0, bd=0)
        sb = tk.Scrollbar(logbox, command=self.log.yview)
        self.log.configure(yscrollcommand=sb.set)
        self.log.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        self.logbox = logbox

        for i in range(1, 9):
            self.root.bind(str(i), self._key_slot)

    def _key_slot(self, ev):
        if self.phase == "place":
            self.select_slot(int(ev.char) - 1)

    # -------------------------------------------------------------- skins

    def toggle_skin(self):
        self.skin.set("retro" if self.skin.get() == "modern" else "modern")
        self.apply_theme()
        self.redraw()

    def apply_theme(self):
        t = self.th
        self.root.configure(bg=t["bg"])
        for w in (self.top, self.actions, self.logbox):
            w.configure(bg=t["bg"])
        for c in (self.board, self.info, self.hand):
            c.configure(bg=t["bg"])
        for b in (self.b_new, self.b_scores, self.b_skin):
            b.configure(bg=t["btn"], fg=t["btn_fg"], font=t["font"],
                        activebackground=t["sel"], activeforeground=t["btn_fg"],
                        relief="flat", padx=10, pady=3, bd=0,
                        highlightthickness=0, cursor="hand2")
        self.c_snel.configure(bg=t["bg"], fg=t["fg"], font=t["font"],
                              selectcolor=t["panel"], activebackground=t["bg"],
                              activeforeground=t["fg"], highlightthickness=0)
        self.l_turn.configure(bg=t["bg"], fg=t["fg"], font=t["title"])
        self.l_msg.configure(bg=t["bg"], fg=t["muted"], font=t["font"])
        self.log.configure(bg=t["log_bg"], fg=t["log_fg"], font=t["small"],
                           insertbackground=t["fg"])
        self.b_skin.configure(text=f"Weergave: {t['naam']}")
        for w in self.actions.winfo_children():
            self._style_action(w)

    def _style_action(self, w):
        t = self.th
        if isinstance(w, tk.Button):
            w.configure(bg=t["btn"], fg=t["btn_fg"], font=t["font"], relief="flat",
                        padx=9, pady=3, bd=0, highlightthickness=0, cursor="hand2",
                        activebackground=t["sel"], activeforeground=t["btn_fg"])
        elif isinstance(w, tk.Label):
            w.configure(bg=t["bg"], fg=t["fg"], font=t["font"])
        elif isinstance(w, tk.Frame):
            w.configure(bg=t["bg"])

    # ---------------------------------------------------------- nieuw spel

    def new_game(self):
        dlg = NewGameDialog(self.root, self.th)
        self.root.wait_window(dlg)
        if dlg.result is None:
            return
        seat, naam = dlg.result
        self.game = Game(human_seat=seat, human_name=naam)
        self.phase = "idle"
        self.sel_slot = None
        self.log.configure(state="normal")
        self.log.delete("1.0", "end")
        self.log.configure(state="disabled")
        if seat:
            self.write(f"Je speelt als speler {seat}: "
                       f"{self.game.players[seat - 1].name}.")
            self.write("Kies een steen (klik of toets 1-8) en leg hem neer, "
                       "of sticht er een nieuwe keten mee.")
        else:
            self.write("Kijkmodus: de zes computerspelers spelen zelf.")
        self.write("-" * 44)
        self.redraw()
        self.root.after(400, self.step)

    # ------------------------------------------------------------ beurten

    def step(self):
        """Volgende speler aan zet; computerspelers spelen zichzelf."""
        g = self.game
        if g is None:
            return
        pno = g.next_player()
        if pno is None:
            self.game_over()
            return
        self.clear_actions()
        self.redraw()
        if g.players[pno - 1].human:
            self.begin_human_turn()
        else:
            self.root.after(60 if self.snel.get() else 350, self.run_computer, pno)

    def run_computer(self, pno):
        g = self.game
        for line in g.ai_turn(pno):
            self.write(line)
        self.redraw()
        self.root.after(40 if self.snel.get() else 320, self.step)

    # ------------------------------------------------- beurt van de speler

    def begin_human_turn(self):
        g = self.game
        pno = g.current
        self.sel_slot = None
        self.phase = "place"
        legal = g.legal_slots(pno)
        self.redraw()
        if not legal:
            self.message("Je kunt geen enkele steen kwijt.")
            self.clear_actions()
            self.add_button("Pas deze beurt", self.do_pass)
            self.phase = "pass"
            return
        self.message("Kies een steen: klik op het bord of op je hand, "
                     "of druk 1-8.")
        self.clear_actions()

    def do_pass(self):
        g = self.game
        g.passes(g.current)
        self.write(f"{g.players[g.current - 1].name} kan niet")
        self.clear_actions()
        self.step()

    def select_slot(self, slot: int):
        g = self.game
        if self.phase != "place" or not (0 <= slot < 8):
            return
        tile = g.hands[g.current][slot]
        if tile is None:
            self.message("Die handplaats is leeg.")
            return
        self.sel_slot = slot
        x, y = tile_pos(tile)
        m = g.analyse(x, y)
        self.clear_actions()
        tk.Label(self.actions, text=f"Steen {slot + 1} = {label(x, y)}").pack(side="left",
                                                                             padx=(0, 8))
        if m.legal:
            if not m.adj:
                txt = f"Leg {label(x, y)} neer (losse steen)"
            else:
                txt = f"Leg {label(x, y)} bij keten {chain_letter(m.winner)}"
            self.add_button(txt, lambda: self.do_place(slot))
        else:
            tk.Label(self.actions, text="hier leggen mag niet").pack(side="left")
        if g.can_found(x, y) and g.available_chains():
            self.add_button(f"Sticht keten op {label(x, y)}",
                            lambda: self.ask_found_colour(slot))
        self.add_button("Annuleer", self.begin_human_turn)
        for w in self.actions.winfo_children():
            self._style_action(w)
        self.redraw()

    def ask_found_colour(self, slot):
        g = self.game
        self.phase = "found"
        self.clear_actions()
        tk.Label(self.actions, text="Welke keten stichten?").pack(side="left", padx=(0, 8))
        for c in g.available_chains():
            self.add_chain_button(c, lambda c=c: self.do_found(slot, c))
        self.add_button("Annuleer", self.begin_human_turn)
        for w in self.actions.winfo_children():
            self._style_action(w)

    def do_found(self, slot, chain):
        g = self.game
        r = g.found(g.current, slot, chain)
        self.write(f"{g.players[g.current - 1].name} sticht keten "
                   f"{chain_letter(chain)} op {label(r['x'], r['y'])} "
                   f"(+1 oprichtersaandeel)")
        self.begin_buy()

    def do_place(self, slot, winner=None):
        g = self.game
        tile = g.hands[g.current][slot]
        x, y = tile_pos(tile)
        m = g.analyse(x, y)
        if m.tie and winner is None:
            self.phase = "tie"
            self.clear_actions()
            tk.Label(self.actions,
                     text="Twee even grote ketens. Welke blijft bestaan?"
                     ).pack(side="left", padx=(0, 8))
            for c in m.tied:
                self.add_chain_button(c, lambda c=c: self.do_place(slot, c))
            for w in self.actions.winfo_children():
                self._style_action(w)
            return
        r = g.place(g.current, slot, winner)
        for line in g._describe(g.players[g.current - 1].name, r):
            self.write(line)
        self.begin_buy()

    # ------------------------------------------------------------- kopen

    def begin_buy(self):
        g = self.game
        self.sel_slot = None
        self.phase = "buy"
        self.redraw()
        chains = [c for c in g.existing_chains()
                  if g.price(c) <= g.players[g.current - 1].money]
        self.clear_actions()
        if not chains:
            self.message("Er valt niets te kopen.")
            self.add_button("Verder", self.finish_turn)
        else:
            self.message("Koop maximaal drie aandelen van een keten.")
            tk.Label(self.actions, text="Kopen:").pack(side="left", padx=(0, 6))
            for c in chains:
                self.add_chain_button(c, lambda c=c: self.ask_amount(c),
                                      extra=f" {g.price(c)}")
            self.add_button("Koop niets", self.finish_turn)
        for w in self.actions.winfo_children():
            self._style_action(w)

    def ask_amount(self, chain):
        g = self.game
        pr = g.price(chain)
        geld = g.players[g.current - 1].money
        self.clear_actions()
        tk.Label(self.actions,
                 text=f"Keten {chain_letter(chain)}, {pr} per aandeel. Hoeveel?"
                 ).pack(side="left", padx=(0, 8))
        for n in (1, 2, 3):
            if pr * n <= geld:
                self.add_button(str(n), lambda n=n: self.do_buy(chain, n))
        self.add_button("Terug", self.begin_buy)
        for w in self.actions.winfo_children():
            self._style_action(w)

    def do_buy(self, chain, n):
        g = self.game
        pr = g.price(chain)
        got = g.buy(g.current, chain, n)
        if got:
            self.write(f"{g.players[g.current - 1].name} koopt {got} x "
                       f"{chain_letter(chain)} a {pr}")
        self.finish_turn()

    def finish_turn(self):
        self.clear_actions()
        self.message("")
        self.redraw()
        self.root.after(40 if self.snel.get() else 200, self.step)

    # ------------------------------------------------------------- einde

    def game_over(self):
        g = self.game
        self.phase = "over"
        self.clear_actions()
        self.redraw()
        self.write("=" * 44)
        self.write(f"EINDE: {g.end_reason}")
        for i, (nr, naam, worth) in enumerate(g.standings(), start=1):
            self.write(f"{i}. {naam:<8} {worth}")
        save_scores([(p.name, g.net_worth(p.number)) for p in g.players])
        self.message(f"Spel afgelopen - {g.end_reason}")
        self.add_button("Scorelijst", self.show_scores)
        self.add_button("Nieuw spel", self.new_game)
        for w in self.actions.winfo_children():
            self._style_action(w)

    def show_scores(self):
        t = self.th
        win = tk.Toplevel(self.root)
        win.title("Scorelijst")
        win.configure(bg=t["bg"])
        tk.Label(win, text="NR   SPELER     VERMOGEN", bg=t["head_bg"],
                 fg=t["head_fg"], font=t["bold"], anchor="w"
                 ).pack(fill="x", padx=12, pady=(12, 2), ipady=3)
        rows = load_scores()
        if not rows:
            tk.Label(win, text="nog geen scores", bg=t["bg"], fg=t["muted"],
                     font=t["font"]).pack(padx=12, pady=8)
        for i, d in enumerate(rows, start=1):
            tk.Label(win, text=f"{i:>2}   {d['naam']:<10} {d['vermogen']:>9}",
                     bg=t["bg"], fg=t["fg"], font=t["font"], anchor="w"
                     ).pack(fill="x", padx=12)
        tk.Button(win, text="Sluiten", command=win.destroy, bg=t["btn"],
                  fg=t["btn_fg"], font=t["font"], relief="flat", bd=0, padx=10,
                  pady=3).pack(pady=12)

    # ------------------------------------------------------ knoppen/tekst

    def clear_actions(self):
        for w in self.actions.winfo_children():
            w.destroy()

    def add_button(self, text, cmd):
        b = tk.Button(self.actions, text=text, command=cmd)
        b.pack(side="left", padx=3)
        self._style_action(b)
        return b

    def add_chain_button(self, c, cmd, extra=""):
        bg, fg = self.th["chain"][c]
        b = tk.Button(self.actions, text=chain_letter(c) + extra, command=cmd,
                      bg=bg, fg=fg, font=self.th["bold"], relief="flat", bd=0,
                      padx=9, pady=3, highlightthickness=1,
                      highlightbackground=self.th["grid"], cursor="hand2",
                      activebackground=bg, activeforeground=fg)
        b.pack(side="left", padx=2)
        return b

    def message(self, text):
        self.l_msg.configure(text=text)

    def write(self, text):
        self.log.configure(state="normal")
        self.log.insert("end", text + "\n")
        self.log.see("end")
        self.log.configure(state="disabled")

    # ------------------------------------------------------------ tekenen

    def redraw(self):
        if self.game is None:
            return
        self.draw_board()
        self.draw_info()
        self.draw_hand()
        g = self.game
        if g.finished:
            self.l_turn.configure(text="spel afgelopen")
        else:
            p = g.players[g.current - 1] if g.current else None
            wie = f"beurt {g.turn}/210  -  {p.name}" if p else ""
            self.l_turn.configure(text=wie)

    def draw_board(self):
        t = self.th
        cv = self.board
        cv.delete("all")
        g = self.game
        human = g.human_seat
        mine = {}
        if human and g.current == human and not g.finished:
            for slot, tile in enumerate(g.hands[human]):
                if tile is not None:
                    mine[tile_pos(tile)] = slot

        # randlabels: kolomcijfers boven en onder, rijletters links en rechts.
        # De retro-weergave houdt de "123456789012345" van het BBC-scherm aan.
        retro = self.skin.get() == "retro"
        for i in range(1, SIZE + 1):
            px = MARGIN + (i - 0.5) * CELL
            kop = str(i)[-1] if retro else str(i)
            for py in (MARGIN / 2, BOARD_PX - MARGIN / 2):
                cv.create_text(px, py, text=kop, fill=t["muted"], font=t["small"])
            py = MARGIN + (i - 0.5) * CELL
            for px in (MARGIN / 2, BOARD_PX - MARGIN / 2):
                cv.create_text(px, py, text=chr(64 + i), fill=t["muted"], font=t["small"])

        for x in range(1, SIZE + 1):
            for y in range(1, SIZE + 1):
                x0 = MARGIN + (x - 1) * CELL
                y0 = MARGIN + (y - 1) * CELL
                v = g.board[x][y]
                slot = mine.get((x, y))
                legal = slot is not None and g.analyse(x, y).legal

                if 1 <= v <= 8:
                    bg, fg = t["chain"][v]
                elif v == LOOSE:
                    bg, fg = t["band"], t["loose"]
                else:
                    bg, fg = (t["hint"] if legal else t["empty"]), t["muted"]

                cv.create_rectangle(x0 + 1, y0 + 1, x0 + CELL - 1, y0 + CELL - 1,
                                    fill=bg, outline=t["grid"])
                if 1 <= v <= 8:
                    cv.create_text(x0 + CELL / 2, y0 + CELL / 2, text=chain_letter(v),
                                   fill=fg, font=t["cellfont"])
                elif v == LOOSE:
                    cv.create_rectangle(x0 + 8, y0 + 8, x0 + CELL - 8, y0 + CELL - 8,
                                        fill=fg, outline="")
                elif slot is None:
                    cv.create_oval(x0 + CELL / 2 - 2, y0 + CELL / 2 - 2,
                                   x0 + CELL / 2 + 2, y0 + CELL / 2 + 2,
                                   fill=t["dot"], outline="")

                if slot is not None:
                    # zoals regel 3571: de handnummers staan op het bord
                    cv.create_text(x0 + CELL / 2, y0 + CELL / 2, text=str(slot + 1),
                                   fill=t["fg"] if legal else t["muted"],
                                   font=t["bold"])
                    if slot == self.sel_slot:
                        cv.create_rectangle(x0 + 2, y0 + 2, x0 + CELL - 2,
                                            y0 + CELL - 2, outline=t["sel"], width=3)

    def draw_hand(self):
        t = self.th
        cv = self.hand
        cv.delete("all")
        g = self.game
        human = g.human_seat
        if not human:
            cv.create_text(10, 32, text="kijkmodus - geen eigen stenen",
                           anchor="w", fill=t["muted"], font=t["font"])
            return
        cv.create_text(4, 12, text=f"stenen van {g.players[human - 1].name}",
                       anchor="w", fill=t["muted"], font=t["small"])
        legal = set(g.legal_slots(human)) if g.current == human else set()
        for slot in range(8):
            x0 = 4 + slot * 66
            y0 = 22
            tile = g.hands[human][slot]
            if tile is None:
                cv.create_rectangle(x0, y0, x0 + 60, y0 + 36, fill=t["empty"],
                                    outline=t["grid"], dash=(2, 2))
                continue
            sel = (slot == self.sel_slot)
            bg = t["band"] if slot in legal else t["empty"]
            cv.create_rectangle(x0, y0, x0 + 60, y0 + 36, fill=bg,
                                outline=t["sel"] if sel else t["grid"],
                                width=3 if sel else 1)
            cv.create_text(x0 + 8, y0 + 9, text=str(slot + 1), anchor="w",
                           fill=t["muted"], font=t["small"])
            cv.create_text(x0 + 34, y0 + 19, text=label(*tile_pos(tile)),
                           fill=t["fg"] if slot in legal else t["muted"],
                           font=t["bold"])

    def draw_info(self):
        t = self.th
        cv = self.info
        cv.delete("all")
        g = self.game
        W = 372

        def band(y, cols):
            """Kopregel met de teksten precies boven hun eigen kolom."""
            cv.create_rectangle(0, y, W, y + 20, fill=t["head_bg"], outline="")
            for x, anchor, text in cols:
                cv.create_text(x, y + 10, text=text, anchor=anchor,
                               fill=t["head_fg"], font=t["bold"])
            return y + 22

        # ---- ketens: prijs en aantal tegels ----------------------------
        y = band(0, [(6, "w", "KETEN"), (150, "e", "PRIJS"),
                     (230, "e", "TEGELS"), (250, "w", "STATUS")])
        for c in CHAINS:
            bg, fg = t["chain"][c]
            cv.create_rectangle(6, y, 30, y + 18, fill=bg, outline=t["grid"])
            cv.create_text(18, y + 9, text=chain_letter(c), fill=fg, font=t["bold"])
            n = g.chain_size(c)
            cv.create_text(150, y + 9, text=str(g.price(c)) if n else "-",
                           anchor="e", fill=t["fg"], font=t["font"])
            cv.create_text(230, y + 9, text=str(n) if n else "-",
                           anchor="e", fill=t["fg"], font=t["font"])
            cv.create_text(250, y + 9,
                           text="te stichten" if n == 0 else "",
                           anchor="w", fill=t["muted"], font=t["small"])
            y += 19

        # ---- aandelenbezit per speler ----------------------------------
        y = band(y + 6, [(6, "w", "AANDELEN PER SPELER")])
        for i in range(1, NPLAYERS + 1):
            cv.create_text(60 + i * 44, y + 8, text=str(i), fill=t["muted"],
                           font=t["small"])
        y += 18
        for c in CHAINS:
            bg, fg = t["chain"][c]
            cv.create_rectangle(6, y, 30, y + 18, fill=bg, outline=t["grid"])
            cv.create_text(18, y + 9, text=chain_letter(c), fill=fg, font=t["bold"])
            ranked = g.rank(c)
            top1, top2 = ranked[0], ranked[1]
            for i in range(1, NPLAYERS + 1):
                n = g.players[i - 1].shares[c]
                col = t["fg"]
                if n:
                    if i == top1:
                        col = t["sel"]
                    elif i == top2:
                        col = t["muted"]
                cv.create_text(60 + i * 44, y + 9, text=str(n) if n else ".",
                               fill=col if n else t["grid"],
                               font=t["bold"] if n and i in (top1, top2) else t["font"])
            y += 19

        # ---- spelers: geld en vermogen ---------------------------------
        y = band(y + 6, [(8, "w", "SPELER"), (250, "e", "GELD"),
                         (360, "e", "VERMOGEN")])
        for p in g.players:
            actief = (p.number == g.current and not g.finished)
            if actief:
                cv.create_rectangle(0, y, W, y + 19, fill=t["band"], outline="")
            naam = f"{p.number} {p.name}" + (" (jij)" if p.human else "")
            cv.create_text(8, y + 9, text=naam, anchor="w", fill=t["fg"],
                           font=t["bold"] if actief else t["font"])
            cv.create_text(250, y + 9, text=f"{p.money}", anchor="e",
                           fill=t["fg"], font=t["font"])
            cv.create_text(360, y + 9, text=f"{g.net_worth(p.number)}", anchor="e",
                           fill=t["fg"], font=t["font"])
            y += 19

        cv.create_text(8, y + 12, anchor="w", fill=t["muted"], font=t["small"],
                       text=f"stenen in de zak: {len(g.bag)}")

    # ------------------------------------------------------------- muis

    def on_board_click(self, ev):
        g = self.game
        if g is None or self.phase not in ("place",) or not g.human_seat:
            return
        x = (ev.x - MARGIN) // CELL + 1
        y = (ev.y - MARGIN) // CELL + 1
        if not (1 <= x <= SIZE and 1 <= y <= SIZE):
            return
        for slot, tile in enumerate(g.hands[g.current]):
            if tile is not None and tile_pos(tile) == (x, y):
                self.select_slot(slot)
                return
        self.message(f"{label(x, y)} zit niet in je hand.")

    def on_hand_click(self, ev):
        if self.phase != "place":
            return
        slot = (ev.x - 4) // 66
        if 0 <= slot < 8:
            self.select_slot(int(slot))


def main():
    root = tk.Tk()
    tkfont.nametofont("TkDefaultFont").configure(family="Segoe UI", size=9)
    AkwaierApp(root)
    root.mainloop()


if __name__ == "__main__":
    main()
