using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Dice : MonoBehaviour
{
    public System.Action<int> OnDiceRolled;

    [Header("References")]
    public EventScript eventScript; // 👈 Assign in Inspector

    [Header("UI References")]
    public Image diceImage;
    public Button rollButton;
    public Sprite[] diceFaces; // Assign sprites 1–6 in Inspector

    [Header("Animation Settings")]
    public float rollDuration = .7f;
    public float minInterval = 0.05f;
    public float maxInterval = 0.25f;

    private bool isRolling = false;
    private int lastRoll = 1;

    void Start()
    {
        rollButton.onClick.AddListener(OnRollButtonPressed);
        diceImage.sprite = diceFaces[0];
    }

    void Update()
    {
        // 👇 Automatically disable button during Shopping state
        if (eventScript != null)
        {
            rollButton.interactable = (eventScript.currentState == State.Normal);
        }
    }

    void OnRollButtonPressed()
    {
        if (isRolling || eventScript == null) return;

        // no rolling if shop is open or not normal
        if (eventScript.currentState != State.Normal)
        {
            Debug.Log("Cannot roll while shop is open!");
            return;
        }

        StartCoroutine(RollDiceAnimation());
    }

    public void RollForBot(System.Action<int> onRolled)
    {
        if (isRolling) return;
        StartCoroutine(RollDiceAnimation(onRolled));
    }

    private IEnumerator RollDiceAnimation(System.Action<int> onRolled = null)
    {
        isRolling = true;
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            int randomFace = Random.Range(0, diceFaces.Length);
            diceImage.sprite = diceFaces[randomFace];

            float t = elapsed / rollDuration;
            float currentInterval = Mathf.Lerp(minInterval, maxInterval, t);

            yield return new WaitForSeconds(currentInterval);
            elapsed += currentInterval;
        }

        lastRoll = Random.Range(1, 7);
        diceImage.sprite = diceFaces[lastRoll - 1];

        isRolling = false;
        Debug.Log($"🎲 Dice rolled: {lastRoll}");

        // Invoke callback if provided (for bot)
        onRolled?.Invoke(lastRoll);

        // Trigger normal event for any listeners
        OnDiceRolled?.Invoke(lastRoll);
    }


}
