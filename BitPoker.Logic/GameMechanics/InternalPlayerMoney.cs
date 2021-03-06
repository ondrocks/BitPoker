﻿namespace BitPoker.Logic.GameMechanics
{
	using System;

	using BitPoker.Logic.Players;
	using BitPoker.Models.Players;

	public class InternalPlayerMoney
    {
        public InternalPlayerMoney(Int64 startMoney)
        {
            this.Money = startMoney;
            this.NewHand();
            this.NewRound();
        }

        // Player money in the game
        public Int64 Money { get; set; }

        // The amount of money the player is currently put in the pot
        public Int64 CurrentlyInPot { get; private set; }

        // The amount of money the player is currently bet
        public Int64 CurrentRoundBet { get; private set; }

        // False when player folds
        public bool InHand { get; private set; }

        // Player action is expected (some other player raised)
        public bool ShouldPlayInRound { get; set; }

        public void NewHand()
        {
            this.CurrentlyInPot = 0;
            this.CurrentRoundBet = 0;
            this.InHand = true;
            this.ShouldPlayInRound = true;
        }

        public void NewRound()
        {
            this.CurrentRoundBet = 0;
            if (this.InHand)
            {
                this.ShouldPlayInRound = true;
            }
        }

        // TODO: Currently there is no limit in the raise amount as long as it is positive number
        public PlayerAction DoPlayerAction(PlayerAction action, Int64 maxMoneyPerPlayer)
        {
            if (action.Type == PlayerActionType.Raise)
            {
                this.CallTo(maxMoneyPerPlayer);

                if (this.Money <= 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                if (this.Money > action.Money)
                {
                    this.PlaceMoney(action.Money);
                }
                else
                {
                    // All-in
                    action.Money = this.Money;
                    this.PlaceMoney(action.Money);
                }
            }
            else if (action.Type == PlayerActionType.CheckCall)
            {
                this.CallTo(maxMoneyPerPlayer);
            }
            else //// PlayerActionType.Fold
            {
                this.InHand = false;
                this.ShouldPlayInRound = false;
            }

            return action;
        }

        public void NormalizeBets(Int64 moneyPerPlayer)
        {
            if (moneyPerPlayer < this.CurrentRoundBet)
            {
                var diff = this.CurrentRoundBet - moneyPerPlayer;
                this.PlaceMoney(-diff);
            }
        }

        private void PlaceMoney(Int64 money)
        {
            this.CurrentRoundBet += money;
            this.CurrentlyInPot += money;
            this.Money -= money;
        }

        private void CallTo(Int64 maxMoneyPerPlayer)
        {
            var moneyToPay = Math.Min(this.CurrentRoundBet + this.Money, maxMoneyPerPlayer);
            var diff = moneyToPay - this.CurrentRoundBet;
            this.PlaceMoney(diff);
        }
    }
}
