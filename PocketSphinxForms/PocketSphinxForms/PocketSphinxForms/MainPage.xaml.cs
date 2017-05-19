using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PocketSphinxForms
{
    public partial class MainPage : ContentPage
    {
        public static MainPageViewModel ViewModel { get; set; }

        public MainPage()
        {
            this.BindingContext = ViewModel = new MainPageViewModel();

            InitializeComponent();

            ViewModel.IsKeyphraseMode = true;
            ViewModel.IsKeywordsMode = false;

            ViewModel.IsStartEnabled = true;
            ViewModel.IsStopEnabled = false;
        }
    }
}
