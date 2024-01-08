namespace Map
{
    public struct Region
    {
        #region Biome Factors
        public float desiredElavation;
        public float desiredMoisture;
        public float desiredTemperature;
        #endregion

        #region Water Features
        public float riverFrequency;
        public float riverSize;
        public float lakeFrequency;
        #endregion

        // Constructor
        public Region(float desiredElavation, float desiredMoisture, float desiredTemperature, float riverFrequency, float riverSize, float lakeFrequency)
        {
            this.desiredElavation = desiredElavation;
            this.desiredMoisture = desiredMoisture;
            this.desiredTemperature = desiredTemperature;
            this.riverFrequency = riverFrequency;
            this.riverSize = riverSize;
            this.lakeFrequency = lakeFrequency;
        }
    }
}
