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
using PocketSphinx;

namespace PocketSphinxForms.Droid.SpeechToText
{
    public class SpeechResultEvent
    {
        public SpeechResultEvent(Hypothesis hypothesis, bool finalResult)
        {
            Hypothesis = hypothesis;
            FinalResult = finalResult;
        }

        public Hypothesis Hypothesis { get; set; }
        public bool FinalResult { get; set; }
    }
}