using Forth.Core.Interpreter;
using Forth.Core;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    public class BlockSystemPerBlockTests
    {
        [Fact]
        public async Task OPEN_BLOCK_DIR_SaveCreatesPerBlockFile()
        {
            var forth = new ForthInterpreter();
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            try
            {
                // Open directory for per-block storage (accepts pushed string or token)
                Assert.True(await forth.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));

                // Load block 0 (c-addr u) and write two bytes using OVER so addr/u remain for SAVE
                Assert.True(await forth.EvalAsync("0 BLOCK"));
                Assert.True(await forth.EvalAsync("OVER 72 SWAP C!"));
                Assert.True(await forth.EvalAsync("OVER 1 + 105 SWAP C!"));

                // Now SAVE block 0 (stack currently: c-addr u)
                Assert.True(await forth.EvalAsync("0 SAVE"));

                var fn = Path.Combine(dir, "block-0.bin");
                Assert.True(File.Exists(fn));
                var buf = File.ReadAllBytes(fn);
                Assert.True(buf.Length >= 2);
                Assert.Equal((byte)72, buf[0]);
                Assert.Equal((byte)105, buf[1]);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task OPEN_BLOCK_DIR_PersistenceAcrossInstances()
        {
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            try
            {
                // First interpreter writes block 0
                var forth1 = new ForthInterpreter();
                Assert.True(await forth1.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));
                Assert.True(await forth1.EvalAsync("0 BLOCK"));
                Assert.True(await forth1.EvalAsync("OVER 33 SWAP C!"));
                Assert.True(await forth1.EvalAsync("OVER 1 + 34 SWAP C!"));
                Assert.True(await forth1.EvalAsync("0 SAVE"));

                // New interpreter reads back first byte
                var forth2 = new ForthInterpreter();
                Assert.True(await forth2.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));
                Assert.True(await forth2.EvalAsync("0 BLOCK DROP C@")); // pushes first byte
                Assert.Single(forth2.Stack);
                Assert.Equal(33L, (long)forth2.Stack[0]);

                // New interpreter reads back second byte
                var forth3 = new ForthInterpreter();
                Assert.True(await forth3.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));
                Assert.True(await forth3.EvalAsync("0 BLOCK DROP 1 + C@"));
                Assert.Single(forth3.Stack);
                Assert.Equal(34L, (long)forth3.Stack[0]);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }
            }
        }
    }
}
