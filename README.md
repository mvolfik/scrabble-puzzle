# Zápočtový program Matěj Volf LS 2022/2023

## Specifikace

Generování "Scrabble" šifer (nikoliv kryptografických): pro uživatelem zadaný _cleartext_ vygenerovat posloupnost slov s vyznačenými _překryvy_, kterou je možné poskládat dle pravidel hry Scrabble jediným způsobem, a to tak, že výsledné bodové ohodnocení jednotlivých slov dá po převodu čísel na písmena (1 = 27 = A, 2 = 28 = B ...) daný _cleartext_.

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

**Algoritmické řešení:** řekl bych, že jde o poměrně unikátní problém, netroufám si tedy rovnou říct, o jak složitý problém jde. Úvodní implementace bude prohledávání do hloubky všech možností, na tomto základu pak lze implementovat optimalizace: předpočítání slov s vhodnými hodnotami, hodnocení stavů dle počtu "volných" písmen a prioritizace prohledávání (A\*-like algoritmus), průzkum možnosti paralelizace.

## Jaké jsou vlastně podmínky na validní zadání šifry

Zadání je neúplným záznamem o hře Scrabble. Když slova ohodnotíme podle pravidel Scrabble, dostaneme čísla, která odpovídají písmenům v abecedě (0 = 26 = A).

Zásadní pravidlo je, že souvislý blok písmen ve sloupci nebo řádku je vždy platné slovo. Když tedy herní dílky pokládáme, pokud si nechceme komplikovat život kontrolováním platnosti více slov zároveň, musíme hlídat, že se nové slovo nedotýká (mimo rohů) žádného jiného slova kromě toho, na nějž cíleně navazujeme.

Zadání určuje pořadí slov. Slova na sebe samozřejmě nemusí navazovat lineárně za sebou - např. čtvrté slovo může být klidně připojené na to, které bylo položené jako úplně první.

Hra však musí být dle zadání jednoznačná. V zadání specifikujeme pouze které políčko z prvního slova je středové, a u každého dalšího slova kterým políčkem se připojuje (dané písmeno tedy už leželo na plánku, a neplatí z něj tedy ani bonusy). Nespecifikujeme ani zda je slovo položené vodorovně ani svisle. Musí ale být jeden jediný způsob, jak slovo na herní plán položit.

Nejsme limitováni fyzických počtem dílků ve stolní verzi Scrabble, každé písmeno můžeme použít kolikrát chceme.

## Uživatelská dokumentace

```
dotnet build -c Release
./bin/Release/net7.0/ScrabblePuzzleGenerator [-cgs] [-n [<modulo>]] [-d <dictionary filename>] [-v <letter values filename>] <plaintext>
```

Flag `-c` zapíná barevný výstup, vypisovaný pomocí ANSI escape sekvencí.

Flag `-g` zapíná vypisování písmen rozmístěných na plánku. Pro dané zadání zkopírujte vyznačený segment, a můžete jej vložit např. do [této tabulky][sheet], kde jsou připravená podbarvená bonusová pole.

[sheet]: https://docs.google.com/spreadsheets/d/1Xv547_N-K_u6HknHW3xy-1PbUxHfXeMuLBIJmagLzjU/edit?usp=sharing

Pokud je zapnutý flag `-s`, program vypíše pouze první nalezené zadání.

Flag `-n` přepíná do číselného režimu - zadání není plaintext, ale sekvence čísel oddělených čárkami. Pokud za flag přidáte ještě argument `<modulo>`, můžou po vyřešení šifry vyjít vyšší čísla a až po odečtení celočíselného násobku modula vyjde zadané číslo.

Argumenty `-d <filename>` a `-v <filename>` umožňují použít vlastní slovník povolených slov, respektive specifikaci ohodnocení písmen. Slovník je seznam slov oddělených řádky, formát ohodnocení písmen můžete vidět ve výchozím souboru `letter_values.txt`.

---

Pokud není použit flag `-n`, řešení šifry (plaintext) musí být složeno výhradně z písmen a-z bez diakritiky.

Na výstup program postupně vypisuje nalezená zadání, v tomto formátu:

```raw
 J  >Ó<

 B   A   Č   U  [J]

[Ó]  D   E   I   A 

 G   A   M  [B]  Y 

 H   O   Ř  [Č]

[D]  É 

 A   K   I   J  [E]

 R   Ó   B  [Y]
```

`>O<` v prvním slově značí, že dané písmeno je umístěné na prostředním začátečním políčku. `[E]` značí že toto písmeno už na plánku bylo od některého předchozího tahu.

V případě barevného výstupu jsou použity standartní Scrabble barvy: červená pro bonusy na slova, modrá pro bonusy na písmena. Ztrojení je tmavší a sytější než zdvojení.

Mezi sadami slov pro jednotlivá řešení jsou 3 prázdné řádky.

## Architektura programu

