using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SphinxBase;
using Java.IO;

namespace PocketSphinxForms.Droid.SpeechToText
{
    public class SpeechRecognizerSetup
    {
        private Config config ;
        public static SpeechRecognizerSetup defaultSetup()
        {
            return new SpeechRecognizerSetup(PocketSphinx.Decoder.DefaultConfig());
        }

        public SpeechRecognizerSetup(Config config)
        {
            this.config = config;
        }

        public SpeechRecognizer GetRecognizer()
        {
            return new SpeechRecognizer(config);
        }

        public SpeechRecognizerSetup SetAcousticModel(File model)
        {
            return setString("-hmm", model.Path);
        }

        public SpeechRecognizerSetup SetDictionary(File dictionary)
        {
            return setString("-dict", dictionary.Path);
        }

        public SpeechRecognizerSetup SetSampleRate(int rate)
        {
            return setFloat("-samprate", rate);
        }

        public SpeechRecognizerSetup SetRawLogDir(File dir)
        {
            return setString("-rawlogdir", dir.Path);
        }

        public SpeechRecognizerSetup setKeywordThreshold(float threshold)
        {
            return setFloat("-kws_threshold", threshold);
        }

        public SpeechRecognizerSetup setBoolean(String key, bool value)
        {
            config.SetBoolean(key, value);
            return this;
        }

        public SpeechRecognizerSetup setInteger(String key, int value)
        {
            config.SetInt(key, value);
            return this;
        }

        public SpeechRecognizerSetup setFloat(String key, double value)
        {
            config.SetFloat(key, value);
            return this;
        }

        public SpeechRecognizerSetup setString(String key, String value)
        {
            config.SetString(key, value);
            return this;
        }
    }
}