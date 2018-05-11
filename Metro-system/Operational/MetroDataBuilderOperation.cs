using Metro_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Operational
{
    public static class MetroDataBuilderOperation
    {
        private static List<Station> line1 = new List<Station>();
        public static List<Station> Line1
        {
            get
            {
                return line1;
            }
            set
            {
                line1 = value;
            }
        }

        private static List<Station> line2 = new List<Station>();
        public static List<Station> Line2
        {
            get
            {
                return line2;
            }
            set
            {
                line2 = value;
            }
        }

        private static List<Station> line3 = new List<Station>();
        public static List<Station> Line3
        {
            get
            {
                return line3;
            }
            set
            {
                line3 = value;
            }
        }

        public static List<Station> TotalLine { get; set; } = new List<Station>();
        public static HashSet<List<Station>> LineSet { get; set; } = new HashSet<List<Station>>();

        private static HashSet<Station> stationSet;
        public static HashSet<Station> StationSet
        {
            get
            {
                return stationSet;
            }
            set
            {
                stationSet = value;
            }
        }
        public static int TotalStation { get; set; }

        const string line1str = "后卫寨、三桥、皂河、枣园、汉城路、开远门、劳动路、玉祥门、洒金桥、北大街、五路口、朝阳门、康复路、通化门、" +
                     "万寿路、长乐坡、浐河、半坡、纺织城";
        const string line2str = "北客站、北苑、运动公园、行政中心、凤城五路、市图书馆、大明宫西、龙首原、安远门、北大街、钟楼、永宁门、南稍门、" +
                 "体育场、小寨、纬一街、会展中心、三爻、凤栖原、航天城、韦曲南";
        const string line3str = "鱼化寨、丈八北路、延平门、科技路、太白南路、吉祥村、小寨、大雁塔、北池头、青龙寺、延兴门、咸宁路、长乐公园、" +
                 "通化门、胡家庙、石家街、辛家庙、广泰门、桃花潭、浐灞中心、香湖湾、务庄、国际港务区、双寨、新筑、保税区";
        static MetroDataBuilderOperation()
        {
            BuildStation(line1str, ref line1);
            BuildStation(line2str, ref line2);
            BuildStation(line3str, ref line3);


            TotalLine.AddRange(Line1);
            TotalLine.AddRange(Line2);
            TotalLine.AddRange(Line3);
            StationSet = new HashSet<Station>(TotalLine);

            SetLineNumber(Line1, 1, ref stationSet);
            SetLineNumber(Line2, 2, ref stationSet);
            SetLineNumber(Line3, 3, ref stationSet);

            LineSet.Add(Line1);
            LineSet.Add(Line2);
            LineSet.Add(Line3);

            TotalStation = StationSet.Count;


        }

        private static void BuildStation(string lineStr,ref List<Station> line)
        {
            string[] lineArr = lineStr.Split('、');
            foreach(string s in lineArr)
            {
                line.Add(new Station(s));
            }
            for(int i=0;i<line.Count;i++)
            {
                if(i<line.Count-1)
                {
                    line[i].Next = line[i + 1];
                    line[i + 1].Prev = line[i];
                }
            }
        }

        private static void SetLineNumber(List<Station> line, int number,ref HashSet<Station> stationSet)
        {
            foreach (Station station in stationSet)
            {
                if (line.Contains(station))
                {
                    station.LineNumber.Add(number);
                }
            }
        }
    }
}