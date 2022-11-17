using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using ConnectFour.Model;
using ConnectFour.Persistence;
using ConnectFour.View;
using ConnectFour.ViewModel;
using Microsoft.Win32;

namespace ConnectFour
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private Fields

        private Game _model = null!;
        private ConnectFourViewModel _viewModel = null!;
        private MainWindow _view = null!;
        private DispatcherTimer _timer = null!;

        #endregion

        #region Constructor

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application Event Handlers

        private void App_Startup(object? sender, StartupEventArgs e)
        {
            // modell létrehozása
            _model = new Game(new SaveFileDataAcess());
            _model.GameWon += new EventHandler<GameWonEventArgs>(Model_GameWon);
            _model.GameEnd += new EventHandler(Model_GameEnded);
            _model.StartGame();


            // nézemodell létrehozása
            _viewModel = new ConnectFourViewModel(_model);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGame);
            _viewModel.LoadGame += new EventHandler(ViewModel_LoadGame);
            _viewModel.PauseGame += new EventHandler(ViewModel_PauseGame);

            // nézet létrehozása
            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            // időzítő létrehozása
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _model.Timer_Tick(this, EventArgs.Empty);
        }

        #endregion

        #region View Event Handlers

        /// <summary>
        /// Nézet bezárásának eseménykezelője.
        /// </summary>
        private void View_Closing(object? sender, CancelEventArgs e)
        {
            bool isEnabled = _timer.IsEnabled;

            _timer.Stop();

            if (MessageBox.Show("Are you sure you want to quit?", "Connect Four", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást

                if (isEnabled) _timer.Start();
            }
        }

        #endregion

        #region ViewModel Event Handlers

        private void ViewModel_NewGame(object? sender, EventArgs e)
        {
            _model.StartGame();
            _timer.Start();
        }

        private async void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            bool isEnabled = _timer.IsEnabled;

            _timer.Stop();

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save Game";
                saveFileDialog.Filter = "Connect Four Save File | *.cfs";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // játéktábla mentése
                        await _model.SaveGameAsync(saveFileDialog.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Saving unsuccesful!" + Environment.NewLine + "Wrong file path, or corrupt save file.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Saving unsuccesful!", "Connect Four", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (isEnabled) _timer.Start();
        }

        private async void ViewModel_LoadGame(object? sender, EventArgs e)
        {
            bool isEnabled = _timer.IsEnabled;

            _timer.Stop();

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title  = "Load Game";
                openFileDialog.Filter = "Conncet Four Save File | *.cfs";
                if (openFileDialog.ShowDialog() == true)
                {
                    // játék betöltése
                    await _model.LoadGameAsync(openFileDialog.FileName);



                    _timer.Start();
                }
            }
            catch
            {
                MessageBox.Show("Loading unsuccesful!", "Connect Four", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (isEnabled) _timer.Start();
        }

        private void ViewModel_PauseGame(object? sender, EventArgs e)
        {
            _viewModel.AppTimerEnabled = !_timer.IsEnabled;
            if (_timer.IsEnabled) _timer.Stop(); else _timer.Start();
            _viewModel.App_TimerPaused(this, EventArgs.Empty);
        }

        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            _view.Close();
        }

        #endregion

        #region Model Event Handlers

        private void Model_GameWon(object? sender, GameWonEventArgs e)
        {
            _timer.Stop();

            if (e.State == GameState.WON_BY_X)
            {
                MessageBox.Show("X won the game.", "Conncet Four - Game Won");
            }
            else
            {
                MessageBox.Show("O won the game.", "Connect Four - Game Won");
            }

            _model.StartGame();
            _timer.Start();
        }

        private void Model_GameEnded(object? sender, EventArgs e)
        {
            _timer.Stop();

            MessageBox.Show("Draw!", "Conncet Four - Draw");

            _model.StartGame();
            _timer.Start();
        }

        #endregion
    }
}