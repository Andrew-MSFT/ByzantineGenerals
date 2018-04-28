using System;
using System.Collections.Generic;
using System.Linq;

namespace ByzantineGenerals.PowBlockchain
{
    public class Blockchain
    {
        public static readonly byte[] GenesisBlockHash = HashUtilities.ComputeSHA256("Byzantine generals blockchain example");

        private List<Block> _blocks { get; set; } = new List<Block>();

        public int Count { get { return _blocks.Count; } }
        public Block this[int i] { get { return _blocks[i]; } }

        public Blockchain()
        {
            Block genesisBlock = Block.MineNewBlock(new List<Message>(), GenesisBlockHash);
            _blocks.Add(genesisBlock);
        }

        public Blockchain(Blockchain blockchain)
        {
            foreach (Block block in blockchain.GetBlocks())
            {
                Block newBlock = block;
                _blocks.Add(newBlock);
            }
        }

        public void Add(Block block)
        {
            _blocks.Add(block);
        }

        public bool IsValidBlock(Block block)
        {
            foreach (Message message in block.Messages)
            {
                for (int i = _blocks.Count - 1; i >= 0; i--)
                {
                    //_blocks[i].ContainsOutTransaction(message.Inputs.)
                }
            }

            return true;
        }

        public bool ContainsBlock(Block block)
        {
            byte[] blockHash = block.ComputeSHA256();

            for (int i = _blocks.Count - 1; i >= 0; i--)
            {
                Block currentBlock = _blocks[i];
                byte[] currentHash = currentBlock.ComputeSHA256();
                if (currentHash.SequenceEqual(blockHash))
                {
                    return true;
                }
            }
            return false;
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

        public bool IsValidTransaction(Message tx)
        {
            List<MessageIn> inputsToValidate = new List<MessageIn>(tx.Inputs);

            for (int i = _blocks.Count - 1; i > 0; i--)
            {
                var block = _blocks[i];
                for (int x = 0; x < inputsToValidate.Count; x++)
                {
                    MessageIn currentInput = inputsToValidate[x];
                    if (block.ContainsMessageOut(currentInput.PreviousMessageHash, out MessageOut previousOutput))
                    {
                        //inputsToValidate.RemoveAt(x--);
                        //bool isValidInput = Transaction.InputMatchesOutput(previousOutput, currentInput);

                        //if (!isValidInput)
                        //{
                        //    return false;
                        //}
                    }
                }

            }

            return inputsToValidate.Count == 0;
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
