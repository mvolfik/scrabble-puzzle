# Zápočtový program Matěj Volf LS 2022/2023

## Specifikace

Generování "Scrabble" šifer (nikoliv kryptografických): pro uživatelem zadaný *cleartext* vygenerovat posloupnost slov s vyznačenými *překryvy*, kterou je možné poskládat dle pravidel hry Scrabble jediným způsobem, a to tak, že výsledné bodové ohodnocení jednotlivých slov dá po převodu čísel na písmena (1 = 27 = A, 2 = 28 = B ...) daný *cleartext*.

**Zdroj inspirace:** 3. ročník táborské šifrovací hry ŠifT: https://www.sifrovacihra.cz/sift/index.html > Minulé ročníky > 3. ročník > Šifra 1: Hra

**Interface:** jednoduchá interaktivní konzolová aplikace.

**Vstup:** očekává cleartext jako parametr, pokud není poskytnut, čte ze standardního vstupu. Možnost dalších parametrů při spuštění: použití jiného slovníku, ohodnocení písmen (Scrabble dílků), možnost definovat cleartext pouze čísly (šifra pak může pracovat s vlastní abecedou nebo používat další úrovně kódování).

**Výstup:** jednoduchý textový formát s označením dílků, které na plánu už ležely.

Např.:
```
 K  Á >M< E  N (vodorovně)
[K] Á  Ď
 L  O [Ď]
```

Volitelně (zapnutí vstupním parametrem) možnost označit barevně ve výstupu dílky (písmena), které leží na bonusových polích.

**Algoritmické řešení:** řekl bych, že jde o poměrně unikátní problém, netroufám si tedy rovnou říct, o jak složitý problém jde. Úvodní implementace bude prohledávání do hloubky všech možností, na tomto základu pak lze implementovat optimalizace: předpočítání slov s vhodnými hodnotami, hodnocení stavů dle počtu "volných" písmen a prioritizace prohledávání (A*-like algoritmus), průzkum možnosti paralelizace.
