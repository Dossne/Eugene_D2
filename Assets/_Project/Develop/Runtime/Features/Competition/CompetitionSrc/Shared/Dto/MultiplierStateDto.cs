namespace Features.Competition
{
    public struct MultiplierStateDto
    {
        public int multiplierPrevNum;
        public int multiplierPrevIdx;
        public int multiplierCurNum;
        public int multiplierCurIdx;


        public bool IsMultiplierChanged()
        {
            return multiplierPrevNum != multiplierCurNum;
        }
    }
}