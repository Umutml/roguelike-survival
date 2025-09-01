using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    private CancellationTokenSource _cancellationTokenSource;
        private TimeSpan _timeDifference;
        private TimerUpdateType _timerUpdateType;

        private void OnDestroy()
        {
            StopUpdating();
        }

        public async void CreateTimer(TMP_Text timerText, string title, string colorHex, DateTime dateTime, Action onCompleted = null, TimerUpdateType updateType = TimerUpdateType.Minute)
        {
            StopUpdating();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    _timeDifference = GetTimeDifference(dateTime);

                    _timerUpdateType = AdjustUpdateType(_timeDifference, updateType);

                    SetTimerText(timerText, title, colorHex);
                    if (_timeDifference.TotalSeconds <= 0)
                    {
                        StopUpdating();
                        onCompleted?.Invoke();
                        break;
                    }

                    await Task.Delay(_timerUpdateType switch
                        {
                            TimerUpdateType.Hour => TimeSpan.FromHours(1),
                            TimerUpdateType.Minute => TimeSpan.FromMinutes(1),
                            TimerUpdateType.Second => TimeSpan.FromSeconds(1),
                            _ => throw new ArgumentOutOfRangeException(nameof(_timerUpdateType), _timerUpdateType, null)
                        },
                        token);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.Log("CreateTimer: Task Canceled");
            }
        }

        private void SetTimerText(TMP_Text text, string title, string colorHex)
        {
            var formattedTime = GetFormattedTime(colorHex);
            text.text = string.IsNullOrEmpty(title) ? formattedTime : $"{title}: {formattedTime}";
        }

        private string GetFormattedTime(string colorHex)
        {
            var day = "d";
            var hour = "h";
            var minute = "m";
            var second = "s";
            return (_timeDifference, _timerUpdateType) switch
            {
                ({ Days: > 0 }, TimerUpdateType.Hour) => FormatTime(colorHex, $"{_timeDifference.Days}{day} {_timeDifference.Hours}{hour}"),
                ({ Hours: > 0 }, TimerUpdateType.Hour) => FormatTime(colorHex, $"{_timeDifference.Hours}{hour}"),

                ({ Hours: > 0 }, TimerUpdateType.Minute) => FormatTime(colorHex, $"{_timeDifference.Hours}{hour} {_timeDifference.Minutes}{minute}"),
                ({ Minutes: > 0 }, TimerUpdateType.Minute) => FormatTime(colorHex, $"{_timeDifference.Minutes}{minute}"),

                ({ Hours: > 0 }, TimerUpdateType.Second) => FormatTime(colorHex, $"{_timeDifference.Hours}{hour} {_timeDifference.Minutes}{minute} {_timeDifference.Seconds}{second}"),
                ({ Minutes: > 0 }, TimerUpdateType.Second) => FormatTime(colorHex, $"{_timeDifference.Minutes}{minute} {_timeDifference.Seconds}{second}"),
                _ => FormatTime(colorHex, $"{_timeDifference.Seconds}{second}")
            };
        }

        private string FormatTime(string colorHex, string timeString)
        {
            return $"<color=#{colorHex}>{timeString}</color>";
        }
        

        private TimerUpdateType AdjustUpdateType(TimeSpan timeDifference, TimerUpdateType currentUpdateType)
        {
            return currentUpdateType switch
            {
                TimerUpdateType.Hour when timeDifference is { Hours: <= 0 } => timeDifference.Minutes > 0 ? TimerUpdateType.Minute : TimerUpdateType.Second,
                TimerUpdateType.Minute when timeDifference.Minutes <= 0 => TimerUpdateType.Second,
                _ => currentUpdateType
            };
        }

        public void StopUpdating()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        private TimeSpan GetTimeDifference(DateTime dateTime)
        {
            return dateTime - DateTime.Now;
        }
    }

    public enum TimerUpdateType
    {
        Hour,
        Minute,
        Second
    }

