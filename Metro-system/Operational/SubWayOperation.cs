using Metro_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using RO.Tools;

namespace Metro_system.Operational
{
    public class SubWayOperation
    {
        private static Dictionary<Station, List<List<Station>>> SubWayDict = new Dictionary<Station, List<List<Station>>>();
        private List<Station> OutList = new List<Station>();
        private static Log Log = new Log(typeof(SubWayOperation));
        static SubWayOperation()
        {
            foreach (Station start in MetroDataBuilderOperation.StationSet)
            {
                List<List<Station>> list = new List<List<Station>>();
                foreach (Station end in MetroDataBuilderOperation.StationSet)
                {
                    if (start.Equals(end))
                    {
                        continue;
                    }
                    SubWayOperation sw = new SubWayOperation();
                    sw.Calculate(start, end);
                    List<Station> tempList = new List<Station>();
                    tempList.AddRange(start.GetAllPassedStations(end));

                    for (int i = 0; i < tempList.Count; i++)
                    {
                        Station station = null;
                        foreach (Station s in MetroDataBuilderOperation.StationSet)
                        {
                            if (tempList[i].Name.Equals(s.Name))
                            {
                                station = s;
                                break;
                            }
                        }
                        tempList[i] = station;
                    }


                    list.Add(tempList);
                }
                SubWayDict.Add(start, list);
            }
        }



        #region
        ///<exception cref="Exception">起点终点不存在时抛出异常</exception>
        public static Dictionary<string,int> GetSubway(string startStation,string endStation)
        {
            Station start = new Station(startStation);
            Station end = new Station(endStation);
            try
            {
                
                if(!MetroDataBuilderOperation.StationSet.Contains(start) || !MetroDataBuilderOperation.StationSet.Contains(end))
                {
                    throw new ROException("起点或终点不存在");
                }
                List<Station> subway = SerchSubway(start, end);
                string result = ChangeStationStr(subway);
                Dictionary<string, int> resultDict = new Dictionary<string, int>{{ result, subway.Count }};

                return resultDict;
            }
            catch(Exception e)
            {
                Log.Error("起点或终点不存在", e);
                throw e;
            }
        }

        private static List<Station> SerchSubway(Station start, Station end)
        {
            List<Station> subway = null;
            List<List<Station>> list = SubWayDict[start];
            foreach (List<Station> l in list)
            {
                if (l[l.Count - 1].Equals(end))
                {
                    subway = l;
                }
            }
            return subway;
        }

        private static string ChangeStationStr(List<Station> subway)
        {
            StringBuilder result = new StringBuilder();
            for(int i=0;i<subway.Count;i++)
            {
                Station station = subway[i];
                if(i == 0)
                {
                    result.Append(station.Name).Append("到");

                }
                else if (i > 1)
                {
                    if (!CheckLine(station, subway[i-2]))
                    {
                        result.Append(subway[i-1].Name).Append(",").Append("换乘")
                                .Append(station.LineNumber[0]).Append("号线,")
                                .Append(subway[i-1].Name).Append(" 到 ");
                    }
                }

                if (i == subway.Count -1)
                {
                    result.Append(subway[i].Name);
                }
            }
            return result.ToString();
        }

        private static bool CheckLine(Station s1,Station s2)
        {
            if (s1.LineNumber.Count > 1 || s2.LineNumber.Count > 1)
            {
                //如果s1 or s2是换乘站  直接返回true
                return true;
            }
            return s1.LineNumber[0].Equals(s2.LineNumber[0]);
            //return Objects.equals(s1.getLineNumber().get(0), s2.getLineNumber().get(0));
        }

        #endregion




        #region
        private void Calculate(Station s1, Station s2)
        {
            if(OutList.Count == MetroDataBuilderOperation.TotalStation)
            {
                return;
            }
            if(!OutList.Contains(s1))
            {
                OutList.Add(s1);
            }
            if(s1.OrderSetDict.Count == 0)
            {
                List<Station> stations = GetAllLinkedStations(s1);
                foreach(Station s in stations)
                {
                    s1.GetAllPassedStations(s).Add(s);
                }
            }

            Station parent = GetShortestPath(s1);

            if(parent.Equals(s2))
            {
                return;
            }
            foreach(Station child in GetAllLinkedStations(parent))
            {
                if (OutList.Contains(child))
                {
                    continue;
                }
                int shortestPath = (s1.GetAllPassedStations(parent).Count - 1) + 1;
                if(s1.GetAllPassedStations(child).Contains(child))
                {
                    if((s1.GetAllPassedStations(child).Count -1) > shortestPath)
                    {
                        s1.GetAllPassedStations(child).Clear();
                        s1.GetAllPassedStations(child).UnionWith(s1.GetAllPassedStations(parent));
                        s1.GetAllPassedStations(child).Add(child);
                    }
                }
                else
                {
                    s1.GetAllPassedStations(child).UnionWith(s1.GetAllPassedStations(parent));
                    s1.GetAllPassedStations(child).Add(child);
                }
            }
            OutList.Add(parent);
            Calculate(s1, s2);
        }

        private List<Station> GetAllLinkedStations(Station station)
        {
            List<Station> linkedStations = new List<Station>();
            foreach(List<Station> line in MetroDataBuilderOperation.LineSet)
            {
                if(line.Contains(station))
                {
                    Station s = line[line.IndexOf(station)];
                    if(s.Prev != null)
                    {
                        linkedStations.Add(s.Prev);
                    }
                    if(s.Next != null)
                    {
                        linkedStations.Add(s.Next);
                    }
                }
            }
            return linkedStations;
        }

        private Station GetShortestPath(Station station)
        {
            int minpath = int.MaxValue;
            Station rets = null;
            foreach (Station s in station.OrderSetDict.Keys)
            {
                if (OutList.Contains(s))
                {
                    continue;
                }
                HashSet<Station> set = station.GetAllPassedStations(s);
                if (set.Count < minpath)
                {
                    minpath = set.Count;
                    rets = s;
                }
            }
            return rets;
        }

        #endregion

    }
}