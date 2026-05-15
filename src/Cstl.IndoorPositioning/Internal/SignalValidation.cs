namespace Cstl.IndoorPositioning.Internal
{
    internal static class SignalValidation
    {
        public static void ThrowIfInvalidRssi(int rssi, string parameterName)
        {
            if (rssi >= 0 || rssi < -127)
                throw new ArgumentOutOfRangeException(parameterName, "RSSI must be a negative dBm value between -127 and -1.");
        }

        public static void ThrowIfInvalidTxPower(int txPower, string parameterName)
        {
            if (txPower < -127 || txPower > 20 || txPower == 0)
                throw new ArgumentOutOfRangeException(parameterName, "TxPower must be between -127 and 20 dBm, excluding zero. Use null when unknown.");
        }
    }
}
