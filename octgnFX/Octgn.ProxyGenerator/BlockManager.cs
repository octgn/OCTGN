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
        private static BlockManager instance = null;

        private List<BlockDefinition> blocks = new List<BlockDefinition>();

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

        public void LoadBlocks(XmlNode node)
        {
            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(subNode))
                {
                    continue;
                }
                BlockDefinition blockDef = BlockDefinition.LoadSectionDefinition(subNode);
                AddBlock(blockDef);
            }
        }

        public void ClearBlocks()
        {
            blocks.Clear();
        }

        public static BlockManager GetInstance()
        {
            if (instance == null)
            {
                instance = new BlockManager();
            }
            return (instance);
        }
    }
}
