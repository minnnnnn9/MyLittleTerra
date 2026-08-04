using UnityEngine;
using MLT.Core;

namespace MLT.Weather
{
    public class WeatherManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _rainParticle;

        private void OnEnable()
        {
            // 이벤트 버스 구독
            EventBus.OnDayStarted += UpdateWeatherVisuals;
        }

        private void OnDisable()
        {
            // 구독 해제
            EventBus.OnDayStarted -= UpdateWeatherVisuals;
        }

        private void UpdateWeatherVisuals()
        {
            // 오늘 비가 오는지 확인하여 파티클 On/Off
            if (TimeManager.Instance.IsRainyDay)
            {
                if (!_rainParticle.isPlaying) _rainParticle.Play();
            }
            else
            {
                _rainParticle.Stop();
            }
        }
    }
}