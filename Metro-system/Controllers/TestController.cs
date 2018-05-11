using Metro_system.Models;
using Metro_system.Operational;
using Metro_system.Operations;
using RO.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Metro_system.Controllers
{
    public class TestController : Controller
    {
        //private static Log Log = new Log(typeof(TestController));
        // GET: Test
        public string Index()
        {
            //Log.Error("111111111111111111111111111111111111111111111111111111");
            //var dict = SubWayOperation.GetSubway("汉城路", "会展中心");

            //return dict.First().Key;
            return "111";
        }
    }
}