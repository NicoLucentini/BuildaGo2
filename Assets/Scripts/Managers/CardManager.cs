using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    public static System.Action OnEndCardDraw;

    public List<Card> cards = new();
    public List<Card> hand = new();
    public List<Card> discard = new();
    public List<Card> pile = new();

    public int handAmount = 5;


    public Transform pileTransform;
    public Transform discardTransform;
    public Transform handTransform;
    public Canvas canvasParent;

    public HorizontalLayoutGroup horizontalLayoutGroup;

    public List<Sprite> workerPortraits = new();

    public Card genericCardPrefab;
    private void Awake()
    {
        instance = this;
    }

    public void AddCardToCards(Card card) {
        cards.Add(card);
    }
    public Card CreateCard(CardSO cardSo)
    {
        var c = Instantiate(genericCardPrefab, transform);
        c.Setup(cardSo);
        AddCardToCards(c);
        return c;
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            //Initial Draw
            pile.AddRange(cards);
            ShuffleAndRearmPile();

        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            TurnDrawCards();
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            EndTurn();
        }
    }
    public void InitialDraw() {
        pile.AddRange(cards);
        ShuffleAndRearmPile();
    }
    public void TurnDrawCards() {
        DrawCardsIntoHand(handAmount);
    }
    public void DestroyCard(Card card)
    {
        if (hand.Contains(card)) hand.Remove(card);
        if (cards.Contains(card)) cards.Remove(card);
        Destroy(card.gameObject);
    }
   
    public void EndTurn() {
        foreach (var c in hand) {
            AddCardToDiscardPile(c, false);
        }
        hand.Clear();
    }
    void DrawCardsIntoHand(int amount) {
        StartCoroutine(DcIntoHand(amount));
    }
    public void DrawCardsIntoHandWihoutDiscard(int amount) {
        StartCoroutine(CTDrawCardsIntoHandWihoutDiscard(amount));
    }
    IEnumerator CTDrawCardsIntoHandWihoutDiscard(int amount) {
        for (int i = 0; i < amount; i++)
        {
            yield return DrawCardFromPileToHand();
        }
    }
    IEnumerator DcIntoHand(int amount) {

        int initialDraw = amount;
        int handCount = hand.Count;
        if(handCount > 0)
        { 
            for (int i = handCount -1; i >= 0; i--) {
                if (!hand[i].retain)
                    AddCardToDiscardPile(hand[i]);
            }
        }
        initialDraw -= hand.Count;
        for (int i = 0; i < initialDraw; i++) {
            yield return DrawCardFromPileToHand();
        }
        OnEndCardDraw?.Invoke();
    }
    void ShuffleAndRearmPile() {
        pile.AddRange(discard);
        discard.Clear();
        pile.Shuffle();
        pile.ForEach(x =>
        {
            x.transform.SetParent(pileTransform);
            x.transform.position = pileTransform.position;
            x.cardPlace = CardPlace.PILE;
        });
    }
    IEnumerator DrawCardFromPileToHand() {
        if (pile.Count == 0) {
            ShuffleAndRearmPile();
        }
        var card = pile.Random();
        pile.Remove(card);
        hand.Add(card);
        card.gameObject.SetActive(true);
        card.cardPlace = CardPlace.HAND;
        yield return StartCoroutine(CardAnimation(card, handTransform, .5f, () => { card.transform.SetParent(handTransform); }));
    }
    public void AddCardToHand(Card card, bool removeFromPile = true) {
        if (removeFromPile) {
            if (pile.Contains(card)) pile.Remove(card);
        }
        hand.Add(card);
        card.gameObject.SetActive(true);
        card.cardPlace = CardPlace.HAND;
        StartCoroutine(CardAnimation(card, handTransform, .5f, () => { card.transform.SetParent(handTransform); }));
    }
    public void AddCardToDiscardPile(Card card, bool removeFromHand = true) {
        if(hand.Contains(card) && removeFromHand) { hand.Remove(card); }

        discard.Add(card);
        card.cardPlace = CardPlace.DISCARD;
        StartCoroutine(CardAnimation(card, discardTransform, .25f, 
             () => { card.transform.SetParent(discardTransform);
                 card.gameObject.SetActive(false);
             }));
    }
    public void AddCardToPile(Card card, bool removeFromOthers = true) {
        if (removeFromOthers) {
            if (hand.Contains(card)) { hand.Remove(card); }
            if (discard.Contains(card)) { discard.Remove(card); }
        }
        if (pile.Contains(card)) return;

        pile.Add(card);
        StartCoroutine(CardAnimation(card, pileTransform, .25f,
             () => {
                 card.transform.SetParent(pileTransform);
                 card.gameObject.SetActive(false);
             }));
        card.cardPlace = CardPlace.PILE;
    }
    IEnumerator CardAnimation(Card card,Transform targetTransform, float duration, Action onAnimationEnd)
    {
        var startPos = card.transform.position;
        var endPos = targetTransform.position + new Vector3(card.GetComponent<RectTransform>().rect.width, 0, 0) * (hand.Count - 1);
        card.transform.localScale = Vector3.one * 0.25f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            card.transform.position = Vector3.Slerp(startPos, endPos, timer / duration);
            card.transform.localScale = Vector3.Slerp(Vector3.one * 0.3f, Vector3.one, timer / duration);
            yield return null;
        }
        card.transform.position = endPos;
        card.transform.localScale = Vector3.one;
        yield return 0.1f;
        onAnimationEnd?.Invoke();
    }
}
