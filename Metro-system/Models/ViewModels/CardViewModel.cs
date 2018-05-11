using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metro_system.Models
{
    public class CardViewModel
    {
        public string CardId { get; set; }
        public string CardBalance { get; set; }
        public string CardType { get; set; }
        public string CardStatus { get; set; }

        public CardViewModel()
        {

        }
        public CardViewModel(Card card)
        {
            this.CardId = card.CardId;
            this.CardBalance = card.CardBalance;
            switch (card.CardStatus)
            {
                case CardStatusEnum.IN:
                    this.CardStatus = "已入站";
                    break;
                case CardStatusEnum.OUT:
                    this.CardStatus = "已出站";
                    break;
            }

            switch (card.CardType)
            {
                case CardTypeEnum.NORMAL:
                    this.CardType = "普通卡";
                    break;
                case CardTypeEnum.STUDENT:
                    this.CardType = "学生卡";
                    break;
                case CardTypeEnum.SENIOR:
                    this.CardType = "老年卡";
                    break;
                case CardTypeEnum.DISABILITY:
                    this.CardType = "残障卡";
                    break;
            }
        }
    }
}