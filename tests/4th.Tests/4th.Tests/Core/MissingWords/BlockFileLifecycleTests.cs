using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.MissingWords
{
    public class BlockFileLifecycleTests
    {
        [Fact]
        public async Task FlushAndCloseBlockFileSingleFileMode()
        {
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            Directory.CreateDirectory(Path.GetDirectoryName(dir) ?? Path.GetTempPath());
            var file = Path.Combine(dir, "blocks.bin");
            try
            {
                var forth = new ForthInterpreter(blockCacheSize: 16);
                // open single-file backing
                Assert.True(await forth.EvalAsync($"\"{file}\" OPEN-BLOCK-FILE"));

                // allocate some blocks to create accessors/mappings
                for (int i = 0; i < 50; i++)
                {
                    Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
                }

                // flush
                Assert.True(await forth.EvalAsync("FLUSH-BLOCK-FILE"));

                // close
                Assert.True(await forth.EvalAsync("CLOSE-BLOCK-FILE"));

                // inspect internals: mmf accessors should be empty and file path null
                Assert.Equal(0, forth.MmfAccessorCount);
                Assert.Null(forth.BlockFilePath);
            }
            finally
            {
                try { File.Delete(file); } catch { }
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        [Fact]
        public async Task FlushAndCloseBlockFilePerBlockMode()
        {
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            try
            {
                var forth = new ForthInterpreter(blockCacheSize: 16);
                Assert.True(await forth.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));

                for (int i = 0; i < 50; i++)
                {
                    Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
                }

                // flush should be a no-op but not throw
                Assert.True(await forth.EvalAsync("FLUSH-BLOCK-FILE"));

                // close should clear directory mode
                Assert.True(await forth.EvalAsync("CLOSE-BLOCK-FILE"));

                Assert.Null(forth.BlockFileDir);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }
    }
}
