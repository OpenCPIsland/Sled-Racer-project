using UnityEngine;
using TMPro;

public class CPSNCommentaryTMP : MonoBehaviour
{
    [TextArea]
    public string[] quotes = new string[]
    {
        "He’s sliding faster than a puffle on a banana peel!",
        "That’s gotta be the fifth time this week he’s hit that exact same log!",
        "Oh! And that’s what we call a penguin pancake! Flattened, flippered, and finished!",
        "He's got more style than a penguin in a bowtie! Too bad that didn’t help with the turn.",
        "Gravity: 1. Sled: 0.",
        "Let’s watch that wipeout again in slow motion because comedy is better at 12 frames per second.",
        "You’d think after the seventh fish sign they’d know a cliff is coming.",
        "That was a legendary launch, into a snowbank. 10/10 for effort.",
        "If crashing were a sport, this penguin just won the gold!",
        "And there he goes, no helmet no fear no control!"
    };

    [Tooltip("Optional: Show this in a TextMeshProUGUI component if assigned")]
    public TextMeshProUGUI commentaryTMP;

    public float minSecondsBetweenQuotes = 7f;
    public float maxSecondsBetweenQuotes = 10f;

    private void Start()
    {
        DisplayRandomQuote(); // Show first quote immediately
    }

    private void ScheduleNextQuote()
    {
        float delay = Random.Range(minSecondsBetweenQuotes, maxSecondsBetweenQuotes);
        Invoke(nameof(DisplayRandomQuote), delay);
    }

    private void DisplayRandomQuote()
    {
        if (quotes.Length == 0)
        {
            Debug.LogWarning("No CPSN quotes assigned!");
            return;
        }

        string quote = quotes[Random.Range(0, quotes.Length)];

        if (commentaryTMP != null)
        {
            commentaryTMP.text = quote;
        }
        else
        {
            Debug.Log($"[CPSN] {quote}");
        }

        ScheduleNextQuote(); // Loop it
    }
}