Program je rozdělen do několika tříd.

`Program.cs` obsahuje hlavní třídu `Program` s funkcí `Main`, která zpracuje a validuje argumenty, a na základě nich inicializuje databázi slov, spustí generátor šifer a pro každý výsledek zavolá generátor textového výstupu.

Třída `WordsDatabase` si načítá seznam slov, které může v zadání používat, a definici bodových hodnot jednotlivých písmen. Z tohoto si předpočítá velkou tabulku, díky které je pak schopná pro konkrétní situaci na plánku a bodovou hodnotu v _průměrně_ konstantním čase vrátit seznam slov. Konkrétně se klíč sestává z:

- délky slova
- písmeno, které už na plánku máme, a jeho pozici ve slově
- indexy písmen, která budou mít zdvojenou či ztrojenou hodnotu
- hodnotu, kterou má slovo mít po započítání těchto zdvojených a ztrojených písmen

Tato tabulka je poměrně velká: pro používaný seznam 34 tisíc tří- až pětipísmenných slov obsahuje 62 tisíc klíčů a u nich uložené přes 2 miliony referencí na slova. Toto by šlo zlepšovat pouze drobnými konstantami (nejvíce typický "duplikát" je pětipísmenné slovo se zadaným písmenem a zdvojenými okolními dvěma písmeny - tato konfigurace je v tabulce 3x, vždy jen poposunutá o políčko), nicméně celková paměťová náročnost programu se udrží okolo 1GB, a tato tabulka reverzních lookupů ze situace na plánku na seznam odpovídajících slov je klíčová, pokud chceme být schopní rychle prohledávat opravdu všechny možnosti.

Samotný algoritmus v třídě `PuzzleGenerator` už je pak poměrně přímočaré prohledávání do hloubky. V prvním kroku generujeme všechny slova s danou hodnotou, v dalších úrovních rekurze pak procházíme dříve položená slova, a zjišťujeme, zda je možné kolmo na ně položit nové slovo. Všechna tato dostupná místa si v daném kroku posbíráme, a pak se podíváme, jaké "specifikace", tedy délka a "zadané" písmeno (to, přes které se slova kříží), byly unikátní, a můžeme je do zadání použít. Pro tyto se už pouze zeptáme třídy `WordsDatabase` na seznam slov už podle konkrétních bonusových políček, která na daném místě jsou, a pokračujeme v rekurzi.

Nejdůležitější zde bylo si rozmyslet, jak zjišťovat, zda je možné slovo na danou pozici položit. Já používám tabulku, kde si u každého pole značím, zda je volné, obsazené konkrétním slovem, nebo obsazené alespoň dvěmi, tedy už nepoužitelné. Když položím slovo a tuto tabulku posílám do rekurze, pouze v tabulce označím okolí nového slova, což je výrazdně jednodušší než se na okolí koukat při vyhledávání. Implementace tohoto rozhraní je ve třídě `OccupiedSquaresTracker`.

Ve chvíli, kdy úspěšně položíme slovo pro každé písmeno/číslo z tajenky, pomocí iterátoru (rozhraní `IEnumerable`) vrátíme pouze seznam slov a jejich pozic. Tento seznam pak zpracuje třída `ResultPrinter`, která si opět dohledá bonusová políčka, podle pořadí skládání správně zaznačí překryvová políčka, a vše StringBuilderem zformátuje do stringu s nastavením podle argumentů.

Mimo tyto hlavní třídy existují dvě malé statické třídy: ve třídě `Helpers` je abstrahovaný často používaný pattern přidávání prvků pro slovníky, které jako hodnoty obsahují seznamy prvků, a třída `GridFormatter` vyděluje formátování výstupu do tabulky pro copy-paste do vizualizace řešení.

## Testy

[tests.md](tests.md)

## Známé nedostatky

- Pro plaintexty začínající lichými písmeny abecedy (a = 0, tedy 1 = b, 3 = d...) nejsou žádná řešení. Úvodní slovo se totiž pokládá na pole zdvojené hodnoty slova, a nepomůže ani modulo, protože délka abecedy je také sudá.
- Program aktuálně počítá pouze s nejvýše pětipísmennými slovy. To nám jednak omezuje velikost předpočítané tabulky, a také o něco zjednodušuje možné kombinace zdvojených a ztrojených písmen, která musíme předpočítat. Takové úpravy by nebyly příliš náročné, a vzhledem k tomu, že se po implementaci algoritmu ukázalo, že pro většinu zadání je možných řešení opravdu velké množství, lepší určení tohoto programu by možná bylo pracovat s ručně zadaným seznamem slov (např. když chceme mít šifru tematickou), a zde by větší množství předpočítaných hodnot pro každé slovo nevadilo.
- Program pracuje pouze s kolmými překryvy jedním písmenem. Neumí prodlužování slov ani křížení více slov najednou.
