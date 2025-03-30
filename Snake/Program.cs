using System;
using System.IO;

namespace Snake
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "map.txt");
            char[,] map = ReadMap(filePath);
            int[,] snakeElementsPositions = SetStartPosition(map);
            ConsoleKeyInfo pressedKey;
            bool isWork = true;
            bool canMove = false;

            Console.WindowWidth = 120;
            Console.WindowHeight = 50;
            Console.CursorVisible = false;

            DrawMap(map);
            DrawSnake(snakeElementsPositions);

            while (isWork)
            {
                pressedKey = Console.ReadKey();
                Console.Clear();
                DrawMap(map);

                int[] direction = GetDirect(pressedKey, snakeElementsPositions, map, ref canMove);
                char reducer = '-';

                if (snakeElementsPositions.GetLength(0) == 1 && map[direction[0], direction[1]] == reducer)
                {
                    isWork = false;
                    continue;
                }

                if (canMove)
                    snakeElementsPositions = GetNextPositions(snakeElementsPositions, direction, map);

                DrawSnake(snakeElementsPositions);
                ShowInfo(pressedKey, direction, snakeElementsPositions);
            }

            Console.Clear();
            Console.WriteLine("GAME OVER");
            Console.ReadKey();
        }

        static void ShowInfo(ConsoleKeyInfo pressedKey, int[] direction, int[,] elementsPositions)
        {
            Console.SetCursorPosition(0, 11);
            Console.Write("pressedKey - " + pressedKey.Key + "\n\n" +
                "direction X - " + direction[0] + "\n" +
                "direction Y - " + direction[1] + "\n\n" +
                "elements amount - " + elementsPositions.GetLength(0) + "\n\n");

            for (int i = 0; i < elementsPositions.GetLength(0); i++)
            {
                Console.Write($"element {i + 1} coordinate X - " + elementsPositions[elementsPositions.GetLength(0) - i - 1, 0] + "\n" +
                              $"element {i + 1} coordinate Y - " + elementsPositions[elementsPositions.GetLength(0) - i - 1, 1] + "\n\n");
            }
        }

        static void DrawSnake(int[,] elementsPositions, char snakeSymbol = 'o', ConsoleColor headColor = ConsoleColor.Red, ConsoleColor bodyColor = ConsoleColor.Green)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = headColor;
            Console.SetCursorPosition(elementsPositions[elementsPositions.GetLength(0) - 1, 0], elementsPositions[elementsPositions.GetLength(0) - 1, 1]);
            Console.Write(snakeSymbol);
            Console.ForegroundColor = defaultColor;

            if (elementsPositions.GetLength(0) > 1)
            {
                Console.ForegroundColor = bodyColor;

                for (int i = elementsPositions.GetLength(0) - 2; i >= 0; i--)
                {
                    Console.SetCursorPosition(elementsPositions[i, 0], elementsPositions[i, 1]);
                    Console.Write(snakeSymbol);
                }

                Console.ForegroundColor = defaultColor;
            }
        }

        static int[,] EnlargeLength(int[,] elementsPositions)
        {
            int[,] buffer = new int[elementsPositions.GetLength(0) + 1, 2];

            for (int i = 0; i < elementsPositions.GetLength(0); i++)
            {
                buffer[i, 0] = elementsPositions[i, 0];
                buffer[i, 1] = elementsPositions[i, 1];
            }

            return buffer;
        }

        static int[,] ReduceLength(int[,] elementsPositions)
        {
            int[,] buffer = new int[elementsPositions.GetLength(0) - 1, 2];

            for (int i = 0; i < buffer.GetLength(0) - 1; i++)
            {
                buffer[i, 0] = elementsPositions[i + 2, 0];
                buffer[i, 1] = elementsPositions[i + 2, 1];
            }

            return buffer;
        }

        static int[,] GetNextPositions(int[,] elementsPositions, int[] direction, char[,] map, char enlarger = '+', char reducer = '-', char freeSpace = ' ')
        {
            if (map[direction[0], direction[1]] == enlarger)
            {
                elementsPositions = EnlargeLength(elementsPositions);
                map[direction[0], direction[1]] = freeSpace;
            }
            else if (map[direction[0], direction[1]] == reducer)
            {
                elementsPositions = ReduceLength(elementsPositions);
                map[direction[0], direction[1]] = freeSpace;
            }
            else
            {
                if (elementsPositions.GetLength(0) > 1)
                {
                    for (int i = 0; i < elementsPositions.GetLength(0) - 1; i++)
                    {
                        elementsPositions[i, 0] = elementsPositions[i + 1, 0];
                        elementsPositions[i, 1] = elementsPositions[i + 1, 1];
                    }
                }
            }

            elementsPositions[elementsPositions.GetLength(0) - 1, 0] = direction[0];
            elementsPositions[elementsPositions.GetLength(0) - 1, 1] = direction[1];

            return elementsPositions;
        }

        static bool CheckSelfCrossing(int[] oneDimensionArray, int[,] twoDimensionArray)
        {
            int matchesAmount = 0;

            for (int i = 0; i < twoDimensionArray.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < twoDimensionArray.GetLength(1); j++)
                {
                    if (twoDimensionArray[i, j] == oneDimensionArray[j])
                    {
                        matchesAmount++;
                    }
                }

                if (matchesAmount == 2)
                    return false;
                else
                    matchesAmount = 0;
            }

            return true;
        }

        static int[] GetDirect(ConsoleKeyInfo pressedKey, int[,] elementsPositions, char[,] map, ref bool canMove, char wall = '#')
        {
            int nextPositionX = elementsPositions[elementsPositions.GetLength(0) - 1, 0];
            int nextPositionY = elementsPositions[elementsPositions.GetLength(0) - 1, 1];

            int[] direction = { nextPositionX, nextPositionY };

            if (pressedKey.Key == ConsoleKey.UpArrow)
                nextPositionY--;
            if (pressedKey.Key == ConsoleKey.DownArrow)
                nextPositionY++;
            if (pressedKey.Key == ConsoleKey.LeftArrow)
                nextPositionX--;
            if (pressedKey.Key == ConsoleKey.RightArrow)
                nextPositionX++;

            bool isNotWall = map[nextPositionX, nextPositionY] != wall;
            bool isNotSelfCrossing;

            if (elementsPositions.GetLength(0) > 1)
            {
                direction = new int[] { nextPositionX, nextPositionY };
                isNotSelfCrossing = CheckSelfCrossing(direction, elementsPositions);
            }
            else
            {
                isNotSelfCrossing = true;
            }

            canMove = isNotWall && isNotSelfCrossing;

            if (canMove)
                direction = new int[] { nextPositionX, nextPositionY };

            return direction;
        }

        static int[,] SetStartPosition(char[,] map, char freeSpace = ' ')
        {
            Random random = new Random();
            int positionX = random.Next(1, 30);
            int positionY = random.Next(1, 10);

            while (map[positionX, positionY] != freeSpace)
            {
                positionX = random.Next(1, 30);
                positionY = random.Next(1, 10);
            }

            return new int[,] { { positionX, positionY } };
        }

        static void DrawMap(char[,] map)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    Console.Write(map[x, y]);
                }

                Console.Write("\n");
            }
        }
        
        static char[,] ReadMap(string filePath)
        {
            string[] file = File.ReadAllLines(filePath);
            char[,] map = new char[GetMaxLengthOfLine(file), file.Length];

            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                    map[x, y] = file[y][x];

            return map;
        }

        static int GetMaxLengthOfLine(string[] lines)
        {
            int maxLength = lines[0].Length;

            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Length > maxLength)
                    maxLength = lines[i].Length;

            return maxLength;
        }
    }
}