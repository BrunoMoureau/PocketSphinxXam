using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Android.Util;
using Java.Lang;
using PocketSphinx;
using SphinxBase;
using Config = SphinxBase.Config;
using String = System.String;
using Thread = System.Threading.Thread;

namespace PocketSphinxForms.Droid.SpeechToText
{
    public class SpeechRecognizer : IDisposable
    {
        public event EventHandler<bool> InSpeechChange;
        public event EventHandler<SpeechResultEvent> Result;
        public event EventHandler Timeout;
        public event EventHandler Stopped;


        protected static String TAG => nameof(SpeechRecognizer);

        private readonly Decoder _decoder;
        private int sampleRate;
        private static float BUFFER_SIZE_SECONDS = 0.4f;
        private int bufferSize;
        private readonly AudioRecord _recorder;

        private Task _recognizerThread;
        private CancellationTokenSource _interruption;

        public SpeechRecognizer(Config config)
        {
            _decoder = new Decoder(config);
            sampleRate = (int)_decoder.GetConfig().GetFloat("-samprate");
            bufferSize = Java.Lang.Math.Round(sampleRate * BUFFER_SIZE_SECONDS * 2);
            _recorder = new AudioRecord(
                    AudioSource.VoiceRecognition, sampleRate,
                    ChannelIn.Mono,
                    Encoding.Pcm16bit, bufferSize * 2);

            if (_recorder.State == State.Uninitialized)
            {
                _recorder.Release();
                throw new IOException(
                        "Failed to initialize recorder. Microphone might be already in use. Check if you gave the required permissions to this app.");
            }
        }


        public bool StartListening(String searchName, int timeout = -1)
        {
            if (null != _recognizerThread)
                return false;

            _decoder.SetSearch(searchName);

            _interruption = new CancellationTokenSource();
            
            _recognizerThread = Task.Run(() =>
             {
                 try
                 {
                     RecognizeAsync(timeout);
                 }
                 catch (OperationCanceledException e)
                 {
                     System.Diagnostics.Debug.WriteLine(TAG, "!! received cancel token !!");
                 }
                 catch (System.Exception e)
                 {
                     System.Diagnostics.Debug.WriteLine("EXCEPTION: unhandled exception : " + e.Message);
                 }
                 finally
                 {
                     try
                     {
                         _recorder.Stop();
                         _decoder.EndUtt();
                     }
                     catch (System.Exception e)
                     {
                         System.Diagnostics.Debug.WriteLine("EXCEPTION: calling recorder stop method. " + e.Message);
                     }
                     System.Diagnostics.Debug.WriteLine(TAG, "Recognizer thread is stopped");
                     OnStopped();
                 }
             }, _interruption.Token);

            return true;
        }


        private async Task<bool> StopRecognizerThread()
        {
            if (null == _recognizerThread)
                return false;

            _interruption.Cancel();
            await _recognizerThread;

            _recognizerThread = null;
            return true;
        }

        public async Task<bool> Stop()
        {
            bool result = await StopRecognizerThread();
            if (result)
            {
                System.Diagnostics.Debug.WriteLine(TAG, "Stop recognition");
                Hypothesis hypothesis = _decoder.Hyp();
                OnResult(hypothesis, true);
            }
            return !result;
        }

        public async Task<bool> Cancel()
        {
            bool result = await StopRecognizerThread();
            if (result)
            {
                System.Diagnostics.Debug.WriteLine(TAG, "Cancel recognition");
            }

            return result;
        }

        private void RecognizeAsync(int timeout = -1)
        {

            int remainingSamples;
            int timeoutSamples;
            int NO_TIMEOUT = -1;

            if (timeout != NO_TIMEOUT)
                timeoutSamples = timeout * sampleRate / 1000;
            else
                timeoutSamples = NO_TIMEOUT;
            remainingSamples = timeoutSamples;

            _recorder.StartRecording();

            if (_recorder.RecordingState == RecordState.Stopped)
            {
                _recorder.Stop();

                throw new Java.IO.IOException("Failed to start recording. Microphone might be already in use.");
            }

            System.Diagnostics.Debug.WriteLine(TAG, "Starting decoding");

            _decoder.StartUtt();
            byte[] buffer = new byte[bufferSize];
            bool inSpeech = _decoder.GetInSpeech();

            _recorder.Read(buffer, 0, buffer.Length);

            Debug.WriteLine("Start loop: " + Thread.CurrentThread.ManagedThreadId);

            while ((timeoutSamples == NO_TIMEOUT) || (remainingSamples > 0))
            {
                _interruption.Token.ThrowIfCancellationRequested();
                int nread = _recorder.Read(buffer, 0, buffer.Length);

                if (nread == -1)
                {
                    throw new RuntimeException("error reading audio buffer");
                }
                else if (nread > 0)
                {
                    _decoder.ProcessRaw(buffer, nread, false, false);


                    if (_decoder.GetInSpeech() != inSpeech)
                    {
                        inSpeech = _decoder.GetInSpeech();
                        //System.Diagnostics.Debug.Write("\nSpeech mode = " + inSpeech );
                        OnInSpeechChange(inSpeech);
                    }

                    if (inSpeech)
                        remainingSamples = timeoutSamples;

                    Hypothesis hypothesis = _decoder.Hyp();
                    if (hypothesis != null)
                    {
                        OnResult(hypothesis, false);
                    }
                }

                if (timeoutSamples != NO_TIMEOUT)
                {
                    remainingSamples = remainingSamples - nread;
                }
            }

            System.Diagnostics.Debug.WriteLine(TAG, "Stopped decoding");


            //// If we met timeout signal that speech ended
            //if (timeoutSamples != NO_TIMEOUT && remainingSamples <= 0)
            //{
            //    Log.Debug(TAG, "Timeout");
            //    OnTimeout();
            //}
        }

        private void OnResult(Hypothesis hypothesis, bool finalResult)
        {
            Result?.Invoke(this, new SpeechResultEvent(hypothesis, finalResult));
        }

        protected virtual void OnInSpeechChange(bool e)
        {
            InSpeechChange?.Invoke(this, e);
        }

        protected virtual void OnTimeout()
        {
            Timeout?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _recorder.Dispose();
        }

        public String GetSearchName()
        {
            return _decoder.GetSearch();
        }

        public void AddFsgSearch(String searchName, FsgModel fsgModel)
        {
            _decoder.SetFsg(searchName, fsgModel);
        }

        public void AddGrammarSearch(String name, Java.IO.File file)
        {
            Log.Debug(TAG, string.Format("Load JSGF %s", file));
            _decoder.SetJsgfFile(name, file.Path);
        }

        public void AddGrammarSearch(String name, String jsgfString)
        {
            _decoder.SetJsgfString(name, jsgfString);
        }

        public void AddNgramSearch(String name, Java.IO.File file)
        {
            Log.Debug(TAG, string.Format("Load N-gram model %s", file));
            _decoder.SetLmFile(name, file.Path);
        }

        public void AddKeyphraseSearch(String name, String phrase)
        {
            _decoder.SetKeyphrase(name, phrase);
        }

        public void AddKeywordSearch(String name, Java.IO.File file)
        {
            _decoder.SetKws(name, file.Path);
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}