using System;
using System.Collections.Generic;
using System.Linq;

namespace ByzantineGenerals.PowBlockchain
{
    class Blockchain
    {
        public static readonly byte[] GenesisBlockHash = HashUtilities.ComputeSHA256("Byzantine generals blockchain example");

        private List<Block> _blocks { get; set; } = new List<Block>();

        public int Count { get { return _blocks.Count; } }
        public Block this[int i] { get { return _blocks[i]; } }

        public Blockchain()
        {
            Block genesisBlock = Block.MineNewBlock(new List<Transaction>(), GenesisBlockHash);
            _blocks.Add(genesisBlock);
        }

        public Blockchain(List<Block> chain)
        {
            foreach (Block block in chain)
            {
                Block newBlock = block;
                _blocks.Add(newBlock);
            }
        }

        public void Add(Block block)
        {
            _blocks.Add(block);
        }

        public List<Block> GetBlocks()
        {
            List<Block> chainCopy = new List<Block>();
            foreach (Block block in _blocks)
            {
                Block newBlock = block;
                chainCopy.Add(newBlock);
            }

            return chainCopy;
        }

        public void CompareBlockchains(Blockchain chain, out bool match, out bool consistent)
        {
            consistent = ConsistentButNotFullMatch(chain);
            match = consistent ? ChainsMatch(chain) : false;
        }

        private bool ChainsMatch(Blockchain chain)
        {
            if (_blocks.Count != chain.Count)
            {
                return false;
            }
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (!_blocks[i].Equals(chain[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ConsistentButNotFullMatch(Blockchain chain)
        {
            int count = _blocks.Count <= chain.Count ? _blocks.Count : chain.Count;
            for (int i = 0; i < count; i++)
            {
                if (!_blocks[i].Equals(chain[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public Block LastBlock
        {
            get { return _blocks[_blocks.Count - 1]; }
        }

        public byte[] GetChainHash()
        {
            const int HashLength = 32;
            byte[] bytes = new byte[this._blocks.Count * HashLength];

            for (int i = 0; i < _blocks.Count; i++)
            {
                Block block = _blocks[i];
                byte[] hash = block.ComputeSHA256();
                Buffer.BlockCopy(hash, 0, bytes, i * HashLength, HashLength);
            }

            return HashUtilities.ComputeSHA256(bytes);
        }
    }
}
