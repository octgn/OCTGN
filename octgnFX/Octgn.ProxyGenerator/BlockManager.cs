using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    public class BlockManager
    {

        private List<BlockDefinition> blocks = new List<BlockDefinition>();


        public BlockManager()
        {

        }

        public List<BlockDefinition> GetBlocks()
        {
            return blocks;
        }
        
        public void AddBlock(BlockDefinition block)
        {
            if (ContainsBlock(block))
            {
                blocks.Remove(block);
            }
            blocks.Add(block);
        }

        public bool ContainsBlock(BlockDefinition block)
        {
            bool ret = false;

            foreach (BlockDefinition findBlock in blocks)
            {
                if (findBlock.id == block.id)
                {
                    return (true);
                }
            }

            return (ret);
        }

        public BlockDefinition GetBlock(string id)
        {
            BlockDefinition ret = null;

            foreach (BlockDefinition findBlock in blocks)
            {
                if (findBlock.id == id)
                {
                    return (findBlock);
                }
            }

            return (ret);
        }

        public void ClearBlocks()
        {
            blocks.Clear();
        }
    }
}
