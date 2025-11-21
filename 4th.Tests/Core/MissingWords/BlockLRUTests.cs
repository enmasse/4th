using System;
using System.IO;
using System.Reflection;
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
            var forth = new ForthInterpreter();
            const int allocs = 200;
            for (int i = 0; i < allocs; i++)
            {
                // allocate block i
                Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
            }

            // use reflection to inspect private _blockAddrMap
            var t = typeof(ForthInterpreter);
            var fmap = t.GetField("_blockAddrMap", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(fmap);
            var map = (System.Collections.IDictionary)fmap.GetValue(forth)!;
            // Eviction should have removed some mappings, so count must be less than total allocations
            Assert.True(map.Count < allocs, "LRU eviction did not remove older block mappings");
        }

        [Fact]
        public async Task EvictionDisposesMmfAccessorsWhenUsingBlockFile()
        {
            var dir = Path.Combine(Path.GetTempPath(), "forth-blocks-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            try
            {
                var forth = new ForthInterpreter();
                // Open directory for per-block storage (this disables single-file mmf, but ensures backing exists)
                Assert.True(await forth.EvalAsync($"\"{dir}\" OPEN-BLOCK-DIR"));

                const int allocs = 150;
                for (int i = 0; i < allocs; i++)
                {
                    // allocate block and drop c-addr/u
                    Assert.True(await forth.EvalAsync($"{i} BLOCK DROP DROP"));
                }

                var t = typeof(ForthInterpreter);
                var fmap = t.GetField("_blockAddrMap", BindingFlags.Instance | BindingFlags.NonPublic);
                var facc = t.GetField("_mmfAccessors", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(fmap);
                Assert.NotNull(facc);
                var mapFinal = (System.Collections.IDictionary)fmap.GetValue(forth)!;
                var accs = (System.Collections.IDictionary)facc.GetValue(forth)!;

                // Ensure eviction happened
                Assert.True(mapFinal.Count < allocs, "LRU eviction did not remove older block mappings (per-block)");

                // Accessor dictionary size should not exceed mapping count
                Assert.True(accs.Count <= System.Math.Max(1, mapFinal.Count), "MMF accessors remain larger than block mappings after eviction");
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }
    }
}
