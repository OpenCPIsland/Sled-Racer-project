using DevonLocalization.Core;
using Disney.ClubPenguin.Common.Utils;
using UnityEngine;
using TMPro;

namespace Disney.ClubPenguin.SledRacer
{
    public class RecordMarker : SpawnableTrackObject
    {
        public enum Mode
        {
            AllTime,
            Weekly
        }

        public TMP_Text Text;

        public LayerMask CollisionLayerMask;

        private PlayerController player;

        public void SetMode(HighScoreMarkerType type)
        {
            string text = null;
            switch (type)
            {
                case HighScoreMarkerType.LocalAllTime:
                    text = "recordMarker.BestDistanceEver";
                    break;
                case HighScoreMarkerType.LocalWeekly:
                    text = "recordMarker.BestDistanceWeek";
                    break;
            }
            if (string.IsNullOrEmpty(text) && Text != null)
            {
                Text.text = string.Empty;
            }
            else
            {
                Text.text = Localizer.Instance.GetTokenTranslation(text);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CollisionLayerMask.IsSet(other.gameObject.layer))
            {
                player = SledRacerGameManager.Instance.playerScript;
                player.TriggerAnimation("RiderCelebrate");
            }
        }
    }
}