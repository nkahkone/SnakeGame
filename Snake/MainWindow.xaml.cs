
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 },
        };

        private readonly int rows = 20, cols = 20;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;

        private AudioManager audio;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);

            audio = new AudioManager("snakegamesoundtrackloop.wav", "apple_crunch.wav", "game_over.wav");


            // subscribe to events
            gameState.AppleEaten += OnAppleEaten;
            gameState.GameOverEvent += OnGameOverSound;
        }

        private void OnAppleEaten()
        {
            Dispatcher.Invoke(() => audio.PlayApple());
        }

        private void OnGameOverSound()
        {
            Dispatcher.Invoke(() =>
            {
                audio.PlayGameOver();
                audio.StopMusic();
            });
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            audio.PlayMusic();
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
            gameState.AppleEaten += OnAppleEaten;
            gameState.GameOverEvent += OnGameOverSound;
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // this will prevent Window_KeyDown from being called when the overlay is visible
            // and only calls Window_PreviewKeyDown
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left); break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right); break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up); break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down); break;
                case Key.A:
                    gameState.ChangeDirection(Direction.Left); break;
                case Key.D:
                    gameState.ChangeDirection(Direction.Right); break;
                case Key.W:
                    gameState.ChangeDirection(Direction.Up); break;
                case Key.S:
                    gameState.ChangeDirection(Direction.Down); break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver) 
            {
                await Task.Delay(100);
                gameState.Move();
                Draw();
            }
        }

        // dispose audio when window closes
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            audio.Dispose();
        }
        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            // loop over all grid positions
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO START";
        }
    }
}