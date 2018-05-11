using Metro_system.Models;
using Metro_system.Operations;
using RO.Tools;
using System;
using System.Collections.Generic;

namespace Metro_system.Operational
{
    public static class CardOperation
    {
        private static Object CardIdLocker = new Object();
        private static Log Log = new Log(typeof(CardOperation));

        ///<exception cref="Exception">新建公交卡插入失败时抛出异常</exception>
        public static Card CreatCard()
        {
            lock(CardIdLocker)
            {
                Card newCard;
                string newCardId;

                try
                {
                    Card PreviousCard = MongoOperation.FindLastByKey<Card>(MongoCollectionName.Cards, "CardId");

                    if (PreviousCard == null)
                    {
                        newCardId = "10000000";
                    }
                    else
                    {   
                        newCardId = (long.Parse(PreviousCard.CardId) + 1).ToString();
                    }
                    newCard = new Card(newCardId);
                    MongoOperation.InsertDocument(MongoCollectionName.Cards, newCard);
                    return newCard;
                }
                catch (Exception e)
                {
                    Log.Error("新建公交卡错误", e);
                    throw e;
                }
            }
        }

        public static Card GetCard(string cardId)
        {
            var resultList = MongoOperation.FindDocument<Card>(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", cardId } });
            if(resultList.Count == 0)
            {
                return null;
            }
            return resultList[0];
        }

        public static void DestroyCard(string cardId)
        {
            try
            {
                MongoOperation.RemoveDocument(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", cardId } });
            }
            catch(Exception e)
            {
                Log.Error("删除公交卡失败", e);
                throw e;
            }
        }
    }
}