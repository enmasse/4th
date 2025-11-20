TODO: ANS?Forth conformity gap analysis

Mål
- Jämför nuvarande implementation mot ANS Forth core wordlist och identifiera vilka ord som saknas eller är delvis implementerade.

Metod
- Scan av befintliga `Primitive`-attribut och testfall i repot användes för att avgöra vad som finns.

Status — implementerade/obvious stöd (icke-exklusiv lista)
- Definitions/kompilationsord: `:`, `;`, `IMMEDIATE`, `POSTPONE`, `[`, `]`, `'`, `LITERAL`
- Kontrollflöde: `IF`, `ELSE`, `THEN`, `BEGIN`, `WHILE`, `REPEAT`, `UNTIL`, `DO`, `LOOP`, `LEAVE`, `UNLOOP`, `I`, `RECURSE`
- Definieringsord: `CREATE`, `DOES>`, `VARIABLE`, `CONSTANT`, `VALUE`, `TO`, `DEFER`, `IS`, `MARKER`, `FORGET`
- Stack/minne: `@`, `!`, `C@`, `C!`, `,`, `ALLOT`, `HERE`, `COUNT`, `MOVE`, `FILL`, `ERASE`
- I/O: `.`, `.S`, `CR`, `EMIT`, `TYPE`, `WORDS`, pictured numeric (`<#`, `HOLD`, `#`, `#S`, `SIGN`, `#>`)
- Fil?I/O (subset): `READ-FILE`, `WRITE-FILE`, `APPEND-FILE`, `FILE-EXISTS`, `INCLUDE`
- Async / concurrency: `SPAWN`, `FUTURE`, `TASK`, `JOIN`, `AWAIT`, `TASK?` (inkl. ValueTask via reflection)
- Exceptions / control: `CATCH`, `THROW`, `ABORT`, `EXIT`, `BYE`, `QUIT`
- Numeric base & parsing: `BASE`, `HEX`, `DECIMAL`, `>NUMBER`, `STATE`
- Introspection: `SEE`

Saknade eller ofullständiga ANS?ord (prioriterad lista)
1. Wordlist / search-order API (viktigt för ANS)
   - `GET-ORDER` (saknas)
   - `SET-ORDER` (saknas)
   - `WORDLIST` / `DEFINITIONS` / `FORTH` (hantering av flera wordlists saknas eller ofullständig)
2. Interaktiv input / source position
   - `KEY` (saknas)
   - `KEY?` (saknas)
   - `ACCEPT` (saknas)
   - `EXPECT` (saknas)
   - `SOURCE` (saknas)
   - `>IN` (saknas)
3. Fullställt fil?API (streams & metadata)
   - `OPEN-FILE` (saknas)
   - `CLOSE-FILE` (saknas)
   - `READ-FILE` (stream variant) / `READ-LINE` (saknas)
   - `FILE-SIZE` (saknas)
   - `REPOSITION-FILE` / `SET-FILE-POS` (saknas)
4. Block?system (klassiska ANS blockord) — om målet inkluderar block I/O
   - `BLOCK`, `LOAD`, `SAVE`, `BLK` etc. (saknas)
5. Vissa ord för sträng/input format och tangentläsning
   - `KEY?`, `ACCEPT`, `EXPECT` (se ovan)
6. Robusthet/semantikfixar
   - `AWAIT`/`TASK?` använder reflection och ett `VoidTaskResult`-namncheck — bör ersättas med en robust detection/hantering av Task/Task<T>/ValueTask<T>/IValueTaskSource
   - Verifiera att true = -1 semantics används konsekvent (ANS kräver all?bits?set för true)
7. Dubbelcells?ord och vissa matematiska helpers (beroende på mål)
   - `D+`, `D-`, `M*`, `*/MOD` (begränsat stöd finns; kontrollera komplett lista)

Förslag — nästa steg (kort)
- Skapa ett litet script som extraherar alla `Primitive("NAME")` från koden (eller kör reflection mot kompilerad assembly) och jämför mot en canonical ANS core?wordlista för att få exakt maskinlista.
- Prioritera implementering av `GET-ORDER`/`SET-ORDER` och input?ord (`KEY`/`ACCEPT`/`>IN`) för att förbättra interaktiv konformitet.
- Åtgärda `AWAIT`/`TASK?` ValueTask?hantering för att undvika fragila reflection?kontroller.
- Integrera en ANS?testsvit (eller portera relevanta tester) och kör kontinuerligt.

Arbetsuppgift för repot (kan utföras automatiskt)
- [ ] Skapa script `tools/ans-diff` som samlar `Primitive`-namn och jämför mot ANS lista
- [ ] Implementera högprioriterade ord (GET-ORDER, SET-ORDER, KEY, ACCEPT, OPEN-FILE variants)
- [ ] Förbättra `AWAIT`/`TASK?` robusthet
- [ ] Lägg till konformitetstester och kör `dotnet test` i CI

Kommentar
- Den här filen är en manuell sammanställning baserad på snabb scanning av repot. För exakt komplett lista rekommenderas att köra det automatiska jämförelsescriptet som föreslås ovan.
