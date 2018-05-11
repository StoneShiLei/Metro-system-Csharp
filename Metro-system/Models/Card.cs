using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Models
{
    public class Card
    {
        public string CardId { get; set; }
        public string CardBalance { get; set; }
        public CardTypeEnum CardType { get; set; }
        public CardStatusEnum CardStatus { get; set; }
        public string StartStation { get; set; }
        public string EndStation { get; set; }

        public Card(string CardId)
        {
            this.CardId = CardId;
            this.CardType = CardTypeEnum.NORMAL;
            this.CardBalance = "0";
            this.CardStatus = CardStatusEnum.OUT;
            this.StartStation = null;
            this.EndStation = null;
        }
    }
}