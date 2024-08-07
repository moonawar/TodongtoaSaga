using UnityEngine;
using DG.Tweening;
using System;

public class InteractableObject : Interactable
{
    [Header("Hint")]
    [SerializeField] private Transform interactHint;
    [SerializeField] private float hintOffset = 1f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float bounceAmplitude = 0.1f;
    [SerializeField] private float bounceDuration = 0.5f;

    private bool interactable = false;
    private SpriteRenderer hintSpriteRenderer;
    private Vector3 hintInitialPosition;
    private Tweener bounceAnimation;
    private Tween fadeAnimation;

    private Action onInteraction;
    public bool ShouldDissapear = false;
    private bool interacted = false;

    private void Awake()
    {
        if (!interactHint.TryGetComponent(out hintSpriteRenderer))
        {
            Debug.LogError("SpriteRenderer not found on interactHint. Please add a SpriteRenderer component.");
        }

        hintInitialPosition = interactHint.localPosition;
    }

    private void Start()
    {
        interactHint.gameObject.SetActive(false);
    }

    public override void OnInteractableEnter()
    {
        if (interacted) return;
        fadeAnimation?.Kill();

        interactHint.gameObject.SetActive(true);
        interactable = true;

        interactHint.localPosition = hintInitialPosition - new Vector3(0, hintOffset, 0);
        Color startColor = hintSpriteRenderer.color;
        startColor.a = 0f;
        hintSpriteRenderer.color = startColor;

        fadeAnimation = DOTween.Sequence()
            .Join(hintSpriteRenderer.DOFade(1f, fadeDuration))
            .Join(interactHint.DOLocalMove(hintInitialPosition, slideDuration).SetEase(Ease.OutBack))
            .OnComplete(() => {
                fadeAnimation = null;
                StartBounceAnimation();
            });
    }

    public override void OnInteractableExit()
    {
        if (interacted) return;
        fadeAnimation?.Kill();

        interactable = false;

        bounceAnimation?.Kill();

        fadeAnimation = DOTween.Sequence()
            .Join(hintSpriteRenderer.DOFade(0f, fadeDuration))
            .Join(interactHint.DOLocalMove(hintInitialPosition - new Vector3(0, hintOffset, 0), slideDuration).SetEase(Ease.InBack))
            .OnComplete(() => {
                interactHint.gameObject.SetActive(false); 
                fadeAnimation = null;
            });
    }

    private void StartBounceAnimation()
    {
        bounceAnimation = interactHint
            .DOLocalMoveY(hintInitialPosition.y + bounceAmplitude, bounceDuration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void AddInteractionListener(Action action)
    {
        interacted = false;
        onInteraction += action;
    }

    public void RemoveInteractionListener(Action action)
    {
        onInteraction -= action;
    }

    public void ClearInteractionListeners()
    {
        onInteraction = null;
    }

    public override void Interact()
    {
        if (!interactable) return;
        onInteraction?.Invoke();
        OnInteractableExit();
        interactable = false;
        interacted = true;
        ClearInteractionListeners();
    }

    private void OnBecameInvisible() {
        if (ShouldDissapear) {
            gameObject.SetActive(false);
            ShouldDissapear = false;
        }
    }
}