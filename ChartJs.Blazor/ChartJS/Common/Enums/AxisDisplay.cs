﻿using ChartJs.Blazor.ChartJS.Common.Enums.JsonConverter;
using Newtonsoft.Json;

namespace ChartJs.Blazor.ChartJS.Common.Enums
{
    /// <summary>
    /// As per documentation here https://www.chartjs.org/docs/latest/axes/#common-configuration
    /// </summary>
    public class AxisDisplay: ObjectEnum
    {
        /// <summary>
        /// Hidden
        /// </summary>
        public static AxisDisplay False => new AxisDisplay(false);

        /// <summary>
        /// Visible
        /// </summary>
        public static AxisDisplay True => new AxisDisplay(true);

        /// <summary>
        /// Visible only if at least one associated dataset is visible
        /// </summary>
        public static AxisDisplay Auto => new AxisDisplay("auto");


        private AxisDisplay(string stringValue) : base(stringValue) { }
        private AxisDisplay(bool boolValue) : base(boolValue) { }
    }
}
