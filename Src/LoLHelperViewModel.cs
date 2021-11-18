using Stylet;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LoLHelper.Src
{
    public class LoLHelperViewModel : Screen
    {
        public bool IsRunning { get; set; }
        public bool IsFuntionCheckboxEnable { get => !IsRunning; }
        public bool AutoQueue { get; set; }
        public bool AutoAccept { get; set; }
        public bool AutoPickLane { get; set; }
        public bool AutoPickChmapion { get; set; }
        public bool AutoChangeRune { get; set; }
        public bool IsMinimizie { get; set; }

        public string SelectedChampion { get; set; }
        public string SelectedLane { get; set; }
        public string LeagueClientPath { get; set; }

        public ObservableCollection<string> ChampionList { get; set; }
        public ObservableCollection<string> LaneList { get; set; }

        public LoLHelperViewModel()
        {
            ChampionList = new();
            LaneList = new();

            Task.Factory.StartNew(ProcessAutoQueue, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoAccept, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoPickLane, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoPickChampion, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoChangeRune, TaskCreationOptions.LongRunning);
        }

        private void ProcessAutoQueue()
        {

        }

        private void ProcessAutoAccept()
        {

        }

        private void ProcessAutoPickLane()
        {

        }

        private void ProcessAutoPickChampion()
        {

        }

        private void ProcessAutoChangeRune()
        {

        }
    }
}
