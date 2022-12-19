using System;

namespace HomeControlFunctions.Models
{
    public class GasRecordDao
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public int? Value { get; set; }

        public string ValueRaw { get; set; }

        public string Image { get; set; }
    }
}
