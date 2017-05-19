using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PocketSphinxForms
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isInSpeech;
        private bool _isListening;
        private bool _isKeyphraseMode;
        private bool _isKeywordsMode;
        private bool _isStartEnabled;
        private bool _isStopEnabled;
        private string _hypothesis;
        private string _newKeyPhrase;
        private string _currKeyPhrase;

        public bool IsListening
        {
            get { return _isListening; }
            set
            {
                if (_isListening != value)
                {
                    _isListening = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInSpeech
        {
            get { return _isInSpeech; }
            set
            {
                if (_isInSpeech != value)
                {
                    _isInSpeech = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsKeyphraseMode
        {
            get { return _isKeyphraseMode; }
            set
            {
                if (_isKeyphraseMode != value)
                {
                    _isKeyphraseMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsKeywordsMode
        {
            get { return _isKeywordsMode; }
            set
            {
                if (_isKeywordsMode != value)
                {
                    _isKeywordsMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsStartEnabled
        {
            get { return _isStartEnabled; }
            set
            {
                if (_isStartEnabled != value)
                {
                    _isStartEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsStopEnabled
        {
            get { return _isStopEnabled; }
            set
            {
                if (_isStopEnabled != value)
                {
                    _isStopEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Hypothesis
        {
            get { return _hypothesis; }
            set
            {
                if (_hypothesis != value)
                {
                    _hypothesis = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NewKeyPhrase
        {
            get { return _newKeyPhrase; }
            set
            {
                if (_newKeyPhrase != value)
                {
                    _newKeyPhrase = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrKeyPhrase
        {
            get { return _currKeyPhrase; }
            set
            {
                if (_currKeyPhrase != value)
                {
                    _currKeyPhrase = value;
                    OnPropertyChanged();
                }
            }
        }

        public Command StartListening { get; set; }

        public Command StopListening { get; set; }

        public Command ChangeKeyPhrase { get; set; }

        public Command ChangeModeRequestCommand { get; set; }

        public MainPageViewModel()
        {
            StartListening = new Command(OnStartListening);
            StopListening = new Command(OnStopListening);
            ChangeKeyPhrase = new Command(OnChangeKeyPhrase);
            ChangeModeRequestCommand = new Command(OnChangeModeRequest);

            NewKeyPhrase = "oh mighty computer";
            CurrKeyPhrase = "oh mighty computer"; // manually set here and in SpeechService for init

            RunSetup(); // wait the end of the setup
        }

        private async void RunSetup()
        {
            await DependencyService.Get<ISpeechService>().Setup();
        }

        private void OnStartListening(object obj)
        {
            DependencyService.Get<ISpeechService>().StartListening(); // start the recognizer process
        }

        private async void OnStopListening(object obj)
        {
            await DependencyService.Get<ISpeechService>().StopListening(); // wait everything is stopped
        }

        private void OnChangeKeyPhrase(object obj)
        {
            if (!String.IsNullOrWhiteSpace(NewKeyPhrase))
            {
                if(IsListening == true)
                    OnStopListening(null);

                DependencyService.Get<ISpeechService>().ChangeKeyPhrase(NewKeyPhrase);
            }
        }

        private void OnChangeModeRequest(object obj)
        {
            IsKeyphraseMode = !IsKeyphraseMode;
            IsKeywordsMode = !IsKeywordsMode;

            if (IsListening)
                OnStopListening(null);

            if (IsKeyphraseMode == true)
            {
                DependencyService.Get<ISpeechService>().ChangeKeyPhrase(NewKeyPhrase);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
