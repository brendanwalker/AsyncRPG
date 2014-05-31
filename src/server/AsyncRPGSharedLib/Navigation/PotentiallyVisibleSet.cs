using AsyncRPGSharedLib.Utility;
using System.Collections;
using System.Text;

namespace AsyncRPGSharedLib.Navigation
{
    public class PotentiallyVisibleSet
    {
        private uint m_setSize;
        private BitArray m_visibilityFlags;

        public PotentiallyVisibleSet()
        {
            m_setSize = 0;
            m_visibilityFlags = null;
        }

        public PotentiallyVisibleSet(uint setSize)
        {
            m_setSize = setSize;
            m_visibilityFlags = new BitArray(BitsNeededForSetSize(setSize)); // All bits set to false
        }

        public PotentiallyVisibleSet(PotentiallyVisibleSet pvs)
        {
            m_setSize = pvs.m_setSize;
            m_visibilityFlags = new BitArray(pvs.m_visibilityFlags);
        }

        public bool Equals(PotentiallyVisibleSet other)
        {
            if (m_setSize != other.m_setSize)
            {
                return false;
            }

            if ((m_visibilityFlags == null && other.m_visibilityFlags != null) ||
                (m_visibilityFlags != null && other.m_visibilityFlags == null))
            {
                return false;
            }
            else if (m_visibilityFlags != null && other.m_visibilityFlags != null)
            {
                if (m_visibilityFlags.Length != other.m_visibilityFlags.Length)
                {
                    return false;
                }
                else
                {
                    for (int bitIndex = 0; bitIndex < m_visibilityFlags.Length; bitIndex++)
                    {
                        if (m_visibilityFlags[bitIndex] != other.m_visibilityFlags[bitIndex])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static PotentiallyVisibleSet FromCompressedRawByteArray(uint setSize, byte[] compressedBytes)
        {
            PotentiallyVisibleSet pvs = new PotentiallyVisibleSet();
            byte[] uncompressedBytes =
                CompressionUtilities.RunLengthDecodeByteArray(
                    compressedBytes, CompressionUtilities.eEncodeType.Ones, null);

            pvs.m_visibilityFlags = new BitArray(uncompressedBytes);
            pvs.m_visibilityFlags.Length = BitsNeededForSetSize(setSize);
            pvs.m_setSize = setSize;

            return pvs;
        }

        public byte[] ToCompressedRawByteArray()
        {
            int bytesNeeded = (m_visibilityFlags.Length + 7) / 8;
            byte[] uncompressedBytes = new byte[bytesNeeded];

            m_visibilityFlags.CopyTo(uncompressedBytes, 0);

            return CompressionUtilities.RunLengthEncodeByteArray(uncompressedBytes, CompressionUtilities.eEncodeType.Ones, null);
        }

        public void ToString(StringBuilder report)
        {
            report.AppendLine("PVS:");

            for (uint pvsCellIndex = 0; pvsCellIndex < m_setSize; pvsCellIndex++)
            {
                for (uint otherPvsCellIndex = 0; otherPvsCellIndex < m_setSize; otherPvsCellIndex++)
                {
                    bool bit = CanCellSeeOtherCell(pvsCellIndex, otherPvsCellIndex);

                    report.Append(bit ? '1' : '0');
                }

                report.Append('\n');
            }
        }

        public void SetCellCanSeeOtherCell(uint pvsCellIndex, uint otherPvsCellIndex)
        {
            // If CellA can see CellB then CellB can see CellA
            if (otherPvsCellIndex != pvsCellIndex)
            {
                int bitIndex = GetCellBitIndexForOtherCell(pvsCellIndex, otherPvsCellIndex);
                int otherBitIndex = GetCellBitIndexForOtherCell(otherPvsCellIndex, pvsCellIndex);

                m_visibilityFlags[bitIndex] = true;
                m_visibilityFlags[otherBitIndex] = true;
            }
        }

        public bool CanCellSeeOtherCell(uint pvsCellIndex, uint otherPvsCellIndex)
        {
            bool canSee = false;

            if (pvsCellIndex != otherPvsCellIndex)
            {
                int bitIndex = GetCellBitIndexForOtherCell(pvsCellIndex, otherPvsCellIndex);

                canSee = m_visibilityFlags[bitIndex];
            }
            else
            {
                canSee = true;
            }

            return canSee;
        }

        private int GetCellBitIndexForOtherCell(uint pvsCellIndex, uint otherPvsCellIndex)
        {
            // Bits past the omitted pvsCellIndex bit are shifted down by one
            uint cellBaseBitIndex = pvsCellIndex * (m_setSize - 1);
            uint otherCellBitOffset = (otherPvsCellIndex < pvsCellIndex) ? otherPvsCellIndex : otherPvsCellIndex - 1;

            return (int)(cellBaseBitIndex + otherCellBitOffset);
        }

        private static int BitsNeededForSetSize(uint setSize)
        {
            // A cell doesn't need a pvs bit for itself since a cell can see itself
            return (int)(setSize * (setSize - 1));
        }
    }
}
