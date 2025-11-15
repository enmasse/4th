using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class DictionaryAndVocabTests
{
    [Fact(Skip = "Template: implement WORDLIST WORDS ONLY ALSO PREVIOUS VOCABULARY RECURSE LATEST MARKER")]
    public async Task Dictionary_And_Vocabulary_Management()
    {
        var forth = new ForthInterpreter();
        // WORDS should list defined words; for now ensure it runs without error when implemented
        Assert.True(await forth.EvalAsync("WORDS"));
        // WORDLIST/ONLY/ALSO change search order; tests should verify visibility and resolution
        // RECURSE should allow recursive reference inside definitions
        Assert.True(true);
    }
}
