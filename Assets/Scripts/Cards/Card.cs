using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CardPlace { 
    HAND,
    PILE,
    DISCARD
}
public enum ApplyEffectsOn { 
    PLAY_CARD,
    ON_START_ROUND
}
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static System.Action<Card> OnClickedCard;


    [Header("Data")]
    public Data<int> moneyCost;
    public Data<int> energyCost;
    public string cardName;
    public bool retain;
    public bool isNegative;
    public bool removeOnPlay;
    public ApplyEffectsOn applyEffectsOn;

    [Header("Status")]
    public bool blocked;
    public CardPlace cardPlace;
    private Color initialColor;
    private CardSO cardSo;

    [Header("UI")]

    protected RectTransform rectTransform;
    protected Canvas canvas;
    protected CanvasGroup canvasGroup;
    public bool droppedOnValidTarget;
    protected Vector3 originalPosition;

    [SerializeField]
    private TextMeshProUGUI cardNameText;
    [SerializeField]
    private TextMeshProUGUI cardEnergyCostText;
    [SerializeField]
    private TextMeshProUGUI cardDescription;
    [SerializeField]
    private Image cardBackground;



    [SerializeReference]public List<BaseEffect> effects = new();

    protected void Awake() {
        rectTransform = GetComponent<RectTransform>();
        //canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        cardNameText.text = cardName;

        energyCost.onValueChanged += (x) => cardEnergyCostText.text = x.ToString();
        initialColor = cardBackground.color;

    }
    void Start() {
        canvas = CardManager.instance.canvasParent;
    }
    public void Setup(CardSO cardSo) {
        this.cardSo = cardSo;
        cardName = cardSo.cardName;
        retain = cardSo.retain;
        removeOnPlay = cardSo.removeOnPlay;
        energyCost.Value = cardSo.energyCost;
        applyEffectsOn = cardSo.applyEffectsOn;
        isNegative = cardSo.isNegative;

        cardSo.effects.ForEach(x => {
            var y = x.Copy();
            effects.Add(y);
            y.description = x.description;
            y.SetCard(this);
        });

        string generalDescription = string.Join(", ", effects.Select(e => e.GetDescription()));
        cardDescription.text = cardSo.cardDescription + ".\n " + generalDescription;
        cardNameText.text = cardSo.cardName;
    }

    public void SetBlocked(bool isBlocked) {
        blocked = isBlocked;
        cardBackground.color = blocked ? Color.red : initialColor;
    }
    public void UpdateUI() {
        cardEnergyCostText.text = energyCost.Value.ToString();
        string generalDescription = string.Join(", ", effects.Select(e => e.GetDescription()));
        cardDescription.text = generalDescription;
    }

    public virtual void ApplyEffects() {
        Debug.Log("ApplyEffectsBase");
        GameManager.instance.ConsumeEnergy(energyCost.Value);
        if(applyEffectsOn == ApplyEffectsOn.PLAY_CARD)
        { 
            if (effects.Count > 0) {
                currentEffectIndex = 0;
                effects[currentEffectIndex].ApplyEffects();
            }
        }
        if(removeOnPlay)
        {
            effects.ForEach(x => x.Discard());
            CardManager.instance.DestroyCard(this);
        }
        else
        { 
            CardManager.instance.AddCardToDiscardPile(this, true);
        }
    }
    public virtual void ApplyEffectsOnStartTurn() {
        Debug.Log($"{cardName} ApplyEffectsOnStartTurn");
        if (applyEffectsOn == ApplyEffectsOn.ON_START_ROUND)
        {
            if (effects.Count > 0)
            {
                currentEffectIndex = 0;
                effects[currentEffectIndex].ApplyEffects();
            }
        }
    }
    private int currentEffectIndex;
    public void NextEffect() {
        if (currentEffectIndex >= effects.Count -1) return;

        currentEffectIndex++;
        effects[currentEffectIndex].ApplyEffects();
    }

    //UI

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (blocked) return;
        canvasGroup.blocksRaycasts = false;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (blocked) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("On End Drag");
        if(blocked) return;
        OnEndDragActions();
    }
    public void OnEndDragActions() {
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = originalPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickedCard?.Invoke(this);
    }
}

