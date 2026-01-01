# Option A: Power Through - Test by Test Strategy

## Current State
- 84/876 passing (9.6%)
- 783 failures due to mixing parsing modes
- CharacterParser IS working, primitives are trying to read tokens that don't exist

## Key Insight
**The CharacterParser already creates the right tokens!**
- S" "text" ? becomes token "S\"" followed by parser consuming the string
- ." "text" ? becomes token ".\"" followed by parser consuming the string
- ABORT" "msg" ? becomes token "ABORT\"" followed by parser consuming the string

**The problem**: Primitives call `ReadNextTokenOrThrow()` expecting another token, but CharacterParser already consumed it!

## Strategy: Fix Primitives ONE BY ONE

### Phase 1: Fix String Parsing Primitives (Highest Impact)

**Priority Order** (by test impact):
1. S" - Most used, affects ~200 tests
2. ." - Common in tests, affects ~100 tests  
3. ABORT" - Less common, affects ~50 tests

### Phase 2: Fix Name-Reading Primitives

**Priority Order**:
1. : (colon) - Reads word name, affects ALL definitions
2. VARIABLE - Affects ~50 tests
3. CONSTANT - Affects ~30 tests
4. CREATE - Complex, affects ~20 tests
5. All others - Lower impact

## Execution Plan: Fix and Test Incrementally

### Step 1: Fix S" Primitive (Expected: +200 tests)

**Current code** (CorePrimitives.DictionaryVocab.cs line ~420):
```csharp
var next = i.ReadNextTokenOrThrow("Expected text after S\"");
if (next.Length < 2 || next[0] != '"' || next[^1] != '"')
    throw new ForthException(ForthErrorCode.CompileError, "S\" expects quoted token");
var str = next[1..^1];
```

**Problem**: CharacterParser consumed the string already, there's NO next token!

**Solution**: S" token means "read until closing quote from parser"
```csharp
// CharacterParser already consumed the string and returned just S"
// We need to read the string from the character position after S"
if (i._parser == null) 
    throw new ForthException(ForthErrorCode.CompileError, "Parser not available");

var str = i._parser.ReadUntil('"');
```

**Wait - Let me check CharacterParser.cs again...**

Looking at line ~183-206, CharacterParser handles S" by:
1. Recognizing 'S"'  
2. Returning "S\"" as the token
3. But NOT consuming the string content!

So the string IS still in the source for the primitive to read!

**Revised Solution**:
```csharp
// The token "S\"" was returned, now read the string from current position
if (i._parser == null)
    throw new ForthException(ForthErrorCode.CompileError, "Parser not available");

var str = i._parser.ReadUntil('"');
```

### Step 2: Fix ." Primitive (Expected: +100 tests)

Same pattern as S"

### Step 3: Fix : Primitive (Expected: +300 tests)

**Current** (CorePrimitives.Compilation.cs line ~8):
```csharp
var name = i.ReadNextTokenOrThrow("Expected name after ':'");
```

**Solution**:
```csharp
if (i._parser == null)
    throw new ForthException(ForthErrorCode.CompileError, "Parser not available");
i._parser.SkipWhitespace();
var name = i._parser.ParseNext();
if (name == null || string.IsNullOrWhiteSpace(name))
    throw new ForthException(ForthErrorCode.CompileError, "Expected name after ':'");
```

### Testing After Each Fix

```powershell
# After EACH primitive fix:
dotnet build --no-incremental
.\health.ps1

# Document:
# - How many tests now passing
# - What new failures appeared (if any)
# - Adjust strategy if needed
```

## Let's START!

### Fix #1: S" Primitive

Actually, wait. Let me re-read CharacterParser lines 183-206 more carefully...

**LINE 183-206 of CharacterParser.cs**:
```csharp
// Handle S" string literal (case-insensitive)
if ((ch == 'S' || ch == 's') && _position + 1 < _source.Length && _source[_position + 1] == '"')
{
    _position += 2; // skip 'S"' or 's"'
    // Skip at most one leading space
    if (_position < _source.Length && _source[_position] == ' ')
    {
        _position++;
    }
    var textBuilder = new StringBuilder();
    textBuilder.Append('"');
    while (_position < _source.Length && _source[_position] != '"')
    {
        textBuilder.Append(_source[_position]);
        _position++;
    }
    if (_position < _source.Length && _source[_position] == '"')
    {
        textBuilder.Append('"');
        _position++; // skip closing '"'
    }
    return "S\"";  // Return normalized uppercase, string will be consumed by primitive
}
```

**IMPORTANT**: CharacterParser DOES consume the string! It returns just `"S\""` but the string content is ALREADY CONSUMED and _position moved past the closing quote!

So the primitive CANNOT read the string again - it's already gone!

**The REAL solution**: CharacterParser should return the WHOLE TOKEN including the string!

Let me fix CharacterParser to return composite tokens:

