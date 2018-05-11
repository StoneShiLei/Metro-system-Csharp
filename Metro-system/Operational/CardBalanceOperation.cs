using Metro_system.Models;
using Metro_system.Operations;
using RO.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Operational
{
    public static class CardBalanceOperation
    {
        private static Log Log = new Log(typeof(CardBalanceOperation));
        private static Object CardBalanceLocker = new Object();

        ///<exception cref="Exception">查询余额时抛出异常</exception>
        public static decimal? GetCardBalance(string cardId,out Card card)
        {
            card = CardOperation.GetCard(cardId);
            if(card == null)
            {
                throw new ROException("公交卡不存在");
            }

            decimal balance = decimal.Parse(card.CardBalance);
            try
            {
                if(balance > 1000 || balance < 0)
                {
                    throw new ROException("卡内余额异常:" + balance);
                }
                return balance;
            }
            catch (Exception e)
            {
                Log.Error("查询余额失败", e);
                throw e;
            }
        }

        ///<exception cref="Exception">储值余额时抛出异常</exception>
        public static decimal? Recharge(string cardId,decimal amount)
        {
            try
            {
                lock(CardBalanceLocker)
                {
                    if (amount <= 0 || amount % 10 != 0)
                    {
                        throw new ROException("充值金额为0或不为10的整数倍");
                    }
                    decimal? balance = GetCardBalance(cardId,out Card card);
                    balance += amount;
                    if (balance > 1000 || balance < 0)
                    {
                        throw new ROException("充值后卡内余额大于1000或小于0");
                    }
                    MongoOperation.UpdateDocument(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", cardId } },
                        new MongoUpdateModel(MongoUpdateTypeEnum.Set, "CardBalance", balance.ToString()));
                    return balance;
                }
            }
            catch(Exception e)
            {
                Log.Error("充值失败", e);
                throw e;
            }
        }

        ///<exception cref="Exception">入闸时抛出异常</exception>
        public static void In (string cardId,string startStation)
        {
            try
            {
                lock(CardBalanceLocker)
                {
                    decimal? balance = GetCardBalance(cardId,out Card card);
                    if (card.CardStatus == CardStatusEnum.OUT)
                    {
                        if (!MetroDataBuilderOperation.StationSet.Contains(new Station(startStation)))
                        {
                            throw new ROException("起始站不存在");
                        }
                        if (balance < 1)
                        {
                            throw new ROException("卡内余额不足，无法入站");
                        }

                        card.CardBalance = balance.ToString();
                        card.CardStatus = CardStatusEnum.IN;
                        card.StartStation = startStation;
                        MongoOperation.ReplaceDocument(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", card.CardId } }, card);
                    }
                    else if(card.CardStatus == CardStatusEnum.IN)
                    {
                        balance -= 5m;
                        if (balance < 1)
                        {
                            throw new ROException("卡内余额不足，无法入站");
                        }
                        card.CardBalance = balance.ToString();
                        card.StartStation = startStation;
                        MongoOperation.ReplaceDocument(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", card.CardId } }, card);

                    }
                    
                }
            }
            catch(Exception e)
            {
                Log.Error("入站失败", e);
                throw e;
            }
        }

        ///<exception cref="Exception">出闸时抛出异常</exception>
        public static void Out (string cardId,string endStation)
        {
            try
            {
               lock(CardBalanceLocker)
                {
                    decimal? balance = GetCardBalance(cardId, out Card card);

                    if (card.CardStatus == CardStatusEnum.IN)
                    {
                        if (!MetroDataBuilderOperation.StationSet.Contains(new Station(endStation)))
                        {
                            throw new ROException("终点站不存在");
                        }
                        card.EndStation = endStation;
                        Dictionary<string, int> subWay = SubWayOperation.GetSubway(card.StartStation, card.EndStation);
                        int stations = subWay.First().Value;
                        decimal subBalance = Calculate(card, stations);
                        balance -= subBalance;
                        if (balance < 0)
                        {
                            throw new ROException("余额不足，无法出站，请先充值后出站");
                        }
                        else
                        {
                            card.CardStatus = CardStatusEnum.OUT;
                            card.CardBalance = balance.ToString();
                            MongoOperation.ReplaceDocument(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", card.CardId } }, card);
                        }
                    } else if(card.CardStatus == CardStatusEnum.OUT)
                    {
                        balance -= 5m;
                        if (balance < 0)
                        {
                            throw new ROException("余额不足，无法出站，请先充值后出站");
                        }
                        else
                        {
                            card.CardBalance = balance.ToString();
                            MongoOperation.ReplaceDocument(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", card.CardId } }, card);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("出站失败", e);
                throw e;
            }
        }

        private static decimal Calculate(Card card,int stations)
        {
            decimal result = 0m;

            if(stations <= 5)
            {
                result += 1;
            }
            else if(stations > 5 && stations <= 10)
            {
                result += 2;
            }
            else if(stations > 10)
            {
                int tmp = stations - 10;
                double num = tmp / 5.0;
                result = 3m + Math.Floor((decimal)num);
            }

            switch(card.CardType)
            {
                case CardTypeEnum.NORMAL:
                    return result;
                case CardTypeEnum.STUDENT:
                    return result * 0.5m;
                case CardTypeEnum.SENIOR:
                    //TODO
                    return 0m;
                case CardTypeEnum.DISABILITY:
                    //TODO
                    return 0m;
            }
            return result;
        }
    }
}