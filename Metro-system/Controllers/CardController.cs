using Metro_system.Models;
using Metro_system.Operational;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Metro_system.Controllers
{
    public class CardController : Controller
    {
        // GET: Card
        public ActionResult Index()
        {
            CardViewModel model = null; ;
            Card card = null;
            if(Request.Cookies["CardId"] != null)
            {
                card = CardOperation.GetCard(Request.Cookies["CardId"].Value);

                if (card != null)
                {
                    model = new CardViewModel(card);
                }
                else
                {
                    model = null;
                }
            }
            return View(model);
        }

        public ContentResult CreatCard()
        {
            Response.ContentType = "application/json";
            ResultInfo<Card> result;
            try
            {
                if (Request.Cookies["CardId"] != null)
                {
                    throw new Exception("已有公交卡，不可重复创建，请先删除现有公交卡");
                }
                Card card = CardOperation.CreatCard();
                result = new ResultInfo<Card>(ResultInfoEnum.OK, "访问成功", Request.Url.ToString(), card);

                HttpCookie cookie = new HttpCookie("CardId")
                {
                    Value = card.CardId,
                    Expires = DateTime.MaxValue
                };
                Response.Cookies.Add(cookie);

                return Content(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                result = result = new ResultInfo<Card>(ResultInfoEnum.ERROR, e.Message, Request.Url.ToString(), null);
                return Content(JsonConvert.SerializeObject(result));
            }
        }

        public ContentResult DestroyCard()
        {
            Response.ContentType = "application/json";
            ResultInfo<Card> result;
            try
            {
                if(Request.Cookies["CardId"] == null)
                {
                    throw new Exception("没有公交卡可供销毁");
                }

                HttpCookie cookie = new HttpCookie("CardId")
                {
                    Expires = DateTime.Now.AddDays(-2)
                };
                Response.Cookies.Add(cookie);

                CardOperation.DestroyCard(Request.Cookies["CardId"].Value);
                result = new ResultInfo<Card>(ResultInfoEnum.OK, "访问成功", Request.Url.ToString(), null);

                
                return Content(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                HttpCookie cookie = new HttpCookie("CardId")
                {
                    Expires = DateTime.Now.AddDays(-2)
                };
                Response.Cookies.Add(cookie);
                result = result = new ResultInfo<Card>(ResultInfoEnum.ERROR, e.Message, Request.Url.ToString(), null);
                return Content(JsonConvert.SerializeObject(result));
            }
        }
    }
}