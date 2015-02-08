using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Library
{
    public class LatencyCounter
    {
        long[] _LatencyReadings;
        int _bufferPointer = 0;
        int _bufferEndPointer = 0;

        public LatencyCounter(int bufferSize)
        {
            _LatencyReadings = new long[bufferSize];
        }


        public double Latency
        {
            get
            {
                if (_bufferEndPointer == 0)
                {
                    return 0;
                }

                long result = 0L;

                for (int pos = 0; pos < _bufferEndPointer; ++pos)
                {
                    result += _LatencyReadings[pos];
                }

                return (double)result / (_bufferEndPointer + 1);
            }
        }

        public void PushLatencyReading(long reading)
        {
            if (_bufferPointer > _LatencyReadings.Length - 1)
            {
                _bufferPointer = 0;
            }

            _LatencyReadings[_bufferPointer] = reading;

            if (_bufferEndPointer < _LatencyReadings.Length - 1)
            {
                _bufferEndPointer = _bufferPointer;
            }

            _bufferPointer++;
        }

    }
}
