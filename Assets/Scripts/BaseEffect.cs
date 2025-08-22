using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public abstract class BaseEffect
{
    public string description;
    protected Card card;
    public void SetCard(Card card) {
        this.card = card;
    }
    public abstract BaseEffect Copy();
    public abstract void ApplyEffects();
    public virtual string GetDescription() => description;
    public virtual void Discard() { }
}
[Serializable]
public class LaborEffect : BaseEffect {
    public int amount;
    public LaborType type;

    public override string GetDescription() {
        description = "Does @amount @type labor";
        string val = description;
        return val.Replace("@amount", amount.ToString()).Replace("@type", type.ToString());
    }
    public override void ApplyEffects()
    {
        GameManager.instance.selectedBuilding.AssignLabor(type, amount);
        card.NextEffect();
    }


    public override BaseEffect Copy()
    {
        return new LaborEffect { amount = amount, type = type };
    }
}
[Serializable]
public class ImproveSalary : BaseEffect
{
    public int amount;

    public override void ApplyEffects()
    {
        card.NextEffect();
        throw new NotImplementedException();
    }

    public override BaseEffect Copy()
    {
        return new ImproveSalary { amount = amount };
    }
}
public class DiscardCard : BaseEffect
{
    public override void ApplyEffects()
    {
        Card.OnClickedCard += Discard;
    }
    public void Discard(Card x) {
        CardManager.instance.AddCardToDiscardPile(x);
        Card.OnClickedCard -= Discard;
        card.NextEffect();
    }
    public override BaseEffect Copy()
    {
        //throw new NotImplementedException();
        return new DiscardCard();
    }
}
public class DrawCard : BaseEffect {
    public int amount;
    public override BaseEffect Copy()
    {
        return new DrawCard { amount = amount };
    }

    public override void ApplyEffects()
    {
        CardManager.instance.DrawCardsIntoHandWihoutDiscard(amount);
        card.NextEffect();
    }
}
public class BlockCards : BaseEffect
{
    public int amount;

    private List<Card> blockedCards = new();

    public override void ApplyEffects()
    {
        CardManager.instance.hand
            .Where(x => x != card)
            .OrderBy(x=> new Random().Next())
            .Take(amount)
            .ToList()
            .ForEach(x=> 
        {
            x.SetBlocked(true);
            blockedCards.Add(x);
        });
        card.NextEffect();
    }
    public override void Discard()
    {
        blockedCards.ForEach(x =>x.SetBlocked(false));
        blockedCards.Clear();
    }
    public override BaseEffect Copy()
    {
        return new BlockCards { amount = amount };
    }
}
public class ChangeMoney : BaseEffect {
    public int amount;

    public override BaseEffect Copy() { return new ChangeMoney { amount = amount }; }
    public override void ApplyEffects()
    {
        GameManager.instance.AddMoney(amount);
    }
}
[Serializable]
public class ImproveLaborType : BaseEffect {
    public LaborType type;
    public int amount;
    public ImproveType improveType;
    public int weeksAmount;
    public CardAffects cardAffects;
    public bool permanent;
    public override BaseEffect Copy()
    {
        return new ImproveLaborType { type = type, 
            amount = amount, 
            improveType = improveType, 
            weeksAmount = weeksAmount, 
            cardAffects = cardAffects, 
            permanent = permanent};
    }
    public override string GetDescription()
    {
        string val = description;
        return val.Replace("@amount", amount.ToString())
            .Replace("@improve", ImproveDescription(improveType))
            .Replace("@type", type.ToString())
            .Replace("@affects", cardAffects.ToString())
            .Replace("@duration", permanent ? "PERMANENT" : $"{weeksAmount} weeks");
    }
    string ImproveDescription(ImproveType type) { 
        switch(type)
        {
            case ImproveType.MULTIPLICATION: return "Multiply"; 
            case ImproveType.SUM: return "Sum";
            case ImproveType.PERC: return "%";
            default:  return "Sum";
        }
    }

    public override void ApplyEffects()
    {
        var cardsToAffect = new List<Card>();

        if (cardAffects == CardAffects.ONLY_HAND)
        {
            cardsToAffect.AddRange(CardManager.instance.hand);
        }
        else if (cardAffects == CardAffects.THIS_ONE)
        {
            cardsToAffect.Add(card);
        }
        else {
            cardsToAffect.Add(card);
        }

        cardsToAffect.ForEach(c =>
        {
            foreach (var effect in c.effects.OfType<LaborEffect>().Where(x=>x.type == type)) // Filter only LaborEffect
            {

                int originalAmount = effect.amount;

                // Apply improvement
                effect.amount = improveType switch
                {
                    ImproveType.SUM => effect.amount + amount,
                    ImproveType.MULTIPLICATION or ImproveType.PERC => effect.amount * amount,
                    _ => effect.amount
                };

                c.UpdateUI();

                // If not permanent, schedule reset
                if (!permanent)
                {
                    EndTurnTimers.CreateTimer(weeksAmount, () =>
                    {
                        effect.amount = originalAmount;
                        c.UpdateUI();
                    });
                }
            }
        });
        card.NextEffect();
    }
}

public enum ImproveType { 
    MULTIPLICATION,
    SUM,
    PERC
}
public enum CardAffects { 
    ONLY_HAND,
    THIS_ONE,
    ALL
}