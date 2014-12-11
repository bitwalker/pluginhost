using System;
using System.Text;
using System.Security.Cryptography;

namespace SharpTools.Configuration.Crypto.Engines
{
    /// <summary>
    /// An implementation of <see cref="ICryptoTransform"/> for the Salsa20 streaming cipher algorithm.
    /// </summary>
    internal sealed class Salsa20CryptoTransform : ICryptoTransform
    {
        private static readonly byte[] SIGMA = Encoding.ASCII.GetBytes("expand 32-byte k");
        private static readonly byte[] TAU   = Encoding.ASCII.GetBytes("expand 16-byte k");

        private readonly int _rounds;
        private uint[] _state;

        public bool CanReuseTransform { get { return false; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public int InputBlockSize { get { return 64; } }
        public int OutputBlockSize { get { return 64; } }

        public Salsa20CryptoTransform(byte[] key, byte[] iv, int rounds)
        {
            if (key.Length != 16 && key.Length != 32)
                throw new ArgumentException("Invalid key length. Must be 16 or 32 bytes.", "key");
            if (iv.Length != 8)
                throw new ArgumentException("Invalid initialization vector. Must be 8 bytes.", "iv");
            if (rounds < 0 || rounds%2 != 0)
                throw new ArgumentException("Rounds must be a positive, even integer. Recommended value is 20", "rounds");

            Initialize(key, iv);
            _rounds = rounds;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException("inputBuffer");
            if (inputOffset < 0 || inputOffset >= inputBuffer.Length)
                throw new ArgumentOutOfRangeException("inputOffset");
            if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length)
                throw new ArgumentOutOfRangeException("inputCount");
            if (outputBuffer == null)
                throw new ArgumentNullException("outputBuffer");
            if (outputOffset < 0 || outputOffset + inputCount > outputBuffer.Length)
                throw new ArgumentOutOfRangeException("outputOffset");
            if (_state == null)
                throw new ObjectDisposedException(GetType().Name);

            var output = new byte[64];
            var bytesTransformed = 0;

            while (inputCount > 0)
            {
                Hash(output, _state);

                _state[8] = AddOne(_state[8]);
                if (_state[8] == 0)
                {
                    // NOTE: stopping at 2^70 bytes per nonce is user's responsibility
                    _state[9] = AddOne(_state[9]);
                }

                var blockSize = Math.Min(64, inputCount);
                for (int i = 0; i < blockSize; i++)
                    outputBuffer[outputOffset + i] = (byte) (inputBuffer[inputOffset + i] ^ output[i]);
                bytesTransformed += blockSize;

                inputCount   -= 64;
                outputOffset += 64;
                inputOffset  += 64;
            }

            return bytesTransformed;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputCount < 0)
                throw new ArgumentOutOfRangeException("inputCount");

            var output = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        public void Dispose()
        {
            if (_state != null)
                Array.Clear(_state, 0, _state.Length);
            _state = null;
        }

        private static uint Rotate(uint v, int c)
        {
            return (v << c) | (v >> (32 - c));
        }

        private static uint Add(uint v, uint w)
        {
            return unchecked(v + w);
        }

        private static uint AddOne(uint v)
        {
            return unchecked(v + 1);
        }

        private void Hash(byte[] output, uint[] input)
        {
            uint[] state = (uint[]) input.Clone();

            for (int round = _rounds; round > 0; round -= 2)
            {
                state[4]  ^= Rotate(Add(state[0],  state[12]), 7);
                state[8]  ^= Rotate(Add(state[4],  state[0]),  9);
                state[12] ^= Rotate(Add(state[8],  state[4]),  13);
                state[0]  ^= Rotate(Add(state[12], state[8]),  18);
                state[9]  ^= Rotate(Add(state[5],  state[1]),  7);
                state[13] ^= Rotate(Add(state[9],  state[5]),  9);
                state[1]  ^= Rotate(Add(state[13], state[9]),  13);
                state[5]  ^= Rotate(Add(state[1],  state[13]), 18);
                state[14] ^= Rotate(Add(state[10], state[6]),  7);
                state[2]  ^= Rotate(Add(state[14], state[10]), 9);
                state[6]  ^= Rotate(Add(state[2],  state[14]), 13);
                state[10] ^= Rotate(Add(state[6],  state[2]),  18);
                state[3]  ^= Rotate(Add(state[15], state[11]), 7);
                state[7]  ^= Rotate(Add(state[3],  state[15]), 9);
                state[11] ^= Rotate(Add(state[7],  state[3]),  13);
                state[15] ^= Rotate(Add(state[11], state[7]),  18);
                state[1]  ^= Rotate(Add(state[0],  state[3]),  7);
                state[2]  ^= Rotate(Add(state[1],  state[0]),  9);
                state[3]  ^= Rotate(Add(state[2],  state[1]),  13);
                state[0]  ^= Rotate(Add(state[3],  state[2]),  18);
                state[6]  ^= Rotate(Add(state[5],  state[4]),  7);
                state[7]  ^= Rotate(Add(state[6],  state[5]),  9);
                state[4]  ^= Rotate(Add(state[7],  state[6]),  13);
                state[5]  ^= Rotate(Add(state[4],  state[7]),  18);
                state[11] ^= Rotate(Add(state[10], state[9]),  7);
                state[8]  ^= Rotate(Add(state[11], state[10]), 9);
                state[9]  ^= Rotate(Add(state[8],  state[11]), 13);
                state[10] ^= Rotate(Add(state[9],  state[8]),  18);
                state[12] ^= Rotate(Add(state[15], state[14]), 7);
                state[13] ^= Rotate(Add(state[12], state[15]), 9);
                state[14] ^= Rotate(Add(state[13], state[12]), 13);
                state[15] ^= Rotate(Add(state[14], state[13]), 18);
            }

            for (int index = 0; index < 16; index++)
                ToBytes(Add(state[index], input[index]), output, 4*index);
        }

        private void Initialize(byte[] key, byte[] iv)
        {
            _state = new uint[16];
            _state[1] = ToUInt32(key, 0);
            _state[2] = ToUInt32(key, 4);
            _state[3] = ToUInt32(key, 8);
            _state[4] = ToUInt32(key, 12);

            byte[] constants = key.Length == 32 ? SIGMA : TAU;
            int keyIndex = key.Length - 16;

            _state[11] = ToUInt32(key, keyIndex + 0);
            _state[12] = ToUInt32(key, keyIndex + 4);
            _state[13] = ToUInt32(key, keyIndex + 8);
            _state[14] = ToUInt32(key, keyIndex + 12);
            _state[0]  = ToUInt32(constants, 0);
            _state[5]  = ToUInt32(constants, 4);
            _state[10] = ToUInt32(constants, 8);
            _state[15] = ToUInt32(constants, 12);
            _state[6]  = ToUInt32(iv, 0);
            _state[7]  = ToUInt32(iv, 4);
            _state[8]  = 0;
            _state[9]  = 0;
        }

        private static uint ToUInt32(byte[] input, int inputOffset)
        {
            return unchecked((uint) (((input[inputOffset] | (input[inputOffset + 1] << 8)) |
                                      (input[inputOffset + 2] << 16)) |
                                      (input[inputOffset + 3] << 24)));
        }

        private static void ToBytes(uint input, byte[] output, int outputOffset)
        {
            unchecked
            {
                output[outputOffset]     = (byte) input;
                output[outputOffset + 1] = (byte) (input >> 8);
                output[outputOffset + 2] = (byte) (input >> 16);
                output[outputOffset + 3] = (byte) (input >> 24);
            }
        }
    }
}