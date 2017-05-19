using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketSphinxForms
{
    public interface ISpeechService
    {
        void StartListening();
        Task StopListening();
        Task Setup();
        void ChangeKeyPhrase(string keyPhrase);
    }
}
