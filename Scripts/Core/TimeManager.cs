using MLT.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MLT
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [SerializeField] public Season Season = Season.SPRING;
        [SerializeField] public int Day = 1;
        [SerializeField] public int Hour = 6;
        [SerializeField] public int Minute = 0;
        [SerializeField] public int TotalMinutes = 0;

        public float TimeScale = 1.0f;
        private float _timer;

        public bool IsRainyDay { get; private set; }

        public Action OnMinuteChanged;
        public Action OnDayEnded;
        public Action OnDayStarted;
        public Action<Season> OnSeasonChanged;

        private float GetRainChance() => Season switch // ���� ������ �´� ���� Ȯ�� ���� ��ȯ
        {
            Season.SPRING => 0.126f,
            Season.SUMMER => 0.250f,
            Season.FALL => 0.200f,
            Season.WINTER => 0.125f,
            _ => 0f
        };

        private void Awake() // �̱��� �ν��Ͻ��� �����ϰ� �ߺ� ������ ����
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        void Update() // �� ������ �ð��� ����Ͽ� TimeScale�� ���� _timer�� ����
        {
            _timer += Time.deltaTime * TimeScale;

            if (_timer >= 1f)
            {
                _timer = 0;
                AddMinute(1);
            }
        }

        void AddMinute(int mins) // ���� �� �ð��� Ư�� ��(min)��ŭ ������Ű��, ��/�� ��ȭ�� üũ
        {
            TotalMinutes += mins;
            Minute += mins;
            if (Minute >= 60)
            {
                Hour++;
                Minute = 0;
            }

            if (Hour >= 24)
            {
                Hour = 0;
                StartCoroutine(TempChangeDateRoutine());
            }

            OnMinuteChanged?.Invoke();
        }

        private void CheckSeasonChange() // 28���� ������ �� ���� ������ �����ϰ� �̺�Ʈ�� �߻�
        {
            if (Day > 28)
            {
                Day = 1;
                Season = Season switch
                {
                    Season.SPRING => Season.SUMMER,
                    Season.SUMMER => Season.FALL,
                    Season.FALL => Season.WINTER,
                    Season.WINTER => Season.SPRING,
                    _ => Season.SPRING
                };
                OnSeasonChanged?.Invoke(Season);
            }
        }

        private void DetermineWeather() // ������ Ȯ���� ���� ���� �� �� ���� ����
        {
            IsRainyDay = UnityEngine.Random.value < GetRainChance();
        }

        public string GetFormattedTime() // ���� �ð��� 12�ð���(AM/PM) ���ڿ��� ��ȯ
        {
            string ampm = (Hour < 12) ? "AM" : "PM";
            int displayHour = (Hour % 12 == 0) ? 12 : Hour % 12;
            return $"{displayHour:D2}:{Minute:D2} {ampm}";
        }

        //void UpdateSchedule()
        //{
        //    int totalMins = (Hour * 60) + Minute;

        //    if (totalMins >= 120 && totalMins < 360)
        //    {
        //        StartCoroutine(TempChangeDateRoutine());
        //    }
        //    else if (totalMins >= 0 && totalMins < 120)
        //    {
        //        _timeUI.TimeColorChange(Color.red);
        //    }
        //}

        public string Get24FormattedTime() // ���� �ð��� 24�ð��� ���ڿ��� ��ȯ
        {
            int displayHour = (Hour % 24 == 0) ? 0 : Hour % 24;
            return $"{displayHour:D2}:{Minute:D2}";
        }

        //public string GetSeasonDate()
        //{
        //    return $"{Util.EnumChangeKOR(Season.ToString())} {Day} ��";
        //}

        // �ð�, ���� �Ҽ������� ��ȯ�Ͽ� �����ϴ� �Լ�. ������ üũ�� �����.  - ������ 26.04.22
        public float GetCurrentTimeByFloat() => Hour + (Minute / 60f);
        
        // �ð�, ���� �д����� ���ļ� ������ �����ϴ� �Լ�. �ൿ�� �ð� ����� �ʿ��� �� �����    - ������ 26.04.22
        public float GetCurrentTimeByInt() => (Hour * 60f) + Instance.Minute;
        

        public void SetTimeSpeed(float scale) // ���� �ð��� �帧 �ӵ�(TimeScale)�� ����
        {
            TimeScale = scale;
        }

        IEnumerator TempChangeDateRoutine() // ��¥�� �Ѿ �� 5�ʰ� ����ϸ� Ÿ�� ���¸� �����ϰ� ������ �����ϴ� �ڷ�ƾ
        {
            TimeScale = 0;
            OnDayEnded?.Invoke();

            yield return new WaitForSeconds(5f);

            Day++;
            CheckSeasonChange();
            DetermineWeather();
            OnDayStarted?.Invoke();
            EventBus.RaiseDayStarted();
            Hour = 6;
            AddMinute(0);
            TimeScale = 1;
        }

        //public string GetYesterday()
        //{
        //    return $"{Util.EnumChangeKOR(Season.ToString())} {Day - 1} ��";
        //}
    }

}