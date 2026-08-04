using UnityEngine;
using UnityEngine.UI;

namespace MLT.Player
{
    public class AimUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _needle;
        [SerializeField] private RectTransform _hitZone;
        [SerializeField] private RectTransform _meterBg;

        private float _hitThreshold;
        private float _range;

        // Show할 때 threshold, range 받아서 HitZone 한 번만 계산
        public void Show(float hitThreshold, float range)
        {
            _hitThreshold = hitThreshold;
            _range = range;
            _panel?.SetActive(true);

            Canvas.ForceUpdateCanvases();
            RefreshHitZone();
        }

        public void Hide() => _panel?.SetActive(false);

        // 바늘 이동만 담당
        public void UpdateAim(float angle, float range, float hitThreshold)
        {
            if (_needle != null && _meterBg != null)
            {
                float halfWidth = _meterBg.rect.width * 0.5f;
                float ratio = angle / range;
                float xPos = ratio * halfWidth;

                Debug.Log($"[디버그] MeterBg 너비: {_meterBg.rect.width}, 바늘 X좌표: {xPos}");
                _needle.anchoredPosition = new Vector2(xPos, _needle.anchoredPosition.y);
            }
        }

        // HitZone 너비 계산
       
        private void RefreshHitZone()
        {
            if (_hitZone == null || _meterBg == null) return;

            float totalWidth = _meterBg.rect.width;
            float ratio = _hitThreshold / _range;

            float zoneWidth = totalWidth * ratio;

            _hitZone.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, zoneWidth);
        }
    }
}