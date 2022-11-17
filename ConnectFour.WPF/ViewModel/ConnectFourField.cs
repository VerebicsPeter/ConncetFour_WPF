using System;

namespace ConnectFour.ViewModel
{
    /// <summary>
    /// Sudoku játékmező típusa.
    /// </summary>
    public class ConnectFourField : ViewModelBase
    {
        private String _text = String.Empty;

        /// <summary>
        /// Felirat lekérdezése, vagy beállítása.
        /// </summary>
        public String Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value; 
                    OnPropertyChanged();
                }
            } 
        }

        public int Row { get; set; }

        public int Col { get; set; }

        public int Number { get; set; }

        public DelegateCommand? StepCommand { get; set; }
    }
}