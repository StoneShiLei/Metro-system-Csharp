using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Models
{
    public class Station
    {
        public string Name { get; set; }
        public Station Prev { get; set; }
        public Station Next { get; set; }
        public List<int> LineNumber { get; set; }

        public Dictionary<Station, HashSet<Station>> OrderSetDict { get; set; }

        public Station(string name)
        {
            this.Name = name;
            this.LineNumber = new List<int>();
            this.OrderSetDict = new Dictionary<Station, HashSet<Station>>();
        }

        public HashSet<Station> GetAllPassedStations(Station station)
        {
            if (!OrderSetDict.TryGetValue(station, out HashSet<Station> set))
            {
                set = new HashSet<Station> { this };
                OrderSetDict.Add(station, set);
            }
            return OrderSetDict[station];
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            else if (obj is Station s)
            {
                return s.Name.Equals(this.Name);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}