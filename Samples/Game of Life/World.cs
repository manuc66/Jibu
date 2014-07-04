using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Jibu;

namespace GameOfLife
{
    enum CellAction { NONE, KILL, GIVE_LIFE }
    enum CellState { DEAD, ALIVE }

    class Cell
    {
        private CellAction action = CellAction.NONE;
        private CellState state = CellState.DEAD;

        public CellAction CellAction
        {
            set
            {
                this.action = value;
            }
            get
            {
                return action;
            }
        }

        public CellState CellState
        {
            get
            {
                return state;
            }

            set
            {
                this.state = value;
            }
        }
    }

    public class World
    {
        private Cell[,] grid;
        private int width, height;

        private Color GRIDCOLOR = Color.White;
        private Color ALIVECOLOR = Color.Orange;
        private Color DEADCOLOR = Color.Gray;

        public World(int width, int height)
        {
            this.width = width;
            this.height = height;

            grid = new Cell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Cell();
                }
            }
        }


        public int Height
        {
            get { return height; }
        }

        public int Width
        {
            get { return width; }
        }

        /**************************************
         * Finds the state that a given cell
         * will be in in the next generation.
         **************************************/
        private void FindNextState(int x, int y)
        {
            int alive = 0;

            int xBegin = (x == 0 ? x : x - 1);
            int xEnd = (x == width - 1 ? x : x + 1);

            int yBegin = (y == 0 ? y : y - 1);
            int yEnd = (y == height - 1 ? y : y + 1);

            for (int xLook = xBegin; xLook <= xEnd; xLook++)
            {
                for (int yLook = yBegin; yLook <= yEnd; yLook++)
                {
                    if ((x != xLook || y != yLook) && grid[xLook, yLook].CellState == CellState.ALIVE)
                        alive++;
                }
            }


            Cell c = grid[x, y];

            switch (c.CellState)
            {
                case CellState.ALIVE:
                    if (alive == 2 || alive == 3)
                        c.CellAction = CellAction.NONE;
                    else
                        c.CellAction = CellAction.KILL;
                    break;
                case CellState.DEAD:
                    if (alive == 3)
                        c.CellAction = CellAction.GIVE_LIFE;
                    else
                        c.CellAction = CellAction.NONE;
                    break;
            }
        }

        /**************************************
         * Changes the state for a cell.
         **************************************/
        private void ChangeState(int x, int y)
        {
            Cell c = grid[x, y];

            switch (c.CellAction)
            {
                case CellAction.KILL:
                    c.CellState = CellState.DEAD;
                    break;
                case CellAction.GIVE_LIFE:
                    c.CellState = CellState.ALIVE;
                    break;
                case CellAction.NONE:
                    break;
            }
        }

        /**************************************
         * Sets the state of a single cell to
         * ALIVE.
         **************************************/
        public void SetAlive(int x, int y)
        {
            if ((x >= 0 && x < width) && (y >= 0 && y < height))
                grid[x, y].CellState = CellState.ALIVE;
        }

        /**************************************
         * Transitions to the next iteration
         * of life.
         * Sequential version.
         **************************************/
        public void SequentialTransition()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    FindNextState(x, y);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ChangeState(x, y);
                }

            }
        }

        /**************************************
         * Transitions to the next iteration
         * of life.
         * Parallel version.
         **************************************/
        public void ParallelTransition()
        {
            Parallel.For(0, width, 10, delegate(int x)
            {
                for (int y = 0; y < height; y++)
                {
                    FindNextState(x, y);
                }

            });

            Parallel.For(0, width, 10, delegate(int x)
            {
                for (int y = 0; y < height; y++)
                {
                    ChangeState(x, y);
                }

            });

        }

        /**************************************
         * Draws the world to a BufferedImage
         * instance.
         * Sequential version.
         **************************************/
        public void ImageDraw(Graphics g, int cellSize)
        {
            
            Brush gridBrush = new SolidBrush(GRIDCOLOR);
            g.FillRectangle(gridBrush, 0, 0, width * cellSize, height * cellSize);            

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x,y].CellState == CellState.ALIVE)
                    {
                        Brush aliveBrush = new SolidBrush(ALIVECOLOR);
                        g.FillRectangle(aliveBrush, x * cellSize, y * cellSize, cellSize, cellSize);
                    }
                    else
                    {
                        Pen deadPen = new Pen(DEADCOLOR);
                        g.DrawRectangle(deadPen, x * cellSize, y * cellSize, cellSize, cellSize);
                    }
                }
            }
        }

        /**************************************
         * Draws the world to a the console.
         **************************************/
        public void ConsoleDraw()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Cell c = grid[x, y];

                    if (c.CellState == CellState.ALIVE)
                        Console.Write(" o ");
                    else
                        Console.Write(" x ");

                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /**************************************
         * Adds an array of LifeForms to the
         * world.
         **************************************/
        public void AddLifeForms(LifeForm[] lifeForms)
        {
            foreach (LifeForm lf in lifeForms)
            {
                int xOffset = lf.Offset.x;
                int yOffset = lf.Offset.y;

                foreach (Point p in lf.Points)
                {
                    SetAlive(p.x + xOffset, p.y + yOffset);
                }
            }
        }
    }

}
