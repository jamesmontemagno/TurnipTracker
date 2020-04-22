﻿using System;

namespace TurnipTracker.Shared
{
    /// <summary>
    /// This is information that will be pulled down for friends list
    /// </summary>
    public class User
    {
        public string Name { get; set; }
        public string IslandName { get; set; }

        //could be tiny int?
        public int Fruit { get; set; }
        public string TimeZone { get; set; }
        public string Status { get; set; }

        //could be small int
        public int AMPrice { get; set; }
        public int PMPrice { get; set; }

        public int BuyPrice { get; set; }

        //could be smallint
        public int DayOfYear { get; set; }
        public int Year { get; set; }
    }
}