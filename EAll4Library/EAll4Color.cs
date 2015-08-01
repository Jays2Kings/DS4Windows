namespace EAll4Windows
{
    public struct EAll4Color
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public EAll4Color(System.Drawing.Color c)
        {
            Red = c.R;
            Green = c.G;
            Blue = c.B;
        }
        public EAll4Color(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        public override bool Equals(object obj)
        {
            if (obj is EAll4Color)
            {
                EAll4Color dsc = ((EAll4Color)obj);
                return (this.Red == dsc.Red && this.Green == dsc.Green && this.Blue == dsc.Blue);
            }
            else
                return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 37; // prime

                result *= 397; // also prime (see note)
                result += Red.GetHashCode();

                result *= 397;
                result += Green.GetHashCode();

                result *= 397;
                result += Blue.GetHashCode();

                return result;
            }

        }
        public override string ToString()
        {
            return ("Red: " + Red + " Green: " + Green + " Blue: " + Blue);
        }
    }
}