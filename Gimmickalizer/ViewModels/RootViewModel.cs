using Stylet;
using System.Collections.ObjectModel;
using Serilog;
using Screen = Stylet.Screen;

namespace Gimmickalizer.ViewModels
{
    public class RootViewModel : PropertyChangedBase
    {
        private string _title = "Gimmickalizer";
        public string Title
        {
            get { return _title; }
            set { SetAndNotify(ref _title, value); }
        }

        public RootViewModel()
        {

        }
    }
}
