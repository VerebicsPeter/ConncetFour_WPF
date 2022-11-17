using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ConnectFour.Model;

namespace ConnectFour.ViewModel
{
    public class ConnectFourViewModel : ViewModelBase
    {
        #region Private Fields
        
        private Game _model;

        #endregion

        #region Properties

        public DelegateCommand NewGameCommand { get; private set; }

        public DelegateCommand SaveGameCommand { get; private set; }

        public DelegateCommand LoadGameCommand { get; private set; }

        public DelegateCommand PauseGameCommand { get; private set; }

        public DelegateCommand ExitCommand { get; private set; }


        public ObservableCollection<ConnectFourField> Fields { get; set; }

        public GameSize Size { get; private set; }
        public bool IsSmallGame
        { 
            get { return _model.Size == GameSize.SMALL; }
            set
            { 
                if (_model.Size == GameSize.SMALL) { return; }

                _model.Size = GameSize.SMALL;
                OnPropertyChanged(nameof(IsSmallGame));
                OnPropertyChanged(nameof(IsMediumGame));
                OnPropertyChanged(nameof(IsLargeGame));
            }
        }
        public bool IsMediumGame
        { 
            get { return _model.Size == GameSize.MEDIUM;}
            set
            {
                if (_model.Size == GameSize.MEDIUM) { return; }

                _model.Size = GameSize.MEDIUM;
                OnPropertyChanged(nameof(IsSmallGame));
                OnPropertyChanged(nameof(IsMediumGame));
                OnPropertyChanged(nameof(IsLargeGame));
            }
        }
        public bool IsLargeGame
        { 
            get { return _model.Size == GameSize.LARGE; }
            set
            {
                if (_model.Size == GameSize.LARGE) { return; }

                _model.Size = GameSize.LARGE;
                OnPropertyChanged(nameof(IsSmallGame));
                OnPropertyChanged(nameof(IsMediumGame));
                OnPropertyChanged(nameof(IsLargeGame));
            }
        }

        public bool AppTimerEnabled { get; set; }

        public int  GameMovesCount { get { return _model.Moves; } }
        public bool HasMoves { get { return GameMovesCount > 0; } }
        public String PlayerString { get { return GameMovesCount % 2 == 0 ? "X" : "O"; } }

        public String TurnTime { get { return TimeSpan.FromSeconds(_model.TurnTime).ToString(@"mm\:ss"); } }
        public String X_Time { get { return TimeSpan.FromSeconds(_model.X_Time).ToString("g"); } }
        public String O_Time { get { return TimeSpan.FromSeconds(_model.O_Time).ToString("g"); } }

        #endregion

        #region Events

        public event EventHandler? NewGame;

        public event EventHandler? SaveGame;
        
        public event EventHandler? LoadGame;

        public event EventHandler? PauseGame;

        public event EventHandler? ExitGame;

        #endregion

        #region Constructors

        public ConnectFourViewModel(Game model)
        {
            // játék csatlakoztatása
            _model = model;
			_model.GameCreated += new EventHandler(Model_GameCreated);
            _model.TimerAdvanced += new EventHandler<TimerAdvancedEventArgs>(Model_TimerAdvanced);
            _model.TileChanged += new EventHandler<TileChangedEventArgs>(Model_TileChanged);

            // parancsok kezelése
            NewGameCommand  = new DelegateCommand(param => OnNewGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            PauseGameCommand = new DelegateCommand(param => OnPauseGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());

            // játéktábla létrehozása
            Fields = new ObservableCollection<ConnectFourField>();
            for (int i = 0; i < _model.Height; i++)
            {
                for (int j = 0; j < _model.Width; j++)
                {
                    Fields.Add(new ConnectFourField
                    {
                        Text = String.Empty,
                        Row = i,
                        Col = j,
                        Number = i * _model.Width + j, // gomb sorszáma
                        StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                    });
                }
            }

            AppTimerEnabled = true;

            RefreshTable();
        }

        #endregion

        #region Private Methods

        private void InitTable()
        {
            if (_model.Size == this.Size)
            {
                for (int i = 0; i < _model.Height; i++)
                {
                    for (int j = 0; j < _model.Width; j++)
                    {
                        Fields[i * _model.Width + j] = new ConnectFourField()
                        {
                            Text = String.Empty,
                            Row = i,
                            Col = j,
                            Number = i * _model.Width + j, // gomb sorszáma
                            StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                        };
                    }
                }
            }
            else
            {
                Fields.Clear();
                for (int i = 0; i < _model.Height; i++)
                {
                    for (int j = 0; j < _model.Width; j++)
                    {
                        Fields.Add(new ConnectFourField()
                        {
                            Text = String.Empty,
                            Row = i,
                            Col = j,
                            Number = i * _model.Width + j, // gomb sorszáma
                            StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                        });
                    }
                }
                Size = _model.Size;
            }
        }

        private void RefreshTable()
        {
            foreach (ConnectFourField field in Fields)
            {
                field.Text = _model.GetValue(field.Row, field.Col) == 'e'
                    ? String.Empty
                    : _model.GetValue(field.Row, field.Col).ToString().ToUpper();
            }

            OnPropertyChanged(nameof(TurnTime));
        }

        private void StepGame(int index)
        {
            ConnectFourField field = Fields[index];

            _model.Move(field.Col);

            OnPropertyChanged(nameof(TurnTime));
            OnPropertyChanged(nameof(PlayerString));
            OnPropertyChanged(nameof(HasMoves));
        }

        #endregion

        #region Model Event Handlers

        private void Model_GameCreated(object? sender, EventArgs e)
        {
            InitTable(); RefreshTable(); AppTimerEnabled = true;
            OnPropertyChanged(nameof(HasMoves));
            OnPropertyChanged(nameof(AppTimerEnabled));
        }

        private void Model_TimerAdvanced(object? sender, TimerAdvancedEventArgs e)
        {
            OnPropertyChanged(nameof(TurnTime));
            OnPropertyChanged(nameof(X_Time));
            OnPropertyChanged(nameof(O_Time));
        }

        private void Model_TileChanged(object? sender, TileChangedEventArgs e)
        {
            String player = String.Empty;
            if (e.PlayerOnTile == Player.X)
            {
                player = "X";
            }
            if (e.PlayerOnTile == Player.O)
            {
                player = "O";
            }

            Fields[e.X * _model.Width + e.Y].Text = player;
        }

        #endregion

        #region Event Triggers

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnPauseGame()
        {
            PauseGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public void App_TimerPaused(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AppTimerEnabled));
        }
    }
}
