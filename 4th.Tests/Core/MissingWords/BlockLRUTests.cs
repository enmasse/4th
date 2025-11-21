using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.MissingWords
{
    public class BlockLRUTests
    {
        [Fact]
        public async Task EvictionReducesBlockMappings()
        {
            var forth = new ForthInterpreter(blockCacheSize: 16);
            Assert.Equal(16, forth.BlockCacheSize);
            const int allocs = 200;
            for (int i = 0; i < allocs; i++)
            {
                // allocate block i
                Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
            }

            // Eviction should have removed some mappings, so count must be less than total allocations
            Assert.True(forth.BlockMappingCount < allocs, "LRU eviction did not remove older block mappings");
        }

        [Fact]
        public async Task EvictionDisposesMmfAccessorsWhenUsingBlockFile()
        {
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            try
            {
                var forth = new ForthInterpreter(blockCacheSize: 16);
                Assert.Equal(16, forth.BlockCacheSize);
                // Open directory for per-block storage (this disables single-file mmf, but ensures backing exists)
                Assert.True(await forth.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));

                const int allocs = 150;
                for (int i = 0; i < allocs; i++)
                {
                    // allocate block and drop c-addr/u
                    Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
                }

                // Ensure eviction happened
                Assert.True(forth.BlockMappingCount < allocs, "LRU eviction did not remove older block mappings (per-block)");

                // Accessor dictionary size should not exceed mapping count
                Assert.True(forth.MmfAccessorCount <= System.Math.Max(1, forth.BlockMappingCount), "MMF accessors remain larger than block mappings after eviction");
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }
    }
}
