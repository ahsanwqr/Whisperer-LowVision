/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Oculus.Voice;
using UnityEngine;

namespace Whisperer
{
        /// <summary>
        ///     Watches tracked hands controllers for valid "speak pose"
        ///     Raycasts for & selects Listenable objects
        /// </summary>
        public class SpeakGestureWatcher : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform _head;

        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _speakGesturePoint;
        [SerializeField] private AppVoiceExperience _appVoiceExperience;
        [SerializeField] private RigHandsControl _hands;

        [Header("Settings")] [SerializeField] private float _sphereCastRadius = 0.1f;

        [SerializeField] private float _sphereCastRadiusMax = 0.25f;
        [SerializeField] private float _distance = 6;
        [SerializeField] private float _handsDistThresh = 0.1f;
        [SerializeField] private Vector3 _handsMidPointOffset;

        [Header("Raycast")] [SerializeField] private LayerMask _layerMask;

        [SerializeField] private Listenable _selectedListenable;

        [SerializeField] private bool _selectActivated,
            _allowSpeak,
            _haveSpeakPose,
            _selectLocked,
            _castMode;

        [SerializeField] private GameObject _fPrefab;

        private Vector3 _handsMidPoint;

        private Collider _hitCollider;

        private bool _haveListenable => _selectedListenable != null;

        public bool AllowSpeak
        {
            get => _allowSpeak;
            set => _allowSpeak = value;
        }

        public bool HaveSpeakPose => _haveSpeakPose;
        public bool HaveListenable => _haveListenable;

        public bool CastMode
        {
            get => _castMode;
            set => _castMode = value;
        }

        public Vector3 RaycastDirection { get; private set; }

        private void PoseCheck()
        {
            var leftDist = Vector3.Distance(_leftHand.position, _speakGesturePoint.position);
            var rightDist = Vector3.Distance(_rightHand.position, _speakGesturePoint.position);

            _handsMidPoint = _leftHand.position + (_rightHand.position - _leftHand.position) * 0.5f;
            _handsMidPoint += _head.TransformVector(_handsMidPointOffset);
            RaycastDirection = (_handsMidPoint - _head.position).normalized;

            var havePose = leftDist < _handsDistThresh && rightDist < _handsDistThresh;
            if (!_allowSpeak || !_hands.SpeakHandsReady) havePose = false;

            /// Set pose state
            if (havePose != _haveSpeakPose)
            {
                if (havePose) StartSpeakPose();
                else CancelSpeakPose();
                _haveSpeakPose = havePose;
            }
        }

        private void StartSpeakPose()
        {
            SetHandOutline(_leftHand, true);
            SetHandOutline(_rightHand, true);
        }

        private void CancelSpeakPose()
        {
            SetHandOutline(_leftHand, false);
            SetHandOutline(_rightHand, false);
            _castMode = false;

            if (!_selectLocked)
                SetSelectedListenable(null);
        }

        private void LookForListenables()
        {
            if (_haveSpeakPose &&
                Utilities.ConeCast(_head.transform.position,
                    RaycastDirection,
                    _distance,
                    _sphereCastRadius,
                    _sphereCastRadiusMax,
                    out _hitCollider,
                    _layerMask))
            {
                var l = _hitCollider.GetComponent<Listenable>();
                if (l is not null && l.AllowSelect) SetSelectedListenable(l);
            }
            else
            {
                if (_haveListenable) SetSelectedListenable(null);
            }
        }

        private void SetSelectedListenable(Listenable listenable)
        {
            /// Listenables changed?
            if (listenable != _selectedListenable)
            {
                /// Deselect previous
                if (_haveListenable)
                {
                    Logger.Instance.AddLog("De-selecting " + _selectedListenable.gameObject.name);
                    _selectedListenable.SetSelected(false);
                    _selectedListenable.OnResponseProcessed.RemoveListener(ListenableProcessingFinished);
                    _selectedListenable.OnListeningDisabled.RemoveListener(DeselectListenable);
                    _appVoiceExperience.Deactivate();
                }

                _selectedListenable = listenable;

                /// Select current
                if (_haveListenable)
                {
                    Logger.Instance.AddLog("Selecting " + _selectedListenable.gameObject.name);
                    _selectedListenable.SetSelected(true);
                    _selectedListenable.OnResponseProcessed.AddListener(ListenableProcessingFinished);
                    _selectedListenable.OnListeningDisabled.AddListener(DeselectListenable);
                    _selectedListenable.OnWitSubscribed.AddListener(StartedProcessing);
                    _appVoiceExperience.Activate();
                }
            }

            if (listenable is null) _selectLocked = false;
        }

        private void StartedProcessing(Listenable listenable)
        {
            if (_haveListenable)
                _selectLocked = true;
        }

        private void ListenableProcessingFinished(ListenableEventArgs eventArgs)
        {
            SetSelectedListenable(null);
        }

        private void DeselectListenable(Listenable listenable)
        {
            SetSelectedListenable(null);
        }

        private void SetHandOutline(Transform hand, bool activated)
        {
            var o = hand.GetComponentInChildren<HighlightObject>();
            o.HighlightColor = _castMode ? Color.red : new Color(0, 1, 59 / 255);
            o.EnableHighlight(activated);
        }

        #region For Fun

        private void OnTranscription(string text)
        {
            if (!_haveSpeakPose) return;

            if (text.ToLower().Contains("cast") &&
                text.ToLower().Contains("fire"))
            {
                SetSelectedListenable(null);
                _castMode = true;
                SetHandOutline(_leftHand, true);
                SetHandOutline(_rightHand, true);

                var fb = Instantiate(_fPrefab);
                fb.transform.position = _handsMidPoint;
                fb.GetComponent<FSpell>().SGW = this;
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _allowSpeak = false;
        }

        private void Start()
        {
            _appVoiceExperience.VoiceEvents.onFullTranscription.AddListener(OnTranscription);
        }

        private void OnDestroy()
        {
            _appVoiceExperience.VoiceEvents.onFullTranscription.RemoveListener(OnTranscription);
        }

        private void Update()
        {
            if (_haveListenable && !_selectedListenable.IsSelected)
            {
                Debug.LogError("SpeakGestureWatcher thought " + _selectedListenable.gameObject.name +
                               " was selected but it's NOT. This is a BUG");
                SetSelectedListenable(null);
            }

            /// Are we in the correct pose to activate speaking?
            PoseCheck();

            /// When we are in speak pose and have not yet activated wit,
            /// we can select a new listenable object.
            if (!_selectLocked && _haveSpeakPose)
                LookForListenables();
        }

        #endregion
    }
}
