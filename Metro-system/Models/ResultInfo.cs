using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Models
{
    public class ResultInfo<T>
    {
        public ResultInfoEnum ResultInfoEnum { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public T Data { get; set; }

        public ResultInfo(ResultInfoEnum resultInfoEnum,string message,string url,T data)
        {
            this.ResultInfoEnum = resultInfoEnum;
            this.Message = message;
            this.Url = url;
            this.Data = data;
        }
    }

    public enum ResultInfoEnum
    {
        OK,ERROR,
    }
}