using GameCore.Tutorial.Steps;
using UI.Game.Architectural;
using UnityEngine;
using System.Collections;
using GameCore.Player;
using GameCore.Scriptables;

namespace _Scripts.UI.Game.InGame.NpcDialog
{
    public class NpcDialogSegment : Content
    {
        [SerializeField] private SpriteDatabase spriteDatabase;

        private const string TitleTextKey = "TitleText";
        private const string DescriptionTextKey = "DescriptionText";
        private const string RadioIconKey = "RadioIcon";
        private readonly WaitForSecondsRealtime typeDelay = new(0.05f);

        private Coroutine _typeTextCoroutine;
        private NpcDialogData? _dialogData;

        public bool IsPlaying { get; private set; }

        public void Initialize(NpcDialogData dialogData)
        {
            IsPlaying = true;
            _dialogData = dialogData;

            SetText(TitleTextKey, $"{dialogData.title}:");
            _typeTextCoroutine = StartCoroutine(TypeText(dialogData.description));
            SetDialogImage();
        }

        private async void SetDialogImage()
        {
            if (!_dialogData.HasValue)
            {
                return;
            }

            var spriteType = _dialogData.Value.conversationType switch
            {
                ConversationType.InPerson => SpriteType.NpcDialogSheriff,
                ConversationType.VoiceCall => SpriteType.NpcDialogRadio,
                ConversationType.Hattori => SpriteType.NpcDialogHattori,
                ConversationType.Soldier => SpriteType.NpcDialogSoldier,
                _ => SpriteType.NpcDialogSheriff
            };

            var sprite = await spriteDatabase.GetSpriteByType(spriteType);
            SetImage(RadioIconKey, sprite);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_typeTextCoroutine != null)
            {
                StopCoroutine(_typeTextCoroutine);
            }
        }

        private IEnumerator TypeText(string text)
        {
            SetText(DescriptionTextKey, string.Empty);
            foreach (var character in text)
            {
                AppendText(DescriptionTextKey, character.ToString());
                yield return typeDelay;
            }

            IsPlaying = false;
        }

        public void Skip()
        {
            if (_typeTextCoroutine != null)
            {
                StopCoroutine(_typeTextCoroutine);
            }

            if (_dialogData != null)
            {
                SetText(DescriptionTextKey, _dialogData.Value.description);
            }

            IsPlaying = false;
        }
    }
}