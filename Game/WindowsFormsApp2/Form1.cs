using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private const int BoardWidth = 10;
        private const int BoardHeight = 22;
        private const int SquareSize = 30;
        private int score;
        private Label scoreLabel;
        private Timer gameTimer;
        private bool gameEnded;
        private List<List<Point>> currentShapeRotations;
        private int currentRotationIndex;
        private Point shapePosition;
        private bool[,] board;
        private bool isFirstBlock = true;
        private bool isFirstBlockPlaced = false;
        private bool blockMovedDown = false;
        private bool isAutoFalling = false;
        private DateTime lastTickTime;
        private Label rowsLabel;
        private int rowsCleared;
        private int level; 
        private int linesPerLevel = 25; 
        private int initialTimerInterval = 800;
        private int minTimerInterval = 200; 
        private Label levelLabel;
        private Color blockColor = Color.Red; 
        private bool startDialogShown = false; 





        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Load += Form1_Load;
            Paint += Form1_Paint;
            KeyDown += TetrisForm_KeyDown;

            
            score = 0;  
            scoreLabel = new Label();
            scoreLabel.Text = "Score: " + score;
            scoreLabel.Location = new Point(BoardWidth * SquareSize + 20, 20);
            Controls.Add(scoreLabel);

            rowsLabel = new Label();
            rowsLabel.Text = "Rows: 0";
            rowsLabel.Location = new Point(BoardWidth * SquareSize + 20, 50);
            Controls.Add(rowsLabel);


            

            levelLabel = new Label();
            levelLabel.Text = "Level: 1";
            levelLabel.Location = new Point(BoardWidth * SquareSize + 20, 80);
            Controls.Add(levelLabel);

            InitializeGame();
        }

        private void ShowStartDialog()
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.ShowHelp = true;
            colorDialog.Color = blockColor;

            DialogResult result = MessageBox.Show("Select a color for the blocks.", "Tetris Game", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                   
                    blockColor = colorDialog.Color;
                    Invalidate(); 
                }
            }
            else
            {
                
                Close();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            if (!startDialogShown)
            {
                ShowStartDialog(); 
            }
            
            InitializeGame();
            lastTickTime = DateTime.Now;
        }

        private void InitializeGame()
        {
            isFirstBlock = true;
            isFirstBlockPlaced = false;
            board = new bool[BoardWidth, BoardHeight];
            gameEnded = false;
            gameTimer = new Timer();
            gameTimer.Interval = initialTimerInterval; 
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            GenerateNewShape();
            gameEnded = false; 
            score = 0; 
            UpdateScore(0); 

            rowsCleared = 0;
            rowsLabel.Text = "Rows: 0"; 

            level = 1;
            UpdateLevelLabel();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!gameEnded)
            {
                // Calculate time since last tick
                TimeSpan timeSinceLastTick = DateTime.Now - lastTickTime;

                // Decrease timer interval based on level
                int timerInterval = Math.Max(minTimerInterval, initialTimerInterval - (level - 1) * 100);

                if (timeSinceLastTick.TotalMilliseconds >= timerInterval)
                {
                    lastTickTime = DateTime.Now;

                    if (MoveShape(Direction.Down))
                    {
                        RefreshBoard();
                        return;
                    }
                    else
                    {
                        LockShape();
                        int linesCleared = CheckForLines();
                        UpdateScore(linesCleared);
                        GenerateNewShape();
                        RefreshBoard();

                        // Check for level completion
                        if (rowsCleared >= linesPerLevel * level)
                        {
                            level++;
                            UpdateLevelLabel();
                            gameTimer.Interval = Math.Max(minTimerInterval, initialTimerInterval - 100);
                        }
                    }

                    if (CheckEndGame())
                    {
                        gameEnded = true;
                        gameTimer.Stop();
                        MessageBox.Show("Game Over!");
                        InitializeGame();
                    }
                }
            }
        }

        private void UpdateLevelLabel()
        {
            levelLabel.Text = "Level: " + level;
        }



        private bool CheckEndGame()
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                if (board[x, 0])
                {
                    return true; 
                }
            }
            return false; 
        }



        private void LockShape()
        {
            foreach (var point in currentShapeRotations[currentRotationIndex])
            {
                int x = shapePosition.X + point.X;
                int y = shapePosition.Y + point.Y;
                board[x, y] = true;
            }
        }

        private bool MoveShape(Direction direction)
        {
            Point newPosition = shapePosition;
            switch (direction)
            {
                case Direction.Left:
                    newPosition.X--;
                    break;
                case Direction.Right:
                    newPosition.X++;
                    break;
                case Direction.Down:
                    newPosition.Y++;
                    break;
            }

            if (IsValidPosition(currentShapeRotations[currentRotationIndex], newPosition))
            {
                shapePosition = newPosition;
                return true;
            }

            return false;
        }


        private bool IsValidPosition(List<Point> shape, Point position)
        {
            foreach (var point in shape)
            {
                int x = position.X + point.X;
                int y = position.Y + point.Y;

                if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight || board[x, y])
                    return false;
            }

            return true;
        }

        private void GenerateNewShape()
        {
            var shapes = TetrisShapes.Shapes;
            var random = new Random();
            int shapeIndex = random.Next(shapes.Count);
            currentShapeRotations = shapes[shapeIndex];
            currentRotationIndex = 0;

            shapePosition = new Point(BoardWidth / 2 - 1, 0);


            if (!IsValidPosition(currentShapeRotations[currentRotationIndex], shapePosition))
            {
                if (!gameEnded)
                {
                    gameEnded = true;
                    gameTimer.Stop();
                    MessageBox.Show("Game Over!");
                    gameEnded = true;
                    InitializeGame();
                }
            }
        }

        private int CheckForLines()
        {
            int linesCleared = 0;

            for (int y = BoardHeight - 1; y >= 0; y--)
            {
                if (IsLineComplete(y))
                {
                    RemoveLine(y);
                    linesCleared++;
                    y++;
                }
            }

            return linesCleared;
        }

        private bool IsLineComplete(int y)
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                if (!board[x, y])
                    return false;
            }
            return true;
        }

        private void RemoveLine(int y)
        {
            for (int i = y; i > 0; i--)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    board[x, i] = board[x, i - 1];
                }
            }
            rowsCleared++;
            rowsLabel.Text = "Rows: " + rowsCleared;
        }

        private void UpdateScore(int linesCleared)
        {
            score += linesCleared;

            if (isFirstBlockPlaced)
            {
                score += 5;
            }
            else
            {
                isFirstBlockPlaced = true;
            }

            switch (linesCleared)
            {
                case 1:
                    score += 50;
                    break;
                case 2:
                    score += 150;
                    break;
                case 3:
                    score += 300;
                    break;
                case 4:
                    score += 600;
                    break;
            }

            

            scoreLabel.Text = "Score: " + score;
        }



        private void RefreshBoard()
        {
            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                for (int y = 0; y < BoardHeight; y++)
                {
                    if (board[x, y])
                    {
                        e.Graphics.FillRectangle(new SolidBrush(blockColor), x * SquareSize, y * SquareSize, SquareSize, SquareSize);
                        e.Graphics.DrawRectangle(Pens.Black, x * SquareSize, y * SquareSize, SquareSize, SquareSize);
                    }
                    else
                    {
                        e.Graphics.DrawRectangle(Pens.Black, x * SquareSize, y * SquareSize, SquareSize, SquareSize);
                    }
                }
            }

            foreach (var point in currentShapeRotations[currentRotationIndex])
            {
                int x = (shapePosition.X + point.X) * SquareSize;
                int y = (shapePosition.Y + point.Y) * SquareSize;
                e.Graphics.FillRectangle(new SolidBrush(blockColor), x, y, SquareSize, SquareSize);
                e.Graphics.DrawRectangle(Pens.Black, x, y, SquareSize, SquareSize);
            }
        }


        private void TetrisForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    MoveShape(Direction.Left);
                    break;
                case Keys.Right:
                    MoveShape(Direction.Right);
                    break;
                case Keys.Down:
                    MoveShape(Direction.Down);
                    isAutoFalling = false; 
                    break;
                case Keys.Up:
                    RotateShape(true);
                    break;
            }

            RefreshBoard();
        }

        private void RotateShape(bool rotateClockwise)
        {
            int nextRotationIndex = (currentRotationIndex + (rotateClockwise ? 1 : -1)) % currentShapeRotations.Count;
            if (nextRotationIndex < 0)
                nextRotationIndex += currentShapeRotations.Count;

            List<Point> rotatedShape = currentShapeRotations[nextRotationIndex];

            Point newPosition = shapePosition;

            if (IsValidPosition(rotatedShape, newPosition))
            {
                currentRotationIndex = nextRotationIndex;
            }
            else
            {
                if (TryWallKick(rotatedShape, newPosition))
                {
                    currentRotationIndex = nextRotationIndex;
                }
                else if (TryCornerKick(rotatedShape, newPosition, rotateClockwise))
                {
                    currentRotationIndex = nextRotationIndex;
                }
            }
        }

        private bool TryWallKick(List<Point> rotatedShape, Point position)
        {
            Point newPosition = new Point(position.X + 1, position.Y);
            if (IsValidPosition(rotatedShape, newPosition))
            {
                shapePosition = newPosition;
                return true;
            }

            newPosition = new Point(position.X - 1, position.Y);
            if (IsValidPosition(rotatedShape, newPosition))
            {
                shapePosition = newPosition;
                return true;
            }

            return false;
        }

        private bool TryCornerKick(List<Point> rotatedShape, Point position, bool rotateClockwise)
        {
            Point newPosition = new Point(position.X + (rotateClockwise ? -1 : 1), position.Y);
            if (IsValidPosition(rotatedShape, newPosition))
            {
                shapePosition = newPosition;
                return true;
            }

            return false;
        }


        public enum Direction
        {
            Left,
            Right,
            Down
        }

        public static class TetrisShapes
        {
            public static List<List<List<Point>>> Shapes = new List<List<List<Point>>>()
    {
        // I
        new List<List<Point>>()
        {
            new List<Point>() { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0) },
            new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3) }
        },

        // Kształt "L"
        new List<List<Point>>()
        {
            new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 2) }, 
            new List<Point>() { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(0, 1) },  
            new List<Point>() { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(1, 2) },
            new List<Point>() { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(2, 0) }
        },


        // J
        new List<List<Point>>()
        {
            new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 0) },
            new List<Point>() { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(2, 1) },
            new List<Point>() { new Point(0, 2), new Point(1, 0), new Point(1, 1), new Point(1, 2) },
            new List<Point>() { new Point(0, 1), new Point(0, 2), new Point(1, 2), new Point(2, 2) }
        },


        // O
        new List<List<Point>>()
        {
            new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(1, 0), new Point(1, 1) }
        },

        // S
        new List<List<Point>>()
        {
            new List<Point>() { new Point(1, 0), new Point(2, 0), new Point(0, 1), new Point(1, 1) },
            new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 2) }
        },

        // T
        new List<List<Point>>()
        {
            new List<Point>() { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 0) },
            new List<Point>() { new Point(1, 0), new Point(1, 1), new Point(1, 2), new Point(0, 1) },
            new List<Point>() { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(1, 1) },
            new List<Point>() { new Point(1, 0), new Point(1, 1), new Point(1, 2), new Point(2, 1) }
        },

        // Z
        new List<List<Point>>()
        {
            new List<Point>() { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(2, 1) },
            new List<Point>() { new Point(2, 0), new Point(1, 1), new Point(2, 1), new Point(1, 2) }
        }
    };
        }

    }
}