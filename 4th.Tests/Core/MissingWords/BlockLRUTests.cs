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
            var forth = new ForthInterpreter(blockCacheSize: 8);
            Assert.Equal(8, forth.BlockCacheSize);
            const int allocs = 32;
            for (int i = 0; i < allocs; i++)
            {
                // allocate block i
                Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
            }

            // Eviction should have removed some mappings, so count must be less than total allocations
            Assert.True(forth.BlockMappingCount <= 8, "LRU eviction did not enforce cache size");
            forth.BlockCacheSize = 4;
            Assert.Equal(4, forth.BlockCacheSize);
            forth.EvalAsync("0 BLOCK DROP DROP").Wait();
            Assert.True(forth.BlockMappingCount <= 4, "LRU eviction did not shrink after reducing cache size");
        }

        [Fact]
        public async Task EvictionDisposesMmfAccessorsWhenUsingBlockFile()
        {
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            try
            {
                var forth = new ForthInterpreter(blockCacheSize: 8);
                Assert.Equal(8, forth.BlockCacheSize);
                // Open directory for per-block storage (this disables single-file mmf, but ensures backing exists)
                Assert.True(await forth.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));

                const int allocs = 32;
                for (int i = 0; i < allocs; i++)
                {
                    // allocate block and drop c-addr/u
                    Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
                }

                // Ensure eviction happened
                Assert.True(forth.BlockMappingCount <= 8, "LRU eviction did not enforce cache size (per-block)");
                forth.BlockCacheSize = 4;
                Assert.Equal(4, forth.BlockCacheSize);
                forth.EvalAsync("0 BLOCK DROP DROP").Wait();
                Assert.True(forth.BlockMappingCount <= 4, "LRU eviction did not shrink after reducing cache size (per-block)");

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
